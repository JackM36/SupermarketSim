using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CustomerActionsUI : MonoBehaviour 
{
    public CustomerController customer;

    [Header("UI Elements")]
    public Image actionImg;

    [Header("Actions Icons")]
    public Sprite emptyIcon;
    public Sprite lookingOnShelveIcon;
    public Sprite talkingIcon;
    public Sprite goingToCashierIcon;

    GameManager gameManager;

    void Awake()
    {
        // get components
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

	void Update () 
    {
        switch (customer.action)
        {
            case CustomerController.CustomerAction.lookingOnShelf:
                actionImg.sprite = lookingOnShelveIcon;
                break;
            case CustomerController.CustomerAction.lookingOnStaff:
                actionImg.sprite = talkingIcon;
                break;
            case CustomerController.CustomerAction.goingToCashier:
                actionImg.sprite = goingToCashierIcon;
                break;
            case CustomerController.CustomerAction.none:
                actionImg.sprite = emptyIcon;
                break;
        }

        faceCamera();
	}

    void faceCamera()
    {
        Camera cam = gameManager.currentCamera;
        transform.LookAt(transform.position + cam.transform.rotation * Vector3.forward, cam.transform.rotation * Vector3.up);
    }
}
