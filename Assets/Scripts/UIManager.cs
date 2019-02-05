using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text agentsLimit;
    public Text newAgents;
    public Text count;
    public Text maxSpeedT;

    private void Start()
    {
        var settings = GameObject.Find("Settings").GetComponent<Settings>();
        agentsLimit.text = "Agents limit: " + settings.agentsLimit;
        newAgents.text = "New agents: " + settings.newAgents;
        maxSpeedT.text = "Max speed: " + settings.maxAgentSpeed;
    }

    private void Update()
    {
        count.text = "Agents count: " + SpawnAgentSystem.agents.Length;
    }

    public void incrementLimit()
    {
        if(World.Active.GetExistingManager<SpawnAgentSystem>().Enabled)
        {
            if (SpawnAgentSystem.limit > SpawnAgentSystem.maxLimit)
                return;
            SpawnAgentSystem.limit += 100;
            agentsLimit.text = "Agents limit: " + SpawnAgentSystem.limit;
        }
        else
        {
            if (Bootstrap.Settings.agentsLimit > 19900)
                return;
            Bootstrap.Settings.agentsLimit += 100;
            agentsLimit.text = "Agents limit: " + Bootstrap.Settings.agentsLimit;
        }
    }
    
    public void decrementLimit()
    {
        if (World.Active.GetExistingManager<SpawnAgentSystem>().Enabled)
        {
            if (Bootstrap.Settings.agentsLimit == 0)
                return;
            SpawnAgentSystem.limit -= 100;
            agentsLimit.text = "Agents limit: " + SpawnAgentSystem.limit;
        }
        else
        {
            if (Bootstrap.Settings.agentsLimit == 0)
                return;
            Bootstrap.Settings.agentsLimit -= 100;
            agentsLimit.text = "Agents limit: " + Bootstrap.Settings.agentsLimit;
        }
    }
    
    public void incrementNewAgents()
    {
        if (World.Active.GetExistingManager<SpawnAgentSystem>().Enabled)
        {
            SpawnAgentSystem.newAgents += 2;
            newAgents.text = "New agents: " + SpawnAgentSystem.newAgents;
        }
        else
        {
            Bootstrap.Settings.newAgents += 2;
            newAgents.text = "New agents: " + Bootstrap.Settings.newAgents;
        }
    }
    
    public void decrementNewAgents()
    {
        if (World.Active.GetExistingManager<SpawnAgentSystem>().Enabled)
        {
            SpawnAgentSystem.newAgents -= 2;
            newAgents.text = "New agents: " + SpawnAgentSystem.newAgents;
        }
        else
        {
            Bootstrap.Settings.newAgents -= 2;
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
        World.Active.GetExistingManager<SpawnAgentSystem>().Enabled = !World.Active.GetExistingManager<SpawnAgentSystem>().Enabled;
    }
}
