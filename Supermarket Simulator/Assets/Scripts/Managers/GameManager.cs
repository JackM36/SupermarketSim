using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour 
{
    public GameObject entrance;
    public GameObject exit;

    [HideInInspector]
    public float profit = 0;
    public List<GameObject> customers;
    [HideInInspector]
    public mode gameMode;
    [HideInInspector]
    public GameObject[] planogramPoints;

    int customersCount = 0;

    public enum mode
    {
        play,
        edit
    }

    void Awake()
    {
        // Initializations
        gameMode = mode.edit;
        customers = new List<GameObject>();
    }

    public int totalCustomers
    {
        get
        {
            return customersCount;
        }
    }

    public void addCustomer(GameObject customer)
    {
        customers.Add(customer);
        customersCount++;
    }

    public void removeCustomer(int customerID)
    {
        customers[customerID] = null;
        customersCount--;
    }
}
