using UnityEngine;
using System.Collections;

public class SelectorBehave : MonoBehaviour {

    public string[] productNames;
    private int indexOfProduct = 0;
    //for looping between products
    public int IndexOfProduct
    {
        get
        {
            return indexOfProduct;
        }
        set
        {
            if (value < 0)
                indexOfProduct = productNames.Length - 1;
            else if (value > productNames.Length - 1)
            {
                indexOfProduct = 0;
            }
            else
                indexOfProduct = value;
        }
    }
}
