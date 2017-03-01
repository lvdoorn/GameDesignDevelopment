using UnityEngine;
using System.Collections;
using System.Collections.Generic;


#if (UNITY_EDITOR)

public class MTLevelLayer : MonoBehaviour
{
  private Dictionary<int, List<GameObject>> collision_boxes = new Dictionary<int, List<GameObject>>();
  private Dictionary<int, GameObject> layer_sprites = new Dictionary<int, GameObject>();
  private Dictionary<int, List<GameObject>> layer_game_objects = new Dictionary<int, List<GameObject>>();
}

public class MTLLoader : MonoBehaviour
{
  public TextAsset level_file;
  public float scale = 1.0f;



  private MTLevel lvl;
  private Dictionary<int, Texture2D> tileset_textures= new Dictionary<int, Texture2D>();
  private Dictionary<int, List<GameObject> > tiles = new Dictionary<int, List<GameObject>>() ;
  private Dictionary<int, GameObject> layer_objects = new Dictionary<int, GameObject>();
  private Dictionary<int, List<GameObject>> layer_game_objects = new Dictionary<int, List<GameObject>>();  
  private Dictionary<int, GameObject> level_layers = new Dictionary<int, GameObject>();
  private Dictionary<int, Sprite> tile_sprites = new Dictionary<int, Sprite>();

  private MeshFilter mf;
  private MeshRenderer mr;
  private MeshCollider mc;

  public Shader shad_;//= Shader.Find("Unlit/Transparent");

  //public Shader dummy = Material.S

  public void Clear()
  {
    Debug.Log("Clearing");
    tileset_textures.Clear();
  
    // destroy layers
    foreach( KeyValuePair<int, GameObject> obj in layer_objects  )
    {
      DestroyImmediate(obj.Value);
    }
    layer_objects.Clear();

    // destroy collision boxes
    foreach (KeyValuePair<int, List<GameObject>> layer_tiles in tiles)
    {
      foreach(GameObject obj in layer_tiles.Value  )
      {
        DestroyImmediate(obj);
      }
      layer_tiles.Value.Clear();
    }
    tiles.Clear();

    // destroy objects
    foreach (KeyValuePair<int, List<GameObject>> layer_game_tiles in layer_game_objects)
    {
      foreach (GameObject obj in layer_game_tiles.Value)
      {
        DestroyImmediate(obj);
      }
      layer_game_tiles.Value.Clear();
    }
    layer_game_objects.Clear();

    // destroy level layers
    foreach (KeyValuePair<int, GameObject> level_layer in level_layers)
    {
      DestroyImmediate(level_layer.Value);
    }
    level_layers.Clear();
  }
  public void Load()
  {
    transform.localScale = new Vector3(scale,scale,scale);
    LevelScript ls =   this.gameObject.GetComponent<LevelScript>();
    //name = "Level";
    Debug.Log("LoadLevel");
    MTLParser p = new MTLParser();
    lvl = p.Parse(level_file);
    LoadTilesets();
    Debug.Log("Tileset loaded");
    CreateLevel();
    Debug.Log("Level created");
    if (lvl.objectlayers != null)
      CreateLevelObjects();
    ls.Init();
  }
  private void LoadTilesets()
  {
    shad_ = Shader.Find("Unlit/Transparent");
    for ( int x=0; x< lvl.tilesets.Count;x++)
    {
      // Debug.Log(lvl.tilesets[x].image);
      string [] ps = lvl.tilesets[x].image.Split('.');
      Texture2D ts = (Texture2D) Resources.Load("Tiledmaps/"+ ps[0]) as Texture2D;
      ts.filterMode = FilterMode.Point;

      if (ts == null)
        Debug.Log("Couldn't load tileset");

      if (!lvl.tilesets[x].image.StartsWith("Tilesets/animation"))
      {

        int realWidth = lvl.tilewidth * lvl.width;
        int realHeight = lvl.tileheight * lvl.height;

        Texture2D flipped = new Texture2D(ts.width, ts.height);

        for (int i = 0; i < ts.width; i++)
        {
          for (int j = 0; j < ts.height; j++)
          {
            flipped.SetPixel(ts.width - i - 1, ts.height - j - 1, ts.GetPixel(i, j));
          }
        }

        flipped.Apply();
        ts = flipped;
      }

      tileset_textures.Add(x, ts);
    }
  }

