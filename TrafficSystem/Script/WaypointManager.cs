using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class WaypointManager : MonoBehaviour
{
    public List<Waypoint> waypoints = new List<Waypoint>();
    private Dictionary<Vector3Int, List<Waypoint>> spatialGrid = new Dictionary<Vector3Int, List<Waypoint>>();
    private float cellSize = 10f; // Adjust based on your scene size

    private void Awake()
    {
        BuildSpatialGrid();
    }

    private void BuildSpatialGrid()
    {
        spatialGrid.Clear();
        foreach (var waypoint in waypoints)
        {
            if (waypoint == null) continue;

            Vector3Int gridKey = GetGridKey(waypoint.transform.position);
            if (!spatialGrid.ContainsKey(gridKey))
            {
                spatialGrid[gridKey] = new List<Waypoint>();
            }
            spatialGrid[gridKey].Add(waypoint);
        }
    }

    private Vector3Int GetGridKey(Vector3 position)
    {
        return new Vector3Int(
            Mathf.FloorToInt(position.x / cellSize),
            Mathf.FloorToInt(position.y / cellSize),
            Mathf.FloorToInt(position.z / cellSize)
        );
    }

    public Waypoint GetClosestWaypoint(Transform target)
    {
        if (waypoints.Count == 0) return null;

        Vector3Int targetGridKey = GetGridKey(target.position);
        Waypoint closest = null;
        float minDistance = float.MaxValue;

        // Check current cell and neighboring cells
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                for (int z = -1; z <= 1; z++)
                {
                    Vector3Int checkKey = targetGridKey + new Vector3Int(x, y, z);
                    if (spatialGrid.TryGetValue(checkKey, out List<Waypoint> cellWaypoints))
                    {
                        foreach (var waypoint in cellWaypoints)
                        {
                            if (waypoint == null) continue;

                            float distance = Vector3.SqrMagnitude(target.position - waypoint.transform.position);
                            if (distance < minDistance)
                            {
                                minDistance = distance;
                                closest = waypoint;
                            }
                        }
                    }
                }
            }
        }

        return closest;
    }

    // Add waypoint at runtime
    public void RegisterWaypoint(Waypoint waypoint)
    {
        if (!waypoints.Contains(waypoint))
        {
            waypoints.Add(waypoint);
            Vector3Int gridKey = GetGridKey(waypoint.transform.position);
            if (!spatialGrid.ContainsKey(gridKey))
            {
                spatialGrid[gridKey] = new List<Waypoint>();
            }
            spatialGrid[gridKey].Add(waypoint);
        }
    }

    // Remove waypoint at runtime
    public void UnregisterWaypoint(Waypoint waypoint)
    {
        waypoints.Remove(waypoint);
        Vector3Int gridKey = GetGridKey(waypoint.transform.position);
        if (spatialGrid.ContainsKey(gridKey))
        {
            spatialGrid[gridKey].Remove(waypoint);
        }
    }
}