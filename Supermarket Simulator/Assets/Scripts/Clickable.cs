using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Clickable : MonoBehaviour {

    string clickedShelf;
    int clicked = 0;
    GameObject[] allShelves;

    void OnMouseDown()
    {
       GameObject[] allShelves = GameObject.FindGameObjectsWithTag("Shelve");
        //make all shelfs white
        for (int i =0; i<allShelves.Length; i++)
        {
            allShelves[i].GetComponentInChildren<MeshRenderer>().material.color = Color.white;
        }
        //make shelf red
      gameObject.GetComponentInChildren<MeshRenderer>().material.color = Color.red ;
    
      //later
        clickedShelf = gameObject.name;
        
        clicked = 1;
       
    }

    public void OkButton()
    {

    }
}
