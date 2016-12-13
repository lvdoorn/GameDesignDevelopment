using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Credits to https://github.com/adampassey/Unity-Tiled-Importer/blob/master/Assets/Scripts/Tiled/Parser/JSONMapParser.cs

public class MTLLoader : MonoBehaviour
{
  public TextAsset level_file;
  private MTLevel lvl;
  private Dictionary<int, Texture2D> tileset_textures= new Dictionary<int, Texture2D>();

  private Dictionary<int, List<GameObject> > tiles = new Dictionary<int, List<GameObject>>() ;
  private Dictionary<int, GameObject> layer_objects = new Dictionary<int, GameObject>();

  public void Clear()
  {
    Debug.Log("Clearing");
    tileset_textures.Clear();
  
    // destroy layers
    foreach( KeyValuePair<int, GameObject> obj in layer_objects  )
    {
      DestroyImmediate(obj.Value);
    }

    // destroy collision boxes
    foreach(KeyValuePair<int, List<GameObject>> layer_tiles in tiles)
    {
      foreach(GameObject obj in layer_tiles.Value  )
      {
        DestroyImmediate(obj);
      }
      layer_tiles.Value.Clear();
    }
    tiles.Clear();

    layer_objects.Clear();
  }
  public void Load()
  {
    Debug.Log("LoadLevel");
    MTLParser p = new MTLParser();
    lvl = p.Parse(level_file);
    LoadTilesets();
    CreateLevel();
  }
  private void LoadTilesets()
  {
    
    for( int x=0; x< lvl.tilesets.Count;x++)
    {
     // Debug.Log(lvl.tilesets[x].image);
      Texture2D ts = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("assets/Tiledmaps/"+ lvl.tilesets[x].image);

      if (ts == null)
        Debug.Log("Couldn't load tileset");

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

      tileset_textures.Add(x, ts);
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
      List<GameObject> tiles_layer = new List<GameObject>();        

      //Debug.Log(realWidth);
      Texture2D overlay = new Texture2D((int)(realWidth), (int)(realHeight));

      overlay.SetPixels(new Color[realWidth* realHeight]);
   
      for (int x = 0; x < lvl.width; x++)
      {
        for (int y = 0; y < lvl.height; y++)
        {
          int tileType = layer.data[y * lvl.height + x];

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
              if ( (lvl.tilesets[tc].firstgid <= tileType)  && (lvl.tilesets[tc+1].firstgid > tileType))
              {
                ts = tileset_textures[tc];
                mt_ts = lvl.tilesets[tc];
                tc = tileset_textures.Count;

              }
            }
          
          }
        

          Color[] col = overlay.GetPixels((int)((lvl.width - 1) * lvl.tilewidth) - (int)(x * lvl.tilewidth), (int)(y * lvl.tileheight), (int)mt_ts.tilewidth, (int)mt_ts.tileheight);
          Color[] c2 = GetTilePixels(tileType, ts, mt_ts);

          for (int o = 0; o < c2.Length; o++)
          {
            if (c2[o].a != 0.0f)
              col[o] = c2[o];
          
          }
          overlay.SetPixels(32, 32, (int)lvl.tilewidth, (int)lvl.tileheight, c2);
    
          overlay.SetPixels((int)((lvl.width - 1) * lvl.tilewidth) - (int)(x * lvl.tilewidth), (int)(y * lvl.tileheight), (int)mt_ts.tilewidth, (int)mt_ts.tileheight, col);

          // create collision boxes
          if( tileType != 0 )
          {
            if (mt_ts.tiles.ContainsKey(tileType-1) )
            {
              MTTile mt_tile = mt_ts.tiles[tileType-1];

              Debug.Log(tileType-1);
              foreach(KeyValuePair<int, MTObject> iobj in mt_tile.objects)
              {

                float scaleX = 10.0f / (float) (lvl.tilewidth * lvl.width);
                float scaleY = 10.0f / (float)(lvl.tileheight * lvl.height);
                MTObject obj = iobj.Value;
                GameObject go = new GameObject();
                go.name= x.ToString() +" " + y.ToString();
     
                BoxCollider2D b2d =  go.AddComponent<BoxCollider2D>();
                b2d.transform.SetParent(transform);
                b2d.size = new Vector2((float) obj.width *scaleX, (float) obj.height *scaleY);
                Vector3 off = new Vector3( (float)(x * lvl.tilewidth)  *scaleX,- (float)(y * lvl.tileheight) *scaleY, 0);
                off += new Vector3(obj.x*scaleX , -(obj.y)*scaleY, 0);
                off -= new Vector3(-(float)obj.width * scaleX/2.0f, (float)obj.height * scaleY/2.0f, 0);
                off += new Vector3(-5.0f, 5.0f,0);

                Rigidbody2D body = go.AddComponent<Rigidbody2D>();
                body.isKinematic = true;

                b2d.transform.position = transform.position + off;

                tiles_layer.Add(go);
              }
              
            }
          }

        }
      }
      overlay.filterMode = FilterMode.Trilinear;
      overlay.wrapMode = TextureWrapMode.Clamp;
      overlay.Apply();

      GameObject overlayObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
      overlayObject.transform.SetParent(transform);
      overlayObject.transform.localPosition = new Vector3(0.0f,0.0f, 0.01f);
     // overlayObject.layer = 7 + lvl.layers.Count-c;
      layer_objects.Add(c, overlayObject);
      

      overlayObject.transform.localEulerAngles = new Vector3(-90, 0, 0);
      overlayObject.transform.localPosition = new Vector3(0.0f, 0.0f, 0.01f);
      overlayObject.name = "Layer "+ c.ToString();


      MeshRenderer meshRenderer = overlayObject.GetComponent<MeshRenderer>();
    
      Material material = new Material(Shader.Find("Unlit/Transparent"));
      material.mainTexture = overlay;
      meshRenderer.material = material;

      float scale = 1.0f;// ((LevelGridSize.y * TilePixelSize.y) / (LevelGridSize.x * TilePixelSize.x))
     
      meshRenderer.transform.localScale = new Vector3(scale, 1, scale * ((lvl.height * lvl.tileheight) / (lvl.width * lvl.tilewidth)));
      meshRenderer.gameObject.transform.localPosition = transform.localPosition + new Vector3(0.0f, 0.0f, -0.01f*c);

      tiles.Add(c, tiles_layer);

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

      if (mt_ts.image != "Tilesets/scifitiles-sheet.png")
      {
        //Debug.Log(mt_ts.image);
       // Debug.Log(xIndex);
       // Debug.Log(yIndex);
      }
     // Debug.Log(tileType);
      if (tileType >= 85)
      {
       // Debug.Log(mt_ts.image);
        //Debug.Log(xIndex);
       // Debug.Log(yIndex);
      }
      // xIndex = TiledLevel.tilesets[0].columns - xIndex;
      //return tileset_textures[0].GetPixels(0, 0, (int)lvl.tilewidth, (int)lvl.tileheight);
      return ts.GetPixels(xIndex * (int)mt_ts.tilewidth, (yIndex * (int)mt_ts.tileheight), (int)mt_ts.tilewidth, (int)mt_ts.tileheight);
    }
  }

}
