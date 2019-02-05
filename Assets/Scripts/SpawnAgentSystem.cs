using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using RVO;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;

public class SpawnAgentSystem : ComponentSystem
{
    private EntityArchetype agentArchetype;
    private MeshInstanceRenderer agentLook;
    private EntityManager em;
    private int2 gridSize;
    private int count;
    private float currentx = -24.9f;
    private float currenty = -24.9f;

    public static int maxLimit = 19900;
    public static int limit;
    public static int newAgents;

    public static NativeHashMap<int, Entity> agents;

    protected override void OnCreateManager()
    {
        base.OnCreateManager();
        agents = new NativeHashMap<int, Entity>(maxLimit + 300, Allocator.Persistent);
        em = World.Active.GetOrCreateManager<EntityManager>();
        
        agentArchetype = em.CreateArchetype
        (
            typeof(Position),
            typeof(MeshInstanceRenderer),
            typeof(Agent),
            typeof(Target),
            typeof(Waypoints)
        );
        
        Enabled = false;
    }

    protected override void OnStartRunning()
    {
        base.OnStartRunning();

        count = 300;
             
        // RVO2 Init
        Simulator.Instance.setTimeStep(0.25f);
        Simulator.Instance.setAgentDefaults(3.0f, 10, 5.0f, 5.0f, 0.035f, 0.12f, new float2(0.0f, 0.0f));
        
        limit = Bootstrap.Settings.agentsLimit;
        newAgents = Bootstrap.Settings.newAgents;
        gridSize = Bootstrap.Settings.gridSize;
    }

    protected override void OnUpdate()
    {
        count++;

        if (agents.Length >= limit)
        {
            World.Active.GetExistingManager<RVOSystem>().Enabled = true;
            Enabled = false;
        }
        
        if (limit - agents.Length < newAgents)
            newAgents = limit - agents.Length;

        count = 0;
        Random rnd = new Random((uint)(Time.time*10)+1);

        for (int i = 0; i < newAgents; i++)
        {
            var y = (gridSize.y / 2) - 0.1f;
            var rndY2 = rnd.NextFloat(-y, y);
            
            var agent = em.CreateEntity(agentArchetype);
            em.SetSharedComponentData(agent, Bootstrap.agentLook);
            
            var pos = new Position {Value = new float3(currentx, 0, currenty)};
            var tar = new Target {Value = new float3(-currentx, 0, -currenty)};

            em.SetComponentData(agent, pos);
            em.SetComponentData(agent, tar);
            
            currenty += 0.19f;
            if (currenty >= 24.9f)
            {
                currenty = -24.9f;
                currentx += 0.19f;
            }
            
            var index = Simulator.Instance.addAgent(pos.Value.xz);
            agents.TryAdd(index, agent);
        }
    }

    protected override void OnDestroyManager()
    {
        agents.Dispose();
        base.OnDestroyManager();
    }
}
