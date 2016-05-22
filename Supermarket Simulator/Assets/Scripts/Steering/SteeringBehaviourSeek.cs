using UnityEngine;
using System.Collections;

public class SteeringBehaviourSeek : SteeringBehaviour
{
    Vector3 desiredVelocity;

    public SteeringBehaviourSeek(SteeringManager manager)
    {
        this.manager = manager;
    }

    public override Vector3 perform()
    {
        // velocity vector towards target
        desiredVelocity = (manager.targetPos - manager.currentPos).normalized * manager.maxSpeed;

        // calculate the steerforce required for the desired velocity based on current velocity
        steerForce = desiredVelocity - manager.currentVelocity;
        steerForce = removeVectorY(steerForce);

        // clamp it to the maximum steer
        steerForce = Vector3.ClampMagnitude(steerForce, manager.maxSteer);

        return steerForce;
    }
}
