using UnityEngine;
using System.Collections;

public class DecisionTreeCondition_CustomerNotBusy : DecisionTreeCondition 
{

    public override bool check(object obj)
    {
        // Get customer object
        CustomerController customer = obj as CustomerController;

        // Check if customer is busy
        if (customer.isBusy)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
