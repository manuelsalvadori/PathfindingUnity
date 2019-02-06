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
    private int count;
    private float currentx = -24.9f;
    private float currenty = -24.9f;
    private float delta = 0.19f;

    public static int maxLimit = 19900;
    public static int limit;
    public static int newAgents;

    public static NativeHashMap<int, Entity> agents;

    protected override void OnCreateManager()
    {
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
        // RVO2 Init
        Simulator.Instance.setTimeStep(0.25f);
        Simulator.Instance.setAgentDefaults(3.0f, 10, 5.0f, 5.0f, 0.035f, 0.12f, new float2(0.0f, 0.0f));
        
        limit = Bootstrap.Settings.agentsLimit;
        newAgents = Bootstrap.Settings.newAgents;

        delta = delta * 20000f / limit;
    }

    protected override void OnUpdate()
    {
        if (agents.Length >= limit)
        {
            World.Active.GetExistingManager<RVOSystem>().Enabled = true;
            Enabled = false;
        }
        
        if (limit - agents.Length < newAgents)
            newAgents = limit - agents.Length;

        for (int i = 0; i < newAgents; i++)
        {            
            var agent = em.CreateEntity(agentArchetype);
            em.SetSharedComponentData(agent, Bootstrap.agentLook);
            
            var pos = new Position {Value = new float3(currentx, 0, currenty)};
            var tar = new Target {Value = new float3(-currentx, 0, -currenty)};

            em.SetComponentData(agent, pos);
            em.SetComponentData(agent, tar);
            
            currenty += delta;
            if (currenty >= 24.9f)
            {
                currenty = -24.9f;
                currentx += 0.2f;
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
