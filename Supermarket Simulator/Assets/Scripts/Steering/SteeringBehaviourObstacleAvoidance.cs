using UnityEngine;
using System.Collections;

public class SteeringBehaviourObstacleAvoidance : SteeringBehaviour
{
    Vector3 desiredVelocity;

    public SteeringBehaviourObstacleAvoidance(SteeringManager manager)
    {
        this.manager = manager;
    }

    public override Vector3 perform()
    {
        Vector3 avoidanceForce = Vector3.zero;

        // Create the ray to check for obstacles
        Ray ray = new Ray(manager.currentPos, manager.transform.forward);
        RaycastHit hit;

        // shoot a shperecast with the ray created, to check for collisions
        if (Physics.SphereCast(ray, manager.boundingSphereRadius, out hit, manager.obstacleMaxDistance, manager.staticObstaclesLayers))
        {
            // Check the angle of the hit normal and the up vector of the transform.
            // If it is bigger than the max floor angle, it means it can not be walked and it is an obstacle
            if (Vector3.Angle(hit.normal, manager.transform.up) > manager.maxFloorAngle)
            {
                // The avoidance force is the reflection of the velocity on the hit normal
                avoidanceForce = Vector3.Reflect(manager.currentVelocity, hit.normal);
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
