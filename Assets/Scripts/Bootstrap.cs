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
                        
        agentLook = GetLook("AgentLook");
        pathLook = GetLook("PathLook");
        openLook = GetLook("openLook");
        closedLook = GetLook("ClosedLook");
    }

    private static void CreateArchetypes(EntityManager entityManager)
    {
        _nodeArchetype = entityManager.CreateArchetype(
            typeof(Position),
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