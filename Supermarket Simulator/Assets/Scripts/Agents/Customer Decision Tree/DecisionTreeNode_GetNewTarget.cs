﻿using UnityEngine;
using System.Collections;

public class DecisionTreeNode_GetNewTarget : DecisionTreeNode 
{
    DecisionTreeCondition condition;

    void Awake()
    {
        condition = GetComponent<DecisionTreeCondition>();
    }

    public override object execute(object obj)
    {
        // Get customer
        CustomerController customer = obj as CustomerController;

        if (condition.check(obj))
        {
            customer.getNewTarget();
        }

        return 1;
    }
}