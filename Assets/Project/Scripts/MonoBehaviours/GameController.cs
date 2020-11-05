using Unity.Entities;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;



namespace AprilGames.DOTS.Tetris {



   public class GameController : MonoBehaviour {



       public delegate void RemoveCallBack(Entity entity);
       public delegate void DropCallBack(Entity entity,int rows);



      [SerializeField] Camera       Cam;
      [SerializeField] GameObject   GameOverUIPrefab;
      [SerializeField] GameObject[] Pieces;
      [SerializeField] Text         Score;
      [SerializeField] int          GridWidth;
      [SerializeField] int          GridHeight;
      [SerializeField] int          GridTopMargin;
      [SerializeField] int          GridRightMargin;
      [SerializeField] float        TickTime;
      [SerializeField] float        InputRepeatTime;

      

      static GameController Instance;
      static GameObject     GameOverUI;
      static int            GridLinearSize;



      public static NativeArray<Entity> Grid;
      public static PieceBlueprint[]    PiecesInfo;
      public static int                 PiecesCount;
      public static int                 Width;
      public static int                 Height;
      public static int                 SpawnX;
      public static int                 SpawnY;
      public static int                 CurrentPiece;
      public static int                 NextPiece;
      public static int                 TopRow;
      public static bool                Overflow;
      public static int                 CurrentScore;
      public static float               Tick_Time;
      public static float               Input_Repeat_Time;
      public static Entity              BlocKPrefab;
      public static EntityArchetype     PieceArchetype;
      public static EntityArchetype     SpawnArchetype;
      public static EntityArchetype     EndTurnArchetype;
      public static EntityArchetype     GameOverArchetype;
      public static EntityArchetype     RestartArchetype;



      private void Awake() {
         if(Instance!=null) {
            Destroy(this);
         }
         Instance = this;

         if(TickTime<0.01) {
            throw new System.InvalidOperationException("Tick Time must be greater than 0.01");
         }

         if(InputRepeatTime<0.01) {
            throw new System.InvalidOperationException("Tick Time must be greater than 0.01");
         }

         float aspect      = (float)Screen.height/Screen.width;
         float width       = GridWidth/2+1+GridRightMargin;
         float height      = width*aspect;
         float totalHeight = GridHeight+GridTopMargin+1;
         if(height*2f<totalHeight) {
            height = totalHeight/2f;
            width  = height/aspect;
         }
         Cam.orthographicSize   = height;
         Cam.transform.position = new float3(width-0.5f,height-0.5f,-10f);

         Tick_Time         = TickTime;
         Input_Repeat_Time = InputRepeatTime;
         SpawnX            = (GridWidth+1)/2;
         SpawnY            = GridHeight;
         GridLinearSize    = GridWidth*GridHeight;
         Grid              = new NativeArray<Entity>(GridLinearSize,Allocator.Persistent);

         PiecesCount = Pieces.Length;
         if(PiecesCount>0) {

            PiecesInfo = new PieceBlueprint[PiecesCount];
            for(int i = 0;i<PiecesCount;i++) {
               GameObject prefab = Pieces[i];
               Transform tprefab = prefab.transform;
               int       blocks  = tprefab.childCount;
               if(blocks>3) {
                  int2[]    map    = new int2[blocks];
                  Transform tchild = null;
                  for(int b = 0;b<blocks;b++) {
                     tchild          = tprefab.GetChild(b);
                     float3 position = tchild.localPosition;
                     if(position.x!=(int)position.x || position.y!=(int)position.y) {
                        throw new System.InvalidOperationException("Piece: "+Pieces[i].name+"->"+tchild.name+" is not located at an integer position.");
                     }
                     map[b] = new int2((int)position.x,(int)position.y);
                  }
                  MeshRenderer rend = tchild.GetComponent<MeshRenderer>();
                  if(rend==null) {
                     throw new System.InvalidOperationException("Last child of "+Pieces[i].name+" does not contain a MeshRenderer.");
                  } else {
                     PiecesInfo[i].PieceMaterial = rend.sharedMaterial;
                     PiecesInfo[i].BlockMap      = map;
                  }
               } else {
                  throw new System.InvalidOperationException("Piece: "+Pieces[i].name+" must have at least 4 child elements.");
               }
            }

         } else {

            throw new System.InvalidOperationException("Pieces array is empty! Please make sure to include at least 1 prefab on this array");

         }

         Width        = GridWidth;
         Height       = GridHeight;
         CurrentPiece = UnityEngine.Random.Range(0,PiecesCount);
         NextPiece    = UnityEngine.Random.Range(0,PiecesCount);

      }



