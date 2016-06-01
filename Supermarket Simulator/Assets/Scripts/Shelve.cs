using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Shelve : MonoBehaviour
{
    [HideInInspector]
    public int productCategoryID;
    [HideInInspector]
    public string productCategoryName;
    [HideInInspector]
    public float[] prices;
    [HideInInspector]
    public List<Transform> standingPoints;

    void Awake()
    {
        // Initialize shelve products
        productCategoryID = -1;
        productCategoryName = "";
        prices = new float[3];

        // Get standing points
        foreach (Transform child in transform)
        {
            if (child.tag == "StandingPoint")
            {
                standingPoints.Add(child);
            }
        }
    }
}
