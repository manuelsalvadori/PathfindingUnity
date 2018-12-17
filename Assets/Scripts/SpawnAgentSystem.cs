using System.Collections;
using System.Collections.Generic;
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

    protected override void OnCreateManager()
    {
        em = World.Active.GetOrCreateManager<EntityManager>();

        agentArchetype = em.CreateArchetype(
            typeof(Position),
            typeof(MeshInstanceRenderer),
            typeof(Agent),
            typeof(Target),
            typeof(Waypoints)
        );
    }

    protected override void OnUpdate()
    {
        Random rnd = new Random((uint)(Time.time*10)+1);

        for (int i = 0; i < 16; i++)
        {
            var rndY = rnd.NextFloat(-24.9f, 24.9f);
            var agent = em.CreateEntity(agentArchetype);
            em.SetSharedComponentData(agent, Bootstrap.agentLook);
            em.SetComponentData(agent, new Position{Value = new float3(-24.6f, 1, rndY)});
            em.SetComponentData(agent, new Target{Value = new float3(24.9f, 1, rndY)});
        }
    }
//    
//    private class SASBarrier : BarrierSystem {}
//    [Inject] private SASBarrier _SASBarrier;
}
