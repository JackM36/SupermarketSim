﻿using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainUI : MonoBehaviour 
{
    [Header("UI Elements")]
    public Button startButton;
    public Button stopButton;
    public Button fasterButton;
    public Button slowerButton;

    [Header("Time")]
    public float maxTimeScale = 3f;
    public float minTimeScale = 0.5f;
    public float timeScaleStep = 0.5f;

    [Header("Agents")]
    public AgentSpawner spawner;

    public void startSimulation()
    {
        // enable/disable appropriate UI elements
        startButton.interactable = false;
        stopButton.interactable = true;
        fasterButton.interactable = true;
        slowerButton.interactable = true;

        // start spawning agents
        spawner.spawnAgents();
    }

    public void stopSimulation()
    {
        // enable/disable appropriate UI elements
        startButton.interactable = true;
        stopButton.interactable = false;
        fasterButton.interactable = false;
        slowerButton.interactable = false;

        Destroy(GameObject.Find("Customers"));
    }

    public void goFaster()
    {
        Time.timeScale += timeScaleStep;

        if (Time.timeScale >= maxTimeScale)
        {
            Time.timeScale = maxTimeScale;
            fasterButton.interactable = false;
        }
        else
        {
            fasterButton.interactable = true;
        }

        slowerButton.interactable = true;
    }
    public void goSlower()
    {
        Time.timeScale -= timeScaleStep;

        if (Time.timeScale <= minTimeScale)
        {
            Time.timeScale = minTimeScale;
            slowerButton.interactable = false;
        }
        else
        {
            slowerButton.interactable = true;
        }

        fasterButton.interactable = true;
    }
}
