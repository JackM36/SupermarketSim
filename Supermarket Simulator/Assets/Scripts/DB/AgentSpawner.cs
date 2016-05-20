using UnityEngine;
using System.Collections.Generic;
using LitJson;
using System.Collections;

public class AgentSpawner : MonoBehaviour 
{
    [Header("JSON files")]
    public string customersJsonPath = "Assets/Files/customerData.json";

    [Header("Customers")]
    public GameObject spawnArea;
    public int customersNumber;
    public float spawningWaitingTime;
    public List<GameObject> customerModels;

    Item[] items;
    string customerData;
    JsonData data;
    List<Vector3> spawnPositions;
    Vector2[] spawnRange;

    // Use this for initialization
    void Start () 
    {
        // initialize items table 
        items = new Item[(int)Category.CategoryType.CategoryNumber];        

        foreach (Category.CategoryType category in System.Enum.GetValues(typeof(Category.CategoryType)))
        {
            // generate random values for min, med and max price
            double min = System.Math.Round(Random.Range(1.0f, 3.0f), 2); 
            double med = System.Math.Round(Random.Range(3.0f, 6.0f), 2);
            double max = System.Math.Round(Random.Range(6.0f, 9.0f), 2);

            if (category.Equals(Category.CategoryType.CategoryNumber))
            {
                break;
            }

            items[(int)category] = new Item(category.ToString(), min, med, max);
        }
            
        Vector3 spawnAreaDimensions = spawnArea.GetComponent<Collider>().bounds.size;
        Vector3 spawnAreaPosition = spawnArea.gameObject.transform.position;
        spawnRange = new Vector2[2];
        // spawn range for x position
        spawnRange[0] = (new Vector2(spawnAreaPosition.x + spawnAreaDimensions.x / 2, spawnAreaPosition.x - spawnAreaDimensions.x / 2));
        // spawn range for z position
        spawnRange[1] = (new Vector2(spawnAreaPosition.z + spawnAreaDimensions.z / 2, spawnAreaPosition.z - spawnAreaDimensions.z / 2));

        // coroutine for generating customers
        // customers are generated per 5 after the specified waiting time
        StartCoroutine(GenerateCustomers());

	}
	
	// Update is called once per frame
	void Update ()
    {
        //Debug.Log(spawnArea.GetComponent<Collider>().bounds.size);
    }

    IEnumerator GenerateCustomers()
    {
        try
        {
            customerData = System.IO.File.ReadAllText(customersJsonPath);
        }
        catch (System.IO.FileNotFoundException)
        {
            Debug.LogError("Agent Spawner failed: Customers JSON file not found!");
            yield break;
        }

        data = JsonMapper.ToObject(customerData);
        GameObject customersPlaceholder = new GameObject("Customers");

        // store spawn position for every customer in order to avoid collisions between them upon spawn
        spawnPositions = new List<Vector3>(customersNumber);

        for (int i = 0; i < customersNumber; i++)
        {
            // generate customers
            GameObject customer = (GameObject)Instantiate(customerModels[Random.Range(0, customerModels.Count)], new Vector3(0, 0, 0), Quaternion.identity);
            customer.transform.parent = customersPlaceholder.transform;

            // get spawn position
            customer.gameObject.transform.position = getPosition(customer);

            // target position
            customer.GetComponent<CustomerController>().target = GameObject.Find("Target").transform;

            // assign relative values to customers according te input file
            customer.name = data[i]["name"].ToString();
            customer.GetComponent<SteeringBehaviours>().maxSpeed = float.Parse(data[i]["mSpeed"].ToString());
            customer.GetComponent<SteeringBehaviours>().maxSteer = float.Parse(data[i]["mSteer"].ToString());
            customer.GetComponent<SteeringBehaviours>().sightRadius = float.Parse(data[i]["sRadius"].ToString());
            customer.GetComponent<SteeringBehaviours>().slowDownRadius = float.Parse(data[i]["sDownRadius"].ToString());
            customer.GetComponent<CustomerController>().reachedTargetRadius = float.Parse(data[i]["tMaxDistance"].ToString());
            customer.GetComponent<CustomerController>().budget = float.Parse(data[i]["budget"].ToString());

            // initialize customer preferences and shopping list
            customer.GetComponent<CustomerController>().itemPreferences = new float[data[i]["itemPreferences"].Count];
            customer.GetComponent<CustomerController>().budgetPreferences = new float[data[i]["budgetPreferences"].Count];
            customer.GetComponent<CustomerController>().shoppingList = new bool[data[i]["shoppingList"].Count];

            for (int j = 0; j < data[i]["itemPreferences"].Count; j++)
            {
                // fill customer's item preferences
                customer.GetComponent<CustomerController>().itemPreferences[j] = float.Parse(data[i]["itemPreferences"][j].ToString());

                // fill customer's budget preferences
                customer.GetComponent<CustomerController>().budgetPreferences[j] = float.Parse(data[i]["budgetPreferences"][j].ToString());

                // fill customer's shooping list
                customer.GetComponent<CustomerController>().shoppingList[j] = bool.Parse(data[i]["shoppingList"][j].ToString());
            }

            // postpone execution per 5 customers generated
            if ((i+1) % 5 == 0)
            {
                yield return new WaitForSeconds(spawningWaitingTime);
            }
        }        
    }

    Vector3 getPosition(GameObject customer)
    {
        // generate random position
        Vector3 randomPosition = new Vector3(Random.Range(spawnRange[0].x, spawnRange[0].y), 2f, Random.Range(spawnRange[1].x, spawnRange[1].y));
        bool isDone = false;

        // get customer dimensions
        Vector3 customerDimensions = customer.GetComponent<Collider>().bounds.size;

        if (spawnPositions.Count.Equals(0))
        {
            spawnPositions.Add(randomPosition);
        }
           
        else
        {
            while (!isDone)
            {
                for (int i = 0; i < spawnPositions.Count; i++)
                {
                    // check if random position is conflicted with the rest customers' position
                    if ((randomPosition.x > (spawnPositions[i].x - customerDimensions.x)) && (randomPosition.x < (spawnPositions[i].x + customerDimensions.x)))
                    {
                        break;   
                    }
                    if ((randomPosition.z > (spawnPositions[i].z - customerDimensions.z)) && (randomPosition.z < (spawnPositions[i].z + customerDimensions.z)))
                    {
                        break;
                    }
                    isDone = true;
                }
                if (isDone)
                {
                    spawnPositions.Add(randomPosition);
                    return spawnPositions[spawnPositions.Count - 1];
                }
                // generate random position again
                randomPosition = new Vector3(Random.Range(spawnRange[0].x, spawnRange[0].y), 2f, Random.Range(spawnRange[1].x, spawnRange[1].y));
            }
        }
        return spawnPositions[spawnPositions.Count - 1];
    }

}
