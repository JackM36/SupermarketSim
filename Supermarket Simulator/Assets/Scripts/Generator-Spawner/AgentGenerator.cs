using UnityEngine;
using System.Collections.Generic;
using LitJson;

#if UNITY_EDITOR
using UnityEditor;
#endif 

public class AgentGenerator : MonoBehaviour
{
    [Header("JSON files")]
    public string customersJsonPath = "Assets/Files/customersData.json";

    [Header("Customers")]
    public int customersNumber;
    public Vector2 maxSpeedRange, maxSteerRange, sightRadiusRange, slowDownRadiusRange, reachedTargetRadiusRange, budgetRange;

    public void generate()
    {
        // create a list of all data to be written in json file
        List<CustomerData> _data = new List<CustomerData>();
        ProductsManager productManager = GameObject.Find("ProductsManager").GetComponent<ProductsManager>();
        int productCategoriesNumber = productManager.productCategories.Length;

        ProductCustomerInfo[] products = new ProductCustomerInfo[productCategoriesNumber];

        for (int i = 0; i < customersNumber; i++)
        {
            //generate random preferences for each category
            double[] preferences = new double[productCategoriesNumber];
            double[] wtp = new double[productCategoriesNumber];
            bool[] toBuy = new bool[productCategoriesNumber];

            for (int j = 0; j < productCategoriesNumber; j++)
            {
                //products[j].productCategoryName = productManager.productCategories[j].categoryName;
                preferences[j] = (float)System.Math.Round(Random.Range(0.0f, 1.0f), 2);
                wtp[j] = (float)System.Math.Round(Random.Range(0.0f, 1.0f), 2);
                toBuy[j] = (Random.value > 0.5f);
            }

            // add each customer to the list with preset maxSpeed, maxSteer, sightRadius and targetMaxDistance values together with the generated preferences
            double maxSpeed = System.Math.Round(Random.Range(maxSpeedRange.x, maxSpeedRange.y), 2);
            double maxSteer = System.Math.Round(Random.Range(maxSteerRange.x, maxSteerRange.y), 2); 
            double sightRadius = System.Math.Round(Random.Range(sightRadiusRange.x, sightRadiusRange.y), 2);
            double slowDownRadius = System.Math.Round(Random.Range(slowDownRadiusRange.x, slowDownRadiusRange.y), 2);
            double reachedTargetRadius = System.Math.Round(Random.Range(reachedTargetRadiusRange.x, reachedTargetRadiusRange.y), 2);
            double budget = System.Math.Round(Random.Range(budgetRange.x, budgetRange.y), 2);
            _data.Add(new CustomerData("customer " + i, maxSpeed, maxSteer, sightRadius, slowDownRadius, reachedTargetRadius, budget, preferences, wtp, toBuy));
        }

        // convert list to json object and write it to file
        JsonData json = JsonMapper.ToJson(_data);
        System.IO.File.WriteAllText(@customersJsonPath, json.ToString());

        // refresh assets to show file, and print success log
        #if UNITY_EDITOR
        AssetDatabase.Refresh();
        #endif
        print("Customers JSON file created!");
    }

}

// Object structure to be written in file
public class CustomerData
{
    public string name;
    public double maxSpeed, maxSteer, sightRadius, slowDownRadius, reachedTargetRadius;
    public double budget;
    public double[] preferences, wtp;
    public bool[] toBuy;

    public CustomerData(string name, double maxSpeed, double maxSteer, double sightRadius, double slowDownRadius, double reachedTargetRadius, double budget, double[] preferences, double[] wtp, bool[] toBuy)
    {
        this.name = name;
        this.maxSpeed = maxSpeed;
        this.maxSteer = maxSteer;
        this.sightRadius = sightRadius;
        this.slowDownRadius = slowDownRadius;
        this.reachedTargetRadius = reachedTargetRadius;
        this.budget = budget;
        this.preferences = preferences;
        this.wtp = wtp;
        this.toBuy = toBuy;
    }

}


