using System;
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

            var next = GridGenerator.GridToWorldPos(waypoints[agent][l - 1].Value);
            float2 goalVector = next - agentLoc;

            if (RVOMath.absSq(goalVector) > 1.0f)
            {
                goalVector = math.normalize(goalVector);
            }

            Simulator.Instance.setAgentPrefVelocity(index, goalVector);
            
            var newPos = new float3(agentLoc.x, 0.15f, agentLoc.y);
            
            Positions[agent] = new Position {Value = newPos};
            
//            if(agent.Index == 2500)
//                Debug.Log(Positions[agent].Value);

            var dir = next - agentLoc;
            // next waypoint
            if(dir.x < 0.1f && dir.y < 0.1f)
                waypoints[agent].RemoveAt(l - 1);
        }
    }

    public struct doStepJob : IJob
    {
        public void Execute()
        {
            Simulator.Instance.doStep();
        }
    }

    [Inject] private ComponentDataFromEntity<Position> _positions;

    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        var job = new RVOJob
        {
            Positions = _positions,
            Indexes = new NativeArray<int>(Simulator.Instance.getAgentsKeysArray(), Allocator.TempJob),
            waypoints = GetBufferFromEntity<Waypoints>(),
            commands = _rvoBarrier.CreateCommandBuffer().ToConcurrent()
            
        }.Schedule(Simulator.Instance.getNumAgents(), 64, inputDeps);
        job.Complete();

        return new doStepJob().Schedule(job);
        
        //Simulator.Instance.doStep();
        
        //return job;
    }
    
    private class RVOBarrier : BarrierSystem {}
    [Inject] private RVOBarrier _rvoBarrier;
}