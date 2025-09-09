using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class CarAi : MonoBehaviour
{
    [Header("Navigation")]
    [SerializeField] Waypoint currentWaypoint;
    [SerializeField] float waypointCheckDistance = 3f;

    [Header("Performance")]
    [SerializeField] float waypointUpdateFrequency = 0.3f;
    [SerializeField] int waypointLookahead = 2;

    private NavMeshAgent agent;
    private WaypointManager waypointManager;
    private float lastWaypointUpdateTime;
    private Queue<Waypoint> waypointQueue = new Queue<Waypoint>();
    private Vector3 lastWaypointPosition;


    [Header("Wheel Settings")]
    [SerializeField] List<Transform> wheelMesh;
    [SerializeField] float wheelRotationMultiplier = 50f;
    [SerializeField] float maxWheelRotationSpeed = 200f;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        waypointManager = FindAnyObjectByType<WaypointManager>();
    }

    private void Start()
    {
        InitializeWaypoint();
    }

    private void Update()
    {
        if (Time.time - lastWaypointUpdateTime > waypointUpdateFrequency)
        {
            UpdateNavigation();
            lastWaypointUpdateTime = Time.time;
        }
        RotateWheels();
    }

    private void InitializeWaypoint()
    {
        if (currentWaypoint == null)
        {
            currentWaypoint = waypointManager.GetClosestWaypoint(transform);
        }

        if (currentWaypoint != null)
        {
            PrecacheWaypoints();
            agent.SetDestination(currentWaypoint.transform.position);
            lastWaypointPosition = currentWaypoint.transform.position;
        }
    }

    private void PrecacheWaypoints()
    {
        waypointQueue.Clear();
        Waypoint current = currentWaypoint;

        for (int i = 0; i < waypointLookahead && current != null; i++)
        {
            Waypoint next = current.GetNextWaypoint();
            if (next != null)
            {
                waypointQueue.Enqueue(next);
                current = next;
            }
        }
    }

    private void UpdateNavigation()
    {
        if (currentWaypoint == null) return;

        float distanceToWaypoint = Vector3.Distance(transform.position, currentWaypoint.transform.position);

        if (distanceToWaypoint < waypointCheckDistance)
        {
            AdvanceToNextWaypoint();
        }

        // Check if we're stuck or off path
        if (waypointQueue.Count == 0 || IsOffPath())
        {
            FindNewPath();
        }
    }

    private void AdvanceToNextWaypoint()
    {
        if (waypointQueue.Count > 0)
        {
            currentWaypoint = waypointQueue.Dequeue();
            agent.SetDestination(currentWaypoint.transform.position);
            lastWaypointPosition = currentWaypoint.transform.position;

            // Precache next waypoint
            Waypoint next = currentWaypoint.GetNextWaypoint();
            if (next != null)
            {
                waypointQueue.Enqueue(next);
            }
        }
        else
        {
            FindNewPath();
        }
    }

    private bool IsOffPath()
    {
        if (!agent.hasPath || agent.pathPending) return false;

        // Check if we're moving toward the waypoint
        float distanceToWaypoint = Vector3.Distance(transform.position, currentWaypoint.transform.position);
        float distanceMoved = Vector3.Distance(transform.position, lastWaypointPosition);

        return distanceToWaypoint > distanceMoved * 2f; // If we're not getting closer
    }

    private void FindNewPath()
    {
        currentWaypoint = waypointManager.GetClosestWaypoint(transform);
        if (currentWaypoint != null)
        {
            PrecacheWaypoints();
            agent.SetDestination(currentWaypoint.transform.position);
            lastWaypointPosition = currentWaypoint.transform.position;
        }
    }

    public void SetCurrentWaypoint(Waypoint waypoint)
    {
        currentWaypoint = waypoint;
        if (currentWaypoint != null)
        {
            PrecacheWaypoints();
            agent.SetDestination(currentWaypoint.transform.position);
            lastWaypointPosition = currentWaypoint.transform.position;
        }
    }
    void RotateWheels()
    {
        if (wheelMesh.Count == 0 || !agent.hasPath || agent.velocity.magnitude < 0.1f)
            return;

        // Calculate rotation speed (frame-rate independent)
        float speed = agent.velocity.magnitude * wheelRotationMultiplier * Time.deltaTime;

        // Clamp maximum rotation speed
        speed = Mathf.Clamp(speed, -maxWheelRotationSpeed, maxWheelRotationSpeed);

        // Determine rotation direction (forward/backward)
        float moveDirection = Vector3.Dot(transform.forward, agent.velocity.normalized);
        speed *= Mathf.Sign(moveDirection);

        foreach (var wheel in wheelMesh)
        {
            if (wheel != null)
            {
                // Rotate around the wheel's local right axis
                wheel.Rotate(Vector3.right * speed, Space.Self);
            }
        }
    }
    private void OnDrawGizmos()
    {
        if (currentWaypoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, currentWaypoint.transform.position);
            Gizmos.DrawWireSphere(currentWaypoint.transform.position, 1f);
        }
    }
}