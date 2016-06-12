using UnityEngine;
using System.Collections;

public class DecisionTreeCondition_CustomerHasTarget : DecisionTreeCondition 
{
    public override bool check(object obj)
    {
        // Get customer object
        CustomerController customer = obj as CustomerController;

        // Check if customer has a target
        if (customer.finalTarget == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}

