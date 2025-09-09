using UnityEngine;
using System.Collections.Generic;

public class Waypoint : MonoBehaviour
{
    [SerializeField] private List<Waypoint> nextWaypoints = new List<Waypoint>();
    private Dictionary<Waypoint, float> distanceCache = new Dictionary<Waypoint, float>();
    private WaypointManager waypointManager;

    private void Awake()
    {
        waypointManager = FindObjectOfType<WaypointManager>();
        PrecomputeDistances();
    }

    private void PrecomputeDistances()
    {
        foreach (var wp in nextWaypoints)
        {
            if (wp != null)
            {
                distanceCache[wp] = Vector3.Distance(transform.position, wp.transform.position);
            }
        }
    }

    public Waypoint GetNextWaypoint()
    {
        if (nextWaypoints.Count == 0) return null;
        return nextWaypoints[Random.Range(0, nextWaypoints.Count)];
    }

    public Waypoint GetNextWaypointOptimized(Vector3 currentPosition)
    {
        if (nextWaypoints.Count == 0) return null;

        // Get the closest next waypoint to current position
        Waypoint closest = null;
        float minDistance = float.MaxValue;

        foreach (var wp in nextWaypoints)
        {
            if (wp == null) continue;

            float distance = Vector3.SqrMagnitude(currentPosition - wp.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = wp;
            }
        }

        return closest;
    }

    public float GetCachedDistance(Waypoint target)
    {
        if (distanceCache.TryGetValue(target, out float distance))
        {
            return distance;
        }
        return Vector3.Distance(transform.position, target.transform.position);
    }

    public void AddNextWaypoint(Waypoint newNextWaypoint)
    {
        if (!nextWaypoints.Contains(newNextWaypoint))
        {
            nextWaypoints.Add(newNextWaypoint);
            distanceCache[newNextWaypoint] = Vector3.Distance(transform.position, newNextWaypoint.transform.position);
        }
    }

    public void RemoveNextWaypoint(Waypoint waypointToRemove)
    {
        nextWaypoints.Remove(waypointToRemove);
        distanceCache.Remove(waypointToRemove);
    }

    public void ClearAllNextWaypoints()
    {
        nextWaypoints.Clear();
        distanceCache.Clear();
    }

    public List<Waypoint> GetNextWaypoints() => nextWaypoints;

    private void OnDrawGizmos()
    {
        foreach (var waypoint in nextWaypoints)
        {
            if (waypoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, waypoint.transform.position);
                Gizmos.DrawWireSphere(waypoint.transform.position, 0.5f);
            }
        }

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.3f);
    }
}