using UnityEngine;
using System.Collections.Generic;
using LitJson;
using System.Collections;

[RequireComponent(typeof(AgentGenerator))]
public class AgentSpawner : MonoBehaviour 
{
    [Header("JSON files")]
    public string customersJsonPath = "Assets/Files/customersData.json";

    [Header("Customers")]
    public GameObject spawnArea;
    public int totalCustomerNumber;
    public Vector2 customerNumberPerSpawnRange;
    public Vector2 spawnWaitingTimeRange;
    public List<GameObject> customerModels;

    string customerData;
    JsonData data;
    ProductsManager productsManager;
    List<Vector3> spawnPositions;
    Vector2[] spawnRange;
    int productCategoriesNumber;

    // Use this for initialization
    void Start () 
    {
        // Get components
        productsManager = GameObject.Find("ProductsManager").GetComponent<ProductsManager>();
   
        int productCategoriesNumber = productsManager.productCategories.Length;

        for (int i = 0; i < productCategoriesNumber; i++)
        {
            // generate random values for min, med and max price
            double min = System.Math.Round(Random.Range(1.0f, 3.0f), 2); 
            double med = System.Math.Round(Random.Range(3.0f, 6.0f), 2);
            double max = System.Math.Round(Random.Range(6.0f, 9.0f), 2);

            ProductCategory temp = productsManager.productCategories[i];
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

        int productCategoriesNumber = productsManager.productCategories.Length;

        // store spawn position for every customer in order to avoid collisions between them upon spawn
        spawnPositions = new List<Vector3>(totalCustomerNumber);

        for (int i = 0; i < totalCustomerNumber; i++)
        {
            // Create customer gameobject
            GameObject customer = (GameObject)Instantiate(customerModels[Random.Range(0, customerModels.Count)], new Vector3(0, 0, 0), Quaternion.identity);
            // set placeholder as parent
            customer.transform.parent = customersPlaceholder.transform;

            // get spawn position
            customer.gameObject.transform.position = getPosition(customer);

            // assign relative values to customers according te input file
            customer.name = data[i]["name"].ToString();
            customer.GetComponent<SteeringManager>().maxSpeed = float.Parse(data[i]["maxSpeed"].ToString());
            customer.GetComponent<SteeringManager>().maxSteer = float.Parse(data[i]["maxSteer"].ToString());
            customer.GetComponent<SteeringManager>().sightRadius = float.Parse(data[i]["sightRadius"].ToString());
            customer.GetComponent<SteeringManager>().slowDownRadius = float.Parse(data[i]["slowDownRadius"].ToString());
            customer.GetComponent<CustomerController>().reachedTargetRadius = float.Parse(data[i]["reachedTargetRadius"].ToString());
            customer.GetComponent<CustomerController>().budget = float.Parse(data[i]["budget"].ToString());

            ProductCustomerInfo[] productKnowledge = new ProductCustomerInfo[productCategoriesNumber];
            for (int j = 0; j < productCategoriesNumber; j++)
            {
                productKnowledge[j] = new ProductCustomerInfo();
                productKnowledge[j].preference = float.Parse((data[i]["preferences"][j]).ToString());
                productKnowledge[j].willingnessToPay = float.Parse((data[i]["wtp"][j]).ToString());
                productKnowledge[j].toBuy = bool.Parse((data[i]["toBuy"][j]).ToString());
                productKnowledge[j].onShelve = null;
            }

            customer.GetComponent<CustomerController>().productsKnowledge = productKnowledge;

            // wait until next batch of customers are generated
            int customerNumberPerSpawn = Random.Range((int)customerNumberPerSpawnRange.x, (int)customerNumberPerSpawnRange.y+1);
            float spawnWaitingTime = Random.Range(spawnWaitingTimeRange.x, spawnWaitingTimeRange.y);

            if ((i+1) % customerNumberPerSpawn == 0)
            {
                yield return new WaitForSeconds(spawnWaitingTime);
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
