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

    void Awake() 
    {
	}
	
	void Update () 
    {
        if (path.Length == 0)
        {
            // Make a request for a new path
            NavMeshPathRequestManager.requestPath(transform.position, target.position, onPathFound);
        }

        if (currentWaypoint < path.Length)
        {
            moveToTarget();
        }

	}

    void moveToTarget()
    {
        //Look at and dampen the rotation
        Vector3 test = new Vector3(path[currentWaypoint].x, transform.position.y, path[currentWaypoint].z);
        turnRotation = Quaternion.LookRotation(test - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, turnRotation, Time.deltaTime * turnSpeed);

        // Move unit towards the next waypoint
        transform.position += transform.forward * Time.fixedDeltaTime * moveSpeed;

        // If the unit is close enough to the currentWaypoint, register it as reached
        if (Vector3.Distance(transform.position, path[currentWaypoint]) < targetMaxDistance)
        {
            currentWaypoint++;
        }
    }

    public void onPathFound(Vector3[] newPath, bool foundPath)
    {
        // When a path was found, set the recieved path as the new path
        if (foundPath)
        {
            path = newPath;
            currentWaypoint = 0;
        }
    }

    private void OnDrawGizmosSelected() 
    {
        // Draw sightRadius Gizmos
        Gizmos.color = gizmoRadiusColor;
        Gizmos.DrawWireSphere (transform.position, sightRadius);

        // Draw path lines
        Gizmos.color = gizmoPathColor;
        if (path != null && path.Length > 0)
        {
            for (int i = 0; i < path.Length-1; i++)
            {
                Vector3 startPos = new Vector3(path[i].x, transform.position.y, path[i].z);
                Vector3 endPos = new Vector3(path[i+1].x, transform.position.y, path[i+1].z);
                Gizmos.DrawLine(startPos, endPos);
            }
        }
    }
}