  public void CreateLevelObjects()
  {
    int c = 0;

  

    foreach (MTObjectLayer layer in lvl.objectlayers)
    {
      GameObject level_layer;
      GameObject level_layer_objects;

      if (level_layers.ContainsKey(layer.level_layer))
      {
        level_layer = level_layers[layer.level_layer];
        level_layer_objects = level_layer.transform.GetChild(2).gameObject;
      }
      else
      {
        level_layer = new GameObject();
        level_layer.name = "LevelLayer" + layer.level_layer.ToString();
        level_layer.transform.SetParent(transform);
        level_layers.Add(layer.level_layer, level_layer);

        GameObject level_layer_tile_layer = new GameObject();
        GameObject level_layer_collision_boxes = new GameObject();

        level_layer_tile_layer.transform.SetParent(level_layer.transform);
        level_layer_tile_layer.name = "TileLayers";
        //level_layer_tile_layer.transform.position = new Vector3(0, 0, .0f);

        level_layer_collision_boxes.transform.SetParent(level_layer.transform);
        level_layer_collision_boxes.name = "Tiles";

        level_layer_objects = new GameObject();
        level_layer_objects.transform.SetParent(level_layer.transform);
        level_layer_objects.name = "Objects";
      //  level_layer_objects.transform.position = new Vector3(0, 0, 1.0f);
      }
           

      List<GameObject> objects_layer = new List<GameObject>();

      foreach(KeyValuePair<int, MTGameObject> iobj in layer.objects)
      {
        MTGameObject obj = iobj.Value;
        
        GameObject main_obj = new GameObject();
        main_obj.name = obj.name;

        main_obj.transform.SetParent(level_layer_objects.transform);
    

        Texture2D ts = tileset_textures[0];
        MTTileset mt_ts = lvl.tilesets[0];
        for (int tc = 0; tc < tileset_textures.Count; tc++)
        {
          if (tc + 1 == tileset_textures.Count)
          {
            ts = tileset_textures[tc];
            mt_ts = lvl.tilesets[tc];
          }
          else
          {
            if ((lvl.tilesets[tc].firstgid <= obj.gid) && (lvl.tilesets[tc + 1].firstgid > obj.gid))
            {
              ts = tileset_textures[tc];
              mt_ts = lvl.tilesets[tc];
              tc = tileset_textures.Count;

            }
          }
        }

        float scaleX = (10.0f * scale) / (float)(lvl.tilewidth * lvl.width);
        float scaleY = (10.0f * scale) / (float)(lvl.tileheight * lvl.height);

        if (obj.animation != "")
        {
          Debug.Log("Anim request");
          Animator animator = main_obj.AddComponent<Animator>();

          SpriteRenderer sr = main_obj.AddComponent<SpriteRenderer>();
          Rect r = new Rect(0, 0, 32 , 32);


         // string spriteSheet = UnityEditor.AssetDatabase.GetAssetPath("Assets/TiledMaps/animations.png");
         // Object[] sprites = UnityEditor.AssetDatabase.LoadAllAssetsAtPath("Assets/TiledMaps/animations.png");
          Object[] sprites = Resources.LoadAll("TiledMaps/Tilesets/animations_png") ;

          Debug.Log(sprites.Length);
          Sprite sprite = Sprite.Create(ts, r, new Vector2(0.0f, 0.0f));


          if (sprite == null)
          {
            Debug.Log("couldnt create sprite");
          }
          sr.sprite = sprites[0] as Sprite;


          RuntimeAnimatorController animatcontroller = Resources.Load("Tiledmaps/Tilesets/SpecialEffects") as RuntimeAnimatorController;

          main_obj.transform.localScale = new Vector3(scale, scale, 1);
          main_obj.transform.localPosition = new Vector3(obj.x * scaleX / (10*scale),  0, obj.y * scaleY / (10*scale));
          main_obj.transform.position =  - new Vector3((10*scale) / 2.0f,-(10*scale) / 2.0f, 0) + new Vector3(obj.x * scaleX, -obj.y * scaleY, 0) - new Vector3(-(float)obj.width * scaleX / 2.0f, -(float)obj.height * scaleY / 2.0f, 0);//new Vector3(obj.x * scaleX, 0, -obj.y * scaleY); + new Vector3(-( scale) / 2.0f, 0, (scale) / 2.0f) - new Vector3(-(float)obj.width * scaleX / 2.0f,  0, -(float)obj.height * scaleY / 2.0f);
          main_obj.AddComponent<ObjectScript>();

          if (animatcontroller != null)
          {

            animator.runtimeAnimatorController = animatcontroller;           
            animator.enabled = true;
            animator.Play(obj.animation);


            Debug.Log("Loaded anim");
          }
          else
            Debug.Log("Couldn't load anim");

          // create collision boxes
          if (obj.gid != 0)
          {
            Debug.Log("Create Collision");
            Debug.Log("TC" +mt_ts.tilecount);
            if (mt_ts.tiles.ContainsKey(obj.gid - 1))
            {
              MTTile mt_tile = mt_ts.tiles[obj.gid - 1];

              bool f = true;

              foreach (KeyValuePair<int, MTObject> iobj2 in mt_tile.objects)
              {
                MTObject obj2 = iobj2.Value;
                Debug.Log("Create Collision ___");
                BoxCollider2D b2d = main_obj.AddComponent<BoxCollider2D>();
                b2d.transform.SetParent(main_obj.transform);
                b2d.size = new Vector2((float)obj2.width * scaleX, (float)obj2.height * scaleY);
                // Vector3 off = new Vector3((float)(x * lvl.tilewidth) * scaleX, -(float)(y * lvl.tileheight) * scaleY, 0);
                // off += new Vector3(obj2.x * scaleX, -(obj2.y) * scaleY, 0);
                // off -= new Vector3(-(float)obj2.width * scaleX / 2.0f, (float)obj2.height * scaleY / 2.0f, 0);
                // off += new Vector3(-5.0f, 5.0f, 0);
                // b2d.offset = main_obj.transform.position;                
                if (f)
                {
                  Rigidbody2D body = main_obj.AddComponent<Rigidbody2D>();
                  body.isKinematic = true;
                }


                b2d.transform.position = main_obj.transform.position;
                f = false;
              }

            }
          }

          ObjectScript objscript = main_obj.AddComponent<ObjectScript>();
          objscript.action = obj.action;
          objscript.switch_layer = obj.switch_layer;
          objscript.turn_off = obj.turn_off;
          objscript.trigger_vote = obj.trigger_vote;
          objscript.trigger_text = obj.trigger_text;
          objscript.item = obj.item;

          if (obj.sound != "")
          {
            string[] parts = obj.sound.Split('|');

            AudioSource audio_src = main_obj.AddComponent<AudioSource>();

            AudioClip clip1 = (AudioClip)Resources.Load("Sound/" + parts[1]);
            audio_src.clip = clip1;
       

            if (parts[0] == "once")
            {
              audio_src.Stop();
              objscript.trigger_audio = "once";
              audio_src.playOnAwake = false;
            }
            else
            {
              audio_src.loop = true;
              audio_src.Play();
            }
          }


          }
          else
        {
          Texture2D overlay = new Texture2D((int)(obj.width), (int)(obj.height));

          overlay.SetPixels(new Color[(int)obj.width * (int)obj.height]);

          Color[] c2 = GetTilePixels(obj.gid, ts, mt_ts);

          overlay.SetPixels(0, 0, (int)mt_ts.tilewidth, (int)mt_ts.tileheight, c2);

          //overlay.filterMode = FilterMode.Trilinear;
          overlay.filterMode = FilterMode.Point;

          overlay.wrapMode = TextureWrapMode.Clamp;
          overlay.Apply();

       
          GameObject gobj = GameObject.CreatePrimitive(PrimitiveType.Plane);

         
          gobj.name = main_obj.name + "_Render";
          gobj.transform.SetParent(main_obj.transform);
          gobj.transform.localEulerAngles = new Vector3(-90, 0, 0);

          MeshRenderer meshRenderer = gobj.GetComponent<MeshRenderer>();

          DestroyImmediate(gobj.GetComponent<MeshCollider>());


          Material material = new Material(shad_);
          material.mainTexture = overlay;
          meshRenderer.material = material;


          main_obj.transform.position = new Vector3(obj.x * scaleX, -obj.y * scaleY, 0) + new Vector3(-(10.0f * scale) / 2.0f, (10.0f * scale) / 2.0f, 0) - new Vector3(-(float)obj.width * scaleX / 2.0f, -(float)obj.height * scaleY / 2.0f, 0);
          gobj.transform.localScale = new Vector3(scaleX * mt_ts.tilewidth / (10.0f), 1, scaleY * mt_ts.tileheight / (10.0f));
          gobj.transform.localPosition = new Vector3(obj.x * scaleX / (10.0f * scale), obj.y * scaleY / (10.0f * scale), 0);
          gobj.transform.position = new Vector3(obj.x * scaleX, -obj.y * scaleY, 0) + new Vector3(-(10.0f * scale) / 2.0f, (10.0f * scale) / 2.0f, 0) - new Vector3(-(float)obj.width * scaleX / 2.0f, -(float)obj.height * scaleY / 2.0f, 0);
          //  meshRenderer.transform.localScale = new Vector3(scale, 1, scale * ((mt_ts.tileheight) / (mt_ts.tilewidth)));
          // meshRenderer.gameObject.transform.localPosition = transform.localPosition + new Vector3(0.0f, 0.0f, -0.01f * c);

          ObjectScript objscript = main_obj.AddComponent<ObjectScript>();
          objscript.action = obj.action;
          objscript.switch_layer = obj.switch_layer;
          objscript.turn_off = obj.turn_off;
          objscript.trigger_vote = obj.trigger_vote;
          objscript.trigger_text = obj.trigger_text;
          objscript.item = obj.item;

          if ( obj.sound != ""  )
          {
            string[] parts = obj.sound.Split('|');

            AudioSource audio_src = main_obj.AddComponent<AudioSource>();

            AudioClip clip1 = (AudioClip)Resources.Load("Sound/"+parts[1]);
            audio_src.clip = clip1;
      

            if (parts[0] == "once")
            {
              audio_src.Stop();
              objscript.trigger_audio = "once";
              audio_src.playOnAwake = false;
            }
            else
            {
              audio_src.loop = true;
              audio_src.Play();
            }

           
          }

          // create collision boxes
          if (obj.gid != 0)
          {
           // Debug.Log("Create Collision");
            if (mt_ts.tiles.ContainsKey(obj.gid - 1))
            {
              MTTile mt_tile = mt_ts.tiles[obj.gid - 1];

              bool f = true;

              foreach (KeyValuePair<int, MTObject> iobj2 in mt_tile.objects)
              {
                MTObject obj2 = iobj2.Value;

                BoxCollider2D b2d = main_obj.AddComponent<BoxCollider2D>();
                b2d.transform.SetParent(main_obj.transform);
                b2d.size = new Vector2((float)obj2.width * scaleX, (float)obj2.height * scaleY);
                Vector3 off = new Vector3(-(obj.width - obj2.width) * scaleX / 2.0f, (obj.height - obj2.height) * scaleY / 2.0f, 0);
                off += new Vector3(obj2.x * scaleX, -obj2.y * scaleY, 0);
                b2d.offset = off;

                if (f)
                {
                  Rigidbody2D body = main_obj.AddComponent<Rigidbody2D>();
                  body.isKinematic = true;
                }


                b2d.transform.position = main_obj.transform.position;
                f = false;
              }

            }
          }
        }

       

        objects_layer.Add(main_obj);
      }


      layer_game_objects.Add(c, objects_layer);
      c += 1;
    }
  }

