using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[System.Serializable]
public class ProductCategory
{
    public string categoryName;
    public float[] prices;
    public Sprite bannerImg;

    [HideInInspector]
    public int discount;

    public ProductCategory(string categoryName)
    {
        this.categoryName = categoryName;
    }
}
