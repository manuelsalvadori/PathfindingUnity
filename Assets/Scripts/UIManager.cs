using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public Text agentsLimit;
    public Text newAgentsT;
    public Text count;
    public Text maxSpeedT;
    public Text batchT;

    public static int batch = 1;
    public static float startime = 0f;
    
    public static int maxLimit = 14900;
    public static int limit;
    public static int newAgents;

    private void Start()
    {
        var settings = GameObject.Find("Settings").GetComponent<Settings>();
        agentsLimit.text = "Agents limit: " + settings.agentsLimit;
        newAgentsT.text = "New agents: " + settings.newAgents;
        maxSpeedT.text = "Max speed: " + settings.maxAgentSpeed;
        limit = settings.agentsLimit;
        newAgents = settings.newAgents;
    }

    private void Update()
    {
        count.text = "Agents count: " + SpawnAgentSystem.agents.Length;
    }

    public void incrementLimit()
    {
        if (limit > SpawnAgentSystem.maxLimit)
            return;
        limit += 500;
        agentsLimit.text = "Agents limit: " + limit;
    }
    
    public void decrementLimit()
    {
        limit -= 500;
        agentsLimit.text = "Agents limit: " + limit;
    }
    
    public void incrementNewAgents()
    {
        newAgents += 20;
        newAgentsT.text = "New agents: " + newAgents;
    }
    
    public void decrementNewAgents()
    {
        newAgents -= 20;
        newAgentsT.text = "New agents: " + newAgents;
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
    
    public void incrementbatch()
    {
        batch ++;
        batchT.text = "Batch: " + batch;
    }
    
    public void decrementbatch()
    {
        batch --;
        batchT.text = "Batch: " + batch;
    }
    
    public void startSim()
    {
        startime = Time.time;
        World.Active.GetExistingManager<SpawnAgentSystem>().Enabled = true;
        World.Active.GetExistingManager<AStarSystem>().Enabled = true;
    }
}
