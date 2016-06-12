using UnityEngine;
using System.Collections;

public class DecisionTreeCondition_OnPath : DecisionTreeCondition 
{
    public override bool check(object obj)
    {
        // Get customer object
        CustomerController customer = obj as CustomerController;

        // Check if customer is already on a target
        if (customer.onPath && customer.currentWaypoint < customer.path.Length)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
