﻿using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour 
{
    public GameObject entrance;
    public GameObject exit;

    [HideInInspector]
    public float profit = 0;
    [HideInInspector]
    public mode gameMode;

    void Awake()
    {
        gameMode = mode.edit;
    }

    public enum mode
    {
        play,
        edit
    }
}
