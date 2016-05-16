using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class AgentController : MonoBehaviour 
{
    [Header("Movement")]
    public float maxSpeed = 3f;
    public float maxSteer = 0.1f;

    [Header("Perception")]
    public float sightRadius = 5f;

    public Transform target;

    [Header("Path Finding")]
    [HideInInspector]
    public Vector3[] path;
    public float targetMaxDistance = 1;

    [Header("Editor Visuals")]
    public Color gizmoRadiusColor = Color.red;
    public Color gizmoPathColor = Color.green;

    Quaternion turnRotation;
    Vector3 lastPos;
    Vector3 currentVelocity;

    int currentWaypoint;
    bool onPath = false;
    bool requestedPath = false;

    Rigidbody rb;

    void Awake() 
    {
        // Get components
        rb = GetComponent<Rigidbody>();

        // initializations
        lastPos = transform.position;
        currentVelocity = Vector3.zero;
	}
	
	void FixedUpdate () 
    {
        // If the agent does not have a path yet, request one
        if (!onPath && !requestedPath)
        {
            // Make a request for a new path
            NavMeshPathManager.requestPath(transform.position, target.position, onPathRequestProcessed);
            requestedPath = true;
        }

        // If current target/waypoint has been reached, go to the next one
        if (currentWaypoint < path.Length)
        {
            moveToTarget();
        }

        // Calculate current velocity and set last position of agent
        currentVelocity = (transform.position - lastPos) / Time.fixedDeltaTime;
        lastPos = transform.position;
	}

    void moveToTarget()
    {
        // get position of current waypoint
        Vector3 targetPos = new Vector3(path[currentWaypoint].x, transform.position.y, path[currentWaypoint].z);

        // Get required steerforce
        Vector3 steerForce = seek(targetPos);

        // Add steerforce to the velocity vector
        rb.velocity += steerForce;

        // Rotate to look towards new current velocity
        Vector3 lookVector = removeVectorY(rb.velocity);
        transform.rotation = Quaternion.LookRotation(lookVector);

        // If the unit is close enough to the currentWaypoint, register it as reached
        if (Vector3.Distance(transform.position, targetPos) < targetMaxDistance)
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

    Vector3 seek(Vector3 targetPos)
    {
        // velocity vector towards target
        Vector3 desiredVelocity = (targetPos - transform.position).normalized * maxSpeed;

        // calculate the steerforce required for the desired velocity based on current velocity
        Vector3 steerForce = desiredVelocity - rb.velocity;
        steerForce = removeVectorY(steerForce);

        // clamp it to the maximum steer
        //steerForce = Vector3.ClampMagnitude(steerForce, maxSteer);

        return steerForce;
    }

    Vector3 removeVectorY(Vector3 vector)
    {
        return new Vector3(vector.x, 0, vector.z);
    }

    void moveToTarget2()
    {
        //Look at and dampen the rotation
        Vector3 LookPos = new Vector3(path[currentWaypoint].x, transform.position.y, path[currentWaypoint].z);
        turnRotation = Quaternion.LookRotation(LookPos - transform.position);
        //transform.rotation = Quaternion.Slerp(transform.rotation, turnRotation, Time.deltaTime * turnSpeed);

        // Move unit towards the next waypoint
        transform.position += transform.forward * Time.fixedDeltaTime * maxSpeed;

        // If the unit is close enough to the currentWaypoint, register it as reached
        if (Vector3.Distance(transform.position, path[currentWaypoint]) < targetMaxDistance)
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

    public void onPathRequestProcessed(Vector3[] newPath, bool foundPath)
    {
        // When a path was found, set the recieved path as the new path
        if (foundPath)
        {
            path = newPath;
            currentWaypoint = 0;
            onPath = true;
            requestedPath = false;
        }
    }

    void OnDrawGizmosSelected() 
    {
        // Draw sightRadius Gizmos
        Gizmos.color = gizmoRadiusColor;
        Gizmos.DrawWireSphere (transform.position, sightRadius);
    }

    void OnDrawGizmos()
    {
        // Draw path lines
        Gizmos.color = gizmoPathColor;
        if (onPath && path.Length > 0)
        {
            // Draw line from it self node to next node
            Vector3 startPos = new Vector3(path[currentWaypoint].x, transform.position.y, path[currentWaypoint].z);
            //Gizmos.DrawLine(transform.position, startPos);

            for (int i = currentWaypoint; i < path.Length-1; i++)
            {
                Vector3 pos1 = new Vector3(path[i].x, transform.position.y, path[i].z);
                Vector3 pos2 = new Vector3(path[i+1].x, transform.position.y, path[i+1].z);

                // Draw a sphere on the node
                Gizmos.DrawSphere(pos1, 0.2f);

                // Draw line from start node to end node
                Gizmos.DrawLine(pos1, pos2);
            }

            // Draw a sphere on the last node of path
            Vector3 nodePos = new Vector3(path[path.Length-1].x, path[path.Length-1].y, path[path.Length-1].z);
            Gizmos.DrawSphere(nodePos, 0);
        }
    }
}
