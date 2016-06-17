using UnityEngine;
using System.Collections;

[System.Serializable]
public class ProductCustomerInfo : MonoBehaviour 
{
    public string productCategoryName;
    public float preference;
    public float willingnessToPay;
    public bool toBuy;
    public int discount;
    public bool inBasket;
    public bool isAvailable;
    public bool toReturn;
    public float pickedUpPrice;
    public GameObject onShelve; // TO DO on many shelves

    public ProductCustomerInfo(string productCategoryName, float preference, float willingnessToPay, bool toBuy, int discount = 0, bool inBasket = false, bool toReturn = false, bool isAvailable = true, float pickedUpPrice = 0, GameObject onShelve = null)
    {
        this.productCategoryName = productCategoryName;
        this.preference = preference;
        this.willingnessToPay = willingnessToPay;
        this.toBuy = toBuy;
        this.discount = discount;
        this.inBasket = inBasket;
        this.toReturn = toReturn;
        this.isAvailable = isAvailable;
        this.pickedUpPrice = pickedUpPrice;
        this.onShelve = onShelve;
    }

    public ProductCustomerInfo()
    {
        this.productCategoryName = "";
        this.preference = 0;
        this.willingnessToPay = 0;
        this.toBuy = false;
        this.discount = 0;
        this.inBasket = false;
        this.toReturn = false;
        this.isAvailable = true;
        this.pickedUpPrice = 0;
        this.onShelve = null;
    }

    public float pref
    {
        get
        {
            return preference * willingnessToPay; // TO DO: divide by product price
        }
    }

    public float getUtility(float price, float weightPref=1, float weightToBuy = 1, float weightHasDiscount = 1, float weightPlacement = 1, float weightPlanogram = 1)
    {
        // Calculate this product's utility based on the agent's knowledge about this product
        if (!inBasket)
        {
            int toBuyInt = toBuy ? 1 : 0;
            int hasDiscountInt = discount > 0 ? 1 : 0;
            float planogramBoost = onShelve.GetComponent<Shelve>().getPlanogramBoost();
            float placementBoost = onShelve.GetComponent<Shelve>().getPlacementBoost();
            float weightsTotal = weightToBuy + weightHasDiscount + weightHasDiscount + weightPlacement + weightPlanogram;

            float utility = ((weightPref * pref) + (weightToBuy * toBuyInt) + (weightHasDiscount * hasDiscountInt) + (weightPlanogram * planogramBoost) + (weightPlacement * placementBoost)) / weightsTotal;
            return utility + (1 / price);
        }
        else
        {
            return 0;
        }
    }
}
