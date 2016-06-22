using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CustomerController : AgentController
{
    [Header("Customer")]
    public float budget;
    public LayerMask staffLayer;

    [Header("Feelings")]
    public float maxBored = 10;

    [Header("Times")]
    public float lookOnShelveTime = 3;
    public float lookOnStaffTime = 3;

    [HideInInspector]
    public int customerID;
    [HideInInspector]
    public float[] preferences;
    [HideInInspector]
    public float[] willingnessToPay;
    [HideInInspector]
    public bool[] shoppingList;
    [HideInInspector]
    public ProductCustomerInfo[] productsKnowledge;
    [HideInInspector]
    public CustomerAction action;

    List<Transform> aisleEntryPoints;
    GameManager gameManager;
    ProductsManager productsManager;
    DecisionTree decisionTree;

    float bored = 0;
    bool finished = false;
    bool lookForStaff = false;
    int askForProduct = -1;

    struct ShelveLevel
    {
        public float shelveLevelPrice;
        public float shelveLevelPref;

        public ShelveLevel(float shelveLevelPrice, float shelveLevelPref)
        {
            this.shelveLevelPrice = shelveLevelPrice; // 0 = eye level, 1 = hands leve, 2 = feet level
            this.shelveLevelPref = shelveLevelPref;
        }
    }

    public enum CustomerAction
    {
        none,
        lookingOnShelf,
        lookingOnStaff,
        goingToCashier
    }

    void Awake()
    {
        base.Awake();

        // Get components
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        productsManager = GameObject.Find("ProductsManager").GetComponent<ProductsManager>();
        decisionTree = transform.Find("DecisionTree").GetComponent<DecisionTree>();
        aisleEntryPoints = getAisleEntryPoints();

        // Initializations
        stackedTargets = new Stack<Transform>();
        action = CustomerAction.none;
    }

    void FixedUpdate()
    {
        //base.FixedUpdate();
        decisionTree.execute(this); // TO DO: break it more

        if(!finished)
        {
            seeProductsOnShelves();
            if (lookForStaff && (finalTarget == null || (finalTarget.tag != "Staff" && finalTarget.tag != "StandingPoint")))
            {
                getClosestStaff();
            }
        }
    }

    override public void move()
    {
        //TEMP FIX
        transform.position = new Vector3(transform.position.x, 1.431488f, transform.position.z);

        // get position of current waypoint and final target
        Vector3 targetPos = new Vector3(path[currentWaypoint].x, transform.position.y, path[currentWaypoint].z);

        // set targets, and enable steering if is it disabled
        steering.setTargets(targetPos, finalTarget.position);
        if (!steering.enabled)
        {
            steering.enabled = true;
        }

        // if final target is a shelve standing point, use arrive, else use just seek
        if (currentWaypoint == path.Length-1 && finalTarget.tag == "StandingPoint" || finalTarget.tag == "Staff")
        {
            steering.steeringBehaviours[0].enabled = false;
            steering.steeringBehaviours[1].enabled = true;
        }
        else
        {
            steering.steeringBehaviours[0].enabled = true;
            steering.steeringBehaviours[1].enabled = false;
        }

        if (finalTarget.tag == "Staff")
        {
            StaffController staff = finalTarget.GetComponent<StaffController>();
            if (staff.isBusy)
            {
                getNewTarget();
            }
        }

        // If the unit is close enough to the currentWaypoint, register it as reached
        if (isTargetReached(targetPos))
        {
            // node is already reached, so remove penalty for using this node
            NavMeshPathManager.removeUsedNodePenalty(path[currentWaypoint]);

            // Check if agent reached its final target
            if (currentWaypoint == path.Length-1)
            {
                onTarget();
                return;
            }

            currentWaypoint++;
        }
    }

    bool isTargetReached(Vector3 targetPos)
    {
        float distance = Vector3.Distance(transform.position, targetPos);

        Vector3 dir = (targetPos - transform.position).normalized;
        float dot = Vector3.Dot(dir, transform.forward);
        float angle = Mathf.Acos(dot) * Mathf.Rad2Deg;

        if (distance < reachedTargetRadius)
        {
            return true;
        }

        bool visible = !Physics.Linecast(transform.position, targetPos);
        if (distance < reachedTargetGraceRadius && angle > reachedTargetAngle && visible)
        {
            return true;
        }
            
        return false;
    }

    override protected void onTarget()
    {
        // If final target is a shelve standing point, look on shelve
        if (finalTarget.tag == "StandingPoint" && finalTarget.parent.tag == "Shelve")
        {
            StartCoroutine(lookOnShelve(finalTarget.parent));
        }

        // If final target is a staff, look on staff
        if (finalTarget.tag == "Staff")
        {
            StaffController staff = finalTarget.GetComponent<StaffController>();
            if (!staff.isBusy)
            {
                StartCoroutine(lookOnStaff(finalTarget));
            }
        }

        base.onTarget();
    }

    override public void getNewTarget()
    {
        lookForStaff = false;

        if (finished)
        {
            setTarget(gameManager.exit.transform, false);
            action = CustomerAction.none;
            return;
        }

        if (stackedTargets.Count > 0)
        {
            setTarget(stackedTargets.Pop(), false);
            return;
        }

        Vector3 currentPos = transform.position;
        float minDist = float.MaxValue;
        int minDistIndex = -1;
        int desiredProducts = 0;

        // Go through the products on the list, and find the shelve that the agent should go to to get it
        for (int i = 0; i < productsKnowledge.Length; i++)
        {
            // count how many products the customer plans to buy yet
            if (productsKnowledge[i].toBuy && !productsKnowledge[i].inBasket && productsKnowledge[i].onShelve != null)
            {
                desiredProducts++;
            }

            // check if shelve for this product is known, and is not already in basket
            if(toPickUp(productsKnowledge[i]))
            {
                // Find the closest product/shelve from current position
                Vector3 shelvePos = productsKnowledge[i].onShelve.transform.position;
                float dist = Vector3.Distance(currentPos, shelvePos);

                if (dist < minDist)
                {
                    minDist = dist;
                    minDistIndex = i;
                }
            }
        }

        // If at least one product shelve is known, get a standing position and go there
        if (minDistIndex != -1)
        {
            Shelve shelve = productsKnowledge[minDistIndex].onShelve.GetComponent<Shelve>();
            setTarget(shelve.getAvailableStandingPoint(), false);
        }
        else
        {
            // Check if there were any desired products or if the customer is bored
            if (desiredProducts < 0 || bored >= maxBored)
            {
                // Get list of cashiers
                GameObject[] cashiers = GameObject.FindGameObjectsWithTag("Cashier");

                // Go to one cashier
                Transform target;
                Transform cashier = cashiers[UnityEngine.Random.Range(0, cashiers.Length)].transform;

                // Get all standing points
                foreach (Transform child in cashier)
                {
                    if (child.tag == "StandingPoint")
                    {
                        target = child;
                        setTarget(target, false);
                    }
                }

                finished = true;
                action = CustomerAction.goingToCashier;
            }
            else
            {
                Transform target;

                // Get a random aisle entry point as target
                do
                {
                    target = aisleEntryPoints[UnityEngine.Random.Range(0, aisleEntryPoints.Count)].transform;
                }
                while(finalTarget == target);

                setTarget(target, false);
                bored++; // make the customer more bored because of just wandering
                lookForStaff = true;
            }
        }
    }

    void setTarget(Transform newtarget, bool stackPrevious = false)
    {
        if (finalTarget != null && stackPrevious)
        {
            stackedTargets.Push(finalTarget);
        }

        finalTarget = newtarget;
        NavMeshPathManager.requestPath(transform.position, finalTarget.position, onPathRequestProcessed);
    }

    void seeProductsOnShelves()
    {
        Vector3 rightAngle = Quaternion.AngleAxis (perceptionSightAngle, transform.up) * transform.forward;
        Vector3 leftAngle = Quaternion.AngleAxis (-perceptionSightAngle, transform.up) * transform.forward;

        RaycastHit leftHit;
        RaycastHit rightHit;

        // Check left raycast for shelve hit
        bool shelveLeftHit = Physics.Raycast(transform.position, leftAngle, out leftHit, perceptionSightDistance);
        if (shelveLeftHit)
        {
            checkShelveHit(leftHit);
        }

        // Check right raycast for shelve hit
        bool shelveRightHit = Physics.Raycast(transform.position, rightAngle, out rightHit, perceptionSightDistance);
        if (shelveRightHit)
        {
            checkShelveHit(rightHit);
        }
    }

    void checkShelveHit(RaycastHit hit)
    {
        if (hit.transform.tag == "Shelve")
        {
            Shelve shelve = hit.transform.GetComponent<Shelve>();

            // Check if shelve has a product first
            if (shelve.productCategoryID != -1)
            {
                // Get onShelve info
                GameObject onShelve = hit.transform.gameObject;

                // Add new knowledge to list
                productsKnowledge[shelve.productCategoryID].onShelve = onShelve;

                //Debug.Log(name + ": Saw " + productsManager.productCategories[shelve.productCategoryID].categoryName + " with utility " + productsKnowledge[shelve.productCategoryID].getUtility(getProductPrice(productsKnowledge[shelve.productCategoryID].onShelve.GetComponent<Shelve>()), productsManager.weightPref, productsManager.weightToBuy, productsManager.weightHasDiscount, productsManager.weightPlacement, productsManager.weightPlanogram));

                // Check if this shelve has a product the customer was going to pick up
                if (toPickUp(productsKnowledge[shelve.productCategoryID]))
                {
                    // if this shelve was already the target of the customer, just do nothing
                    //if (finalTarget != null && finalTarget.tag != "StandingPoint" && finalTarget.parent.GetComponent<Shelve>() != null && finalTarget.parent.GetComponent<Shelve>() != shelve)
                    //if (finalTarget != null && finalTarget.parent.GetComponent<Shelve>() != null && finalTarget.parent.GetComponent<Shelve>() != shelve)
                    if (finalTarget != null && finalTarget.tag == "StandingPoint" && finalTarget.parent.GetComponent<Shelve>() != null && finalTarget.parent.GetComponent<Shelve>() == shelve)
                    {
                        return;
                    }
                    else
                    {
                        setTarget(shelve.getAvailableStandingPoint(), true);
                    }
                }
            }

            // draw ray line
            Debug.DrawLine(transform.position, hit.point, Color.red);
        }
    }

    void getClosestStaff()
    {
        // Check if there is a product to ask about
        float maxPref = 0;
        int productID = -1;

        for (int i = 0; i < productsKnowledge.Length; i++)
        {
            if (productsKnowledge[i].isAvailable && productsKnowledge[i].onShelve == null && !productsKnowledge[i].inBasket)
            {
                float pref = productsKnowledge[i].pref;
                if(pref > maxPref)
                {
                    maxPref = pref;
                    productID = i;
                }
            }
        }

        // if there is a product to ask about, find the closest staff
        if (productID != -1)
        {
            askForProduct = productID;

            // find staff agents within a radius
            Collider[] colliders = Physics.OverlapSphere(transform.position, perceptionSightDistance, staffLayer);
            float minDistance = perceptionSightDistance;

            // visibility table,-1: not visible, 0: unknown visibility, 1: visible
            int[] staffVisibility = new int[colliders.Length];
            GameObject closestStaff;

            if (colliders.Length > 0)
            {
                closestStaff = colliders[0].gameObject;
            }
            else
            {
                return;
            }

            int index = 0;

            // count how many staff agents are invisible
            int count = 0;

            while (count < colliders.Length)
            {
                // find closest staff
                for (int i = 1; i < colliders.Length; i++)
                {
                    if (staffVisibility[i] > -1)
                    {
                        // find distance from customer to each staff
                        float distance = Vector3.Distance(transform.position, colliders[i].transform.position);
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closestStaff = colliders[i].gameObject;
                            index = i;
                        }
                    }
                }

                // check if closestStaff is visible and not busy
                if (Physics.Linecast(transform.position, closestStaff.transform.position) && !closestStaff.GetComponent<StaffController>().isBusy)
                {
                    setTarget(closestStaff.transform, false);
                    return;
                }
                else
                {
                    staffVisibility[index] = -1;
                    count++;
                }
            }
        }
    }

    public bool toPickUp(ProductCustomerInfo product)
    {
        if (product.isAvailable && product.onShelve != null && !product.inBasket)
        {
            // Calculate utility
            float price = getProductPrice(product.onShelve.GetComponent<Shelve>());
            float utility = product.getUtility(price, productsManager.weightPref, productsManager.weightToBuy, productsManager.weightHasDiscount, productsManager.weightPlacement, productsManager.weightPlanogram);
            float minUtility = 0.4f; // TEEEEEEEMP

            // Decide if this product should be picked up
            if (utility >= minUtility && price != -1)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
    }

    float getProductPrice(Shelve shelve)
    {
        int productID = shelve.productCategoryID;
        float pref = productsKnowledge[productID].pref;
        float prefDegrees = pref * 720;
        float prefRadians = prefDegrees * (Mathf.PI/180);

        //prefRadians = (3 + Mathf.Cos(prefRadians)) / 4;

        //float[] shelvePrices = productsManager.productCategories[productID].prices;
        float[] shelvePrices = shelve.shelveLevelPrices;
        float[] shelvePricesSorted = new float[3];
        Array.Copy(shelvePrices, shelvePricesSorted, shelvePrices.Length);
        Array.Sort<float>(shelvePricesSorted, (x,y) => x.CompareTo(y));

        ShelveLevel[] levels = new ShelveLevel[3];

        float[] shelveLevelsBoosts = new float[3];
        shelveLevelsBoosts[0] = productsManager.boostEyeLevelShelve;
        shelveLevelsBoosts[1] = productsManager.boostHandsLevelShelve;
        shelveLevelsBoosts[2] = productsManager.boostFeetLevelShelve ;

        int discount = productsManager.productCategories[productID].discount;

        // TODO add boosts
        if (prefRadians < Mathf.PI)
        {
            levels[0] = new ShelveLevel(shelvePricesSorted[0], pref);
            levels[1] = new ShelveLevel(shelvePricesSorted[1], 1 - pref);
            levels[2] = new ShelveLevel(shelvePricesSorted[2], 0);
        }
        else if (prefRadians > Mathf.PI && prefRadians < Mathf.PI*3)
        {
            levels[0] = new ShelveLevel(shelvePricesSorted[1], pref);
            levels[1] = new ShelveLevel(shelvePricesSorted[2], 1 - pref);
            levels[2] = new ShelveLevel(shelvePricesSorted[0], 0);
        }
        else if (prefRadians > Mathf.PI*3 && prefRadians < Mathf.PI*4)
        {
            levels[0] = new ShelveLevel(shelvePricesSorted[2], pref);
            levels[1] = new ShelveLevel(shelvePricesSorted[1], 1 - pref);
            levels[2] = new ShelveLevel(shelvePricesSorted[0], 0);
        }

        // add discount and boosts
        for (int i = 0; i < levels.Length; i++)
        {
            levels[i].shelveLevelPrice -= (levels[i].shelveLevelPrice * discount) / 100;

            for (int j = 0; j < levels.Length; j++)
            {
                if (levels[i].shelveLevelPrice == shelvePrices[j])
                {
                    levels[i].shelveLevelPref += shelveLevelsBoosts[j];
                    break;
                }
            }
        }

        Array.Sort<ShelveLevel>(levels, (x,y) => -x.shelveLevelPref.CompareTo(y.shelveLevelPref));

        for (int i = 0; i < levels.Length; i++)
        {
            if (levels[i].shelveLevelPrice <= budget)
            {
                return levels[i].shelveLevelPrice;
            }
        }

        // Not enough budget to get the product from any shelve level
        return -1;
    }

    IEnumerator lookOnShelve(Transform shelveTransform)
    {
        // Check once again before product is picked up. (budget could be spent on the way to this shelve)
        if (!toPickUp(productsKnowledge[shelveTransform.GetComponent<Shelve>().productCategoryID]))
        {
            isBusy = false;
            return true;
        }

        isBusy = true;
        action = CustomerAction.lookingOnShelf;
        disableSteeringAvoidance();

        yield return new WaitForSeconds(lookOnShelveTime);

        // Get Shelve and the product ID
        Shelve shelve = shelveTransform.GetComponent<Shelve>();
        int productID = shelve.productCategoryID;

        // Get the product ID of all neighbouring shelves, and update their placement boost
        for (int i = 0; i < shelve.neighbourShelves.Count; i++)
        {
            int neighbourProductID = shelve.neighbourShelves[i].productCategoryID;

            if (neighbourProductID != -1)
            {
                shelve.neighbourShelves[i].toBoostPlacement = true;
                shelve.neighbourShelves[i].boostedBy = shelve.productCategoryID;
            }
        }

        // Pickup the product and pay
        productsKnowledge[productID].inBasket = true;
        float price = getProductPrice(productsKnowledge[productID].onShelve.GetComponent<Shelve>());
        budget -= price;
        gameManager.profit += price;

        //Debug.Log(name + ": Picked up " + productsManager.productCategories[productID].categoryName + " for " + price);

        isBusy = false;
        action = CustomerAction.none;
        enableSteeringAvoidance();
    }

    IEnumerator lookOnStaff(Transform staffTransform)
    {
        StaffController staff = staffTransform.GetComponent<StaffController>();
        staff.isBusy = true;
        isBusy = true;
        action = CustomerAction.lookingOnStaff;
        disableSteeringAvoidance();

        yield return new WaitForSeconds(lookOnStaffTime);

        GameObject shelveObj = staff.getClosestShelve(askForProduct);

        if (shelveObj != null)
        {
            // get shelve
            Shelve shelve = shelveObj.GetComponent<Shelve>();

            // Add it to customer's knowledge
            productsKnowledge[askForProduct].onShelve = shelve.gameObject;

            setTarget(shelve.getAvailableStandingPoint(), false);
            bool visible = !Physics.Linecast(transform.position, shelveObj.transform.position);
            if (!visible)
            {
                // get visible aisle point
                List<int> visibleAislePoints = new List<int>();

                for (int i = 0; i < aisleEntryPoints.Count; i++)
                {
                    bool visibleAislePoint = !Physics.Linecast(transform.position, aisleEntryPoints[i].position);
                    if (visibleAislePoint)
                    {
                        visibleAislePoints.Add(i);
                    }
                }

                // get a random visible aisle point
                int aislePointIndex = UnityEngine.Random.Range(0, visibleAislePoints.Count);
                Transform aislePoint = aisleEntryPoints[aislePointIndex];

                setTarget(aislePoint, true);
            }
        }
        else
        {
            productsKnowledge[askForProduct].isAvailable = false;
        }
            
        staff.isBusy = false;

        isBusy = false;
        action = CustomerAction.none;
        enableSteeringAvoidance();
    }

    void enableSteeringAvoidance()
    {
        steering.steeringBehaviours[2].enabled = true;
        steering.steeringBehaviours[3].enabled = true;
    }

    void disableSteeringAvoidance()
    {
        steering.steeringBehaviours[2].enabled = false;
        steering.steeringBehaviours[3].enabled = false;
    }

    List<Transform> getAisleEntryPoints()
    {
        List<Transform> aisleEntryPoints = new List<Transform>();
        Transform placeholder = GameObject.Find("AisleEntryPoints").transform;

        foreach (Transform child in placeholder)
        {
            aisleEntryPoints.Add(child);
        }

        return aisleEntryPoints;
    }

    void OnTriggerEnter(Collider other)
    {
        if (finished && other.transform.tag == "Exit")
        {
            gameManager.removeCustomer(customerID);
            Destroy(gameObject);
        }
    }
}