using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SteeringManager))]
public class AgentController : MonoBehaviour 
{
    [Header("Path Finding")]
    [HideInInspector]
    public Vector3[] path;
    public float reachedTargetRadius = 1.5f;

    [Header("Perception")]
    public float perceptionSightDistance;
    [Range(0,90)]
    public float perceptionSightAngle = 40;

    [Header("Editor Visuals")]
    public bool showPathGizmo = true;
    public Color gizmoPathColor = Color.green;

    Vector3 lastVelocity;

    [HideInInspector]
    public int currentWaypoint;
    [HideInInspector]
    public bool onPath = false;
    [HideInInspector]
    public bool requestedPath = false;
    [HideInInspector]
    public bool isBusy = false;

    [HideInInspector]
    public Rigidbody rb;
    protected SteeringManager steering;
    protected List<SteeringBehaviours.Behaviour> steeringBehaviours;
    protected Stack<Transform> stackedTargets;

    //[HideInInspector]
    public Transform finalTarget = null;

    protected void Awake() 
    {
        // Get components
        rb = GetComponent<Rigidbody>();
        steering = GetComponent<SteeringManager>();

        // initializations
        steeringBehaviours = new List<SteeringBehaviours.Behaviour>();
        lastVelocity = Vector3.zero;
	}
	
	protected void FixedUpdate () 
    {
        // If the agent does not have a target, find one
        if (!isBusy && finalTarget == null)
        {
            getNewTarget();
        }

        /*
        // If the agent does not have a path yet, request one
        if (finalTarget != null && !onPath && !requestedPath)
        {
            // Make a request for a new path. Get the top of the stack as new target
            NavMeshPathManager.requestPath(transform.position, finalTarget.position, onPathRequestProcessed);
            requestedPath = true;
        }
        */

        // If current target/waypoint has been reached, go to the next one
        if (onPath && currentWaypoint < path.Length)
        {
            move();
        }
	}

    public virtual void move(){}

    protected virtual void onTarget()
    {
        onPath = false;
        finalTarget = null;
    }

    /*
    void moveToTarget2()
    {
        //Look at and dampen the rotation
        Vector3 LookPos = new Vector3(path[currentWaypoint].x, transform.position.y, path[currentWaypoint].z);
        //turnRotation = Quaternion.LookRotation(LookPos - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, turnRotation, Time.deltaTime * turnSpeed);

        // Move unit towards the next waypoint
        transform.position += transform.forward * Time.fixedDeltaTime * maxSpeed;

        // If the unit is close enough to the currentWaypoint, register it as reached
        if (Vector3.Distance(transform.position, path[currentWaypoint]) < reachedTargetRadius)
        {
            // node is already reached, so remove penalty for using this node
            NavMeshPathManager.removeUsedNodePenalty(path[currentWaypoint]);

            // Check if agent reached its final target
            if (currentWaypoint == path.Length-1)
            {
                onPath = false;
                return;
            }

            currentWaypoint++;
        }
    }
    */

    public void onPathRequestProcessed(Vector3[] newPath, bool foundPath)
    {
        // When a path was found, set the recieved path as the new path
        if (foundPath)
        {
            path = newPath;
            currentWaypoint = 0;
            onPath = true;
            //requestedPath = false;
        }
    }

    public virtual void getNewTarget()
    {}

    void OnDrawGizmos()
    {
        if (showPathGizmo)
        {
            // Draw path lines
            Gizmos.color = gizmoPathColor;
            if (onPath && path.Length > 0)
            {
                // Draw line from it self node to next node
                Vector3 startPos = new Vector3(path[currentWaypoint].x, transform.position.y, path[currentWaypoint].z);
                Gizmos.DrawLine(transform.position, startPos);

                for (int i = currentWaypoint; i < path.Length - 1; i++)
                {
                    Vector3 pos1 = new Vector3(path[i].x, transform.position.y, path[i].z);
                    Vector3 pos2 = new Vector3(path[i + 1].x, transform.position.y, path[i + 1].z);

                    // Draw a sphere on the node
                    Gizmos.DrawSphere(pos1, 0.2f);

                    // Draw line from start node to end node
                    Gizmos.DrawLine(pos1, pos2);
                }

                // Draw a sphere on the last node of path
                Vector3 nodePos = new Vector3(path[path.Length - 1].x, path[path.Length - 1].y, path[path.Length - 1].z);
                Gizmos.DrawSphere(nodePos, 0);
            }
        }
    }
}
