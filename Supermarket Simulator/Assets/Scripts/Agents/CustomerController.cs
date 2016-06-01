using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CustomerController : AgentController
{
    [HideInInspector]
    public float[] preferences;
    [HideInInspector]
    public float[] willingnessToPay;
    [HideInInspector]
    public bool[] shoppingList;

    public ProductCustomerInfo[] productsKnowledge;

    [Header("Customer")]
    public float budget;

    List<Transform> aisleEntryPoints;

    void Awake()
    {
        base.Awake();

        getAisleEntryPoints();
    }

    void FixedUpdate()
    {
        base.FixedUpdate();
        seeProductsOnShelves();
    }

    override protected void getNewTarget()
    {
        // Get a random aisle entry point as target
        finalTarget = aisleEntryPoints[Random.Range(0, aisleEntryPoints.Count)].transform;
    }

    void seeProductsOnShelves()
    {
        Vector3 rightAngle = Quaternion.AngleAxis (perceptionSightAngle, transform.up) * transform.forward;
        Vector3 leftAngle = Quaternion.AngleAxis (-perceptionSightAngle, transform.up) * transform.forward;

        RaycastHit leftHit;
        RaycastHit rightHit;

        // Check left raycast for shelve hit
        bool shelveLeftHit = Physics.Raycast(transform.position, leftAngle, out leftHit, perceptionSightDistance);
        if (shelveLeftHit)
        {
            checkShelveHit(leftHit);
        }

        // Check right raycast for shelve hit
        bool shelveRightHit = Physics.Raycast(transform.position, rightAngle, out rightHit, perceptionSightDistance);
        if (shelveRightHit)
        {
            checkShelveHit(rightHit);
        }
    }

    void checkShelveHit(RaycastHit hit)
    {
        if (hit.transform.tag == "Shelve")
        {
            Shelve shelve = hit.transform.GetComponent<Shelve>();

            // Check if shelve has a product first
            if (shelve.productCategoryID != -1)
            {
                // Get onShelve info
                GameObject onShelve = hit.transform.gameObject;

                // Add new knowledge to list
                productsKnowledge[shelve.productCategoryID].onShelve = onShelve;
            }

            // draw ray line
            Debug.DrawLine(transform.position, hit.point, Color.red);
        }
    }

    void getAisleEntryPoints()
    {
        aisleEntryPoints = new List<Transform>();
        Transform placeholder = GameObject.Find("AisleEntryPoints").transform;

        foreach (Transform child in placeholder)
        {
            aisleEntryPoints.Add(child);
        }
    }
}
