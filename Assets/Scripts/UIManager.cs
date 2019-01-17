using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text agentsLimit;
    public Text newAgents;
    public Text count;

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
}
