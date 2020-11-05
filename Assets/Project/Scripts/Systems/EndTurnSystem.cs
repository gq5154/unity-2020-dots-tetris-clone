using Unity.Entities;



namespace AprilGames.DOTS.Tetris {




   [UpdateAfter(typeof(BlockToGridSystem))]
   public class EndTurnSystem : SystemBase {



      BeginInitializationEntityCommandBufferSystem BufferSystem;
      EntityCommandBuffer                          Buffer;



      protected override void OnCreate() {
         RequireSingletonForUpdate<TagEndTurn>();
         BufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
      }



      protected override void OnUpdate() {

         Entity entity = GetSingletonEntity<TagEndTurn>();
         Buffer = BufferSystem.CreateCommandBuffer();
         Buffer.DestroyEntity(entity);
         if(GameController.Overflow) {
            GameController.GameOver();
         } else {
            Spawn spawn = new Spawn {
               Delay = GameController.RemoveFullRows(Remove,Drop)
            };
            GameController.SelectNextPiece();
            Entity piece = Buffer.CreateEntity(GameController.SpawnArchetype);
            Buffer.SetComponent(piece,spawn);
         }
        
      }



      void Remove(Entity entity) {
         Buffer.DestroyEntity(entity);
      }



      void Drop(Entity entity,int rows) {
         Buffer.AddComponent(entity,new BlockDrop {
            Rows = rows
         });
      }



   }



}