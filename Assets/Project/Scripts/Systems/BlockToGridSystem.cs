using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;



namespace AprilGames.DOTS.Tetris {


   
   public class BlockToGridSystem : ComponentSystem {



      BeginInitializationEntityCommandBufferSystem BufferSystem;



      protected override void OnCreate() {
         BufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
      }



      protected override void OnUpdate() {

         EntityCommandBuffer buffer = BufferSystem.CreateCommandBuffer();

         Entities.ForEach((Entity entity,ref Translation translation, ref TagAddToGrid t)=> {

            float3 position = translation.Value;
            GameController.AddToGrid((int)position.x,(int)position.y,entity);

            buffer.RemoveComponent<TagAddToGrid>(entity);
            buffer.RemoveComponent<BlockOffset>(entity);

         });
         

      }



   }



}