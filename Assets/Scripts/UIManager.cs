﻿using System.Collections.Generic;
using System.IO;
using Tayx.Graphy;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text agentsLimit;
    public Text newAgents;
    public Text count;
    public Text maxSpeedT;
    private List<KeyValuePair<float,double>> fps;
    private bool start = false;

    private void Start()
    {
        var settings = GameObject.Find("Settings").GetComponent<Settings>();
        agentsLimit.text = "Agents limit: " + settings.agentsLimit;
        newAgents.text = "New agents: " + settings.newAgents;
        maxSpeedT.text = "Max speed: " + settings.maxAgentSpeed;
        fps = new List<KeyValuePair<float, double>>();
    }

    private void Update()
    {
        if(start)
            fps.Add(new KeyValuePair<float, double>(Time.time, AStarSystem.elapsed));
            //fps.Add(new KeyValuePair<float, float>(Time.time, GraphyManager.Instance.CurrentFPS));
        count.text = "Agents count: " + SpawnAgentSystem.agents.Length;
    }

    public void saveData()
    {
        
        string path = $"{Application.persistentDataPath}/fpsdataECS {SpawnAgentSystem.newAgents}_{SpawnAgentSystem.limit}.txt";
        
        StreamWriter writer = new StreamWriter(path, true);

        foreach (var pair in fps)
        {
            writer.WriteLine($"{pair.Key} {pair.Value}");
        }
        writer.Close();
    }

    public void incrementLimit()
    {
        if(World.Active.GetExistingManager<SpawnAgentSystem>().Enabled)
        {
            if (SpawnAgentSystem.limit > SpawnAgentSystem.maxLimit)
                return;
            SpawnAgentSystem.limit += 5000;
            agentsLimit.text = "Agents limit: " + SpawnAgentSystem.limit;
        }
        else
        {
            if (Bootstrap.Settings.agentsLimit > 19900)
                return;
            Bootstrap.Settings.agentsLimit += 5000;
            agentsLimit.text = "Agents limit: " + Bootstrap.Settings.agentsLimit;
        }
    }
    
    public void decrementLimit()
    {
        if (World.Active.GetExistingManager<SpawnAgentSystem>().Enabled)
        {
            if (Bootstrap.Settings.agentsLimit == 0)
                return;
            SpawnAgentSystem.limit -= 5000;
            agentsLimit.text = "Agents limit: " + SpawnAgentSystem.limit;
        }
        else
        {
            if (Bootstrap.Settings.agentsLimit == 0)
                return;
            Bootstrap.Settings.agentsLimit -= 5000;
            agentsLimit.text = "Agents limit: " + Bootstrap.Settings.agentsLimit;
        }
    }
    
    public void incrementNewAgents()
    {
        if (World.Active.GetExistingManager<SpawnAgentSystem>().Enabled)
        {
            SpawnAgentSystem.newAgents += 100;
            newAgents.text = "New agents: " + SpawnAgentSystem.newAgents;
        }
        else
        {
            Bootstrap.Settings.newAgents += 100;
            newAgents.text = "New agents: " + Bootstrap.Settings.newAgents;
        }
    }
    
    public void decrementNewAgents()
    {
        if (World.Active.GetExistingManager<SpawnAgentSystem>().Enabled)
        {
            SpawnAgentSystem.newAgents -= 100;
            newAgents.text = "New agents: " + SpawnAgentSystem.newAgents;
        }
        else
        {
            Bootstrap.Settings.newAgents -= 100;
            newAgents.text = "New agents: " + Bootstrap.Settings.newAgents;
        }
    }
    
    public void incrementMaxSpeed()
    {
        RVOSystem.maxSpeed ++;
        maxSpeedT.text = "Max speed: " + RVOSystem.maxSpeed;
    }
    
    public void decrementMaxSpeed()
    {
        RVOSystem.maxSpeed --;
        maxSpeedT.text = "Max speed: " + RVOSystem.maxSpeed;
    }

    public void startSimulation()
    {
        start = true;
        World.Active.GetExistingManager<SpawnAgentSystem>().Enabled = !World.Active.GetExistingManager<SpawnAgentSystem>().Enabled;
    }
}
