using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class EnemyMovementComponent : MonoBehaviour
{
    public NavMeshAgent Agent;

    public void Awake()
    {
        Agent = GetComponent<NavMeshAgent>();
    }

    public void Start()
    {
        Agent.updateRotation = true;
        Agent.stoppingDistance = 2;
    }

    public bool InCover { get; set; }

    public float LastStopped = 0;

    public bool ApplyDefensiveMeasures;

    public GameObject NextWaypoint;

    public GameObject[] Waypoints;

    public LayerMask EnemyWaypointLayer;

    public bool IsMoving;

    public Vector3 LastPosition;

    public Vector3 CurrentDestination;

    [HideInInspector]
    public Vector3 CurrentMovementVector;
}
