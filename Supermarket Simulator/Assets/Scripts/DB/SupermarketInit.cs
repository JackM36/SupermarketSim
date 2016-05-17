using UnityEngine;
using System.Collections;
using LitJson;

public class SupermarketInit : MonoBehaviour {

    Item[] items;
    string customers;
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
            customers = System.IO.File.ReadAllText("Assets/Files/data.json");
            data = JsonMapper.ToObject(customers);

            for (int i = 0; i < data.Count; i++)
            {
                // generate customers

                // name
                Debug.Log(data[i]["name"].ToString());
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
