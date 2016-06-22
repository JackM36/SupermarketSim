using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ProductsManager : MonoBehaviour 
{
    public ProductCategory[] productCategories;
    public float weightPref = 1f;
    public float weightToBuy = 0.5f;
    public float weightHasDiscount = 0.2f;
    public float weightPlacement = 0.3f;
    public float weightPlanogram = 0.2f;
    public float boostEyeLevelShelve = 0.5f;
    public float boostHandsLevelShelve = 0.2f;
    public float boostFeetLevelShelve = 0.1f;

    //TODO fix position and number of products in array
    public float[,] productPosition = new float[10, 10] {
        { 1f, 1.2f, 1.8f, 1.6f, 1.5f, 1.5f, 1.1f, 1.2f, 1.1f, 1.5f },//meat
        { 1.2f, 1f, 1.8f, 1.3f, 1.3f, 1.4f, 1.2f, 1.2f, 1.1f, 1.2f },//fruits
        { 1.8f, 1.8f, 1f, 1.5f, 1.4f, 1.1f, 1.2f, 1.2f, 1.1f, 1.2f },//vegetables
        { 1.5f, 1.3f, 1.5f, 1f, 1.6f, 1.3f, 1.3f, 1.2f, 1.1f, 1.2f },//dairy
        { 1.5f, 1.3f, 1.4f, 1.6f, 1f, 1.3f, 1.8f, 1.2f, 1.1f, 1.3f },//bakery
        { 1.5f, 1.4f, 1.1f, 1.3f, 1.3f, 1f, 1.3f, 1.7f, 1.5f, 1.8f },//alcohol
        { 1.1f, 1.2f, 1.2f, 1.3f, 1.8f, 1.3f, 1f, 1.5f, 1.1f, 1.5f },//candy
        { 1.2f, 1.2f, 1.2f, 1.2f, 1.2f, 1.7f, 1.5f, 1f, 1.1f, 1.8f },//chips
        { 1.1f, 1.1f, 1.1f, 1.1f, 1.1f, 1.5f, 1.1f, 1.1f, 1f, 1.3f },//cleaning
        { 1.5f, 1.2f, 1.2f, 1.2f, 1.3f, 1.8f, 1.5f, 1.8f, 1.3f, 1f },//softdrinks
    };

    //TODO fix position and number of products in array
    public float[,] planogram = new float[3, 10] {
        { 1.8f, 1f, 1f, 1.6f, 1.8f, 1.1f, 1.2f, 1.2f, 1.1f, 1.1f},//distance_entrance
        { 1.6f, 1.3f, 1.3f, 1.7f, 1.6f, 1.5f, 1.5f, 1.4f, 1.1f, 1.4f},//distance_middle
        { 1.2f, 1.2f, 1.2f, 1.3f, 1.8f, 1.8f, 1.7f, 1.6f, 1.7f, 1.6f,},//distance_cashiers   
    };

}
