using Unity.Entities;



namespace AprilGames.DOTS.Tetris {



   public struct PiecePosition : IComponentData {
      public int CenterX;
      public int CenterY;
      public int Rotation;
      public bool Droped;
   }



}