using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Selector : MonoBehaviour {

    public Text TargetText;
    public SelectorBehave Select;

    public void ButtonHandle(int direction)
    {
        if (direction == 0)
        {
            Select.IndexOfProduct--;
        }
        else
        {
            Select.IndexOfProduct++;
        }
        TargetText.text = Select.productNames[Select.IndexOfProduct];
    }
   
}
