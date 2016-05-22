using UnityEngine;
using System.Collections;

public class SteeringBehaviourSeparate : SteeringBehaviour 
{
    Vector3 desiredVelocity;

    public SteeringBehaviourSeparate(SteeringManager manager)
    {
        this.manager = manager;
    }

    public override Vector3 perform()
    {
        Vector3  velocitiesSum = manager.targetPos - manager.currentPos;

        // Get all agents in sight of this agent, and go through all of them
        Collider[] hits = Physics.OverlapSphere(manager.currentPos, manager.personalSpaceRadius, manager.dynamicObstaclesLayers);
        for(int i=0; i < hits.Length; i++)
        {
            if (hits[i].transform != manager.transform)
            {
                // Get the distance from them, and the distance difference vector
                float distance = Vector3.Distance(manager.currentPos, hits[i].transform.position);
                Vector3 distanceVector = (manager.currentPos - hits[i].transform.position).normalized;

                // Add the distance difference vector to the velocities sum
                velocitiesSum += distanceVector;
            }
        }

        // Take the sum of those velocities and scale by the maxSpeed
        desiredVelocity = (velocitiesSum / (hits.Length+1)) * manager.maxSpeed;

        // calculate the steerforce required for the desired velocity based on current velocity
        steerForce = desiredVelocity - manager.currentVelocity;
        steerForce = removeVectorY(steerForce);

        // clamp it to the maximum steer
        steerForce = Vector3.ClampMagnitude(steerForce, manager.maxSteer);

        return steerForce;
    }
}
