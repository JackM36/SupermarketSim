using UnityEngine;
using System.Collections.Generic;
using LitJson;
using UnityEditor;

public class AgentGenerator : MonoBehaviour
{
    [Header("JSON files")]
    public string customersJsonPath = "Assets/Files/customersData.json";

    [Header("Customers")]
    public int customersNumber;
    public Vector2 maxSpeedRange, maxSteerRange, sightRadiusRange, slowDownRadiusRange, reachedTargetRadius, budgetRange;

    public void generate()
    {
        // create a list of all data to be written in json file
        List<CustomerData> _data = new List<CustomerData>();

        for (int i = 0; i < customersNumber; i++)
        {
            //generate random preferences for each category
            double[] preferences = new double[(int)Category.CategoryType.CategoryNumber];
            for (int j = 0; j < preferences.Length; j++)
            {
                preferences[j] = System.Math.Round(Random.Range(0.0f, 1.0f), 2);
            }

            //generate random budgetPreferences for each category
            double[] bPreferences = new double[(int)Category.CategoryType.CategoryNumber];
            for (int j = 0; j < bPreferences.Length; j++)
            {
                bPreferences[j] = System.Math.Round(Random.Range(0.0f, 1.0f), 2);
            }

            // generate shoopingList
            bool[] sList = new bool[(int)Category.CategoryType.CategoryNumber];
            for (int j = 0; j < sList.Length; j++)
            {
                sList[j] = (Random.value > 0.5f);
            }

            // add each customer to the list with preset maxSpeed, maxSteer, sightRadius and targetMaxDistance values together with the generated preferences
            double maxSpeed = System.Math.Round(Random.Range(maxSpeedRange.x, maxSpeedRange.y), 2);
            double maxSteer = System.Math.Round(Random.Range(maxSteerRange.x, maxSteerRange.y), 2); 
            double sightRadius = System.Math.Round(Random.Range(sightRadiusRange.x, sightRadiusRange.y), 2);
            double slowDownRadius = System.Math.Round(Random.Range(slowDownRadiusRange.x, slowDownRadiusRange.y), 2);
            double targetMaxDistance = System.Math.Round(Random.Range(reachedTargetRadius.x, reachedTargetRadius.y), 2);
            double budget = System.Math.Round(Random.Range(budgetRange.x, budgetRange.y), 2);
            _data.Add(new CustomerData("customer " + i, maxSpeed, maxSteer, sightRadius, slowDownRadius, targetMaxDistance, budget , preferences, bPreferences, sList));
           
        }

        // convert list to json object and write it to file
        JsonData json = JsonMapper.ToJson(_data);
        System.IO.File.WriteAllText(@customersJsonPath, json.ToString());

        // refresh assets to show file, and print success log
        AssetDatabase.Refresh();
        print("Customers JSON file created!");
    }

}

// Object structure to be written in file
public class CustomerData
{
    public string name;
    public double maxSpeed, maxSteer, sightRadius, slowDownRadius, reachedTargetRadius;
    public double budget;
    public double[] preferences, willingnessToPay;
    public bool[] shoppingList;

    public CustomerData(string name, double maxSpeed, double maxSteer, double sightRadius, double slowDownRadius, double reachedTargetRadius, double budget, double[] preferences, double[] willingnessToPay, bool[] shoppingList)
    {
        this.name = name;
        this.maxSpeed = maxSpeed;
        this.maxSteer = maxSteer;
        this.sightRadius = sightRadius;
        this.slowDownRadius = slowDownRadius;
        this.reachedTargetRadius = reachedTargetRadius;
        this.budget = budget;
        this.preferences = preferences;
        this.willingnessToPay = willingnessToPay;
        this.shoppingList = shoppingList;
    }

}


