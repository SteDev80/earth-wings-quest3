using CesiumForUnity;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

namespace EarthWings
{
    public static class MunicipalityLabels
    {
        // A compact local gazetteer, intentionally limited to the two starting regions.
        static readonly (string name, double lon, double lat, double height)[] Places =
        {
            ("COURMAYEUR", 6.9731, 45.7874, 2100), ("PRE-SAINT-DIDIER", 6.9850, 45.7640, 1800),
            ("LA SALLE", 7.0740, 45.7440, 1900), ("MORGEX", 7.0390, 45.7560, 1700),
            ("LA THUILE", 6.9500, 45.7160, 2200), ("AOSTA", 7.3170, 45.7370, 1200),
            ("ROMA", 12.4964, 41.9028, 650), ("FIUMICINO", 12.2300, 41.7700, 500),
            ("TIVOLI", 12.7980, 41.9600, 700), ("FRASCATI", 12.6800, 41.8090, 700)
        };

        public static void Add(Transform parent, Camera camera)
        {
            foreach (var place in Places)
            {
                var marker = new GameObject("Comune " + place.name, typeof(CesiumGlobeAnchor), typeof(MunicipalityBillboard), typeof(TimeTrialGate));
                marker.transform.SetParent(parent, false);
                marker.GetComponent<CesiumGlobeAnchor>().longitudeLatitudeHeight = new double3(place.lon, place.lat, place.height);
                var billboard = marker.GetComponent<MunicipalityBillboard>();
                billboard.label = place.name; billboard.cameraToFace = camera;
                marker.GetComponent<TimeTrialGate>().gateName = place.name;
            }
        }
    }

    public sealed class MunicipalityBillboard : MonoBehaviour
    {
        public string label;
        public Camera cameraToFace;
        Transform visual;
        void Start()
        {
            var canvasObject = new GameObject("Label", typeof(Canvas));
            canvasObject.transform.SetParent(transform, false);
            canvasObject.transform.localScale = Vector3.one * .10f;
            visual = canvasObject.transform;
            var canvas = canvasObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.WorldSpace; canvas.worldCamera = cameraToFace;
            canvasObject.GetComponent<RectTransform>().sizeDelta = new Vector2(900,130);
            var textObject = new GameObject("Text", typeof(RectTransform), typeof(Text), typeof(Shadow));
            textObject.transform.SetParent(canvasObject.transform, false);
            textObject.GetComponent<RectTransform>().sizeDelta = new Vector2(900,130);
            var text = textObject.GetComponent<Text>(); text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.text = label; text.fontSize = 54; text.fontStyle = FontStyle.Bold; text.alignment = TextAnchor.MiddleCenter;
            text.color = new Color(.95f,.98f,1f,.95f); textObject.GetComponent<Shadow>().effectColor = new Color(0,0,0,.9f);
            CreateGateRing();
        }
        void CreateGateRing()
        {
            var ring = new GameObject("Time trial gate", typeof(MeshFilter), typeof(MeshRenderer));
            ring.transform.SetParent(transform, false);
            ring.transform.localPosition = Vector3.up * 95f;
            ring.AddComponent<GateBillboard>().cameraToFace = cameraToFace;
            const int segments = 48;
            const float radius = 72f, thickness = 5f;
            var vertices = new Vector3[segments * 2]; var triangles = new int[segments * 6];
            for (int i = 0; i < segments; i++)
            {
                float angle = i * Mathf.PI * 2 / segments;
                Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
                vertices[i * 2] = direction * (radius - thickness);
                vertices[i * 2 + 1] = direction * (radius + thickness);
                int next = (i + 1) % segments, t = i * 6;
                triangles[t] = i * 2; triangles[t + 1] = next * 2; triangles[t + 2] = i * 2 + 1;
                triangles[t + 3] = i * 2 + 1; triangles[t + 4] = next * 2; triangles[t + 5] = next * 2 + 1;
            }
            var mesh = new Mesh { vertices = vertices, triangles = triangles };
            mesh.RecalculateBounds(); ring.GetComponent<MeshFilter>().sharedMesh = mesh;
            var material = new Material(Shader.Find("Sprites/Default")); material.color = new Color(.05f, .95f, 1f, .95f);
            ring.GetComponent<MeshRenderer>().sharedMaterial = material;
        }
        void LateUpdate()
        {
            if (cameraToFace && visual) visual.rotation = Quaternion.LookRotation(visual.position - cameraToFace.transform.position, cameraToFace.transform.up);
        }
    }

    public sealed class GateBillboard : MonoBehaviour
    {
        public Camera cameraToFace;
        void LateUpdate()
        {
            if (cameraToFace) transform.rotation = Quaternion.LookRotation(cameraToFace.transform.position - transform.position, cameraToFace.transform.up);
        }
    }
}
