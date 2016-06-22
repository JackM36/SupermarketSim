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
    public mode gameMode;
    public GameObject[] planogramPoints;

    List<GameObject> customersList;
    List<GameObject> staffList;
    int customersCount = 0;
    int staffCount = 0;

    bool initializedStaff = false;

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
        customersList = new List<GameObject>();
        staffList = new List<GameObject>();
    }

    public int totalCustomers
    {
        get
        {
            return customersCount;
        }
    }

    public int totalStaff
    {
        get
        {
            return staffCount;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        // Initialize or disable staff
        if (gameMode == mode.play && !initializedStaff)
        {
            for (int i = 0; i < staffList.Count; i++)
            {
                staffList[i].GetComponent<StaffController>().getOnShelves();
            }

            initializedStaff = true;
        }
        else if (gameMode == mode.edit && initializedStaff)
        {
            initializedStaff = false;
        }
    }

    public void addCustomer(GameObject customer)
    {
        customersList.Add(customer);
        customersCount++;
    }

    public GameObject getStaff(int staffID)
    {
        return staffList[staffID];
    }

    public void addStaff(GameObject staff)
    {
        staffList.Add(staff);
        staffCount++;
    }

    public void removeCustomer(int customerID)
    {
        customersList[customerID] = null;
        customersCount--;
    }

    public void clearCustomers()
    {
        customersList.Clear();
        customersCount = 0;
    }

    public void clearStaff()
    {
        staffList.Clear();
        staffCount = 0;
    }
}
