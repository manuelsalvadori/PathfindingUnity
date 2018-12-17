using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

[BurstCompile]
[UpdateAfter(typeof(AStarSystem))]
public class AgentMovementSystem : JobComponentSystem
{
    private struct MoveByVelocityJob : IJobParallelFor
    {
        public float deltaTime;

        public ArchetypeChunkComponentType<Position> positionType;
        [ReadOnly] public ArchetypeChunkEntityType entityType;
        
        [ReadOnly]
        [DeallocateOnJobCompletion]
        public NativeArray<ArchetypeChunk> agentChunks;
        
        [NativeDisableParallelForRestriction]
        public BufferFromEntity<Waypoints> waypoints;

        public EntityCommandBuffer.Concurrent commands;


        public void Execute(int index)
        {
            Process(agentChunks[index], index);
        }
         
        private void Process(ArchetypeChunk chunk, int jobIndex)
        {
            NativeArray<Position> positions = chunk.GetNativeArray(this.positionType);
            NativeArray<Entity> entities = chunk.GetNativeArray(entityType);
            for (int i = 0; i < chunk.Count; ++i)
            {
                int l = waypoints[entities[i]].Length;
                if (l == 0)
                {
                    commands.DestroyEntity(jobIndex, entities[i]);
                    break;
                }
                
                int2 nexti2 = waypoints[entities[i]][l-1].Value;
                float3 next = new float3
                    (
                        GridGenerator.GridToWorldPosX(nexti2.x),
                        positions[i].Value.y,
                        GridGenerator.GridToWorldPosY(nexti2.y)
                    );
                
                float3 dir = next - positions[i].Value;
                
                positions[i] = new Position()
                {
                    Value = positions[i].Value + math.normalize(dir) * deltaTime * 2
                };

                if(dir.x < 0.05f && dir.z < 0.05f)
                    waypoints[entities[i]].RemoveAt(waypoints[entities[i]].Length - 1);
            }
        }
    }
    
    private ComponentGroup agentGroup;
     
    protected override void OnCreateManager()
    {
        this.agentGroup = GetComponentGroup(typeof(Position), typeof(Agent));
    }
     
    protected override JobHandle OnUpdate(JobHandle inputDeps)
    {
        NativeArray<ArchetypeChunk> agentChunks = agentGroup.CreateArchetypeChunkArray(Allocator.TempJob);
         
        MoveByVelocityJob moveByVelocityJob = new MoveByVelocityJob()
        {
            deltaTime = Time.deltaTime,
            positionType = GetArchetypeChunkComponentType<Position>(),
            entityType = GetArchetypeChunkEntityType(),
            agentChunks = agentChunks,
            waypoints = GetBufferFromEntity<Waypoints>(),
            commands = _moveBarrier.CreateCommandBuffer().ToConcurrent()
        };
         
        return moveByVelocityJob.Schedule(agentChunks.Length, 64, inputDeps);
    }
    
    private class MoveBarrier : BarrierSystem {}
    [Inject] private MoveBarrier _moveBarrier;

}