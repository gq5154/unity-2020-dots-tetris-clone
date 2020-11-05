using Unity.Entities;



namespace AprilGames.DOTS.Tetris {



   public class RestartSystem : SystemBase {



      EntityQuery PieceQuery;



      protected override void OnCreate() {
         RequireSingletonForUpdate<TagRestart>();
         PieceQuery = EntityManager.CreateEntityQuery(typeof(TagPiece));
      }



      protected override void OnUpdate() {
         EntityManager.DestroyEntity(PieceQuery);
         EntityManager.DestroyEntity(GetSingletonEntity<TagRestart>());
      }



   }



}