using UnityEngine;
using System.Collections;

[System.Serializable]
public class ProductCustomerInfo : MonoBehaviour 
{
    public string productCategoryName;
    public float preference;
    public float willingnessToPay;
    public bool toBuy;
    public bool hasDiscount;
    public GameObject onShelve;

    public ProductCustomerInfo(string productCategoryName, float preference, float willingnessToPay, bool toBuy, bool hasDicount = false, GameObject onShelve = null)
    {
        this.productCategoryName = productCategoryName;
        this.preference = preference;
        this.willingnessToPay = willingnessToPay;
        this.toBuy = toBuy;
        this.hasDiscount = hasDicount;
        this.onShelve = onShelve;
    }

    public ProductCustomerInfo()
    {
        this.productCategoryName = "";
        this.preference = 0;
        this.willingnessToPay = 0;
        this.toBuy = false;
        this.hasDiscount = false;
        this.onShelve = null;
    }
}
