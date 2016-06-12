using UnityEngine;
using System.Collections;

[RequireComponent(typeof(DecisionTreeCondition))]
public class DecisionTreeNode_Selector : DecisionTreeNode 
{
    DecisionTreeCondition condition;

    void Awake()
    {
        condition = GetComponent<DecisionTreeCondition>();
    }

    public override object execute(object obj)
    {
        if(condition.check(obj))
        {
            if (nodes[0] != null)
            {
                return nodes[0].execute(obj);
            }
        }
        else
        {
            if (nodes[1] != null)
            {
                return nodes[1].execute(obj);
            }
        }

        return 0;
    }
}
