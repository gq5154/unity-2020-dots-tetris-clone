using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;



namespace AprilGames.DOTS.Tetris {



   public class StartUp : MonoBehaviour, IConvertGameObjectToEntity {



      [SerializeField] GameObject PieceBlockPrefab;
      [SerializeField] GameObject WallBlockPrefab;
      [SerializeField] GameObject GridPrefab;



      static StartUp Instance;



      private void Awake() {

         if(Instance!=null) {
            Destroy(gameObject);
         }
         Instance = this;
         
      }



      public void Convert(Entity entity,EntityManager dstManager,GameObjectConversionSystem conversionSystem) {

         if(WallBlockPrefab==null) {
            throw new System.InvalidOperationException("Wall Block Prefab is set to None!");
         }

         if(PieceBlockPrefab==null) {
            throw new System.InvalidOperationException("Piece Block Prefab is set to None!");
         }

         Entity wallBlockPrefab;
         Entity gridPrefab;

         using(BlobAssetStore blob = new BlobAssetStore()) {

            var cparam                 = GameObjectConversionSettings.FromWorld(dstManager.World,blob);
            wallBlockPrefab            = GameObjectConversionUtility.ConvertGameObjectHierarchy(WallBlockPrefab,cparam);
            gridPrefab                 = GameObjectConversionUtility.ConvertGameObjectHierarchy(GridPrefab,cparam);
            GameController.BlocKPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(PieceBlockPrefab,cparam);

         }
                  
         GameController.PieceArchetype   = dstManager.CreateArchetype(typeof(PiecePosition));
         GameController.SpawnArchetype   = dstManager.CreateArchetype(typeof(Spawn));
         GameController.EndTurnArchetype = dstManager.CreateArchetype(typeof(TagEndTurn));
         GameController.RestartArchetype = dstManager.CreateArchetype(typeof(TagRestart));

         BeginInitializationEntityCommandBufferSystem bufferSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();
         EntityCommandBuffer                          buffer       = bufferSystem.CreateCommandBuffer();

         int width  = GameController.Width;
         int height = GameController.Height;
         int x2 = width+1;
         for(int y = 0;y<=height;y++) {
            Entity block = buffer.Instantiate(wallBlockPrefab);
            buffer.SetComponent(block,new Translation { Value = new float3(0,y,0) });
            block = buffer.Instantiate(wallBlockPrefab);
            buffer.SetComponent(block,new Translation { Value = new float3(x2,y,0) });
         }
         for(int x = 1;x<=width;x++) {
            for(int y = 1;y<=height;y++) {
               Entity square = buffer.Instantiate(gridPrefab);
               buffer.SetComponent(square,new Translation { Value = new float3(x,y,1) });
            }
            Entity block = buffer.Instantiate(wallBlockPrefab);
            buffer.SetComponent(block,new Translation { Value = new float3(x,0,0) });
         }

         Entity piece = buffer.CreateEntity(GameController.SpawnArchetype);
         buffer.SetComponent(piece,new Spawn {
            Delay = 1
         });

         buffer.DestroyEntity(wallBlockPrefab);
         buffer.DestroyEntity(gridPrefab);
         buffer.DestroyEntity(entity);

      }



   }



}