using UnityEngine;
using UnityEngine.UI;
using CesiumForUnity;

namespace EarthWings
{
    public sealed class NavigationHud : MonoBehaviour
    {
        const double MinLon = 6.0, MaxLon = 18.8, MinLat = 36.0, MaxLat = 47.8;
        public WingsuitPilot pilot;
        RectTransform marker;
        Text locationText, compassText;

        public static void Add(Transform parent, WingsuitPilot pilot, Camera camera)
        {
            var root = new GameObject("Navigation HUD", typeof(RectTransform), typeof(Image), typeof(NavigationHud));
            root.transform.SetParent(parent, false);
            var rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(300, 300);
            rect.anchoredPosition = new Vector2(-720, -345);
            var image = root.GetComponent<Image>();
            image.color = new Color(.01f, .025f, .035f, .42f);

            var map = new GameObject("Italy map", typeof(RectTransform), typeof(ItalyMapGraphic));
            map.transform.SetParent(root.transform, false);
            map.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 230);
            map.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 12);

            AddLabel(root.transform, "N", new Vector2(0, 126), new Vector2(34, 24), 22, TextAnchor.MiddleCenter);
            AddLabel(root.transform, "S", new Vector2(0, -104), new Vector2(34, 24), 18, TextAnchor.MiddleCenter);
            AddLabel(root.transform, "O", new Vector2(-126, 6), new Vector2(34, 24), 18, TextAnchor.MiddleCenter);
            AddLabel(root.transform, "E", new Vector2(126, 6), new Vector2(34, 24), 18, TextAnchor.MiddleCenter);

            var pin = new GameObject("Current position", typeof(RectTransform), typeof(MapHeadingTriangle));
            pin.transform.SetParent(root.transform, false);
            var pinRect = pin.GetComponent<RectTransform>();
            pinRect.sizeDelta = new Vector2(26, 34);
            pin.GetComponent<MapHeadingTriangle>().color = new Color(1f, .28f, .04f, .95f);

            var location = AddLabel(root.transform, "ITALIA", new Vector2(0, -132), new Vector2(270, 30), 19, TextAnchor.MiddleCenter);
            var compass = AddLabel(parent, "Compass", new Vector2(0, -360), new Vector2(560, 38), 22, TextAnchor.MiddleCenter);

