using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Rendering;




namespace AprilGames.DOTS.Tetris {

   

   public class PieceSpawnSystem : ComponentSystem {



      BeginInitializationEntityCommandBufferSystem BufferSystem;



      protected override void OnCreate() {
         BufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
      }



      protected override void OnUpdate() {
         
         Entity tag = Entity.Null;
         Entities.ForEach((Entity entity,ref Spawn spawn) => {

            if(spawn.Delay>0) {
               spawn.Delay--;
               return;
            }
            tag = entity;

         });

         if(tag==Entity.Null) {
            return;
         }

         EntityCommandBuffer buffer = BufferSystem.CreateCommandBuffer();
         buffer.DestroyEntity(tag);

         if(!GameController.CanMove(GameController.SpawnX,GameController.SpawnY,0)) {
            GameController.GameOver();
            return;
         }

         Entity pieceCenter = buffer.CreateEntity(GameController.PieceArchetype);
         buffer.SetComponent(pieceCenter,new PiecePosition {
            CenterX = GameController.SpawnX,
            CenterY = GameController.SpawnY
         });

         PieceBlueprint piece = GameController.PiecesInfo[GameController.CurrentPiece];
         int2[] map = piece.BlockMap;
         int l = map.Length;
         RenderMesh rmesh = World.EntityManager.GetSharedComponentData<RenderMesh>(GameController.BlocKPrefab);
         rmesh.material        = piece.PieceMaterial;

         for(int i = 0;i<l;i++) {
            int2 pos = map[i];
            Entity block = buffer.Instantiate(GameController.BlocKPrefab);
            buffer.SetSharedComponent(block,rmesh);
            buffer.SetComponent(block,new BlockOffset {
               Center  = pieceCenter,
               OffsetX = pos.x,
               OffsetY = pos.y
            });
            buffer.SetComponent(block,new Translation {
               Value = new float3(GameController.SpawnX+pos.x,GameController.SpawnY+pos.y,0f)
            });
         }

      }



   }

   

}