using UnityEngine;
using UnityEngine.UI;
using CesiumForUnity;
using UnityEngine.SpatialTracking;

namespace EarthWings
{
    public sealed class FlightWorld : MonoBehaviour
    {
        [System.Serializable] public class MapConfig { public string ionToken = ""; public long assetId; public double latitude = 45.7874, longitude = 6.9731, originHeight = 0; public float launchHeight = 3200; }
        void Awake()
        {
            Application.targetFrameRate = 72;
            var rig = new GameObject("Wingsuit Rig");
            var pilot = rig.AddComponent<WingsuitPilot>();
            pilot.head = new GameObject("Head", typeof(Camera), typeof(AudioListener)).transform;
            pilot.head.SetParent(rig.transform, false);
            pilot.head.localPosition = Vector3.up * 1.65f;
            var cam = pilot.head.GetComponent<Camera>(); cam.tag = "MainCamera";
            cam.stereoTargetEye = StereoTargetEyeMask.Both;
            cam.ResetStereoViewMatrices();
            cam.ResetStereoProjectionMatrices();
            // Use the XR center-eye pose, including the final update before rendering.
            // The flight controller must never write the camera pose independently.
            var headTracking = pilot.head.gameObject.AddComponent<TrackedPoseDriver>();
            headTracking.SetPoseSource(TrackedPoseDriver.DeviceType.GenericXRDevice, TrackedPoseDriver.TrackedPose.Center);
            headTracking.trackingType = TrackedPoseDriver.TrackingType.RotationAndPosition;
            headTracking.updateType = TrackedPoseDriver.UpdateType.UpdateAndBeforeRender;
            headTracking.UseRelativeTransform = false;
            // At 3 km altitude the geographic horizon is roughly 200 km away. The old
            // 15 km far plane cut off the streamed tiles, leaving an artificial ring.
            cam.nearClipPlane = .15f; cam.farClipPlane = 300000;
            cam.clearFlags = CameraClearFlags.Skybox; cam.backgroundColor = new Color(.32f,.58f,.73f);
            pilot.leftHand = Hand(rig.transform, "Left", new Color(.1f,.9f,.8f));
            pilot.rightHand = Hand(rig.transform, "Right", new Color(1,.6f,.2f));
            var panel = new GameObject("Flight display", typeof(Canvas));
            panel.transform.SetParent(pilot.head, false);
            panel.transform.localPosition = new Vector3(0,0,1.8f);
            panel.transform.localScale = Vector3.one * .0015f;
            var canvas = panel.GetComponent<Canvas>(); canvas.renderMode = RenderMode.WorldSpace;
            canvas.worldCamera = cam;
            panel.GetComponent<RectTransform>().sizeDelta = new Vector2(1800,1100);
            var backdrop = new GameObject("Lower HUD background", typeof(RectTransform), typeof(Image));
            backdrop.transform.SetParent(panel.transform, false);
            backdrop.GetComponent<RectTransform>().anchoredPosition = new Vector2(0,-480);
            backdrop.GetComponent<RectTransform>().sizeDelta = new Vector2(920,150);
            backdrop.GetComponent<Image>().color = new Color(.015f,.035f,.05f,.72f);
            pilot.hudBackground = backdrop.GetComponent<Image>();
            pilot.readout = HudLabel(panel.transform, "Flight status", new Vector2(0,-440), new Vector2(880,55), 16);
            pilot.instructions = HudLabel(panel.transform, "Controls", new Vector2(0,-500), new Vector2(880,75), 20);
            NavigationHud.Add(panel.transform, pilot, cam);
            var sun = new GameObject("Sun", typeof(Light)); sun.transform.rotation = Quaternion.Euler(45,-30,0);
            sun.GetComponent<Light>().type = LightType.Directional;
            RenderSettings.ambientLight = new Color(.65f,.7f,.75f);
            var skyShader = Shader.Find("Skybox/Procedural");
            if (skyShader)
            {
                var sky = new Material(skyShader);
                sky.SetColor("_SkyTint", new Color(.38f,.62f,.86f));
                sky.SetFloat("_AtmosphereThickness", 1.15f);
                sky.SetFloat("_Exposure", 1.08f);
                RenderSettings.skybox = sky;
            }
            // A light atmospheric veil hides LOD transitions at the distant horizon.
            RenderSettings.fog = true;
            RenderSettings.fogMode = FogMode.ExponentialSquared;
            RenderSettings.fogColor = new Color(.32f,.58f,.73f);
            RenderSettings.fogDensity = .000018f;
            var configAsset = Resources.Load<TextAsset>("MapCredentials");
            MapConfig config = configAsset ? JsonUtility.FromJson<MapConfig>(configAsset.text) : null;
            if (config != null && !string.IsNullOrWhiteSpace(config.ionToken) && config.assetId > 0)
            {
                var globe = new GameObject("Google 3D", typeof(CesiumGeoreference));
                var geo = globe.GetComponent<CesiumGeoreference>();
                geo.SetOriginLongitudeLatitudeHeight(config.longitude, config.latitude, config.originHeight);
                rig.transform.SetParent(globe.transform, false);
                var tilesObject = new GameObject("Photorealistic tiles"); tilesObject.transform.SetParent(globe.transform, false);
                var tiles = tilesObject.AddComponent<Cesium3DTileset>();
                tiles.ionAssetID = config.assetId; tiles.ionAccessToken = config.ionToken;
                // The high-altitude view benefits from an ample cache and more parallel loads.
                // Geometry remains at a coarse LOD while the user is far above the ground.
                tiles.maximumScreenSpaceError = 18; tiles.maximumSimultaneousTileLoads = 24;
                tiles.maximumCachedBytes = 1024 * 1024 * 1024; tiles.loadingDescendantLimit = 160;
                tiles.enableFrustumCulling = false;
                tiles.enforceCulledScreenSpaceError = true; tiles.culledScreenSpaceError = 48;
                tiles.createPhysicsMeshes = true;
                tiles.preloadAncestors = true; tiles.preloadSiblings = true;
                tiles.showCreditsOnScreen = true;
                pilot.launchHeight = config.launchHeight; pilot.mapMode = true;
                pilot.locationName = "VALLE D'AOSTA / COURMAYEUR";
                bool inValley = true;
                pilot.nextLocation = () =>
                {
                    inValley = !inValley;
                    pilot.locationName = inValley ? "VALLE D'AOSTA / COURMAYEUR" : "ROMA / COLOSSEO";
                };
                pilot.returnToLaunch = () =>
                {
                    geo.SetOriginLongitudeLatitudeHeight(inValley ? config.longitude : 12.4922,
                        inValley ? config.latitude : 41.8902, inValley ? config.originHeight : 0);
                    pilot.launchHeight = inValley ? config.launchHeight : 1000;
                    rig.transform.position = Vector3.up * pilot.launchHeight;
                };
                pilot.globeAnchor = rig.AddComponent<CesiumGlobeAnchor>();
                pilot.globeAnchor.adjustOrientationForGlobeWhenMoving = true;
                rig.AddComponent<CesiumOriginShift>().distance = 1000;
                MunicipalityLabels.Add(globe.transform, cam);
                panel.AddComponent<VrCredits>();
            }
            else TrainingGround();
        }
        static Text HudLabel(Transform parent, string name, Vector2 position, Vector2 size, int fontSize)
        {
            var label = new GameObject(name, typeof(RectTransform), typeof(Text), typeof(Shadow));
            label.transform.SetParent(parent, false);
            var rect = label.GetComponent<RectTransform>();
            rect.sizeDelta = size; rect.anchoredPosition = position;
            var text = label.GetComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = fontSize; text.alignment = TextAnchor.MiddleCenter; text.color = Color.white;
            label.GetComponent<Shadow>().effectColor = new Color(0,0,0,.85f);
            return text;
        }
        static Transform Hand(Transform parent, string name, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Sphere); go.name = name;
            go.transform.SetParent(parent, false); go.transform.localScale = Vector3.one * .09f;
            Destroy(go.GetComponent<Collider>());
            go.GetComponent<Renderer>().material.color = color;
            return go.transform;
        }
        static void TrainingGround()
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube); ground.name = "Training ground";
            ground.transform.position = new Vector3(0,-20,0); ground.transform.localScale = new Vector3(12000,40,12000);
            ground.GetComponent<Renderer>().material.color = new Color(.13f,.28f,.25f);
            var random = new System.Random(43);
            for (int i=0; i<75; i++)
            {
                var tower = GameObject.CreatePrimitive(PrimitiveType.Cube);
                float height = 40 + (float)random.NextDouble() * 250;
                tower.transform.position = new Vector3((float)random.NextDouble()*5000-2500,height/2,(float)random.NextDouble()*5000-2500);
                tower.transform.localScale = new Vector3(80,height,80);
                tower.GetComponent<Renderer>().material.color = Color.Lerp(new Color(.25f,.4f,.42f),new Color(.65f,.71f,.64f),(float)random.NextDouble());
            }
        }
    }
}
