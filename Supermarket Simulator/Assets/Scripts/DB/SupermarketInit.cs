﻿using UnityEngine;
using System.Collections.Generic;
using LitJson;

public class SupermarketInit : MonoBehaviour {

    Item[] items;
    public List<GameObject> customers;
    string customerData;
    JsonData data;

	// Use this for initialization
	void Start () {
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

        try
        {
            customerData = System.IO.File.ReadAllText("Assets/Files/data.json");
            data = JsonMapper.ToObject(customerData);

            for (int i = 0; i < data.Count; i++)
            {
                // generate customers
                GameObject customer = (GameObject)Instantiate(customers[Random.Range(0,customers.Count)], new Vector3(0,0,0), Quaternion.identity);

                // target position
                customer.GetComponent<CustomerController>().target = GameObject.Find("Target").transform;

                // assign relative values to customers according te input file
                customer.name = data[i]["name"].ToString();
                customer.GetComponent<CustomerController>().maxSpeed = float.Parse(data[i]["mSpeed"].ToString());
                customer.GetComponent<CustomerController>().maxSteer = float.Parse(data[i]["mSteer"].ToString());
                customer.GetComponent<CustomerController>().sightRadius = float.Parse(data[i]["sRadius"].ToString());
                //customer.GetComponent<CustomerController>().slowDownRadius = float.Parse(data[i]["sDownRadius"].ToString());
                customer.GetComponent<CustomerController>().targetMaxDistance = float.Parse(data[i]["tMaxDistance"].ToString());
                customer.GetComponent<CustomerController>().budget = float.Parse(data[i]["budget"].ToString()); 

                // initialize customer preferences and shopping list
                customer.GetComponent<CustomerController>().itemPreferences = new float[data[i]["itemPreferences"].Count];
                customer.GetComponent<CustomerController>().budgetPreferences = new float[data[i]["budgetPreferences"].Count];
                customer.GetComponent<CustomerController>().shoppingList = new bool[data[i]["shoppingList"].Count];

                for (int j = 0; j < data[i]["itemPreferences"].Count; j++)
                {
                    // fill customer's item preferences
                    try
                    {
                        customer.GetComponent<CustomerController>().itemPreferences[j] = float.Parse(data[i]["itemPreferences"][j].ToString());
                    }
                    catch(System.Exception e)
                    {
                        customer.GetComponent<CustomerController>().itemPreferences[j] = 0.0f;
                    }

                    // fill customer's budget preferences
                    try
                    {
                        customer.GetComponent<CustomerController>().budgetPreferences[j] = float.Parse(data[i]["budgetPreferences"][j].ToString());
                    }
                    catch (System.Exception e)
                    {
                        customer.GetComponent<CustomerController>().budgetPreferences[j] = 0.0f;
                    }
                    
                    // fill customer's shooping list
                    customer.GetComponent<CustomerController>().shoppingList[j] = bool.Parse(data[i]["shoppingList"][j].ToString());

                }             
            }
            
        }
        catch (System.IO.FileNotFoundException)
        {
            Debug.Log("File Not Found");
        }
        
	}
	
	// Update is called once per frame
	void Update () {
        //debugging 
        
        /*
        foreach (Item item in items)
        {
            Debug.Log(item.category + " " + item.minPrice + " " + item.medPrice + " " + item.maxPrice);
        }*/
        
        
	}
}
