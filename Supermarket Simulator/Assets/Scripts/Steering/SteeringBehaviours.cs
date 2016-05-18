using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class SteeringBehaviours : MonoBehaviour 
{
    [Header("Movement")]
    public float maxSpeed;
    public float maxSteer;

    [Header("Perception")]
    public float sightRadius = 5f;
    public LayerMask agentsLayers;
    public LayerMask obstaclesLayers;

    [Header("Distances")]
    public float slowDownRadius;
    public float boundingSphereRadius = 1;
    public float obstacleMaxDistance = 8;

    [Header("Angles")]
    [Range(0,90)]
    public float maxFloorAngle = 45;

    [Header("Priorities")]
    public float seekPriority = 1;
    public float arrivePriority = 1;
    public float separatePriority = 1;
    public float obstacleAvoidancePriority = 5;

    [Header("Editor Visuals")]
    public bool showCurrentVelocityGizmo = true;
    public Color gizmoCurrentVelocityColor = Color.green;
    public bool showSightRadiusGizmo = true;
    public Color gizmoSightRadiusColor = Color.red;

    public enum Behaviour
    {
        seek,
        arrive,
        separate,
        obstacleAvoidance
    }

    AgentController agent;

    Vector3 currentVelocity
    {
        get
        {
            return agent.rb.velocity;
        }
    }

    void Awake()
    {
        // Get components
        agent = GetComponent<AgentController>();
    }

    public Vector3 performSteering(List<Behaviour> behaviours, Vector3 targetPos, Vector3 finalTargetPos)
    {
        Vector3 steerForce = Vector3.zero;
        float averageFactor = 0;

        // Take all steering behaviours and add their steerForce together
        for (int i = 0; i < behaviours.Count; i++)
        {
            if (behaviours[i] == Behaviour.seek)
            {
                steerForce += seek(targetPos) * seekPriority;
                averageFactor += seekPriority;
            }
            if (behaviours[i] == Behaviour.arrive)
            {
                steerForce += arrive(finalTargetPos) * arrivePriority;
                averageFactor += arrivePriority;
            }
            if (behaviours[i] == Behaviour.separate)
            {
                Vector3 newSteerForce = separate(targetPos);
                if (newSteerForce != Vector3.zero)
                {
                    steerForce += newSteerForce * separatePriority;
                    averageFactor += separatePriority;
                }
            }
            if (behaviours[i] == Behaviour.obstacleAvoidance)
            {
                Vector3 newSteerForce = obstacleAvoidance();
                if (newSteerForce != Vector3.zero)
                {
                    steerForce += newSteerForce * obstacleAvoidancePriority;
                    averageFactor += obstacleAvoidancePriority;
                }
            }
        }

        // Return the average of the summed forces
        return (steerForce / averageFactor);
    }

    public Vector3 performSteering(List<Behaviour> behaviours, Vector3 targetPos)
    {
        Vector3 finalTargetPos = targetPos;
        return performSteering(behaviours, targetPos, finalTargetPos);
    }

    Vector3 seek(Vector3 targetPos)
    {
        // velocity vector towards target
        Vector3 desiredVelocity = (targetPos - transform.position).normalized * maxSpeed;

        // calculate the steerforce required for the desired velocity based on current velocity
        Vector3 steerForce = desiredVelocity - currentVelocity;
        steerForce = removeVectorY(steerForce);

        // clamp it to the maximum steer
        steerForce = Vector3.ClampMagnitude(steerForce, maxSteer);

        return steerForce;
    }

    Vector3 arrive(Vector3 finalTargetPos)
    {
        float distance = Mathf.Abs(Vector3.Distance(transform.position, finalTargetPos));
        float speed;

        // check if it should start slowing down
        if (distance <= slowDownRadius)
        {
            // Calculate the speed it should have in order to arrive correctly at destination.
            // When in the slow down radius, it start slowing down more the closest it is to target.
            speed = (distance * maxSpeed) / slowDownRadius;
        }
        else
        {
            speed = maxSpeed;
        }

        // velocity vector towards target
        Vector3 desiredVelocity = (finalTargetPos - transform.position).normalized * speed;

        // calculate the steerforce required for the desired velocity based on current velocity
        Vector3 steerForce = desiredVelocity - currentVelocity;
        steerForce = removeVectorY(steerForce);

        // clamp it to the maximum steer
        steerForce = Vector3.ClampMagnitude(steerForce, maxSteer);

        return steerForce;
    }

    Vector3 separate(Vector3 targetPos)
    {
        Vector3  velocitiesSum = targetPos - transform.position;

        // Get all agents in sight of this agent, and go through all of them
        Collider[] hits = Physics.OverlapSphere(transform.position, sightRadius, agentsLayers);
        for(int i=0; i < hits.Length; i++)
        {
            // Get the distance from them, and the distance difference vector
            float distance = Vector3.Distance(transform.position, hits[i].transform.position);
            Vector3 distanceVector = (transform.position - hits[i].transform.position).normalized;

            // Add the distance difference vector to the velocities sum
            velocitiesSum += distanceVector;
        }

        // Take the sum of those velocities and scale by the maxSpeed
        Vector3 desiredVelocity = (velocitiesSum / (hits.Length+1)) * maxSpeed;

        // calculate the steerforce required for the desired velocity based on current velocity
        Vector3 steerForce = desiredVelocity - currentVelocity;
        steerForce = removeVectorY(steerForce);

        // clamp it to the maximum steer
        steerForce = Vector3.ClampMagnitude(steerForce, maxSteer);

        return steerForce;
    }

    Vector3 obstacleAvoidance()
    {
        Vector3 avoidanceForce = Vector3.zero;
        Vector3 steerForce = Vector3.zero;

        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.SphereCast(ray, boundingSphereRadius, out hit, obstacleMaxDistance, obstaclesLayers))
        {
            if (Vector3.Angle(hit.normal, transform.up) > maxFloorAngle)
            {
                avoidanceForce = Vector3.Reflect(currentVelocity, hit.normal);
                DrawArrow.ForDebug(hit.point, avoidanceForce);

                //if (Vector3.Dot(avoidanceForce, currentVelocity) < -0.9f)
                //{
                    //avoidanceForce = transform.right;
                //}
            }
        }
            
        if (avoidanceForce != Vector3.zero)
        {
            // Calculate desired velocity
            Vector3 desiredVelocity = avoidanceForce.normalized * maxSpeed;

            // calculate the steerforce required for the desired velocity based on current velocity
            steerForce = desiredVelocity - currentVelocity;
            steerForce = removeVectorY(steerForce);
            DrawArrow.ForDebug(transform.position, steerForce, Color.red);

            // clamp it to the maximum steer
            steerForce = Vector3.ClampMagnitude(steerForce, maxSteer);
        }

        return steerForce;
    }

    public Quaternion lookTowardsVelocity()
    {
        // rotation to look towards current velocity
        Vector3 lookVector = removeVectorY(currentVelocity);
        return Quaternion.LookRotation(lookVector);
    }

    Vector3 removeVectorY(Vector3 vector)
    {
        return new Vector3(vector.x, 0, vector.z);
    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying && showCurrentVelocityGizmo)
        {
            Gizmos.color = gizmoCurrentVelocityColor;
            DrawArrow.ForGizmo(transform.position, currentVelocity);
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, boundingSphereRadius);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * obstacleMaxDistance);
    }

    void OnDrawGizmosSelected() 
    {
        if (showSightRadiusGizmo)
        {
            // Draw sightRadius Gizmos
            Gizmos.color = gizmoSightRadiusColor;
            Gizmos.DrawWireSphere(transform.position, sightRadius);
        }
    }
}
