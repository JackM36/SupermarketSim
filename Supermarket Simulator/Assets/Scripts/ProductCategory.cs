using UnityEngine;
using System.Collections;

[System.Serializable]
public class ProductCategory
{
    public string categoryName;
    public float pricePremium;
    public float priceMidPrice;
    public float priceCheap;

    public ProductCategory(string categoryName)
    {
        this.categoryName = categoryName;
    }
}
