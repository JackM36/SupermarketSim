using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;

public class AgentController : MonoBehaviour 
{
    public float moveSpeed = 5f;
    public float turnSpeed = 3.0f;
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
    float inputV;
    float inputH;

    int currentWaypoint;
    bool onPath = false;
    bool requestedPath = false;

    void Awake() 
    {
	}
	
	void Update () 
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

	}

    void moveToTarget()
    {
        //Look at and dampen the rotation
        Vector3 LookPos = new Vector3(path[currentWaypoint].x, transform.position.y, path[currentWaypoint].z);
        turnRotation = Quaternion.LookRotation(LookPos - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, turnRotation, Time.deltaTime * turnSpeed);

        // Move unit towards the next waypoint
        transform.position += transform.forward * Time.fixedDeltaTime * moveSpeed;

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
            Gizmos.DrawLine(transform.position, startPos);

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
