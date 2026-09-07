using System;
using CesiumForUnity;
using EarthWings;
using Unity.Mathematics;
using UnityEngine;

public static class FlightChecks
{
    public static void Run()
    {
        var world = new GameObject("Offline georeference checks");
        try
        {
            var geo = world.AddComponent<CesiumGeoreference>();
            geo.SetOriginLongitudeLatitudeHeight(6.9731, 45.7874, 0);
            var rig = new GameObject("Test rig"); rig.transform.SetParent(world.transform, false);
            rig.transform.position = Vector3.up * 3200;
            var anchor = rig.AddComponent<CesiumGlobeAnchor>();
            var shift = rig.AddComponent<CesiumOriginShift>(); shift.distance = 1000;
            var eye = new GameObject("Test eye"); eye.transform.SetParent(rig.transform, false);
            eye.transform.localPosition = new Vector3(.03f,1.65f,0);
            for (int i = 0; i < 250; i++)
            {
                rig.transform.position += rig.transform.forward * 500;
                anchor.Sync();
                double3 before = anchor.positionGlobeFixed;
                shift.SendMessage("LateUpdate");
                if (math.distance(before, anchor.positionGlobeFixed) > .02)
                    throw new Exception("Origin shift changed geographic position");
                if (rig.transform.position.magnitude > 1001)
                    throw new Exception("Origin did not follow the rig");
                if (Vector3.Distance(eye.transform.localPosition, new Vector3(.03f,1.65f,0)) > .0001f)
                    throw new Exception("Origin shift changed eye pose");
            }
            var pilot = rig.AddComponent<WingsuitPilot>(); pilot.globeAnchor = anchor;
            foreach (var destination in new[] {new double3(6.9731,45.7874,3200), new double3(12.4922,41.8902,1000)})
            {
                pilot.returnToLaunch = () =>
                {
                    geo.SetOriginLongitudeLatitudeHeight(destination.x, destination.y, 0);
                    rig.transform.position = Vector3.up * (float)destination.z;
                };
                pilot.Restart();
                if (math.distance(anchor.longitudeLatitudeHeight, destination) > .1)
                    throw new Exception("Return to launch failed after origin shifts");
            }
            Debug.Log("EARTH_WINGS_CHECKS_PASSED: 125 km travel, origin shifts, eye pose, Courmayeur/Rome reset");
        }
        finally { UnityEngine.Object.DestroyImmediate(world); }
    }
}
