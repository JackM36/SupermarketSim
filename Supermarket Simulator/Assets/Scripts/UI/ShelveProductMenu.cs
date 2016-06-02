using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ShelveProductMenu : MonoBehaviour 
{
    [Header("UI Elements")]
    public Image productBanner;
    public Text productCategoryNameTxt;
    public Text eyeLevelPriceCategoryTxt;
    public Text handsLevelPriceCategoryTxt;
    public Text feetLevelPriceCategoryTxt;

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
    int selectedProductID;

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

    public void nextProduct()
    {
        selectedProductID = mod((selectedProductID + 1), productCategories.Length);
        selectedProduct = productCategories[selectedProductID];

        updateUI();
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

    public void previousProduct()
    {
        selectedProductID = mod((selectedProductID - 1), productCategories.Length);
        selectedProduct = productCategories[selectedProductID];

        updateUI();
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
