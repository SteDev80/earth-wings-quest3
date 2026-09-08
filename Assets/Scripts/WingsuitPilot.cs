using UnityEngine;
using UnityEngine.XR;
using UnityEngine.UI;
using CesiumForUnity;

namespace EarthWings
{
    public sealed class WingsuitPilot : MonoBehaviour
    {
        public Transform head, leftHand, rightHand;
        public Text readout, instructions;
        public Image hudBackground;
        public float launchHeight = 900;
        public bool mapMode;
        public string locationName = "";
        public System.Action nextLocation, returnToLaunch;
        public CesiumGlobeAnchor globeAnchor;
        Vector3 localVelocity;
        float span = 1.35f, openness, calibrationTime, cruiseSpeed = 90;
        bool calibrated, paused = true, freeFlight = true, previousA, previousB, previousY, previousMode, trackingWasLost;
        string notice = "Volo libero: braccia rilassate. Premi A";
        readonly System.Collections.Generic.List<XRNodeState> nodeStates = new();
        readonly System.Collections.Generic.List<XRInputSubsystem> subsystems = new();

        void Start()
        {
            SubsystemManager.GetSubsystems(subsystems);
            foreach (var s in subsystems) s.TrySetTrackingOriginMode(TrackingOriginModeFlags.Floor);
            Restart();
        }
        public void Restart()
        {
            if (returnToLaunch != null) returnToLaunch();
            else transform.position = Vector3.up * launchHeight;
            transform.rotation = Quaternion.identity;
            localVelocity = Vector3.zero; paused = true;
            if (globeAnchor) globeAnchor.Sync();
        }
        static bool Pose(InputDevice d, Transform t)
        {
            if (!d.isValid || !d.TryGetFeatureValue(CommonUsages.isTracked, out bool tracked) || !tracked ||
                !d.TryGetFeatureValue(CommonUsages.devicePosition, out Vector3 p) ||
                !d.TryGetFeatureValue(CommonUsages.deviceRotation, out Quaternion q)) return false;
            t.SetLocalPositionAndRotation(p, q); return true;
        }
        bool HeadIsTracked()
        {
            // CenterEye is a pose node, but is not always exposed as a separate input device.
            // Query the same node state data used by the camera's TrackedPoseDriver.
            InputTracking.GetNodeStates(nodeStates);
            foreach (var state in nodeStates)
                if ((state.nodeType == XRNode.CenterEye || state.nodeType == XRNode.Head) && state.tracked &&
                    state.TryGetPosition(out _) && state.TryGetRotation(out _)) return true;
            var device = InputDevices.GetDeviceAtXRNode(XRNode.Head);
            return device.isValid && device.TryGetFeatureValue(CommonUsages.isTracked, out bool tracked) && tracked &&
                device.TryGetFeatureValue(CommonUsages.devicePosition, out _) &&
                device.TryGetFeatureValue(CommonUsages.deviceRotation, out _);
        }
        void Update()
        {
            var l = InputDevices.GetDeviceAtXRNode(XRNode.LeftHand);
            var r = InputDevices.GetDeviceAtXRNode(XRNode.RightHand);
            bool headTracked = HeadIsTracked();
            bool handsTracked = Pose(l, leftHand) & Pose(r, rightHand);
            l.TryGetFeatureValue(CommonUsages.trigger, out float lt);
            r.TryGetFeatureValue(CommonUsages.trigger, out float rt);
            l.TryGetFeatureValue(CommonUsages.grip, out float leftGrip);
            r.TryGetFeatureValue(CommonUsages.grip, out float rightGrip);
            l.TryGetFeatureValue(CommonUsages.gripButton, out bool leftGripButton);
            r.TryGetFeatureValue(CommonUsages.gripButton, out bool rightGripButton);
            r.TryGetFeatureValue(CommonUsages.primaryButton, out bool a);
            r.TryGetFeatureValue(CommonUsages.secondaryButton, out bool b);
            l.TryGetFeatureValue(CommonUsages.primaryButton, out bool boost);
            l.TryGetFeatureValue(CommonUsages.secondaryButton, out bool changeLocation);
            r.TryGetFeatureValue(CommonUsages.primary2DAxis, out Vector2 rightPrimaryStick);
            r.TryGetFeatureValue(CommonUsages.secondary2DAxis, out Vector2 rightSecondaryStick);
            Vector2 rightStick = rightPrimaryStick.sqrMagnitude >= rightSecondaryStick.sqrMagnitude ? rightPrimaryStick : rightSecondaryStick;
            l.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out bool mode);
            if (mode && !previousMode)
            {
                freeFlight = !freeFlight; paused = true; localVelocity = Vector3.zero;
                notice = freeFlight ? "Guida con lo sguardo. Premi A" : calibrated ? "Tuta alare. Premi A" : "Calibra con le braccia aperte e i due grilletti";
            }
            if (changeLocation && !previousY) { Restart(); notice = "Premi A per ripartire"; }
            if (b && !previousB && nextLocation != null)
            { nextLocation(); Restart(); notice = "Attendi il terreno, poi premi A"; }
            float dt = Mathf.Min(Time.deltaTime, .05f);
            bool trackingReady = headTracked && (freeFlight || handsTracked);
            if (!trackingReady)
            {
                paused = true; calibrationTime = 0;
                notice = !headTracked ? "Visore non tracciato: indossalo per riprendere" : "Mani non tracciate. Clic stick SX: volo libero";
                if (!trackingWasLost) Debug.Log("Flight tracking lost: headset=" + headTracked + ", hands=" + handsTracked + ", free=" + freeFlight);
                trackingWasLost = true;
            }
            else
            {
                if (trackingWasLost)
                {
                    notice = "Tracking ripristinato. Premi A";
                    Debug.Log("Flight tracking restored; press A to resume");
                    trackingWasLost = false;
                }
                if (!freeFlight && lt > .8f && rt > .8f)
                {
                    paused = true; calibrationTime += dt;
                    notice = "Tieni le braccia aperte...";
                    float distance = Vector3.Distance(leftHand.localPosition, rightHand.localPosition);
                    if (calibrationTime >= 1.5f && distance > .65f)
                    { span = distance; calibrated = true; notice = "Calibrato. Rilascia i grilletti e premi A"; }
                }
                else
                {
                    calibrationTime = 0;
                    if (a && !previousA && (freeFlight || calibrated))
                    {
                        if (paused && mapMode && !Physics.Raycast(head.position, -transform.up, 50000))
                            notice = "Attendi il caricamento del terreno";
                        else { paused = !paused; notice = ""; }
                    }
                }
                if (handsTracked)
                {
                    float target = Mathf.InverseLerp(span * .25f, span * .9f, Vector3.Distance(leftHand.localPosition, rightHand.localPosition));
                    openness = Mathf.Lerp(openness, target, 1 - Mathf.Exp(-dt * 5));
                }
                if (!paused && (freeFlight || calibrated))
                {
                    Vector3 desired;
                    if (freeFlight)
                    {
                        // The side grips are the throttle: right accelerates, left brakes.
                        float throttle = (rightGripButton || rightGrip > .15f ? 1f : 0f) - (leftGripButton || leftGrip > .15f ? 1f : 0f);
                        cruiseSpeed = Mathf.Clamp(cruiseSpeed + throttle * 95f * dt, 10, 180);
                        transform.Rotate(0, rightStick.x * 58f * dt, 0, Space.Self);
                        Vector3 direction = transform.InverseTransformDirection(head.forward);
                        direction.y += rt * 1.25f - lt * 1.25f;
                        desired = direction.normalized * cruiseSpeed;
                    }
                    else
                    {
                        float turn = Mathf.Clamp((leftHand.localPosition.y - rightHand.localPosition.y) / .45f, -1, 1);
                        transform.Rotate(0, (turn * 28 * openness + rightStick.x * 58) * dt, 0, Space.Self);
                        float pitch = Mathf.Asin(Mathf.Clamp(Vector3.Dot(head.forward, transform.up), -1, 1)) * Mathf.Rad2Deg;
                        float climb = Mathf.SmoothStep(0, 1, Mathf.InverseLerp(8, 40, pitch));
                        desired = new Vector3(0, Mathf.Lerp(-Mathf.Lerp(55, 7, openness), 28, climb), Mathf.Lerp(14, 32, openness));
                    }
                    desired *= boost ? 5f : 1f;
                    localVelocity = Vector3.Lerp(localVelocity, desired, 1 - Mathf.Exp(-dt * .8f));
                    Vector3 step = transform.TransformDirection(localVelocity) * dt;
                    if (step.sqrMagnitude > .000001f && Physics.SphereCast(head.position, .4f, step.normalized, out _, step.magnitude + .5f, ~0, QueryTriggerInteraction.Ignore))
                    { paused = true; localVelocity = Vector3.zero; notice = "Terreno vicino. Premi B per tornare in quota"; }
                    else transform.position += step;
                    if (globeAnchor) globeAnchor.Sync();
                    if (!mapMode && (transform.position.y < 15 || new Vector2(transform.position.x, transform.position.z).magnitude > 4500))
                    { paused = true; notice = "Fine addestramento. Premi B"; }
                }
            }
            previousA = a; previousB = b; previousY = changeLocation; previousMode = mode;
            int altitude = globeAnchor ? Mathf.RoundToInt((float)globeAnchor.longitudeLatitudeHeight.z) : Mathf.RoundToInt(transform.position.y);
            if (hudBackground) hudBackground.gameObject.SetActive(paused);
            if (readout) readout.text = paused
                ? (mapMode ? locationName : "ADDESTRAMENTO") + "  ·  " + (freeFlight ? "LIBERO" : "TUTA") + "  ·  " + notice
                : Mathf.RoundToInt(localVelocity.magnitude * 3.6f) + " km/h   ·   " + altitude + " m";
            if (instructions)
            {
                instructions.gameObject.SetActive(paused);
                if (paused) instructions.text = "Grip DX: accelera  ·  Grip SX: frena  ·  Stick DX: virata\nGrilletti: quota  ·  A avvia  ·  Y riparti  ·  B località";
            }
        }
        void OnApplicationPause(bool value) { if (value) paused = true; }
        void OnApplicationFocus(bool value) { if (!value) paused = true; }
    }
}
