using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;



namespace AprilGames.DOTS.Tetris {



   public class BlockMoveSystem : JobComponentSystem {



      BeginInitializationEntityCommandBufferSystem BufferSystem;



      protected override void OnCreate() {
         BufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
      }


      [BurstCompile]
      [RequireComponentTag(typeof(TagMove))]
      struct MoveJob : IJobForEachWithEntity<Translation,BlockOffset> {

                    public EntityCommandBuffer.Concurrent         Buffer;
         [ReadOnly] public ComponentDataFromEntity<PiecePosition> Center;

         public void Execute(Entity entity,int index,[WriteOnly] ref Translation translation,[ReadOnly] ref BlockOffset offset) {

            PiecePosition position = Center[offset.Center];

            switch(position.Rotation) {
            case 0:
               translation.Value = new float3(position.CenterX+offset.OffsetX,position.CenterY+offset.OffsetY,0f);
               break;
            case 90:
               translation.Value = new float3(position.CenterX+offset.OffsetY,position.CenterY-offset.OffsetX,0f);
               break;
            case 180:
               translation.Value = new float3(position.CenterX-offset.OffsetX,position.CenterY-offset.OffsetY,0f);
               break;
            case 270:
               translation.Value = new float3(position.CenterX-offset.OffsetY,position.CenterY+offset.OffsetX,0f);
               break;
            }

            Buffer.RemoveComponent<TagMove>(index,entity);

         }


      }



      protected override JobHandle OnUpdate(JobHandle jobDeps) {

         MoveJob job = new MoveJob() {
            Buffer = BufferSystem.CreateCommandBuffer().ToConcurrent(),
            Center = GetComponentDataFromEntity<PiecePosition>(true)
         };
         return job.Schedule(this,jobDeps);

      }



   }



}