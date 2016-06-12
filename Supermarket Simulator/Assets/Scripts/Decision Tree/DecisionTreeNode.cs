using UnityEngine;
using System.Collections;

public abstract class DecisionTreeNode : MonoBehaviour 
{
    [SerializeField]
    public DecisionTreeNode[] nodes;

    public abstract object execute(object obj);
}
