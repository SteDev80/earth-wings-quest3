using System.Collections.Generic;
using UnityEngine;

namespace EarthWings
{
    public sealed class TimeTrialGate : MonoBehaviour
    {
        public string gateName;
        Transform ring;
        bool passed;
        void Start() => Invoke(nameof(FindRing), .25f);
        void FindRing()
        {
            var billboard = GetComponent<MunicipalityBillboard>();
            ring = billboard ? billboard.GetComponentInChildren<GateBillboard>()?.transform : null;
            if (ring) TimeTrialRace.Register(this);
        }
        public Transform Ring => ring;
        public bool Passed => passed;
        public void Pass()
        {
            if (passed) return;
            passed = true;
            if (ring) ring.gameObject.SetActive(false);
        }
        public void ResetGate()
        {
            passed = false;
            if (ring) ring.gameObject.SetActive(true);
        }
    }

    public sealed class TimeTrialRace : MonoBehaviour
    {
        static readonly List<TimeTrialGate> gates = new();
        static TimeTrialRace instance;
        static float seconds;
        static bool active;
        AudioSource audioSource;
        WingsuitPilot pilot;
        public static string Status => active ? "  ·  TEMPO " + Mathf.CeilToInt(seconds) + " s" : "";
        public static void Register(TimeTrialGate gate)
        {
            if (!gates.Contains(gate)) gates.Add(gate);
            if (instance == null)
            {
                var go = new GameObject("Crazy Taxi time trial");
                instance = go.AddComponent<TimeTrialRace>();
            }
        }
        public static void ResetRace()
        {
            active = false; seconds = 0;
            foreach (var gate in gates) if (gate) gate.ResetGate();
        }
        void Awake()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 0; audioSource.playOnAwake = false;
        }
        void Update()
        {
            if (pilot == null) pilot = FindFirstObjectByType<WingsuitPilot>();
            if (pilot == null || !pilot.IsFlying) return;
            if (active)
            {
                seconds -= Time.deltaTime;
                if (seconds <= 0) { seconds = 0; active = false; return; }
            }
            TimeTrialGate closest = null; float closestDistance = float.MaxValue;
            foreach (var gate in gates)
            {
                if (!gate || gate.Passed || !gate.Ring) continue;
                float distance = Vector3.Distance(pilot.transform.position, gate.Ring.position);
                if (distance < closestDistance) { closestDistance = distance; closest = gate; }
            }
            if (closest != null && closestDistance < 85f)
            {
                if (!active) { active = true; seconds = 18; }
                float neighbour = NearestRemainingDistance(closest);
                float bonus = Mathf.Clamp(neighbour / 85f, 4f, 22f);
                closest.Pass(); seconds += bonus;
                audioSource.PlayOneShot(CreateCoinClip());
            }
        }
        static float NearestRemainingDistance(TimeTrialGate from)
        {
            float result = 340f;
            foreach (var gate in gates)
                if (gate && gate != from && !gate.Passed && gate.Ring)
                    result = Mathf.Min(result, Vector3.Distance(from.Ring.position, gate.Ring.position));
            return result;
        }
        static AudioClip CreateCoinClip()
        {
            const int rate = 22050, count = 6615;
            var clip = AudioClip.Create("Coin gate", count, 1, rate, false); var samples = new float[count];
            for (int i = 0; i < count; i++)
            {
                float t = i / (float)rate, envelope = Mathf.Exp(-t * 12);
                samples[i] = (Mathf.Sin(t * Mathf.PI * 2 * 1240) + .55f * Mathf.Sin(t * Mathf.PI * 2 * 1860)) * .22f * envelope;
            }
            clip.SetData(samples, 0); return clip;
        }
    }
}
