using Unity.Entities;
using Unity.Jobs;
using RVO;
using Unity.Collections;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

public class RVOSystem : JobComponentSystem
{
    public struct RVOJob : IJobParallelFor
    {
        [NativeDisableParallelForRestriction]
        public ComponentDataFromEntity<Position> Positions;
        
        [NativeDisableParallelForRestriction]
        public BufferFromEntity<Waypoints> waypoints;
        
        public EntityCommandBuffer.Concurrent commands;
        
        [ReadOnly]
        [DeallocateOnJobCompletionAttribute]
        public NativeArray<int> Indexes;
        
        public void Execute(int i)
        {
            var index = Indexes[i];
            float2 agentLoc = Simulator.Instance.getAgentPosition(index);

            SpawnAgentSystem.agents.TryGetValue(index, out var agent);

            int l = waypoints[agent].Length;
            
            if (l == 0)
            {
                commands.DestroyEntity(i, agent);
                SpawnAgentSystem.agents.Remove(index);
                Simulator.Instance.removeAgent(index);
                return;
            }

            var next = GridGeneratorSystem.GridToWorldPos(waypoints[agent][l - 1].Value);
            float2 goalVector = next - agentLoc;

            if (RVOMath.absSq(goalVector) > 1.0f)
            {
                goalVector = math.normalize(goalVector);
            }

            Simulator.Instance.setAgentPrefVelocity(index, goalVector);
            
            var newPos = new float3(agentLoc.x, 0.15f, agentLoc.y);
            
            Positions[agent] = new Position {Value = newPos};

            var dir = next - agentLoc;
            
            // remove waypoint
            if(dir.x < 0.1f && dir.y < 0.1f)
                waypoints[agent].RemoveAt(l - 1);
        }
    }

    public struct BuildKdTreeJob : IJob
    {
        public void Execute()
        {
            Simulator.Instance.buildAgentTree();
        }
    }
    
    public struct RVOStep : IJobParallelFor
    {
        [ReadOnly]
        public NativeArray<int> Indexes;

        public void Execute(int i)
        {
            var index = Indexes[i];
            Simulator.Instance.agents_[index].computeNeighbors();
            Simulator.Instance.agents_[index].computeNewVelocity();
        }
    }
    
    public struct RVOUpdate : IJobParallelFor
    {
        [ReadOnly] public float deltaTime;
        [ReadOnly] public NativeArray<int> Indexes;

        public void Execute(int i)
        {
            Simulator.Instance.agents_[Indexes[i]].update(deltaTime);
        }
    }

    public static int maxSpeed = 70;

    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        maxSpeed = Bootstrap.Settings.maxAgentSpeed;
    }

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var agentsJob = new RVOJob
        {
            Positions = GetComponentDataFromEntity<Position>(),
            Indexes = new NativeArray<int>(Simulator.Instance.getAgentsKeysArray(), Allocator.TempJob),
            waypoints = GetBufferFromEntity<Waypoints>(),
            commands = _rvoBarrier.CreateCommandBuffer().ToConcurrent()
            
        }.Schedule(Simulator.Instance.getNumAgents(), 64, inputDeps);
        agentsJob.Complete();

        var treeJob = new BuildKdTreeJob().Schedule(agentsJob);
        treeJob.Complete();

        var numAgents = Simulator.Instance.getNumAgents();
        var indexes = new NativeArray<int>(Simulator.Instance.getAgentsKeysArray(), Allocator.TempJob);

        var stepJob = new RVOStep{Indexes = indexes}.Schedule(numAgents, 64, treeJob);
        stepJob.Complete();

        var updateJob = new RVOUpdate{Indexes = indexes, deltaTime = Time.deltaTime * maxSpeed}.Schedule(numAgents, 64, stepJob);
        updateJob.Complete();
        
        indexes.Dispose();
        
        Simulator.Instance.doTimeStep();
        return updateJob;
    }
    
    private class RVOBarrier : BarrierSystem {}
    [Inject] private RVOBarrier _rvoBarrier;
}