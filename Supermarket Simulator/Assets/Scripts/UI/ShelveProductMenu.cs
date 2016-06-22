using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class ShelveProductMenu : MonoBehaviour 
{
    [Header("UI Elements")]
    public Image productBanner;
    public Text productCategoryNameTxt;
    public Dropdown[] shelveLevelsPriceDropdowns;
    public Slider discountSlider;
    public Text discountvalueTxt;

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
    int[] selectedPricesIDs = new int[3];
    int selectedProductID;

    GameManager gameManager;
    ProductsManager productsManager;
    MainUI mainUI;

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

    public enum ShelveLevel
    {
        eyeLevel = 0,
        handsLevel = 1,
        feetLevel = 2
    }

    void Awake()
    {
        // Get components
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        productsManager = GameObject.Find("ProductsManager").GetComponent<ProductsManager>();
        mainUI = GameObject.Find("MainUI").GetComponent<MainUI>();
        productCategories = productsManager.productCategories;

        // Initializations
        clearShelve();
    }

    void Update()
    {
        if (gameManager.gameMode == GameManager.mode.edit && !mainUI.addingStaff && !enabled)
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
            if (selectedProduct != null)
            {
                selectedProductID = mod((selectedProductID + 1), productCategories.Length);
            }
            else
            {
                selectedProductID = 0;
            }
        }
        else
        {
            if (selectedProduct != null)
            {
                selectedProductID = mod((selectedProductID - 1), productCategories.Length);
            }
            else
            {
                selectedProductID = productCategories.Length-1;
            }
        }

        selectedProduct = productCategories[selectedProductID];
        updateUI();
    }

    public void nextEyePriceCategory()
    {
        int levelID = 0;

        // get price of product on that level
        if (shelveLevelsPriceDropdowns[levelID].value == 0)
        {
            // if premium
            selectedPricesIDs[levelID] = 0;
        }
        else if(shelveLevelsPriceDropdowns[levelID].value == 1)
        {
            // if mid price
            selectedPricesIDs[levelID] = 1;
        }
        else if(shelveLevelsPriceDropdowns[levelID].value == 2)
        {
            // if cheap
            selectedPricesIDs[levelID] = 2;
        }
    }

    public void nextHandsPriceCategory()
    {
        int levelID = 1;

        // get price of product on that level
        if (shelveLevelsPriceDropdowns[levelID].value == 0)
        {
            // if premium
            selectedPricesIDs[levelID] = 0;
        }
        else if(shelveLevelsPriceDropdowns[levelID].value == 1)
        {
            // if mid price
            selectedPricesIDs[levelID] = 1;
        }
        else if(shelveLevelsPriceDropdowns[levelID].value == 2)
        {
            // if cheap
            selectedPricesIDs[levelID] = 2;
        }
    }

    public void nextFeetCategory()
    {
        int levelID = 2;

        // get price of product on that level
        if (shelveLevelsPriceDropdowns[levelID].value == 0)
        {
            // if premium
            selectedPricesIDs[levelID] = 0;
        }
        else if(shelveLevelsPriceDropdowns[levelID].value == 1)
        {
            // if mid price
            selectedPricesIDs[levelID] = 1;
        }
        else if(shelveLevelsPriceDropdowns[levelID].value == 2)
        {
            // if cheap
            selectedPricesIDs[levelID] = 2;
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

    public void discountValueChanged()
    {
        discountvalueTxt.text = discountSlider.value + "%";
    }

    public void clearShelve()
    {
        selectedProduct = null;
        selectedProductID = 0;

        shelveLevelsPriceDropdowns[0].value = 0;
        shelveLevelsPriceDropdowns[1].value = 0;
        shelveLevelsPriceDropdowns[2].value = 0;

        discountSlider.value = discountSlider.minValue;

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

            //shelve.shelveLevelPricesIDs = new int[](selectedPricesIDs);
            Array.Copy(selectedPricesIDs, shelve.shelveLevelPricesIDs, selectedPricesIDs.Length);
            for (int i = 0; i < shelve.shelveLevelPrices.Length; i++)
            {
                shelve.shelveLevelPrices[i] = productsManager.productCategories[selectedProductID].prices[selectedPricesIDs[i]];
            }

            productsManager.productCategories[shelve.productCategoryID].discount = (int)discountSlider.value;

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

            shelveLevelsPriceDropdowns[0].value = 0;
            shelveLevelsPriceDropdowns[1].value = 0;
            shelveLevelsPriceDropdowns[2].value = 0;

            discountSlider.value = discountSlider.minValue;
        }
        else
        {
            productCategoryNameTxt.text = selectedProduct.categoryName;
            productBanner.sprite = selectedProduct.bannerImg;

            shelveLevelsPriceDropdowns[0].value = shelve.shelveLevelPricesIDs[0];
            shelveLevelsPriceDropdowns[1].value = shelve.shelveLevelPricesIDs[1];
            shelveLevelsPriceDropdowns[2].value = shelve.shelveLevelPricesIDs[2];

            discountSlider.value = productsManager.productCategories[selectedProductID].discount;
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
