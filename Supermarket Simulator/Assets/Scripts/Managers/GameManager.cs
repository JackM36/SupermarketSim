using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour 
{
    [Header("Cameras")]
    public Camera mainCam;

    public GameObject entrance;
    public GameObject exit;

    [HideInInspector]
    public Camera currentCamera;
    [HideInInspector]
    public float profit = 0;
    [HideInInspector]
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
        currentCamera = mainCam;
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
