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
            image.color = new Color(.01f, .035f, .07f, .55f);

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
                locationText.text = pilot.mapMode && pilot.globeAnchor ? RegionFor(pilot.globeAnchor.longitudeLatitudeHeight.x, pilot.globeAnchor.longitudeLatitudeHeight.y) : "AREA DI ADDESTRAMENTO";
            if (compassText)
            {
                float heading = pilot.transform.eulerAngles.y;
                compassText.text = Mathf.RoundToInt(heading).ToString("000") + "°";
            }
        }

        static string RegionFor(double lon, double lat)
        {
            if (Inside(lon, lat, 6.55, 7.95, 45.45, 46.10)) return "VALLE D'AOSTA";
            if (Inside(lon, lat, 6.55, 9.25, 44.05, 46.50)) return "PIEMONTE";
            if (Inside(lon, lat, 8.45, 11.45, 44.65, 46.65)) return "LOMBARDIA";
            if (Inside(lon, lat, 10.35, 13.10, 45.10, 46.75)) return "TRENTINO-ALTO ADIGE";
            if (Inside(lon, lat, 10.60, 13.25, 44.80, 46.75)) return "VENETO";
            if (Inside(lon, lat, 12.30, 13.90, 45.55, 46.70)) return "FRIULI-VENEZIA GIULIA";
            if (Inside(lon, lat, 7.45, 10.15, 43.70, 44.70)) return "LIGURIA";
            if (Inside(lon, lat, 9.10, 12.80, 43.65, 45.25)) return "EMILIA-ROMAGNA";
            if (Inside(lon, lat, 9.60, 12.45, 42.20, 44.45)) return "TOSCANA";
            if (Inside(lon, lat, 12.05, 13.30, 42.65, 43.70)) return "UMBRIA";
            if (Inside(lon, lat, 12.20, 14.00, 42.65, 44.00)) return "MARCHE";
            if (Inside(lon, lat, 11.35, 13.95, 40.75, 42.95)) return "LAZIO";
            if (Inside(lon, lat, 13.00, 14.80, 41.65, 42.95)) return "ABRUZZO";
            if (Inside(lon, lat, 13.70, 15.20, 41.35, 42.20)) return "MOLISE";
            if (Inside(lon, lat, 13.75, 15.85, 39.95, 41.55)) return "CAMPANIA";
            if (Inside(lon, lat, 15.20, 18.55, 39.65, 42.25)) return "PUGLIA";
            if (Inside(lon, lat, 15.35, 16.90, 39.85, 41.25)) return "BASILICATA";
            if (Inside(lon, lat, 15.60, 17.35, 37.85, 40.25)) return "CALABRIA";
            if (Inside(lon, lat, 12.25, 15.75, 36.55, 38.40)) return "SICILIA";
            if (Inside(lon, lat, 8.05, 9.85, 38.75, 41.45)) return "SARDEGNA";
            return "ITALIA";
        }

        static bool Inside(double lon, double lat, double minLon, double maxLon, double minLat, double maxLat)
        {
            return lon >= minLon && lon <= maxLon && lat >= minLat && lat <= maxLat;
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
            new(7.0f,45.9f), new(7.8f,45.6f), new(8.7f,45.4f), new(9.7f,45.1f),
            new(10.6f,45.3f), new(11.8f,45.6f), new(12.8f,45.7f), new(13.6f,45.8f),
            new(13.5f,45.2f), new(12.7f,44.6f), new(12.5f,43.8f), new(13.2f,43.3f),
            new(13.8f,42.7f), new(14.6f,42.2f), new(15.2f,41.5f), new(16.2f,41.1f),
            new(17.3f,40.6f), new(18.2f,40.0f), new(18.5f,39.6f), new(17.7f,39.5f),
            new(16.8f,39.9f), new(16.2f,39.3f), new(16.1f,38.7f), new(15.7f,38.1f),
            new(15.5f,37.9f), new(15.2f,38.4f), new(15.5f,39.0f), new(15.0f,39.5f),
            new(14.5f,40.0f), new(14.1f,40.4f), new(13.8f,41.0f), new(13.2f,41.4f),
            new(12.6f,42.0f), new(12.1f,42.5f), new(11.5f,43.1f), new(10.9f,43.6f),
            new(10.1f,44.0f), new(9.3f,44.1f), new(8.5f,44.0f), new(7.8f,43.8f),
            new(7.4f,44.2f), new(7.6f,44.8f), new(7.0f,45.3f), new(7.0f,45.9f)
        };
        static readonly Vector2[] Sicily =
        {
            new(12.4f,38.1f), new(13.3f,38.2f), new(14.2f,38.0f), new(15.1f,37.8f),
            new(15.6f,37.3f), new(15.1f,36.9f), new(14.0f,36.6f), new(12.9f,37.0f),
            new(12.4f,37.5f), new(12.4f,38.1f)
        };
        static readonly Vector2[] Sardinia =
        {
            new(8.2f,41.3f), new(9.0f,41.2f), new(9.5f,40.6f), new(9.6f,39.8f),
            new(9.2f,38.9f), new(8.6f,38.7f), new(8.2f,39.3f), new(8.0f,40.2f),
            new(8.2f,41.3f)
        };
        static readonly Vector2[][] Bathymetry =
        {
            new [] { new Vector2(6.5f,44.0f), new Vector2(8.8f,42.9f), new Vector2(10.9f,41.9f), new Vector2(12.0f,40.9f) },
            new [] { new Vector2(12.0f,44.8f), new Vector2(13.2f,43.4f), new Vector2(14.3f,42.2f), new Vector2(15.7f,41.5f), new Vector2(17.7f,40.6f) },
            new [] { new Vector2(10.8f,38.2f), new Vector2(12.2f,37.4f), new Vector2(14.2f,36.8f), new Vector2(16.0f,37.3f) },
            new [] { new Vector2(7.3f,41.8f), new Vector2(7.4f,40.4f), new Vector2(7.5f,39.1f), new Vector2(8.4f,38.2f) },
            new [] { new Vector2(9.5f,41.7f), new Vector2(10.9f,40.6f), new Vector2(12.3f,39.4f), new Vector2(14.5f,38.7f) }
        };

        protected override void OnPopulateMesh(VertexHelper vh)
        {
            vh.Clear();
            DrawRect(vh, rectTransform.rect, new Color(.02f, .18f, .30f, .62f));
            foreach (var line in Bathymetry)
                DrawPolyline(vh, line, new Color(.13f, .62f, .88f, .55f), 2.4f);
            DrawPolygon(vh, Mainland, new Color(.13f, .36f, .25f, .82f));
            DrawPolygon(vh, Sicily, new Color(.13f, .36f, .25f, .82f));
            DrawPolygon(vh, Sardinia, new Color(.13f, .36f, .25f, .82f));
            DrawPolyline(vh, Mainland, new Color(.72f, .98f, .78f, .95f), 5f);
            DrawPolyline(vh, Sicily, new Color(.72f, .98f, .78f, .95f), 4.5f);
            DrawPolyline(vh, Sardinia, new Color(.72f, .98f, .78f, .95f), 4.5f);
        }

        void DrawRect(VertexHelper vh, Rect rect, Color color)
        {
            int start = vh.currentVertCount;
            vh.AddVert(new Vector2(rect.xMin, rect.yMin), color, Vector2.zero);
            vh.AddVert(new Vector2(rect.xMin, rect.yMax), color, Vector2.zero);
            vh.AddVert(new Vector2(rect.xMax, rect.yMax), color, Vector2.zero);
            vh.AddVert(new Vector2(rect.xMax, rect.yMin), color, Vector2.zero);
            vh.AddTriangle(start, start + 1, start + 2);
            vh.AddTriangle(start + 2, start + 3, start);
        }

        void DrawPolygon(VertexHelper vh, Vector2[] points, Color color)
        {
            int centerIndex = vh.currentVertCount;
            Vector2 center = Vector2.zero;
            for (int i = 0; i < points.Length - 1; i++) center += ToRect(points[i]);
            center /= Mathf.Max(1, points.Length - 1);
            vh.AddVert(center, color, Vector2.zero);
            for (int i = 0; i < points.Length - 1; i++)
                vh.AddVert(ToRect(points[i]), color, Vector2.zero);
            for (int i = 0; i < points.Length - 1; i++)
            {
                int next = i == points.Length - 2 ? 1 : i + 2;
                vh.AddTriangle(centerIndex, centerIndex + i + 1, centerIndex + next);
            }
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
