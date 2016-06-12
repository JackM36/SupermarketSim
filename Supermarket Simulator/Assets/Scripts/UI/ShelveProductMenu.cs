using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShelveProductMenu : MonoBehaviour 
{
    [Header("UI Elements")]
    public Image productBanner;
    public Text productCategoryNameTxt;
    public Dropdown eyeLevelPriceDropdown;

    [Header("Empty Shelve Info")]
    public string emptyCategoryName = "Empty";
    public Sprite emptyCategoryBannerImg;

    [Header("Materials")]
    public Material selectedShelveMat;
    public Material nonEmptyShelveMat;
    Material orignalShelveMat;

    ProductCategory[] productCategories;
    GameObject shelveObj;
    Shelve shelve;
    ProductCategory selectedProduct;
    float[] selectedPrices = new float[3];
    int selectedProductID;
    int selectedPriceID;

    public bool enabled
    {
        get
        {
            return this.GetComponent<Canvas>().enabled;
        }
        set
        {
            this.GetComponent<Canvas>().enabled = value;
        }
            
    }

    public enum NavigationDirection
    {
        next,
        previous
    }

    public enum shelveLevel
    {
        eyeLevel,
        handsLevel,
        feetLevel
    }

    void Awake()
    {
        // Get components
        productCategories = GameObject.Find("ProductsManager").GetComponent<ProductsManager>().productCategories;

        // Initializations
        clearShelve();
    }

    void Update()
    {
        if (!enabled)
        {
            getClickedShelve();
        }
    }

    public void nextProduct(string navDirStr)
    {
        NavigationDirection navDir = new NavigationDirection();

        // Convert string command to enum
        try
        {
            navDir = (NavigationDirection)System.Enum.Parse(typeof(NavigationDirection), navDirStr);
        }
        catch (System.Exception)
        {
            Debug.LogErrorFormat("nextProduct(string navDir): Can't convert {0} to enum, please check the spell. (Check button OnClick() parameter)", navDirStr);
        }

        // calculate the next selectedID based on if the NEXT or PREVIOUS button was pressed
        if (navDir == NavigationDirection.next)
        {
            selectedProductID = mod((selectedProductID + 1), productCategories.Length);
        }
        else
        {
            selectedProductID = mod((selectedProductID - 1), productCategories.Length);
        }

        selectedProduct = productCategories[selectedProductID];
        updateUI();
    }

    public void nextPriceCategory(string levelStr)
    {
        // STRUCTURE OF THIS FUNCTION SHOULD CHANGE! IT IS TEMPORARY

        shelveLevel level = new shelveLevel();

        // Convert string command to enum
        try
        {
            level = (shelveLevel)System.Enum.Parse(typeof(shelveLevel), levelStr);
        }
        catch (System.Exception)
        {
            Debug.LogErrorFormat("nextProduct(string navDir): Can't convert {0} to enum, please check the spell. (Check button OnClick() parameter)", levelStr);
        }

        // Get level
        if (level == shelveLevel.eyeLevel)
        {
            selectedPriceID = 0;
        }
        else if (level == shelveLevel.handsLevel)
        {
            selectedPriceID = 1;
        }
        else if (level == shelveLevel.feetLevel)
        {
            selectedPriceID = 2;
        }

        // get price of product on that level
        if (eyeLevelPriceDropdown.value == 0)
        {
            // if premium
            selectedPrices[selectedPriceID] = selectedProduct.prices[0];
        }
        else if(eyeLevelPriceDropdown.value == 1)
        {
            // if mid price
            selectedPrices[selectedPriceID] = selectedProduct.prices[1];
        }
        else if(eyeLevelPriceDropdown.value == 2)
        {
            // if cheap
            selectedPrices[selectedPriceID] = selectedProduct.prices[2];
        }
    }

    void getClickedShelve()
    {
        // Check if mouse button was clicked
        if(Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // cast a ray and check if it hits a shelve
            if(Physics.Raycast(ray, out hit))
            {
                if (hit.transform.tag == "Shelve")
                {
                    // Get clicked shelve
                    shelveObj = hit.transform.gameObject;
                    shelve = shelveObj.GetComponent<Shelve>();

                    // get shelve product
                    if (shelve.productCategoryID == -1)
                    {
                        selectedProduct = null;
                        selectedProductID = 0;
                    }
                    else
                    {
                        selectedProductID = shelve.productCategoryID;
                        selectedProduct = productCategories[selectedProductID];
                    }

                    // update gui with shelve's product
                    updateUI();

                    // show shelve UI
                    enabled = true;
                    highlightShelve(true);
                }
            }
        }
    }

    public void clearShelve()
    {
        selectedProduct = null;
        selectedProductID = 0;

        updateUI();
    }

    public void updateShelve()
    {
        // Save the selected product on the shelve
        if (selectedProduct == null)
        {
            shelve.productCategoryID = -1;
            shelve.productCategoryName = "";    
        }
        else
        {
            shelve.productCategoryID = selectedProductID;
            shelve.productCategoryName = selectedProduct.categoryName;

            shelve.shelveLevelPrices = selectedPrices;
        }
            
        HideShelveMenu();
    }

    public void HideShelveMenu()
    {
        // hide shelve UI
        enabled = false;
        highlightShelve(false);
    }

    void updateUI()
    {
        // check if it should update for a certain product, or empty
        if (selectedProduct == null)
        {
            productCategoryNameTxt.text = emptyCategoryName;
            productBanner.sprite = emptyCategoryBannerImg;
        }
        else
        {
            productCategoryNameTxt.text = selectedProduct.categoryName;
            productBanner.sprite = selectedProduct.bannerImg;
        }
    }

    void highlightShelve(bool selected)
    {
        // highlight (or remove highlight) for the selected shelve
        if (selected)
        {
            // save the original shelve material first
            if (orignalShelveMat == null)
            {
                orignalShelveMat = shelveObj.GetComponentInChildren<MeshRenderer>().material;
            }

            // apply the selected material
            shelveObj.GetComponentInChildren<MeshRenderer>().material = selectedShelveMat;
        }
        else
        {
            // Remove selected highlight, but check if the shelve is empty or not
            // If not empty, apply the non-empty material instead of the original
            if (shelve.productCategoryID != -1)
            {
                shelveObj.GetComponentInChildren<MeshRenderer>().material = nonEmptyShelveMat;
            }
            else
            {
                shelveObj.GetComponentInChildren<MeshRenderer>().material = orignalShelveMat;
            }
        }
    }

    int mod(int a, int b)
    {
        return (a % b + b) % b;
    }
}
