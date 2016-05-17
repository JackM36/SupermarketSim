using UnityEngine;
using System.Collections;

[System.Serializable]
public class Item {

    public string category;
    public double minPrice;
    public double medPrice;
    public double maxPrice;

    public Item(string category, double minPrice, double medPrice, double maxPrice)
    {
        this.category = category;
        this.minPrice = minPrice;
        this.medPrice = medPrice;
        this.maxPrice = maxPrice;
    }
}
