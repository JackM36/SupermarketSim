using UnityEngine;
using System.Collections;

public class SteeringBehaviourObstacleAvoidance : SteeringBehaviour
{
    Vector3 desiredVelocity;
    Vector3 conservedVelocity;
    bool avoidingObstacle = false;
    AvoidanceState state = AvoidanceState.none;

    enum AvoidanceState
    {
        none,
        avoiding,
        avoidingAndReadyToConserve,
        conservingAvoidance
    }

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

        Vector3 rightAngle = Quaternion.AngleAxis (manager.avoidanceConservationAngle, manager.transform.up) * manager.transform.forward;
        Vector3 leftAngle = Quaternion.AngleAxis (-manager.avoidanceConservationAngle, manager.transform.up) * manager.transform.forward;

        bool avoidanceHit = Physics.SphereCast(ray, manager.boundingSphereRadius, out hit, manager.obstacleMaxDistance, manager.staticObstaclesLayers);
        bool conserveAvoidanceLeftHit = Physics.Raycast(manager.currentPos, leftAngle, out hit, manager.obstacleMaxDistance, manager.staticObstaclesLayers);
        bool conserveAvoidanceRightHit = Physics.Raycast(manager.currentPos, rightAngle, out hit, manager.obstacleMaxDistance, manager.staticObstaclesLayers);

        // shoot a shperecast with the ray created, to check for collisions
        if (avoidanceHit)
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

        if (conserveAvoidanceLeftHit)
        {
            Debug.DrawLine(manager.currentPos, hit.point, Color.green);
        }
        else if (conserveAvoidanceRightHit)
        {
            Debug.DrawLine(manager.currentPos, hit.point, Color.green);
        }

        // Set avoidance state based on hitting raycasts
        setAvoidanceState(avoidanceHit, conserveAvoidanceLeftHit, conserveAvoidanceRightHit);


        if (avoidanceForce != Vector3.zero)
        {
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

    void setAvoidanceState(bool avoidanceHit, bool conserveAvoidanceLeftHit, bool conserveAvoidanceRightHit)
    {
        if (avoidanceHit)
        {
            // set avoidance state
            if (state == AvoidanceState.none)
            {
                state = AvoidanceState.avoiding;
            }
            if (state == AvoidanceState.conservingAvoidance)
            {
                state = AvoidanceState.avoidingAndReadyToConserve;
            }
        }
        else
        {
            // set avoidance state
            if (state == AvoidanceState.avoiding)
            {
                state = AvoidanceState.none;
            }
            if (state == AvoidanceState.avoidingAndReadyToConserve)
            {
                state = AvoidanceState.conservingAvoidance;
                conservedVelocity = manager.currentVelocity;
            }
        }

        if (conserveAvoidanceLeftHit || conserveAvoidanceRightHit)
        {
            // set avoidance state
            if (state == AvoidanceState.avoiding)
            {
                state = AvoidanceState.avoidingAndReadyToConserve;
            }
        }
        else
        {
            // set avoidance state
            if (state == AvoidanceState.avoidingAndReadyToConserve)
            {
                state = AvoidanceState.avoiding;
            }
            if (state == AvoidanceState.conservingAvoidance)
            {
                state = AvoidanceState.none;
            }
        }
    }
}
