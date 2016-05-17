using UnityEngine;
using System.Collections;

public class CustomerBehavior : MonoBehaviour {

   	// Use this for initialization
	void Start () {

    }
	
	// Update is called once per frame
	void Update () {
	    
	}

    // determine shelve preferences
    double[] shelvePreference(double prefVal)
    {
        //prefVal = preference * budgetPreference
        double[] shelvePref = new double[3];

        if (prefVal >= 0.0f && prefVal < 0.33f)
        {
            shelvePref[0] = 0.1f;
            shelvePref[1] = 0.3f;
            shelvePref[2] = 0.6f;
        }
        else if (prefVal >= 0.33f && prefVal < 0.66f)
        {
            shelvePref[0] = 0.6f;
            shelvePref[1] = 0.3f;
            shelvePref[2] = 0.1f;
        }
        else
        {
            shelvePref[0] = 0.3f;
            shelvePref[1] = 0.6f;
            shelvePref[2] = 0.1f;
        }

        return shelvePref;
    }
}
