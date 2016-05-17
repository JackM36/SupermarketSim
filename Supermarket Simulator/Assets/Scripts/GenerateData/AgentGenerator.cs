using UnityEngine;
using System.Collections.Generic;
using LitJson;
using UnityEditor;

public class AgentGenerator : MonoBehaviour
{
    [Header("JSON files")]
    public string customersJsonPath = "Assets/Files/data.json";

    [Header("Customers")]
    public int customersNumber;
    public Vector2 maxSpeedRange, maxSteerRange, sightRadiusRange, slowDownRadiusRange, targetMaxDistanceRange, budgetRange;

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
            double targetMaxDistance = System.Math.Round(Random.Range(targetMaxDistanceRange.x, targetMaxDistanceRange.y), 2);
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
    public double mSpeed, mSteer, sRadius, sDownRadius, tMaxDistance;
    public double budget;
    public double[] itemPreferences, budgetPreferences;
    public bool[] shoppingList;

    public CustomerData(string name, double mSpeed, double mSteer, double sRadius, double sDownRadius, double tMaxDistance, double budget, double[] itemPreferences, double[] budgetPreferences, bool[] shoppingList)
    {
        this.name = name;
        this.mSpeed = mSpeed;
        this.mSteer = mSteer;
        this.sRadius = sRadius;
        this.sDownRadius = sDownRadius;
        this.tMaxDistance = tMaxDistance;
        this.budget = budget;
        this.itemPreferences = itemPreferences;
        this.budgetPreferences = budgetPreferences;
        this.shoppingList = shoppingList;
    }

}

// generate data button
[CustomEditor(typeof(AgentGenerator))]
public class AgentGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        if (GUILayout.Button("Generate Data"))
        {
            AgentGenerator gen = (AgentGenerator)target;
            gen.generate();
        }
    }

}


