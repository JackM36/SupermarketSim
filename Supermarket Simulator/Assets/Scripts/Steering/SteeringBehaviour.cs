using UnityEngine;
using System.Collections;

public abstract class SteeringBehaviour
{
    public Vector3 steerForce;
    public SteeringManager manager;

    public abstract Vector3 perform();

    public Vector3 removeVectorY(Vector3 vector)
    {
        return new Vector3(vector.x, 0, vector.z);
    }
}
