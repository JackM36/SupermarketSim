using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Shelve : MonoBehaviour
{
    public float neighbourSensorDistance = 1;
    public NeighbourSensorAxis neighbourSensorAxis;

    [HideInInspector]
    public int productCategoryID;
    [HideInInspector]
    public string productCategoryName;
    [HideInInspector]
    public float[] shelveLevelPrices;
    [HideInInspector]
    public int[] shelveLevelPricesIDs;
    [HideInInspector]
    public List<Shelve> neighbourShelves;
    [HideInInspector]
    public bool toBoostPlacement = false;
    [HideInInspector]
    public int boostedBy = -1;

    List<Transform> standingPoints;
    float halfSize = 0;

    GameManager gameManager;
    ProductsManager productsManager;

    public enum NeighbourSensorAxis
    {
        x,
        y,
        z
    }

    void Awake()
    {
        // Get components
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        productsManager = GameObject.Find("ProductsManager").GetComponent<ProductsManager>();

        // Initialize shelve products
        productCategoryID = -1;
        productCategoryName = "";
        shelveLevelPrices = new float[3];
        shelveLevelPricesIDs = new int[3];
        neighbourShelves = new List<Shelve>();
        standingPoints = new List<Transform>();

        getStandingPoints();
        getNeighbourShelves();
    }

    public Transform getAvailableStandingPoint()
    {
        // Get a random & available standing point for this shelve
        int standingPointIndex = Random.Range(0, standingPoints.Count);
        return standingPoints[standingPointIndex].transform;
    }

    void getStandingPoints()
    {
        // Get all standing points
        foreach (Transform child in transform)
        {
            if (child.tag == "StandingPoint")
            {
                standingPoints.Add(child);
            }
        }
    }

    void getNeighbourShelves()
    {
        //get collider size
        Collider col = GetComponent<Collider>();
        Vector3 colSize = col.bounds.extents;


        switch (neighbourSensorAxis)
        {
            case NeighbourSensorAxis.x:
                halfSize = colSize.x;
                break;
            case NeighbourSensorAxis.y:
                halfSize = colSize.y;
                break;
            case NeighbourSensorAxis.z:
                halfSize = colSize.z;
                break;
        }

        RaycastHit hitLeft;
        RaycastHit hitRight;

        bool neighbourLeft = Physics.Raycast(transform.position, Vector3.left * (halfSize + neighbourSensorDistance), out hitLeft);
        bool neighbourRight = Physics.Raycast(transform.position, Vector3.right * (halfSize + neighbourSensorDistance), out hitRight);

        if (neighbourLeft)
        {
            if (hitLeft.collider.tag == "Shelve")
            {
                neighbourShelves.Add(hitLeft.transform.GetComponent<Shelve>());
            }
        }

        if (neighbourRight)
        {
            if (hitRight.collider.tag == "Shelve")
            {
                neighbourShelves.Add(hitRight.transform.GetComponent<Shelve>());
            }
        }
    }

    // calculate distances from the current shelve to the target points of the supermarket
    // return index of the point with the minimum distance
    int getClosestPlanogramPointIndex()
    {
        float minDistance = float.MaxValue;
        int minDistanceIndex = 0;

        for (int i = 0; i < gameManager.planogramPoints.Length; i++)
        {
            float distance = Vector3.Distance(this.transform.position, gameManager.planogramPoints[i].transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                minDistanceIndex = i;
            }
        }

        return minDistanceIndex;
    }

    public float getPlanogramBoost()
    {
        int planogramPointID = getClosestPlanogramPointIndex();
        return productsManager.planogram[planogramPointID, productCategoryID];
    }

    public float getPlacementBoost()
    {
        if (toBoostPlacement)
        {
            return productsManager.productPosition[productCategoryID, boostedBy];
        }
        else
        {
            return 0;
        }
    }
}
