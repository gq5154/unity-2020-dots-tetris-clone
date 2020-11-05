using Unity.Entities;
using UnityEngine;



namespace AprilGames.DOTS.Tetris {


   
   public class GameInputSystem : ComponentSystem {



      float Tick;
      float InputTime;
      Entity DeleteEntity;
      BeginInitializationEntityCommandBufferSystem BufferSystem;



      protected override void OnCreate() {
         BufferSystem = World.GetOrCreateSystem<BeginInitializationEntityCommandBufferSystem>();
      }



      protected override void OnUpdate() {

         bool move = false;
         bool stop = false;

         Entities.ForEach((Entity entity,ref PiecePosition position) => {

            if(position.Droped) {

               Tick = GameController.Tick_Time;

            } else if(InputTime<=0f) {

               if(Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow)) {
                  if(GameController.CanMove(position.CenterX+1,position.CenterY,position.Rotation)) {
                     position.CenterX++;
                     move = true;
                  }
                  InputTime = GameController.Input_Repeat_Time;
               } else if(Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow)) {
                  if(GameController.CanMove(position.CenterX-1,position.CenterY,position.Rotation)) {
                     position.CenterX--;
                     move = true;
                  }
                  InputTime = GameController.Input_Repeat_Time;
               }

               if(Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow)) {
                  int rot = position.Rotation+90;
                  if(rot>270) {
                     rot = 0;
                  }
                  if(GameController.CanMove(position.CenterX,position.CenterY,rot)) {
                     position.Rotation = rot;
                     move              = true;
                  }
                  InputTime = GameController.Input_Repeat_Time;
               } else if(Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow)) {
                  int rot = position.Rotation-90;
                  if(rot<0) {
                     rot = 270;
                  }
                  if(GameController.CanMove(position.CenterX,position.CenterY,rot)) {
                     position.Rotation = rot;
                     move              = true;
                  }
                  InputTime = GameController.Input_Repeat_Time;
               }

               if(Input.GetKey(KeyCode.Space)) {
                  position.Droped = true;
                  InputTime       = GameController.Input_Repeat_Time;
               }

            } else {

               InputTime -= Time.DeltaTime;

            }

            if(move) {
               Tick = 0f;
            } else {
               Tick += Time.DeltaTime;
               if(Tick>GameController.Tick_Time) {
                  if(GameController.CanMove(position.CenterX,position.CenterY-1,position.Rotation)) {
                     position.CenterY--;
                     move = true;
                     Tick = 0f;
                  } else {
                     stop         = true;
                     DeleteEntity = entity;
                  }
               }
            }

         });

         if(stop) {
            EntityCommandBuffer buffer = BufferSystem.CreateCommandBuffer();
            buffer.DestroyEntity(DeleteEntity);
            Entities.ForEach((Entity entity,ref BlockOffset offset) => {
               buffer.AddComponent(entity,typeof(TagAddToGrid));
            });
            buffer.CreateEntity(GameController.EndTurnArchetype);
         } else if(move) {
            EntityCommandBuffer buffer = BufferSystem.CreateCommandBuffer();
            Entities.ForEach((Entity entity,ref BlockOffset offset) => {
               buffer.AddComponent(entity,typeof(TagMove));
            });
         }

      }



   }



}