using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class MainUI : MonoBehaviour 
{
    [Header("UI Elements")]
    public Button startButton;
    public Button stopButton;
    public Button fasterButton;
    public Button slowerButton;

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

        // restart scene
        Application.LoadLevel (Application.loadedLevelName);
    }
}
