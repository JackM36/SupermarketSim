using UnityEngine;
using System.Collections;

public class SteeringBehaviourArrive : SteeringBehaviour 
{
    Vector3 desiredVelocity;

    public SteeringBehaviourArrive(SteeringManager manager)
    {
        this.manager = manager;
    }

    public override Vector3 perform()
    {
        float distance = Mathf.Abs(Vector3.Distance(manager.currentPos, manager.finalTargetPos));
        float speed;

        // check if it should start slowing down
        if (distance <= manager.slowDownRadius)
        {
            // Calculate the speed it should have in order to arrive correctly at destination.
            // When in the slow down radius, it start slowing down more the closest it is to target.
            speed = (distance * manager.maxSpeed) / manager.slowDownRadius;
        }
        else
        {
            speed = manager.maxSpeed;
        }

        // velocity vector towards target
        desiredVelocity = (manager.finalTargetPos - manager.currentPos).normalized * speed;

        // calculate the steerforce required for the desired velocity based on current velocity
        steerForce = desiredVelocity - manager.currentVelocity;
        steerForce = removeVectorY(steerForce);

        // clamp it to the maximum steer
        steerForce = Vector3.ClampMagnitude(steerForce, manager.maxSteer);

        return steerForce;
    }
}
