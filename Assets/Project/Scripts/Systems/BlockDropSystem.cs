using Unity.Entities;
using Unity.Jobs;
using Unity.Burst;
using Unity.Transforms;
using Unity.Collections;



namespace AprilGames.DOTS.Tetris {



   public class BlockDropSystem : JobComponentSystem {



      BeginInitializationEntityCommandBufferSystem BufferSystem;



      protected override void OnCreate() {
         BufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
      }



      [BurstCompile]
      struct DropJob : IJobForEachWithEntity<Translation,BlockDrop> {

         public EntityCommandBuffer.Concurrent Buffer;

         public void Execute(Entity entity,int index,ref Translation translation,ref BlockDrop drop) {

            translation.Value.y--;
            drop.Rows--;
            if(drop.Rows<=0) {
               Buffer.RemoveComponent<BlockDrop>(index,entity);
            }

         }



      }



      protected override JobHandle OnUpdate(JobHandle jobDeps) {

         DropJob job = new DropJob() {
            Buffer = BufferSystem.CreateCommandBuffer().ToConcurrent()
         };
         JobHandle handle = job.Schedule(this,jobDeps);
         handle.Complete();
         return default;

      }



   }



}