//using System.Collections;
//using System.Collections.Generic;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Transforms;
//using UnityEngine;
//
//public class SetWaypointSystem : JobComponentSystem
//{
//    private struct SetWaypointJob : IJobProcessComponentDataWithEntity<Agent>
//    {
//        [ReadOnly] public ComponentDataFromEntity<ParentAgent> parentAgent;
//        [ReadOnly] public ComponentDataFromEntity<PathIndex> pathIndex;
//        [ReadOnly] public ComponentDataFromEntity<PathStep> pathStep;
//
//        public void Execute(Entity entity, int index, ref Agent data)
//        {
//            throw new System.NotImplementedException();
//        }
//    }
//    
//    protected override JobHandle OnUpdate(JobHandle inputDeps)
//    {
//        
//         
//        return moveByVelocityJob.Schedule(agentChunks.Length, 64, inputDeps);
//    }
//    [Inject] private ComponentDataFromEntity<PathStep> _pathStep;
//    [Inject] private ComponentDataFromEntity<PathIndex> _pathIndex;
//    [Inject] private ComponentDataFromEntity<ParentAgent> _parentAgent;
//}
