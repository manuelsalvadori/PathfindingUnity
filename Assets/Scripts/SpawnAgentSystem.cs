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

    public static int maxLimit = 14900;
    public static int limit = 7000;
    public static int newAgents = 70;

    public static NativeHashMap<int, Entity> agents;

    protected override void OnCreateManager()
    {
        em = World.Active.GetOrCreateManager<EntityManager>();

        agentArchetype = em.CreateArchetype
        (
            typeof(Position),
            typeof(MeshInstanceRenderer),
            typeof(Agent),
            typeof(Target),
            typeof(Waypoints)
        );
        count = 300;
        
        agents = new NativeHashMap<int, Entity>(maxLimit + 300, Allocator.Persistent);
        
        // RVO2 Init
        Simulator.Instance.setTimeStep(0.25f);
        Simulator.Instance.setAgentDefaults(3.0f, 10, 5.0f, 5.0f, 0.1f, 0.12f, new float2(0.0f, 0.0f));
        
    }

    protected override void OnUpdate()
    {
        count++;
        
        if(count < 20 || agents.Length > limit)
            return;

        count = 0;
        Random rnd = new Random((uint)(Time.time*10)+1);

        for (int i = 0; i < newAgents/2; i++)
        {
            var rndY = rnd.NextFloat(-24.9f, 24.9f);
            var rndY2 = rnd.NextFloat(-24.9f, 24.9f);
            
            var agent = em.CreateEntity(agentArchetype);
            em.SetSharedComponentData(agent, Bootstrap.agentLook);
            
            var pos = new Position {Value = new float3(-24.6f, 0, rndY)};

            em.SetComponentData(agent, pos);
            em.SetComponentData(agent, new Target{Value = new float3(24.6f, 0, rndY2)});
            
            var index = Simulator.Instance.addAgent(pos.Value.xz);
            agents.TryAdd(index, agent);
        }
        
        for (int i = 0; i < newAgents/2; i++)
        {
            var rndX = rnd.NextFloat(-24.9f, 24.9f);
            var rndX2 = rnd.NextFloat(-24.9f, 24.9f);
            
            var agent = em.CreateEntity(agentArchetype);
            em.SetSharedComponentData(agent, Bootstrap.agentLook);
            
            var pos = new Position {Value = new float3(rndX, 1, -24.6f)};

            em.SetComponentData(agent, pos);
            em.SetComponentData(agent, new Target{Value = new float3(rndX2, 1, 24.6f)});
            
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
