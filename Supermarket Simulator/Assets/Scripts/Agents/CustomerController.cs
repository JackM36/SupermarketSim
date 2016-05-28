using UnityEngine;

public class CustomerController : AgentController
{
    [HideInInspector]
    public float[] preferences;
    [HideInInspector]
    public float[] willingnessToPay;
    [HideInInspector]
    public bool[] shoppingList;

    [Header("Customer")]
    public float budget;

    void Awake()
    {
        base.Awake();
    }

    void FixedUpdate()
    {
        base.FixedUpdate();
    }
}