      private void OnDestroy() {
         if(Grid.IsCreated) {
            Grid.Dispose();
         }
      }



      public static void SelectNextPiece() {
         CurrentPiece = NextPiece;
         NextPiece    = UnityEngine.Random.Range(0,PiecesCount);
      }



      public static void Reset() {
         for(int i=0;i<GridLinearSize;i++) {
            Grid[i] = Entity.Null;
         }
         CurrentPiece = UnityEngine.Random.Range(0,PiecesCount);
         NextPiece    = UnityEngine.Random.Range(0,PiecesCount);
         TopRow       = 0;
         Overflow     = false;
         CurrentScore = 0;
      }



      static int LinearPosition(int x,int y) {
         return y*Width+x;
      }

      

      public static bool CanMove(int x,int y,int rotation) {
         PieceBlueprint piece = PiecesInfo[CurrentPiece];
         int2[]         map   = piece.BlockMap;
         int            l     = map.Length;
         x--;
         y--;
         for(int i = 0;i<l;i++) {
            int tx, ty;
            int2 pos = map[i];
            switch(rotation) {
            case 90:
               tx = x+pos.y;
               ty = y-pos.x;
               break;
            case 180:
               tx = x-pos.x;
               ty = y-pos.y;
               break;
            case 270:
               tx = x-pos.y;
               ty = y+pos.x;
               break;
            default:
               tx = x+pos.x;
               ty = y+pos.y;
               break;
            }

            if(tx<0 || tx>=Width || ty<0 || (ty<Height && Grid[LinearPosition(tx,ty)]!=Entity.Null)) {
               return false;
            }
         }
         return true;
      }



      static public void AddToGrid(int x,int y,Entity entity) {
         if(Overflow) {
            return;
         }
         if(y>Height) {
            Overflow = true;
            return;
         }
         x--;
         y--;
         if(y>TopRow) {
            TopRow = y;
         }
         Grid[LinearPosition(x,y)] = entity;
      }



      static public int RemoveFullRows(RemoveCallBack remove,DropCallBack drop) {
         int dropRows = 0;
         for(int y = 0;y<=TopRow;y++) {
            bool full = true;
            for(int x = 0;x<Width;x++) {
               if(Grid[LinearPosition(x,y)]==Entity.Null) {
                  full = false;
                  break;
               }
            }
            if(full) {
               dropRows++;
               for(int x = 0;x<Width;x++) {
                  remove(Grid[LinearPosition(x,y)]);
                  Grid[LinearPosition(x,y)] = Entity.Null;
               }
            } else if(dropRows>0) {
               for(int x = 0;x<Width;x++) {
                  int lpos = LinearPosition(x,y);
                  if(Grid[lpos]!=Entity.Null) {
                     drop(Grid[lpos],dropRows);
                     Grid[LinearPosition(x,y-dropRows)] = Grid[lpos];
                     Grid[lpos]                         = Entity.Null;
                  }
               }
            }
         }
         if(dropRows>0) {
            TopRow              -= dropRows;
            CurrentScore        += dropRows;
            Instance.Score.text  = CurrentScore.ToString();
         }
         return dropRows;
      }



      static public void GameOver() {
         GameOverUI = Instantiate(Instance.GameOverUIPrefab);
      }



      static public void PlayAgain() {
         Reset();
         BeginInitializationEntityCommandBufferSystem bufferSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();
         EntityCommandBuffer                          buffer       = bufferSystem.CreateCommandBuffer();
         buffer.CreateEntity(RestartArchetype);
         Instance.StartCoroutine(Instance.RestartGame());
      }



      IEnumerator RestartGame() {
         yield return null;

         Destroy(GameOverUI);
         yield return null;
         yield return null;

         BeginInitializationEntityCommandBufferSystem bufferSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<BeginInitializationEntityCommandBufferSystem>();
         EntityCommandBuffer buffer = bufferSystem.CreateCommandBuffer();
         Entity              piece  = buffer.CreateEntity(SpawnArchetype);
         buffer.SetComponent(piece,new Spawn {
            Delay = 0
         });
      }


   }



}