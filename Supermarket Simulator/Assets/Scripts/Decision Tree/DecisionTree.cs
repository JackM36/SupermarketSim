using UnityEngine;
using System.Collections;

public class DecisionTree : MonoBehaviour 
{
    public DecisionTreeNode root;

    public object execute(object obj)
    {
       return root.execute(obj);
    }
}
