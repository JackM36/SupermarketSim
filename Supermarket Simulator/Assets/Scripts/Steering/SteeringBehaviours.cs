using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class SteeringBehaviours : MonoBehaviour 
{
    float maxSpeed;
    float maxSteer;

    float sightradius;
    float slowDownRadius;

    public enum Behaviour
    {
        seek,
        arrive
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

        // Initializations
        maxSpeed = agent.maxSpeed;
        maxSteer = agent.maxSteer;
        sightradius = agent.sightRadius;
        slowDownRadius = agent.sightRadius;
    }

    public Vector3 performSteering(Vector3 targetPos, List<Behaviour> behaviours)
    {
        Vector3 steerForce = Vector3.zero;

        for (int i = 0; i < behaviours.Count; i++)
        {
            if (behaviours[i] == Behaviour.seek)
                steerForce += seek(targetPos);
            
            if (behaviours[i] == Behaviour.arrive)
                steerForce += arrive(targetPos);
        }

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
            print(1);
            // Calculate the speed it should have in order to arrive correctly at destination.
            // When in the slow down radius, it start slowing down more the closest it is to target.
            speed = (distance * maxSpeed) / slowDownRadius;
        }
        else
        {
            print(2);
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
