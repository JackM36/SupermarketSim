using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody))]
public class SteeringManager : MonoBehaviour 
{
    [Header("Movement")]
    public float maxSpeed = 3f;
    public float maxSteer = 0.15f;
    [Range(0.1f, 0.9f)]
    public float steeringForceConservation = 0.9f;

    [Header("Perception")]
    public float sightRadius = 3.5f;
    public LayerMask dynamicObstaclesLayers;
    public LayerMask staticObstaclesLayers;

    [Header("Distances")]
    public float slowDownRadius = 10;
    public float personalSpaceRadius = 3;
    public float boundingSphereRadius = 1;
    public float obstacleMaxDistance = 5;

    [Header("Angles")]
    [Range(0,90)]
    public float maxFloorAngle = 45;

    [Header("Editor Visuals")]
    public bool showGizmo = true;
    public bool showCurrentVelocityGizmo = true;
    public Color gizmoCurrentVelocityColor = Color.green;
    public bool showSightRadiusGizmo = true;
    public Color gizmoSightRadiusColor = Color.red;
    public bool showPersonalSpaceRadiusGizmo = true;
    public Color gizmoPersonalSpaceRadiusColor = Color.cyan;
    public bool showBoundingSphereGizmo = true;
    public Color gizmoBoundingSphereColor = Color.magenta;
    public bool showFutureBoundingSphereGizmo = true;
    public Color gizmoFutureBoundingSphereColor = Color.magenta;

    public List<SteeringBehaviourItem> steeringBehaviours;

    [HideInInspector]
    public Vector3 targetPos;
    [HideInInspector]
    public Vector3 finalTargetPos;

    Vector3 lastVelocity;
    Rigidbody rb = null;

    public Vector3 currentPos
    {
        get
        {
            return transform.position;
        }
    }

    public Vector3 currentVelocity
    {
        get
        {
            return rb.velocity;
        }
    }

    public enum SteeringBehaviourType
    {
        seek,
        arrive,
        seekArrive,
        separate,
        obstacleAvoidance,
        unalignedObstacleAvoidance
    }

    void Awake()
    {
        // get components
        rb = GetComponent<Rigidbody>();

        // initializations
        lastVelocity = Vector3.zero;
        initSteeringBehaviours();
    }

    public void setTargets(Vector3 currentTarget, Vector3 finalTarget)
    {
        targetPos = currentTarget;
        finalTargetPos = finalTarget;
    }

    public void setFinalTarget(Vector3 finalTarget)
    {
        finalTargetPos = finalTarget;
    }

    public void setCurrentTarget(Vector3 currentTarget)
    {
        targetPos = currentTarget;
    }

    void Update()
    {
        performSteering();
    }

    public void performSteering()
    {
        Vector3 steerForce = Vector3.zero;
        float averageFactor = 0;

        // Go through all steering behaviours
        for (int i = 0; i < steeringBehaviours.Count; i++)
        {
            // perform all behaviours that are active
            if (steeringBehaviours[i].enabled)
            {
                Vector3 newSteerForce = steeringBehaviours[i].behaviour.perform();
                if (newSteerForce != Vector3.zero)
                {
                    steerForce += newSteerForce * steeringBehaviours[i].priority;
                    averageFactor += steeringBehaviours[i].priority;
                }
            }
        }

        if (averageFactor > 0)
        {
            // get the average of the summed forces
            steerForce = (steerForce / averageFactor);
        }

        // Add steerforce to the velocity vector
        rb.velocity = lastVelocity + steerForce;
        lastVelocity = rb.velocity;

        // Rotate to look towards new current velocity
        transform.rotation = lookTowardsVelocity();
    }

    Quaternion lookTowardsVelocity()
    {
        // rotation to look towards current velocity
        Vector3 lookVector = removeVectorY(currentVelocity);
        return Quaternion.LookRotation(lookVector);
    }

    void initSteeringBehaviours()
    {
        for (int i = 0; i < steeringBehaviours.Count; i++)
        {
            if(steeringBehaviours[i].type == SteeringBehaviourType.seek)
            {
                steeringBehaviours[i].behaviour = new SteeringBehaviourSeek(this);
            }
            if(steeringBehaviours[i].type == SteeringBehaviourType.arrive)
            {
                steeringBehaviours[i].behaviour = new SteeringBehaviourArrive(this);
            }
            if(steeringBehaviours[i].type == SteeringBehaviourType.seekArrive)
            {
                steeringBehaviours[i].behaviour = new SteeringBehaviourSeekArrive(this);
            }
            if(steeringBehaviours[i].type == SteeringBehaviourType.separate)
            {
                steeringBehaviours[i].behaviour = new SteeringBehaviourSeparate(this);
            }
            if(steeringBehaviours[i].type == SteeringBehaviourType.obstacleAvoidance)
            {
                steeringBehaviours[i].behaviour = new SteeringBehaviourObstacleAvoidance(this);
            }
            if(steeringBehaviours[i].type == SteeringBehaviourType.unalignedObstacleAvoidance)
            {
                steeringBehaviours[i].behaviour = new SteeringBehaviourUnalignedObstacleAvoidance(this);
            }
        }
    }

    Vector3 removeVectorY(Vector3 vector)
    {
        return new Vector3(vector.x, 0, vector.z);
    }

    void OnDrawGizmos()
    {
        if (showGizmo)
        {
            if (Application.isPlaying && showCurrentVelocityGizmo)
            {
                Gizmos.color = gizmoCurrentVelocityColor;
                DrawArrow.ForGizmo(currentPos, currentVelocity);
            }

            if (showBoundingSphereGizmo)
            {
                Gizmos.color = gizmoBoundingSphereColor;
                Gizmos.DrawWireSphere(currentPos, boundingSphereRadius);
                Gizmos.DrawLine(transform.position, transform.position + transform.forward * obstacleMaxDistance);
            }

            if (showPersonalSpaceRadiusGizmo)
            {
                Gizmos.color = gizmoPersonalSpaceRadiusColor;
                Gizmos.DrawWireSphere(currentPos, personalSpaceRadius);
            }

            if (Application.isPlaying && showFutureBoundingSphereGizmo)
            {
                Gizmos.color = gizmoFutureBoundingSphereColor;
                Gizmos.DrawWireSphere(currentPos + currentVelocity, boundingSphereRadius);
            }
        }
    }

    void OnDrawGizmosSelected() 
    {
        if (showGizmo)
        {
            if (showSightRadiusGizmo)
            {
                // Draw sightRadius Gizmos
                Gizmos.color = gizmoSightRadiusColor;
                Gizmos.DrawWireSphere(currentPos, sightRadius);
            }
        }
    }
}

[System.Serializable]
public class SteeringBehaviourItem
{
    public bool enabled;
    public SteeringManager.SteeringBehaviourType type;
    [Range(1,100)]
    public float priority;

    [HideInInspector]
    public SteeringBehaviour behaviour;

    public SteeringBehaviourItem(SteeringManager.SteeringBehaviourType type, bool enabled, float priority)
    {
        this.type = type;
        this.enabled = enabled;
        this.priority = priority;
    }
}