            var hud = root.GetComponent<NavigationHud>();
            hud.pilot = pilot;
            hud.marker = pinRect;
            hud.locationText = location;
            hud.compassText = compass;
        }

        static Text AddLabel(Transform parent, string text, Vector2 position, Vector2 size, int fontSize, TextAnchor alignment)
        {
            var label = new GameObject(text, typeof(RectTransform), typeof(Text), typeof(Shadow));
            label.transform.SetParent(parent, false);
            var rect = label.GetComponent<RectTransform>();
            rect.sizeDelta = size;
            rect.anchoredPosition = position;
            var uiText = label.GetComponent<Text>();
            uiText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            uiText.text = text;
            uiText.fontSize = fontSize;
            uiText.fontStyle = FontStyle.Bold;
            uiText.alignment = alignment;
            uiText.color = new Color(.92f, .98f, 1f, .95f);
            label.GetComponent<Shadow>().effectColor = new Color(0, 0, 0, .85f);
            return uiText;
        }

        void LateUpdate()
        {
            if (!pilot) return;
            if (marker)
            {
                Vector2 position = Vector2.zero;
                if (pilot.globeAnchor)
                {
                    var llh = pilot.globeAnchor.longitudeLatitudeHeight;
                    float x = Mathf.InverseLerp((float)MinLon, (float)MaxLon, (float)llh.x);
                    float y = Mathf.InverseLerp((float)MinLat, (float)MaxLat, (float)llh.y);
                    position = new Vector2((x - .5f) * 230f, (y - .5f) * 205f);
                }
                marker.anchoredPosition = position;
                marker.localRotation = Quaternion.Euler(0, 0, -pilot.transform.eulerAngles.y);
            }
            if (locationText)
                locationText.text = pilot.mapMode ? pilot.locationName : "AREA DI ADDESTRAMENTO";
            if (compassText)
            {
                float heading = pilot.transform.eulerAngles.y;
                compassText.text = Mathf.RoundToInt(heading).ToString("000") + "°";
            }
        }
    }

    public sealed class MapHeadingTriangle : Graphic
    {
        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            var rect = rectTransform.rect;
            var top = new Vector2(0, rect.yMax);
            var left = new Vector2(rect.xMin, rect.yMin);
            var right = new Vector2(rect.xMax, rect.yMin);
            vh.AddVert(top, color, Vector2.zero);
            vh.AddVert(left, color, Vector2.zero);
            vh.AddVert(right, color, Vector2.zero);
            vh.AddTriangle(0, 1, 2);
        }
    }

    public sealed class ItalyMapGraphic : Graphic
    {
        static readonly Vector2[] Mainland =
        {
            new(7.5f,45.9f), new(8.8f,45.2f), new(10.2f,44.5f), new(11.4f,43.7f),
            new(12.5f,42.8f), new(13.3f,41.9f), new(14.5f,41.2f), new(15.7f,40.5f),
            new(16.7f,39.3f), new(17.2f,38.4f), new(16.4f,38.5f), new(15.7f,39.4f),
            new(14.7f,40.1f), new(13.7f,40.8f), new(12.7f,41.7f), new(11.7f,42.8f),
            new(10.5f,43.7f), new(9.1f,44.3f), new(8.0f,44.0f), new(7.4f,44.6f), new(7.5f,45.9f)
        };
        static readonly Vector2[] Sicily =
        {
            new(12.4f,38.1f), new(13.6f,38.2f), new(15.1f,37.8f), new(15.5f,37.1f),
            new(14.0f,36.7f), new(12.7f,37.2f), new(12.4f,38.1f)
        };
        static readonly Vector2[] Sardinia =
        {
            new(8.1f,41.3f), new(9.0f,41.1f), new(9.4f,40.1f), new(9.2f,39.0f),
            new(8.5f,38.7f), new(8.1f,39.7f), new(8.1f,41.3f)
        };

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            DrawPolyline(vh, Mainland, new Color(.62f, .9f, .72f, .9f), 7f);
            DrawPolyline(vh, Sicily, new Color(.62f, .9f, .72f, .9f), 6f);
            DrawPolyline(vh, Sardinia, new Color(.62f, .9f, .72f, .9f), 6f);
        }

        void DrawPolyline(VertexHelper vh, Vector2[] points, Color color, float width)
        {
            for (int i = 0; i < points.Length - 1; i++)
                AddLine(vh, ToRect(points[i]), ToRect(points[i + 1]), color, width);
        }

        Vector2 ToRect(Vector2 lonLat)
        {
            var rect = rectTransform.rect;
            float x = Mathf.InverseLerp(6f, 18.8f, lonLat.x);
            float y = Mathf.InverseLerp(36f, 47.8f, lonLat.y);
            return new Vector2(rect.xMin + x * rect.width, rect.yMin + y * rect.height);
        }

        static void AddLine(VertexHelper vh, Vector2 a, Vector2 b, Color color, float width)
        {
            Vector2 direction = (b - a).normalized;
            Vector2 normal = new Vector2(-direction.y, direction.x) * (width * .5f);
            int start = vh.currentVertCount;
            vh.AddVert(a - normal, color, Vector2.zero);
            vh.AddVert(a + normal, color, Vector2.zero);
            vh.AddVert(b + normal, color, Vector2.zero);
            vh.AddVert(b - normal, color, Vector2.zero);
            vh.AddTriangle(start, start + 1, start + 2);
            vh.AddTriangle(start + 2, start + 3, start);
        }
    }
}
