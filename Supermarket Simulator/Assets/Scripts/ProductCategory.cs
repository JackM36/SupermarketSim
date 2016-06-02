using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class ProductCategory
{
    public string categoryName;
    public float pricePremium;
    public float priceMidPrice;
    public float priceCheap;
    public Sprite bannerImg;

    public ProductCategory(string categoryName)
    {
        this.categoryName = categoryName;
    }
}
