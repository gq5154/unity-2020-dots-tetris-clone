using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using System;



public class StartUp : MonoBehaviour, IConvertGameObjectToEntity {



   [SerializeField] GameObject PieceBlockPrefab;
   [SerializeField] GameObject WallBlockPrefab;



   static StartUp Instance;



   private void Awake() {

      if(Instance!=null) {
         throw new Exception("Design Error: Only one StartUp can exit. Please, delete any duplicates.");
      }
      Instance = this;

      if(PieceBlockPrefab==null) {
         throw new Exception("Design Error: Unassigned reference to the Piece Block Prefab.");
      }

      if(PieceBlockPrefab==null) {
         throw new Exception("Design Error: Unassigned reference to the Wall Block Prefab.");
      }

   }



   public void Convert(Entity entity,EntityManager dstManager,GameObjectConversionSystem conversionSystem) {
      
      var            settings        = GameObjectConversionSettings.FromWorld(dstManager.World,conversionSystem.BlobAssetStore);
      Entity         wallBlockPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(WallBlockPrefab,settings);
      GameControl.BlockPrefab        = GameObjectConversionUtility.ConvertGameObjectHierarchy(PieceBlockPrefab,settings);
      
      int width  = GameControl.Width;
      int height = GameControl.Height;
      int x2     = width+1;
      for(int y=0;y<=height;y++) {
         Entity block = dstManager.Instantiate(wallBlockPrefab);
         dstManager.SetComponentData(block,new Translation { Value = new float3(0,y,0) });
         block = dstManager.Instantiate(wallBlockPrefab);
         dstManager.SetComponentData(block,new Translation { Value = new float3(x2,y,0) });
      }
      for(int x = 1;x<=width;x++) {
         Entity block = dstManager.Instantiate(wallBlockPrefab);
         dstManager.SetComponentData(block,new Translation { Value = new float3(x,0,0) });
      }

      dstManager.DestroyEntity(wallBlockPrefab);
      dstManager.DestroyEntity(entity);

   }



}