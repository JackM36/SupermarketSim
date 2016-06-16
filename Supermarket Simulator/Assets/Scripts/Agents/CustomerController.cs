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

    [HideInInspector]
    public float[] preferences;
    [HideInInspector]
    public float[] willingnessToPay;
    [HideInInspector]
    public bool[] shoppingList;
    [HideInInspector]
    public ProductCustomerInfo[] productsKnowledge;

    List<Transform> aisleEntryPoints;
    GameManager gameManager;
    ProductsManager productsManager;
    DecisionTree decisionTree;

    float bored = 0;
    bool finished = false;

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
    }

    void FixedUpdate()
    {
        //base.FixedUpdate();
        decisionTree.execute(this); // TO DO: break it more

        if(!finished)
        {
            seeProductsOnShelves();
            // TO DO: add closest staff function, when no known products are available
        }
    }

    override public void move()
    {
        // get position of current waypoint and final target
        Vector3 targetPos = new Vector3(path[currentWaypoint].x, transform.position.y, path[currentWaypoint].z);

        // set targets, and enable steering if is it disabled
        steering.setTargets(targetPos, finalTarget.position);
        if (!steering.enabled)
        {
            steering.enabled = true;
        }

        // if final target is a shelve standing point, use arrive, else use just seek
        if (finalTarget.tag == "StandingPoint")
        {
            steering.steeringBehaviours[0].enabled = false;
            steering.steeringBehaviours[1].enabled = true;
        }
        else
        {
            steering.steeringBehaviours[0].enabled = true;
            steering.steeringBehaviours[1].enabled = false;
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

        bool visible = Physics.Linecast(transform.position, targetPos);
        if (distance < reachedTargetGraceRadius && angle > reachedTargetAngle && !visible)
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

        base.onTarget();
    }

    override public void getNewTarget()
    {
        if (finished)
        {
            setTarget(gameManager.exit.transform, false);
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
            //finalTarget = shelve.getAvailableStandingPoint();
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
            }
        }
    }

    void setTarget(Transform newtarget, bool stackPrevious = false)
    {
        if (stackPrevious)
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
                    if (finalTarget != null && finalTarget.tag != "StandingPoint" && finalTarget.parent.GetComponent<Shelve>() != shelve)
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
        // TO DO: Check if staff is busy

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

            // check if closestStaff is visible
            if (Physics.Linecast(transform.position, closestStaff.transform.position))
            {
                setTarget(closestStaff.transform, false); // TO DO: not exact position, go close to him
            }
            else
            {
                staffVisibility[index] = -1;
                count++;
            }
        }
    }

    public bool toPickUp(ProductCustomerInfo product)
    {
        if (product.onShelve != null && !product.inBasket)
        {
            // Calculate utility
            float price = getProductPrice(product.onShelve.GetComponent<Shelve>());
            float utility = product.getUtility(price, productsManager.weightPref, productsManager.weightToBuy, productsManager.weightHasDiscount, productsManager.weightPlacement, productsManager.weightPlanogram);
            float minUtility = 0.3f; // TEEEEEEEMP

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
        float prefRadians = pref * Mathf.PI;

        prefRadians = (3 + Mathf.Cos(prefRadians)) / 4;

        ShelveLevel[] levels = new ShelveLevel[3];
        float[] shelvePrices = productsManager.productCategories[productID].prices;
        int discount = productsManager.productCategories[productID].discount;

        if (prefRadians < Mathf.PI)
        {
            levels[0] = new ShelveLevel(shelvePrices[2] - ((shelvePrices[2]*discount)/100), prefRadians);
            levels[1] = new ShelveLevel(shelvePrices[1] - ((shelvePrices[1]*discount)/100), 1 - prefRadians);
            levels[2] = new ShelveLevel(shelvePrices[0] - ((shelvePrices[0]*discount)/100), 0);
        }
        else if (prefRadians > Mathf.PI && prefRadians < Mathf.PI*3)
        {
            levels[0] = new ShelveLevel(shelvePrices[1] - ((shelvePrices[1]*discount)/100), prefRadians);
            levels[1] = new ShelveLevel(shelvePrices[0] - ((shelvePrices[0]*discount)/100), 1 - prefRadians);
            levels[2] = new ShelveLevel(shelvePrices[2] - ((shelvePrices[2]*discount)/100), 0);
        }
        else if (prefRadians > Mathf.PI*3 && prefRadians < Mathf.PI*4)
        {
            levels[0] = new ShelveLevel(shelvePrices[0] - ((shelvePrices[0]*discount)/100), prefRadians);
            levels[1] = new ShelveLevel(shelvePrices[1] - ((shelvePrices[1]*discount)/100), 1 - prefRadians);
            levels[2] = new ShelveLevel(shelvePrices[2] - ((shelvePrices[2]*discount)/100), 0);
        }

        Array.Sort<ShelveLevel>(levels, (x,y) => x.shelveLevelPref.CompareTo(y.shelveLevelPref));
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
        isBusy = true;
        disableSteeringAvoidance();

        yield return new WaitForSeconds(3);

        // Get Shelve and the product ID
        Shelve shelve = shelveTransform.GetComponent<Shelve>();
        int productID = shelve.productCategoryID;

        // Get the product ID of all neighbouring shelves, and update their placement boost
        for (int i = 0; i < shelve.neighbourShelves.Count; i++)
        {
            int neighbourProductID = shelve.neighbourShelves[i].productCategoryID;
            // productsKnowledge[neighbourProductID] TO DO: ADD PLACEMENT BOOST
        }

        // Pickup the product and pay
        productsKnowledge[productID].inBasket = true;
        float price = getProductPrice(productsKnowledge[productID].onShelve.GetComponent<Shelve>());
        budget -= price;
        gameManager.profit += price;

        //Debug.Log(name + ": Picked up " + productsManager.productCategories[productID].categoryName + " for " + price);

        isBusy = false;
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
            Destroy(gameObject);
        }
    }
}