using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
using Random = UnityEngine.Random;

public class Bootstrap
{
    public static MeshInstanceRenderer agentLook;
    public static MeshInstanceRenderer nodeLook;
    public static MeshInstanceRenderer pathLook;
    public static MeshInstanceRenderer openLook;
    public static MeshInstanceRenderer closedLook;
    public static MeshInstanceRenderer unwalkableLook;

    public static EntityArchetype _nodeArchetype;

    public static Settings Settings;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Init()
    {
        CreateArchetypes(World.Active.GetOrCreateManager<EntityManager>());
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void InitWithScene()
    {
        var settingsGO = GameObject.Find("Settings");
        Settings = settingsGO.GetComponent<Settings>();
                
        var entityManager = World.Active.GetOrCreateManager<EntityManager>();
        
        agentLook = GetLook("AgentLook");
        pathLook = GetLook("PathLook");
        openLook = GetLook("openLook");
        closedLook = GetLook("ClosedLook");
        SpawnAgents(entityManager);
    }

    private static void SpawnAgents(EntityManager entityManager)
    {
        var _agentArchetype = entityManager.CreateArchetype(
            typeof(Position),
            typeof(MeshInstanceRenderer),
            typeof(Agent),
            typeof(Target),
            typeof(Waypoints)
        );
        
//        for (int i = 0; i < 1; i++)
//        {
//            var agent = entityManager.CreateEntity(_agentArchetype);
//            entityManager.SetSharedComponentData(agent, agentLook);
//            entityManager.SetComponentData(agent, new Position{Value = new float3(-24.6f, 1, i*2-24.6f)});
//            entityManager.SetComponentData(agent, new Target{Value = new float3(i + 5, 1, i+5)});
//        }
        
//        // debug chunck iteration        
//        for (int i = 0; i < 100; i++)
//        {
//            var agent = entityManager.CreateEntity(_moveVelocityArchetype);
//            entityManager.SetSharedComponentData(agent, agentLook);
//            entityManager.SetComponentData(agent, new Position{Value = new float3(i+.5f,1,i+.5f)});
//            entityManager.SetComponentData(agent, new Velocity(){Value = 1.0f});
//        }
    }

    private static void CreateArchetypes(EntityManager entityManager)
    {
        _nodeArchetype = entityManager.CreateArchetype(
            typeof(Position),
            //typeof(MeshInstanceRenderer),
            typeof(Node)           
        );
    }

    public static MeshInstanceRenderer GetLook(string protoType)
    {
        var prototype = GameObject.Find(protoType);
        var look = prototype.GetComponent<MeshInstanceRendererComponent>().Value;
        Object.Destroy(prototype);
        return look;
    }
    
    //per abilitare o disabilitare systems:
    //World.Active.GetExistingManager<SomeSystem>().Enabled = false;
    //oppure dentro OnUpdate:
    //someSystem.Enabled = false;
}