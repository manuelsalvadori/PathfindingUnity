using System.Collections.Generic;
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
    private List<KeyValuePair<float,float>> fps;
    private bool start = false;
    private float startime = 0f;
    private int currentAgent;

    private void Start()
    {
        var settings = GameObject.Find("Settings").GetComponent<Settings>();
        agentsLimit.text = "Agents limit: " + settings.agentsLimit;
        newAgents.text = "New agents: " + settings.newAgents;
        maxSpeedT.text = "Max speed: " + settings.maxAgentSpeed;
        fps = new List<KeyValuePair<float, float>>();
    }

    private void Update()
    {
        if(start)
        {
            var delta = Time.time - startime;
            fps.Add(new KeyValuePair<float, float>(delta, GraphyManager.Instance.CurrentFPS));
            if (delta > 12f)
            {
                saveData();
                Application.Quit();
            }
        }
        count.text = "Agents count: " + SpawnAgentSystem.agents.Length;
    }

    public void saveData()
    {
        string path = $"{Application.persistentDataPath}/fpsdataECS {currentAgent}_{SpawnAgentSystem.limit}.txt";
        
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
            SpawnAgentSystem.limit += 500;
            agentsLimit.text = "Agents limit: " + SpawnAgentSystem.limit;
        }
        else
        {
            if (Bootstrap.Settings.agentsLimit > 19900)
                return;
            Bootstrap.Settings.agentsLimit += 500;
            agentsLimit.text = "Agents limit: " + Bootstrap.Settings.agentsLimit;
        }
    }
    
    public void decrementLimit()
    {
        if (World.Active.GetExistingManager<SpawnAgentSystem>().Enabled)
        {
            if (Bootstrap.Settings.agentsLimit == 0)
                return;
            SpawnAgentSystem.limit -= 500;
            agentsLimit.text = "Agents limit: " + SpawnAgentSystem.limit;
        }
        else
        {
            if (Bootstrap.Settings.agentsLimit == 0)
                return;
            Bootstrap.Settings.agentsLimit -= 500;
            agentsLimit.text = "Agents limit: " + Bootstrap.Settings.agentsLimit;
        }
    }
    
    public void incrementNewAgents()
    {
        if (World.Active.GetExistingManager<SpawnAgentSystem>().Enabled)
        {
            SpawnAgentSystem.newAgents += 100;
            currentAgent = SpawnAgentSystem.newAgents;
            newAgents.text = "New agents: " + SpawnAgentSystem.newAgents;
        }
        else
        {
            Bootstrap.Settings.newAgents += 100;
            currentAgent = SpawnAgentSystem.newAgents;
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
