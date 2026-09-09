using CesiumForUnity;
using UnityEngine;

namespace EarthWings
{
    public sealed class HighAltitudeStreamingProfile : MonoBehaviour
    {
        public WingsuitPilot pilot;
        public Camera cameraToTune;
        public Cesium3DTileset tileset;

        const float LowAltitude = 8000f;
        const float HighAltitude = 10000f;
        const float LowFarClip = 300000f;
        const float HighFarClip = 900000f;
        const float LowFogDensity = .000018f;
        const float HighFogDensity = .000006f;

        void Start()
        {
            ApplyTilesetSeamProfile();
        }

        void LateUpdate()
        {
            if (!pilot || !cameraToTune) return;
            float altitude = pilot.globeAnchor ? (float)pilot.globeAnchor.longitudeLatitudeHeight.z : pilot.transform.position.y;
            float highBlend = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(LowAltitude, HighAltitude, altitude));
            cameraToTune.farClipPlane = Mathf.Lerp(LowFarClip, HighFarClip, highBlend);
            cameraToTune.nearClipPlane = Mathf.Lerp(.15f, .45f, highBlend);
            RenderSettings.fogDensity = Mathf.Lerp(LowFogDensity, HighFogDensity, highBlend);
        }

        void ApplyTilesetSeamProfile()
        {
            if (!tileset) return;
            if (!tileset.forbidHoles) tileset.forbidHoles = true;
            if (tileset.enableFogCulling) tileset.enableFogCulling = false;
            if (tileset.enableFrustumCulling) tileset.enableFrustumCulling = false;
            if (!tileset.enforceCulledScreenSpaceError) tileset.enforceCulledScreenSpaceError = true;
        }
    }
}
