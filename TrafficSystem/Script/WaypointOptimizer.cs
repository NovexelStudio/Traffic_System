using UnityEngine;
using System.Collections.Generic;

public class WaypointOptimizer : MonoBehaviour
{
    [SerializeField] float optimizeFrequency = 5f;

    private WaypointManager waypointManager;
    private float lastOptimizeTime;

    private void Awake()
    {
        waypointManager = FindAnyObjectByType<WaypointManager>();
    }

    private void Update()
    {
        if (Time.time - lastOptimizeTime > optimizeFrequency)
        {
            RemoveNullWaypoints();
            lastOptimizeTime = Time.time;
        }
    }

    private void RemoveNullWaypoints()
    {
        // Clean up null references in the waypoint manager
        waypointManager.waypoints.RemoveAll(wp => wp == null);

        // Clean up null references in waypoint connections
        foreach (var waypoint in waypointManager.waypoints)
        {
            waypoint.GetNextWaypoints().RemoveAll(wp => wp == null);
        }
    }
}