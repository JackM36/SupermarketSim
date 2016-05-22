using UnityEngine;
using System.Collections;

public class SteeringBehaviourUnalignedObstacleAvoidance : SteeringBehaviour 
{
    Vector3 desiredVelocity;

    public SteeringBehaviourUnalignedObstacleAvoidance(SteeringManager manager)
    {
        this.manager = manager;
    }

    public override Vector3 perform()
    {
        Vector3 avoidanceForce = Vector3.zero;
        GameObject mostThreateningObstacle = null;
        float mostTheateningObstacleSqrDis = float.MaxValue;

        // Get all agents in sight of this agent, and go through all of them
        Collider[] hits = Physics.OverlapSphere(manager.currentPos, manager.sightRadius, manager.dynamicObstaclesLayers);
        for(int i=0; i < hits.Length; i++)
        {
            if (hits[i].transform != manager.transform)
            {
                Vector3 obstacleFuturePos = hits[i].transform.position + hits[i].attachedRigidbody.velocity;
                Vector3 futurePos = manager.currentPos + manager.currentVelocity;

                float sumRadius = manager.boundingSphereRadius * 2; //+ dynamicObstacle.BoundingSphereRadius;

                if ((obstacleFuturePos - futurePos).sqrMagnitude < sumRadius * sumRadius)
                {
                    float sqrDist = (manager.currentPos - hits[i].transform.position).sqrMagnitude;
                    if (sqrDist < mostTheateningObstacleSqrDis)
                    {
                        mostThreateningObstacle = hits[i].gameObject;
                        mostTheateningObstacleSqrDis = sqrDist;
                    }
                }
            }
        }

        // Calculate avoidance force
        if (mostThreateningObstacle != null)
        {
            avoidanceForce = manager.currentPos + manager.currentVelocity - mostThreateningObstacle.transform.position;
            DrawArrow.ForDebug(manager.currentPos, avoidanceForce, Color.red);
            if (Vector3.Dot(avoidanceForce, manager.currentVelocity) < -0.9f)
            {
                avoidanceForce = manager.transform.right;
            }
        }

        if (avoidanceForce != Vector3.zero)
        {
            // Calculate desired velocity
            desiredVelocity = avoidanceForce.normalized * manager.maxSpeed;

            // calculate the steerforce required for the desired velocity based on current velocity
            steerForce = desiredVelocity - manager.currentVelocity;
            steerForce = removeVectorY(steerForce);

            // clamp it to the maximum steer
            steerForce = Vector3.ClampMagnitude(steerForce, manager.maxSteer);
        }
        else
        {
            // If no avoidance is required, still conserve some of the previous force so it goes back smoothly
            steerForce *= manager.steeringForceConservation;
        }

        return steerForce;
    }
}