   public void CreateLevel()
 { 
   int realWidth = lvl.tilewidth * lvl.width;
   int realHeight = lvl.tileheight * lvl.height;
   tiles = new Dictionary<int, List<GameObject>>();
   int c = 0;


   foreach (MTLayer layer in lvl.layers)
   {
     GameObject level_layer;
     GameObject level_layer_tile_layer;
     GameObject level_layer_collision_boxes ;

     if (level_layers.ContainsKey(layer.level_layer))
     {
       level_layer = level_layers[layer.level_layer];
       level_layer_tile_layer = level_layer.transform.GetChild(0).gameObject;
       level_layer_collision_boxes = level_layer.transform.GetChild(1).gameObject;
     }
     else
     {
        level_layer = new GameObject();
        level_layer.name = "LevelLayer" + layer.level_layer.ToString();
        level_layer.transform.SetParent(transform);
        level_layers.Add(layer.level_layer, level_layer);

        level_layer_tile_layer = new GameObject();
        level_layer_collision_boxes = new GameObject();

        level_layer_tile_layer.transform.SetParent(level_layer.transform);
        level_layer_tile_layer.name = "TileLayers";
      //  level_layer_tile_layer.transform.position = new Vector3(0, 0, 4.0f);

        level_layer_collision_boxes.transform.SetParent(level_layer.transform);
        level_layer_collision_boxes.name = "Tiles";
       // level_layer_collision_boxes.transform.position = new Vector3(0, 0, 3.0f);

        GameObject level_layer_objects = new GameObject();
        level_layer_objects.transform.SetParent(level_layer.transform);
        level_layer_objects.name = "Objects";
       // level_layer_objects.transform.position = new Vector3(0, 0, 1.0f);
      }

     List<GameObject> tiles_layer = new List<GameObject>();

    GameObject overlayObject = new GameObject();
    if (overlayObject == null)
      Debug.Log("couldn't create overlay");

    overlayObject.transform.SetParent(level_layer_tile_layer.transform);
    overlayObject.transform.localPosition = new Vector3(0.0f, 0.0f, 1-0.01f *level_layer_tile_layer.transform.childCount);
    // overlayObject.layer = 7 + lvl.layers.Count-c;
    layer_objects.Add(c, overlayObject);


    overlayObject.transform.localEulerAngles = new Vector3(-90, 0, 0);
    overlayObject.transform.localPosition = new Vector3(0.0f, 0.0f,1-0.01f * level_layer_tile_layer.transform.childCount);
    overlayObject.name = layer.name;


     for (int x = 0; x < lvl.width; x++)
     {
       for (int y = 0; y < lvl.height; y++)
       {
         int tileType = layer.data[y * lvl.height + x];

         // get the right tileset
         Texture2D ts = tileset_textures[0];
         ts.filterMode = FilterMode.Point;
         MTTileset mt_ts = lvl.tilesets[0];

         for (int tc = 0; tc < tileset_textures.Count; tc++)
         {
           if (tc + 1 == tileset_textures.Count)
           {
             ts = tileset_textures[tc];
             mt_ts = lvl.tilesets[tc];
           }
           else
           {
             if ( (lvl.tilesets[tc].firstgid <= tileType)  && (lvl.tilesets[tc+1].firstgid > tileType))
             {
               ts = tileset_textures[tc];
               mt_ts = lvl.tilesets[tc];
               tc = tileset_textures.Count;
             }
           }          
         }


         if (tileType != 0)
         {
           Sprite sp;
           if (tile_sprites.ContainsKey(tileType))
           {
             sp = tile_sprites[tileType];
           }
           else
           {

             int xIndex = (tileType - mt_ts.firstgid) % mt_ts.columns;
             int yIndex = (tileType - mt_ts.firstgid) / mt_ts.columns;
             xIndex = mt_ts.columns - xIndex - 1;

             // add new sprite            
             Rect tr = new Rect(xIndex * (int)mt_ts.tilewidth, (yIndex * (int)mt_ts.tileheight), (int)mt_ts.tilewidth, (int)mt_ts.tileheight);
             sp = Sprite.Create(ts, tr, new Vector2(0, 0));

             Sprite[] sprites = Resources.LoadAll<Sprite>("Tiledmaps/Tilesets/"+mt_ts.name);
              Debug.Log(tileType - mt_ts.firstgid);
              Debug.Log(mt_ts.name);
              Debug.Log(sprites.Length);

              tile_sprites[tileType] = sprites[tileType- mt_ts.firstgid] ;
              sp = sprites[tileType - mt_ts.firstgid];
            }


           GameObject got = new GameObject();
           got.transform.SetParent(overlayObject.transform);

           float scaleX = (10.0f * this.scale) / (float)(lvl.tilewidth * lvl.width);
           float scaleY = (10.0f * this.scale) / (float)(lvl.tileheight * lvl.height);

   
           got.transform.localPosition = new Vector3(x * mt_ts.tilewidth * scaleX,0, -y * mt_ts.tileheight * scaleY) + new Vector3(-(10.0f * scale) / 2.0f,0, (10.0f * scale) / 2.0f) - new Vector3(-(float)32 * scaleX / 2.0f, 0,(float)32 * scaleY / 2.0f);
            float sn = scaleX * (1.0f / sp.bounds.size.x) * mt_ts.tilewidth;
         
           got.transform.localScale = new Vector3( sn,sn, 1.0f);

          //  Debug.Log(sp.bounds.size.x);

           SpriteRenderer sr = got.AddComponent<SpriteRenderer>();
           sr.sprite = sp;
           //sr.flipY = true;

          }  

         // create collision boxes
         if( tileType != 0 )
         {
           if (mt_ts.tiles.ContainsKey(tileType-1) )
           {
             MTTile mt_tile = mt_ts.tiles[tileType-1];    

             foreach (KeyValuePair<int, MTObject> iobj in mt_tile.objects)
             {
               float scaleX = (10.0f * this.scale) / (float) (lvl.tilewidth * lvl.width);
               float scaleY = (10.0f * this.scale) / (float)(lvl.tileheight * lvl.height);
               MTObject obj = iobj.Value;

               BoxCollider2D b2d = level_layer_collision_boxes.AddComponent<BoxCollider2D>();
      
               b2d.size = new Vector2((float) obj.width *scaleX, (float) obj.height *scaleY);
               Vector3 off = new Vector3( (float)(x * lvl.tilewidth)  *scaleX,- (float)(y * lvl.tileheight) *scaleY, 0);
               off += new Vector3(obj.x*scaleX , -(obj.y)*scaleY, 0);
               off -= new Vector3(-(float)obj.width * scaleX/2.0f, (float)obj.height * scaleY/2.0f, 0);
               off += new Vector3(-(10.0f * this.scale) / 2.0f, (10.0f * this.scale) / 2.0f, 0);

               if (level_layer_collision_boxes.GetComponent<Rigidbody2D>() == null)
               {

                 Rigidbody2D body = level_layer_collision_boxes.AddComponent<Rigidbody2D>();
                 body.isKinematic = true;

               }
               b2d.offset =  off;    
             }

           }
         }

       }
     }


     tiles.Add(c, tiles_layer);
     Debug.Log("Layer fin");

     c += 1;
   }

 } 


  Color[] GetTilePixels(int tileType, Texture2D ts, MTTileset mt_ts)
  {
    
    if (tileType == 0)
    {
      // tiletype 0 means we have a background color. Return an empty color array.
      return new Color[(int)mt_ts.tilewidth * (int)mt_ts.tileheight];
    }
    else
    {   

      int xIndex = (tileType - mt_ts.firstgid ) % mt_ts.columns;
      int yIndex = (tileType - mt_ts.firstgid ) / mt_ts.columns;
      xIndex = mt_ts.columns - xIndex -1;

      return ts.GetPixels(xIndex * (int)mt_ts.tilewidth, (yIndex * (int)mt_ts.tileheight), (int)mt_ts.tilewidth, (int)mt_ts.tileheight);
    }
  }

}
#endif