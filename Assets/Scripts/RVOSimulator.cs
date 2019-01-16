//using UnityEngine;
//using System.Collections;
//using System.Collections.Generic;
//using RVO;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Mathematics;
//using Unity.Rendering;
//using Unity.Transforms;
//using Random = Unity.Mathematics.Random;
//
//public class RVOSimulator : MonoBehaviour
//{
//    // goal vectors for each agent
//    public static float3[] goals;
//
//    public static NativeHashMap<int, Entity> agents;
//    private MeshInstanceRenderer look;
//
//    private int count = 300;
//    private EntityManager em;
//    private EntityArchetype agentArchetype;
//
//    public static MeshInstanceRenderer GetLook(string protoType)
//    {
//        var prototype = GameObject.Find(protoType);
//        var look = prototype.GetComponent<MeshInstanceRendererComponent>().Value;
//        Object.Destroy(prototype);
//        return look;
//    }
//
//    // Use this for initialization
//    void Start ()
//    {
//        em = World.Active.GetOrCreateManager<EntityManager>();
//        look = GetLook("agentLook");
//        
//        Simulator.Instance.setTimeStep(0.25f);
//        Simulator.Instance.setAgentDefaults(10.0f, 10, 5.0f, 5.0f, 0.5f, 0.05f, new float2(0.0f, 0.0f));
//
//        agents = new NativeHashMap<int, Entity>(5000, Allocator.Persistent);
//        goals = new float3[20];
//        
//        agentArchetype = em.CreateArchetype
//        (
//            typeof(Position),
//            typeof(MeshInstanceRenderer),
//            typeof(Agent),
//            typeof(Target),
//            typeof(Waypoints)
//        );
//        
////        Random rnd = new Random((uint)(Time.time*10)+1);
////
////        for (int i = 0; i < 20; i++)
////        {
////            var rndY = rnd.NextFloat(-24.9f, 24.9f);
////            var rndY2 = rnd.NextFloat(-24.9f, 24.9f);
////            
////            var agent = em.CreateEntity(agentArchetype);
////            var pos = new Position {Value = new float3(-24.6f, 1, rndY)};
////            var target = new Target {Value = new float3(24.6f, 1, rndY2)};
////            
////            em.SetSharedComponentData(agent, Bootstrap.agentLook);
////            em.SetComponentData(agent, pos);
////            em.SetComponentData(agent, target);
////            
////            var index = Simulator.Instance.addAgent(pos.Value.xz);
////            agents.TryAdd(index, agent);
////            
////        }
////        var obst = new List<float2>();
////        obst.Add(new float2(-5,-5));
////        obst.Add(new float2(5,-5));
////        obst.Add(new float2(5,5));
////        obst.Add(new float2(-5,5));
////        Simulator.Instance.addObstacle(obst);
////        Simulator.Instance.processObstacles();
////        Simulator.Instance.removeAgent(0);
//}
//	
//    void Update()
//    {
//        count++;
//        
//        if(count < 300)
//            return;
//
//        count = 0;
//        Random rnd = new Random((uint)(Time.time*10)+1);
//
//        for (int i = 0; i < 20; i++)
//        {
//            var rndY = rnd.NextFloat(-24.9f, 24.9f);
//            var rndY2 = rnd.NextFloat(-24.9f, 24.9f);
//            
//            var agent = em.CreateEntity(agentArchetype);
//            em.SetSharedComponentData(agent, Bootstrap.agentLook);
//            
//            var pos = new Position {Value = new float3(-24.6f, 1, rndY)};
//
//            em.SetComponentData(agent, pos);
//            em.SetComponentData(agent, new Target{Value = new float3(24.6f, 1, rndY2)});
//            
//            var index = Simulator.Instance.addAgent(pos.Value.xz);
//            agents.TryAdd(index, agent);
//        }
//    }
//
//    private void OnDestroy()
//    {
//        agents.Dispose();
//    }
//}
