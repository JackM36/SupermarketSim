using UnityEngine;

public class CustomerController : AgentController
{
    [HideInInspector]
    public float[] itemPreferences;
    [HideInInspector]
    public float[] budgetPreferences;
    [HideInInspector]
    public bool[] shoppingList;

    [Header("Customer")]
    public float budget;


}
