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

    [Header("Distances")]
    public float slowDownRadius;

    public enum Behaviour
    {
        seek,
        arrive,
        seperate
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

    public Vector3 performSteering(Vector3 targetPos, List<Behaviour> behaviours)
    {
        Vector3 steerForce = Vector3.zero;

        // Take all steering behaviours and add their steerForce together
        for (int i = 0; i < behaviours.Count; i++)
        {
            if (behaviours[i] == Behaviour.seek)
                steerForce += seek(targetPos);
            
            if (behaviours[i] == Behaviour.arrive)
                steerForce += arrive(targetPos);
            if (behaviours[i] == Behaviour.seperate)
                steerForce += seperate(targetPos);
        }

        // Return the average of the summed forces
        return (steerForce / behaviours.Count);
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

    Vector3 arrive(Vector3 targetPos)
    {
        float distance = Mathf.Abs(Vector3.Distance(transform.position, targetPos));
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
        Vector3 desiredVelocity = (targetPos - transform.position).normalized * speed;

        // calculate the steerforce required for the desired velocity based on current velocity
        Vector3 steerForce = desiredVelocity - currentVelocity;
        steerForce = removeVectorY(steerForce);

        // clamp it to the maximum steer
        steerForce = Vector3.ClampMagnitude(steerForce, maxSteer);

        return steerForce;
    }

    Vector3 seperate(Vector3 targetPos)
    {
        Vector3  velocitiesSum = targetPos - transform.position;

        Collider[] hits = Physics.OverlapSphere(transform.position, sightRadius);
        for(int i=0; i < hits.Length; i++)
        {
            float distance = Vector3.Distance(transform.position, hits[i].transform.position);
            Vector3 distanceVector = (transform.position - hits[i].transform.position).normalized;
            velocitiesSum += distanceVector;
        }

        Vector3 desiredVelocity = (velocitiesSum / (hits.Length+1)) * maxSpeed;

        // calculate the steerforce required for the desired velocity based on current velocity
        Vector3 steerForce = desiredVelocity - currentVelocity;
        steerForce = removeVectorY(steerForce);

        // clamp it to the maximum steer
        steerForce = Vector3.ClampMagnitude(steerForce, maxSteer);

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
}
