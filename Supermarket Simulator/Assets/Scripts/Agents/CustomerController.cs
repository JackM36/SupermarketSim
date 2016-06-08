using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CustomerController : AgentController
{
    [Header("Customer")]
    public float budget;

    [HideInInspector]
    public float[] preferences;
    [HideInInspector]
    public float[] willingnessToPay;
    [HideInInspector]
    public bool[] shoppingList;
    [HideInInspector]
    public ProductCustomerInfo[] productsKnowledge;

    List<Transform> aisleEntryPoints;
    ProductsManager productsManager;

    void Awake()
    {
        base.Awake();

        // Get components
        productsManager = GameObject.Find("ProductsManager").GetComponent<ProductsManager>();
        aisleEntryPoints = getAisleEntryPoints();

        // Initializations
        stackedTargets = new Stack<Transform>();
    }

    void FixedUpdate()
    {
        base.FixedUpdate();
        seeProductsOnShelves();
    }

    override protected void move()
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
        if (Vector3.Distance(transform.position, targetPos) < reachedTargetRadius)
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

    override protected void onTarget()
    {
        // If final target is a shelve standing point, look on shelve
        if (finalTarget.tag == "StandingPoint")
        {
            StartCoroutine(lookOnShelve(finalTarget.parent));
        }

        base.onTarget();
    }

    override protected void getNewTarget()
    {
        if (stackedTargets.Count > 0)
        {
            setTarget(stackedTargets.Pop(), false);
            return;
        }

        Vector3 currentPos = transform.position;
        float minDist = float.MaxValue;
        int minDistIndex = -1;

        // Go through the products on the list, and find the shelve that the agent should go to to get it
        for (int i = 0; i < productsKnowledge.Length; i++)
        {
            // check if shelve for this product is known, and is not already in basket
            if(productsKnowledge[i].toPickUp())
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
            Transform target;

            // Get a random aisle entry point as target
            do
            {
                target = aisleEntryPoints[Random.Range(0, aisleEntryPoints.Count)].transform;
            }
            while(finalTarget == target);

            //finalTarget = target;
            setTarget(target, false);
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

                // Check if this shelve has a product the customer was going to pick up
                if (productsKnowledge[shelve.productCategoryID].toPickUp())
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

    IEnumerator lookOnShelve(Transform shelve)
    {
        isBusy = true;
        disableSteeringAvoidance();

        yield return new WaitForSeconds(3);

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
}
