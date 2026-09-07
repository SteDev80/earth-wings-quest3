using CesiumForUnity;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace EarthWings
{
    // Render Cesium's original dynamic attribution UI into a stereo-visible world canvas.
    public sealed class VrCredits : MonoBehaviour
    {
        RenderTexture texture;
        PanelSettings settings;
        void Start()
        {
            var credits = CesiumCreditSystem.GetDefaultCreditSystem();
            var document = credits.GetComponent<UIDocument>();
            if (!document || !document.panelSettings)
            {
                Debug.LogError("Cesium attribution UI unavailable. Do not distribute map mode.");
                return;
            }
            texture = new RenderTexture(1536, 384, 0, RenderTextureFormat.ARGB32);
            texture.Create();
            settings = Instantiate(document.panelSettings);
            settings.targetTexture = texture;
            settings.scaleMode = PanelScaleMode.ConstantPixelSize;
            document.panelSettings = settings;
            var display = new GameObject("Google and data attribution", typeof(RectTransform), typeof(RawImage));
            display.transform.SetParent(transform, false);
            display.transform.localPosition = new Vector3(0,-610,0);
            display.GetComponent<RawImage>().raycastTarget = false;
            display.GetComponent<RectTransform>().sizeDelta = new Vector2(1000,250);
            display.GetComponent<RawImage>().texture = texture;
        }
        void OnDestroy()
        {
            if (texture) { texture.Release(); Destroy(texture); }
            if (settings) Destroy(settings);
        }
    }
}
