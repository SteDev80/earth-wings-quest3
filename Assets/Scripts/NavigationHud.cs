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
        Text locationText;

        public static void Add(Transform parent, WingsuitPilot pilot, Camera camera)
        {
            var root = new GameObject("Navigation HUD", typeof(RectTransform), typeof(Image), typeof(NavigationHud));
            root.transform.SetParent(parent, false);
            var rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(300, 300);
            rect.anchoredPosition = new Vector2(-720, -235);
            var image = root.GetComponent<Image>();
            image.color = new Color(.01f, .035f, .07f, .55f);

            var map = new GameObject("Italy map", typeof(RectTransform), typeof(RawImage));
            map.transform.SetParent(root.transform, false);
            map.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 230);
            map.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 12);
            var rawMap = map.GetComponent<RawImage>();
            rawMap.texture = ItalyMapTexture.Create();
            rawMap.color = Color.white;

            AddLabel(root.transform, "N", new Vector2(0, 126), new Vector2(34, 24), 22, TextAnchor.MiddleCenter);
            AddLabel(root.transform, "S", new Vector2(0, -104), new Vector2(34, 24), 18, TextAnchor.MiddleCenter);
            AddLabel(root.transform, "O", new Vector2(-126, 6), new Vector2(34, 24), 18, TextAnchor.MiddleCenter);
            AddLabel(root.transform, "E", new Vector2(126, 6), new Vector2(34, 24), 18, TextAnchor.MiddleCenter);

            var pin = new GameObject("Current position", typeof(RectTransform), typeof(MapHeadingTriangle));
            pin.transform.SetParent(root.transform, false);
            var pinRect = pin.GetComponent<RectTransform>();
            pinRect.sizeDelta = new Vector2(26, 34);
            pin.GetComponent<MapHeadingTriangle>().color = new Color(1f, .28f, .04f, .95f);

            var location = AddLabel(root.transform, "ITALIA", new Vector2(0, -138), new Vector2(280, 48), 17, TextAnchor.MiddleCenter);

            var hud = root.GetComponent<NavigationHud>();
            hud.pilot = pilot;
            hud.marker = pinRect;
            hud.locationText = location;
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
                locationText.text = pilot.mapMode && pilot.globeAnchor ? PlaceLabel(pilot.globeAnchor.longitudeLatitudeHeight.x, pilot.globeAnchor.longitudeLatitudeHeight.y) : "AREA DI ADDESTRAMENTO";
        }

        static string PlaceLabel(double lon, double lat)
        {
            return RegionFor(lon, lat) + "\n" + ComuneFor(lon, lat);
        }

        static string ComuneFor(double lon, double lat)
        {
            (string name, double lon, double lat)[] places =
            {
                ("COURMAYEUR", 6.9731, 45.7874), ("PRE-SAINT-DIDIER", 6.9850, 45.7640),
                ("LA SALLE", 7.0740, 45.7440), ("MORGEX", 7.0390, 45.7560),
                ("LA THUILE", 6.9500, 45.7160), ("AOSTA", 7.3170, 45.7370),
                ("ROMA", 12.4964, 41.9028), ("FIUMICINO", 12.2300, 41.7700),
                ("TIVOLI", 12.7980, 41.9600), ("FRASCATI", 12.6800, 41.8090)
            };
            string nearest = "COMUNE";
            double best = double.MaxValue;
            foreach (var place in places)
            {
                double x = (lon - place.lon) * System.Math.Cos(lat * System.Math.PI / 180.0);
                double y = lat - place.lat;
                double distance = x * x + y * y;
                if (distance < best)
                {
                    best = distance;
                    nearest = place.name;
                }
            }
            return nearest;
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

    static class ItalyMapTexture
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
        static Texture2D cached;

        public static Texture2D Create()
        {
            if (cached) return cached;
            const int width = 512, height = 512;
            cached = new Texture2D(width, height, TextureFormat.RGBA32, false);
            cached.wrapMode = TextureWrapMode.Clamp;
            cached.filterMode = FilterMode.Trilinear;
            var pixels = new Color32[width * height];
            var seaTop = new Color32(9, 63, 104, 210);
            var seaBottom = new Color32(2, 26, 52, 225);
            for (int y = 0; y < height; y++)
            {
                float t = y / (float)(height - 1);
                var sea = Color32.Lerp(seaBottom, seaTop, t);
                for (int x = 0; x < width; x++) pixels[y * width + x] = sea;
            }
            FillPolygon(pixels, width, height, Mainland, new Color32(34, 89, 60, 235));
            FillPolygon(pixels, width, height, Sicily, new Color32(34, 89, 60, 235));
            FillPolygon(pixels, width, height, Sardinia, new Color32(34, 89, 60, 235));
            foreach (var line in Bathymetry)
                DrawPolyline(pixels, width, height, line, new Color32(65, 196, 239, 140), 3);
            DrawPolyline(pixels, width, height, Mainland, new Color32(185, 255, 198, 250), 5);
            DrawPolyline(pixels, width, height, Sicily, new Color32(185, 255, 198, 250), 5);
            DrawPolyline(pixels, width, height, Sardinia, new Color32(185, 255, 198, 250), 5);
            DrawMountainStroke(pixels, width, height);
            cached.SetPixels32(pixels);
            cached.Apply(false, true);
            return cached;
        }

        static void FillPolygon(Color32[] pixels, int width, int height, Vector2[] points, Color32 color)
        {
            var polygon = new Vector2[points.Length];
            for (int i = 0; i < points.Length; i++) polygon[i] = ToPixel(points[i], width, height);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    if (Contains(polygon, x + .5f, y + .5f)) Blend(pixels, y * width + x, color);
        }

        static bool Contains(Vector2[] polygon, float x, float y)
        {
            bool inside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                var a = polygon[i]; var b = polygon[j];
                if (((a.y > y) != (b.y > y)) && x < (b.x - a.x) * (y - a.y) / (b.y - a.y + .0001f) + a.x)
                    inside = !inside;
            }
            return inside;
        }

        static void DrawPolyline(Color32[] pixels, int textureWidth, int textureHeight, Vector2[] points, Color32 color, int width)
        {
            for (int i = 0; i < points.Length - 1; i++)
                DrawLine(pixels, textureWidth, textureHeight, ToPixel(points[i], textureWidth, textureHeight), ToPixel(points[i + 1], textureWidth, textureHeight), color, width);
        }

        static void DrawMountainStroke(Color32[] pixels, int width, int height)
        {
            var alps = new[] { new Vector2(6.9f,45.6f), new Vector2(8.6f,45.3f), new Vector2(10.4f,45.4f), new Vector2(12.2f,45.7f), new Vector2(13.4f,45.8f) };
            var apennines = new[] { new Vector2(10.0f,44.0f), new Vector2(11.3f,43.0f), new Vector2(12.7f,41.9f), new Vector2(14.2f,40.6f), new Vector2(15.6f,39.1f) };
            DrawPolyline(pixels, width, height, alps, new Color32(126, 171, 126, 150), 2);
            DrawPolyline(pixels, width, height, apennines, new Color32(126, 171, 126, 135), 2);
        }

        static void DrawLine(Color32[] pixels, int textureWidth, int textureHeight, Vector2 a, Vector2 b, Color32 color, int width)
        {
            int steps = Mathf.CeilToInt(Vector2.Distance(a, b));
            for (int i = 0; i <= steps; i++)
            {
                var p = Vector2.Lerp(a, b, i / (float)Mathf.Max(1, steps));
                int radius = Mathf.Max(1, width);
                for (int oy = -radius; oy <= radius; oy++)
                    for (int ox = -radius; ox <= radius; ox++)
                    {
                        if (ox * ox + oy * oy > radius * radius) continue;
                        int x = Mathf.RoundToInt(p.x) + ox;
                        int y = Mathf.RoundToInt(p.y) + oy;
                        if (x < 0 || x >= textureWidth || y < 0 || y >= textureHeight) continue;
                        Blend(pixels, y * textureWidth + x, color);
                    }
            }
        }

        static Vector2 ToPixel(Vector2 lonLat, int width, int height)
        {
            float x = Mathf.InverseLerp(6f, 18.8f, lonLat.x) * (width - 1);
            float y = Mathf.InverseLerp(36f, 47.8f, lonLat.y) * (height - 1);
            return new Vector2(x, y);
        }

        static void Blend(Color32[] pixels, int index, Color32 source)
        {
            float alpha = source.a / 255f;
            var dest = pixels[index];
            pixels[index] = new Color32(
                (byte)Mathf.RoundToInt(source.r * alpha + dest.r * (1f - alpha)),
                (byte)Mathf.RoundToInt(source.g * alpha + dest.g * (1f - alpha)),
                (byte)Mathf.RoundToInt(source.b * alpha + dest.b * (1f - alpha)),
                255);
        }
    }
}

