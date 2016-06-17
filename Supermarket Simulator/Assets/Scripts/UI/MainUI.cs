using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainUI : MonoBehaviour 
{
    [Header("UI Elements")]
    public Button startButton;
    public Button stopButton;
    public Button fasterButton;
    public Button slowerButton;
    public Button addStaffButton;
    public Button clearStaffButton;
    public Text totalCustomersTxt;
    public Text profitsTxt;

    [Header("Time")]
    public float maxTimeScale = 3f;
    public float minTimeScale = 0.5f;
    public float timeScaleStep = 0.5f;

    [Header("Agents")]
    public GameObject staffPrefab;
    public AgentSpawner spawner;
    public LayerMask addStaffRayLayers;

    GameManager gameManager;
    bool addingStaff = false;
    GameObject staffToBePlaced = null;

    void Awake()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    void Update()
    {
        // update customers, profits
        profitsTxt.text = "€ " + gameManager.profit.ToString();
        totalCustomersTxt.text = gameManager.totalCustomers.ToString();

        if (addingStaff)
        {
            addStaff();
        }
    }

    void addStaff()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // cast a ray and check if it hits ground
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, addStaffRayLayers))
        {
            if (staffToBePlaced == null)
            {
                staffToBePlaced = (GameObject)Instantiate(staffPrefab, hit.point, staffPrefab.transform.rotation);
            }

            Vector3 placingPos = new Vector3(hit.point.x, hit.point.y + 1.1f, hit.point.z); // temporary. This should be done with raycasting to place it exactly on the ground. This is just a quick solution
            staffToBePlaced.transform.position = placingPos;

            if (hit.transform.tag == "Ground")
            {
                // enable halo to show it can be placed here
                staffToBePlaced.transform.FindChild("ToPlaceHalo").gameObject.SetActive(true);

                // place te staff on click
                if (Input.GetMouseButtonDown(0))
                {
                    GameObject staff = (GameObject)Instantiate(staffPrefab, placingPos, staffPrefab.transform.rotation);
                    Destroy(staff.transform.FindChild("ToPlaceHalo").gameObject);
                    gameManager.addStaff(staff);
                }
            }
            else
            {
                staffToBePlaced.transform.FindChild("ToPlaceHalo").gameObject.SetActive(false);
            }
        }
    }

    public void startSimulation()
    {
        // enable/disable appropriate UI elements
        startButton.interactable = false;
        stopButton.interactable = true;
        fasterButton.interactable = true;
        slowerButton.interactable = true;
        addStaffButton.gameObject.SetActive(false);
        clearStaffButton.gameObject.SetActive(false);

        if (addingStaff)
        {
            addStaff_OnClick();
        }

        // set to play mode
        gameManager.gameMode = GameManager.mode.play;

        // start spawning agents
        spawner.spawnAgents();
    }

    public void stopSimulation()
    {
        // enable/disable appropriate UI elements
        startButton.interactable = true;
        stopButton.interactable = false;
        fasterButton.interactable = false;
        slowerButton.interactable = false;
        addStaffButton.gameObject.SetActive(true);
        clearStaffButton.gameObject.SetActive(true);

        gameManager.gameMode = GameManager.mode.edit;

        Destroy(GameObject.Find("Customers"));
        gameManager.clearCustomers();
        gameManager.profit = 0;
    }

    public void goFaster()
    {
        Time.timeScale += timeScaleStep;

        if (Time.timeScale >= maxTimeScale)
        {
            Time.timeScale = maxTimeScale;
            fasterButton.interactable = false;
        }
        else
        {
            fasterButton.interactable = true;
        }

        slowerButton.interactable = true;
    }

    public void goSlower()
    {
        Time.timeScale -= timeScaleStep;

        if (Time.timeScale <= minTimeScale)
        {
            Time.timeScale = minTimeScale;
            slowerButton.interactable = false;
        }
        else
        {
            slowerButton.interactable = true;
        }

        fasterButton.interactable = true;
    }

    public void addStaff_OnClick()
    {
        if (addingStaff)
        {
            addingStaff = false;
            Destroy(staffToBePlaced);
            staffToBePlaced = null;
            addStaffButton.GetComponentInChildren<Text>().text = "Add Staff";
        }
        else
        {
            addingStaff = true;
            addStaffButton.GetComponentInChildren<Text>().text = "Stop Adding Staff";
        }
    }

    public void clearStaff_OnClick()
    {
        for (int i = 0; i < gameManager.totalStaff; i++)
        {
            Destroy(gameManager.getStaff(i));
        }

        gameManager.clearStaff();
    }
}
