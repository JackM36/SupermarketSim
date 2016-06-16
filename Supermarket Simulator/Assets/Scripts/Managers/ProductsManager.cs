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

    //TODO fix position and number of products in array
    public float[,] productPosition = new float[10, 10] {
        { 1f, 0.8f, 0.6f, 0.5f, 0.1f, 0.2f, 0.5f, 0.5f, 0.1f, 0.2f },//meat
        { 0.8f, 1f, 0.5f, 0.4f, 0.2f, 0.2f, 0.3f, 0.2f, 0.1f, 0.1f },//fruitsvegetables
        { 0.6f, 0.5f, 1f, 0.6f, 0.3f, 0.2f, 0.2f, 0.2f, 0.1f, 0.2f },//dairy
        { 0.5f, 0.4f, 0.6f, 1f, 0.8f, 0.2f, 0.3f, 0.3f, 0.1f, 0.1f },//bakery
        { 0.1f, 0.2f, 0.3f, 0.8f, 1f, 0.5f, 0.3f, 0.5f, 0.1f, 0.2f },//candy
        { 0.2f, 0.2f, 0.2f, 0.2f, 0.5f, 1f, 0.7f, 0.8f, 0.1f, 0.2f },//chips
        { 0.5f, 0.3f, 0.2f, 0.3f, 0.3f, 0.7f, 1f, 0.8f, 0.5f, 0.4f },//alcohol
        { 0.5f, 0.2f, 0.2f, 0.3f, 0.5f, 0.8f, 0.8f, 1f, 0.3f, 0.4f },//softdrinks
        { 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.1f, 0.5f, 0.3f, 1f, 0.7f },//cleaning
        { 0.1f, 0.1f, 0.2f, 0.1f, 0.2f, 0.2f, 0.4f, 0.4f, 0.7f, 1f },//paper
    };

    //TODO fix position and number of products in array
    public float[,] planogram = new float[3, 10] {
        { 0.8f, 1f, 0.6f, 0.8f, 0.2f, 0.2f, 0.1f, 0.1f, 0.1f, 0.1f },//distance_entrance
        { 0.6f, 0.3f, 0.7f, 0.6f, 0.5f, 0.5f, 0.4f, 0.4f, 0.1f, 0.2f },//distance_middle
        { 0.2f, 0.2f, 0.3f, 0.8f, 0.8f, 0.7f, 0.6f, 0.6f, 0.7f, 0.7f },//distance_cashiers   
    };

}
