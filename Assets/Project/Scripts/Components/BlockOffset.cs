using Unity.Entities;
using UnityEngine;



namespace AprilGames.DOTS.Tetris {



   [GenerateAuthoringComponent]
   public struct BlockOffset : IComponentData {
      [HideInInspector] public Entity Center;
      [HideInInspector] public int OffsetX;
      [HideInInspector] public int OffsetY;
   }



}
