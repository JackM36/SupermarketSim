using UnityEngine;
using System.Collections.Generic;
using LitJson;
using UnityEditor;

public class GenerateData : MonoBehaviour
{
    public int customerNumber;
    public int moveSpeed, turnSpeed, sightRadius;
   

    public void generate()
    {
        List<Data> _data;
        _data = new List<Data>();

        for (int i = 0; i < customerNumber; i++)
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

            _data.Add(new Data("customer " + i, moveSpeed, turnSpeed, sightRadius, System.Math.Round(Random.Range(50.0f, 200.0f), 2), preferences, bPreferences, sList));
           
        }

        JsonData json = JsonMapper.ToJson(_data);
        System.IO.File.WriteAllText(@"Assets/Files/data.json", json.ToString());
    }

}

public class Data
{
    public string name;
    public int mSpeed, tSpeed, sRadius;
    public double budget;
    public double[] itemPreferences, budgetPreferences;
    public bool[] shoppingList;

    public Data(string name, int mSpeed, int tSpeed, int sRadius, double budget, double[] itemPreferences, double[] budgetPreferences, bool[] shoppingList)
    {
        this.name = name;
        this.mSpeed = mSpeed;
        this.sRadius = sRadius;
        this.budget = budget;
        this.itemPreferences = itemPreferences;
        this.budgetPreferences = budgetPreferences;
        this.shoppingList = shoppingList;
    }

}

[CustomEditor(typeof(GenerateData))]
public class GenerateButton : Editor
{
    public override void OnInspectorGUI()
    {
        
        DrawDefaultInspector();
        if (GUILayout.Button("Generate Data"))
        {
            GenerateData gen = (GenerateData)target;
            gen.generate();
        }
    }

}

