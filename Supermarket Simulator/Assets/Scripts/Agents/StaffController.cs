using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StaffController : AgentController 
{
    ProductsManager productsManager;
    List<GameObject>[] onShelves;

    void Awake()
    {
        // Get components
        productsManager = GameObject.Find("ProductsManager").GetComponent<ProductsManager>();

        // Initializations
        onShelves = new List<GameObject>[productsManager.productCategories.Length];
        for (int i = 0; i < onShelves.Length; i++)
        {
            onShelves[i] = new List<GameObject>();
        }
        getOnShelves();
    }

    void getOnShelves()
    {
        GameObject[] shelves = GameObject.FindGameObjectsWithTag("Shelve");

        for (int i = 0; i < shelves.Length; i++)
        {
            Shelve shelve = shelves[i].GetComponent<Shelve>();
            if (shelve.productCategoryID != -1)
            {
                onShelves[shelve.productCategoryID].Add(shelves[i]);
            }
        }
    }

    public Transform getClosestShelve(int productID)
    {
        float minDistance = float.MaxValue;
        int minDistanceIndex = -1;

        for (int i = 0; i < onShelves[productID].Count; i++)
        {
            float distance = Vector3.Distance(transform.position, onShelves[productID][i].transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                minDistanceIndex = i;
            }
        }

        if (minDistanceIndex != -1)
        {
            return onShelves[productID][minDistanceIndex].GetComponent<Shelve>().getAvailableStandingPoint();
        }
        else
        {
            return null;
        }
    }
}
