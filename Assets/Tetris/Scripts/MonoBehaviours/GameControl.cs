using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using System;



public class GameControl : MonoBehaviour {



   [SerializeField] Camera Cam;
   [SerializeField] int    GridWidth;
   [SerializeField] int    GridHeight;
   [SerializeField] int    GridTopMargin;
   [SerializeField] int    GridRightMargin;



   static GameControl Instance;
   static Entity      _BlockPrefab;



   public static Entity BlockPrefab { get { return _BlockPrefab; } set { if(_BlockPrefab==Entity.Null) _BlockPrefab = value; }  }
   public static int    Width       { get; private set; }
   public static int    Height      { get; private set; }



   private void Awake() {

      if(Instance!=null) {
         throw new Exception("Design Error: Only one GameControl can exit. Please, delete any duplicates.");
      }
      Instance = this;

      if(Cam==null) {
         throw new Exception("Design Error: Reference to the main camera is emply.");
      }

      if(GridWidth<8) {
         throw new Exception("Design Error: Grid width must be greater than 7.");
      } else if(GridWidth>16) {
         throw new Exception("Design Error: Grid width must be smaller than 17.");
      }

      if(GridHeight<12) {
         throw new Exception("Desgin Error: Grid height must be greater than 11.");
      } else if(GridHeight>30) {
         throw new Exception("Desgin Error: Grid height must be smaller than 31.");
      }

      if(GridTopMargin<0) {
         throw new Exception("Desgin Error: Grid top margin must be equal or greater than 0.");
      } else if(GridTopMargin>6) {
         throw new Exception("Desgin Error: Grid top margin must be smaller than 7.");
      }

      if(GridRightMargin<0) {
         throw new Exception("Desgin Error: Grid right margin must be equal or greater than 0.");
      } else if(GridRightMargin>6) {
         throw new Exception("Desgin Error: Grid right margin must smaller than 7.");
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
      Width                  = GridWidth;
      Height                 = GridHeight;
      
   }



}