using UnityEngine;
using System.Collections;

public abstract class DecisionTreeCondition : MonoBehaviour 
{
    public abstract bool check(object obj);
}
