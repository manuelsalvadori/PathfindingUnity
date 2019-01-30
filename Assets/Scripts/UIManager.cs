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
        if (SpawnAgentSystem.limit > SpawnAgentSystem.maxLimit)
            return;
        SpawnAgentSystem.limit += 100;
        agentsLimit.text = "Agents limit: " + SpawnAgentSystem.limit;
    }
    
    public void decrementLimit()
    {
        SpawnAgentSystem.limit -= 100;
        agentsLimit.text = "Agents limit: " + SpawnAgentSystem.limit;
    }
    
    public void incrementNewAgents()
    {
        SpawnAgentSystem.newAgents += 2;
        newAgents.text = "New agents: " + SpawnAgentSystem.newAgents;
    }
    
    public void decrementNewAgents()
    {
        SpawnAgentSystem.newAgents -= 2;
        newAgents.text = "New agents: " + SpawnAgentSystem.newAgents;
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
}
