//using Unity.Burst;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Transforms;
//using UnityEngine;
//
//public class GridSystem : JobComponentSystem
//{
//    
//    private struct GridUpdateJob : IJobProcessComponentDataWithEntity<Node, Position>
//    {
//        [ReadOnly] public EntityCommandBuffer Commands;
//        
//        public void Execute(Entity node, int index, ref Node data, ref Position pos)
//        {
//            if (Physics.CheckBox(pos.Value, Vector3.one * 0.5f/*(nodeSize / 2f)/*, Quaternion.identity, unwalkable*/))
//            {
//                Commands.AddComponent(node, new Unwalkable());
//                Commands.SetSharedComponent(node, Bootstrap.unwalkableLook);
//            }
//            else
//            {
//                Commands.SetSharedComponent(node, Bootstrap.nodeLook);
//                Commands.RemoveComponent<Unwalkable>(node);
//            }
//        }
//    }
//
//    private class GridBarrier : BarrierSystem {}
//    
//    [Inject] private GridBarrier _gridBarrier;
//    
//    
//    protected override JobHandle OnUpdate(JobHandle inputDeps)
//    {
//        return new GridUpdateJob()
//        {
//            Commands = _gridBarrier.CreateCommandBuffer()
//        }.Schedule(this, inputDeps);
//    }
//}
