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

    List<Transform> standingPoints;

    void Awake()
    {
        // Initialize shelve products
        productCategoryID = -1;
        productCategoryName = "";
        prices = new float[3];
        standingPoints = new List<Transform>();

        // Get standing points
        foreach (Transform child in transform)
        {
            if (child.tag == "StandingPoint")
            {
                standingPoints.Add(child);
            }
        }
    }

    public Transform getAvailableStandingPoint()
    {
        // Get a random & available standing point for this shelve
        int standingPointIndex = Random.Range(0, standingPoints.Count);
        return standingPoints[standingPointIndex].transform;
    }
}
