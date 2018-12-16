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
    public static MeshInstanceRenderer unwalkableLook;

    public static EntityArchetype _agentArchetype;
    public static EntityArchetype _nodeArchetype;
    public static EntityArchetype _pathStepArchetype;
    public static EntityArchetype _moveVelocityArchetype;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    public static void Init()
    {
        CreateArchetypes(World.Active.GetOrCreateManager<EntityManager>());
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    public static void InitWithScene()
    {
        var entityManager = World.Active.GetOrCreateManager<EntityManager>();
        agentLook = GetLook("AgentLook");
        pathLook = GetLook("PathLook");
        SpawnAgents(entityManager);
    }

    private static void SpawnAgents(EntityManager entityManager)
    {
        for (int i = 0; i < 10; i++)
        {
            var agent = entityManager.CreateEntity(_agentArchetype);
            entityManager.SetSharedComponentData(agent, agentLook);
            entityManager.SetComponentData(agent, new Position{Value = new float3(0,1,i*2)});
            entityManager.SetComponentData(agent, new Target{Value = new float3(i + 5,1,i+5)});
        }
        
        for (int i = 0; i < 100; i++)
        {
            var agent = entityManager.CreateEntity(_moveVelocityArchetype);
            entityManager.SetSharedComponentData(agent, agentLook);
            entityManager.SetComponentData(agent, new Position{Value = new float3(i+.5f,1,i+.5f)});
            entityManager.SetComponentData(agent, new Velocity(){Value = 1.0f});
        }
    }

    private static void CreateArchetypes(EntityManager entityManager)
    {
        _moveVelocityArchetype = entityManager.CreateArchetype(
            typeof(Position),
            typeof(MeshInstanceRenderer),
            typeof(Velocity)
        );
        
        _agentArchetype = entityManager.CreateArchetype(
            typeof(Position),
            typeof(MeshInstanceRenderer),
            typeof(Agent),
            typeof(Target)
        );
        
        _nodeArchetype = entityManager.CreateArchetype(
            typeof(Position),
            typeof(MeshInstanceRenderer),
            typeof(Node)           
        );
        
        _pathStepArchetype = entityManager.CreateArchetype(
            typeof(PathStep),
            typeof(PathIndex),
            typeof(ParentAgent)           
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