#if UNITY_EDITOR 

using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using SimpleJSON;



[Serializable]
public class MTLevel
{
  public int height;
  public List<MTLayer> layers;
  public List<MTObjectLayer> objectlayers;
  public int nextobjectid;
  public string orientation;
  public string renderorder;
  public int tileheight;
  public List<MTTileset> tilesets;
  public int tilewidth;
  public string version;
  public int width;
}

[Serializable]
public class MTLayer
{
  public List<int> data;
  public int height;
  public string name;
  public float opacity;
  public string type;
  public bool visible;
  public int width;
  public int x;
  public int y;
  public int level_layer = 0;
}

[Serializable]
public class MTTileset
{
  public int columns;
  public int firstgid;
  public string image;
  public int imageheight;
  public int imagewidth;
  public int margin;
  public string name;
  public int spacing;
  public int tilecount;
  public int tileheight;
  public int tilewidth;
  public Dictionary<int, MTTile> tiles;
}

[Serializable]
public class MTTile
{
   public string draworder;
   public int height;
   public string name;
   public float opacity;
   public string type;
   public bool visible;
   public int width;
   public int x;
   public int y;
   public Dictionary<int, MTObject> objects;

}

[Serializable]
public class MTObject
{
  public float height;
  public string name;
  public int rotation;
  public string type;
  public bool visible;
  public float width;
  public float x;
  public float y;
}

[Serializable]
public class MTObjectLayer
{
  public string draworder;
  public int height;
  public string name;
  public float opacity;
  public string type;
  public bool visible;
  public int width;
  public int x;
  public int y;
  public Dictionary<int, MTGameObject> objects;
  public int level_layer = 0;

}
[Serializable]
public class MTGameObject
{
  public int gid;
  public float height;
  public int id;
  public string name;
  public int rotation;
  public string type;
  public bool visible;
  public float width;
  public float x;
  public float y;
  public string action = "";
  public string animation = "";
  public int switch_layer = -1;
  public string trigger_vote = "";
  public string turn_off = "";
  public string trigger_text = "";
}


  public class MTLParser
{
  public MTLevel Parse(TextAsset level)
  {
    MTLevel l = new MTLevel();
    JSONNode json = JSON.Parse(level.text);

   // Debug.Log(json);

    l.height = json["height"].AsInt;
    l.width = json["width"].AsInt;
    l.nextobjectid = json["nextobjectid"].AsInt;
    l.tileheight = json["tileheight"].AsInt;
    l.tilewidth = json["tilewidth"].AsInt;
    l.orientation = json["orientation"];
    l.renderorder = json["renderorder"];
    l.version = json["version"];

    l.layers = ParseLayers(json["layers"]);
    l.objectlayers = ParseObjectLayers(json["layers"]);
    l.tilesets = ParseTilesets(json["tilesets"]);

   // Debug.Log(l.tilesets.Count);
    
  //public Tileset[] tilesets;


    return l;
  }
  public List<MTLayer> ParseLayers(JSONNode json)
  {
    List<MTLayer> layers = new List<MTLayer>();
    

    //  Debug.Log(json);

    foreach (JSONNode layerNode in json.AsArray)
    {
      if (layerNode["data"].Count > 0)
      {
        MTLayer layer = new MTLayer();

        layer.height = layerNode["height"].AsInt;
        layer.width = layerNode["width"].AsInt;
        layer.x = layerNode["x"].AsInt;
        layer.y = layerNode["y"].AsInt;
        layer.name = layerNode["name"].Value;
        layer.type = layerNode["type"].Value;
        layer.opacity = layerNode["opacity"].AsFloat;
        layer.visible = layerNode["visible"].AsBool;
        layer.data = new List<int>();

        if(layerNode["properties"] != null)
        {
          if (layerNode["properties"]["level_layer"] != null)
            layer.level_layer = layerNode["properties"]["level_layer"].AsInt;
        }

        foreach (JSONNode dataNode in layerNode["data"].AsArray)
          layer.data.Add(dataNode.AsInt);

        layers.Add(layer);
      }     
    }
   
    return layers;
  }
  public List<MTObjectLayer> ParseObjectLayers(JSONNode json)
  {
    List<MTObjectLayer> layers = new List<MTObjectLayer>();


    //  Debug.Log(json);

    foreach (JSONNode layerNode in json.AsArray)
    {
      if (layerNode["objects"].Count > 0)    
      {
        MTObjectLayer layer = new MTObjectLayer();
        layer.height = layerNode["height"].AsInt;
        layer.width = layerNode["width"].AsInt;
        layer.x = layerNode["x"].AsInt;
        layer.y = layerNode["y"].AsInt;
        layer.name = layerNode["name"].Value;
        layer.type = layerNode["type"].Value;
        layer.opacity = layerNode["opacity"].AsFloat;
        layer.visible = layerNode["visible"].AsBool;

        if (layerNode["properties"] != null)
        {
          if (layerNode["properties"]["level_layer"] != null)
            layer.level_layer = layerNode["properties"]["level_layer"].AsInt;
        }

        layer.objects = ParseGameObjects(layerNode["objects"]);
        layers.Add(layer);
      }
    }

    return layers;
  }

  public Dictionary<int , MTGameObject> ParseGameObjects(JSONNode json)
  {
    Dictionary<int, MTGameObject> list = new Dictionary<int, MTGameObject>();

    foreach( JSONNode node in json.AsArray)
    {
      MTGameObject obj = new MTGameObject();
      obj.height = node["height"].AsFloat;
      obj.width = node["width"].AsFloat;
      obj.name = node["name"].Value;
      obj.type = node["type"].Value;
      obj.visible = node["visible"].AsBool;
      obj.rotation = node["rotation"].AsInt;
      obj.x = node["x"].AsFloat;
      obj.y = node["y"].AsFloat;
      obj.gid = node["gid"].AsInt;
      obj.id = node["id"].AsInt;      

      if (node["properties"] != null)
      {
        if (node["properties"]["action"] != null)
          obj.action = node["properties"]["action"];
        if (node["properties"]["animation"] != null)
          obj.animation = node["properties"]["animation"];
        if (node["properties"]["switch_layer"] != null)
          obj.switch_layer = node["properties"]["switch_layer"].AsInt;
        if (node["properties"]["trigger_vote"] != null)
          obj.trigger_vote = node["properties"]["trigger_vote"];
        if (node["properties"]["turn_off"] != null)
          obj.turn_off = node["properties"]["turn_off"];
        if (node["properties"]["trigger_text"] != null)
          obj.trigger_text = node["properties"]["trigger_text"];
      }


      list.Add(node["id"].AsInt, obj);
    }
    return list;
  }

  public List<MTTileset> ParseTilesets(JSONNode json)
  {
    List<MTTileset> tilesets = new List<MTTileset>();    

    foreach (JSONNode tilesetNode in json.AsArray)
    {
      MTTileset ts = new MTTileset();

      ts.columns = tilesetNode["columns"].AsInt;
      ts.firstgid = tilesetNode["firstgid"].AsInt;
      ts.imageheight = tilesetNode["imageheight"].AsInt;
      ts.imagewidth = tilesetNode["imagewidth"].AsInt;
      ts.margin = tilesetNode["margin"].AsInt;
      ts.spacing = tilesetNode["spacing"].AsInt;
      ts.tilecount = tilesetNode["tilecount"].AsInt;
      ts.tileheight = tilesetNode["tileheight"].AsInt;
      ts.tilewidth = tilesetNode["tilewidth"].AsInt;
      ts.image = tilesetNode["image"].Value;
      ts.name = tilesetNode["name"].Value;
      ts.tiles = ParseTiles(tilesetNode["tiles"], ts.tilecount);
      //Debug.Log( ts.tiles.Count);

      // Debug.Log(tilesetNode["tiles"]);

      tilesets.Add(ts);
    }
    return tilesets;
  }
  public Dictionary<int,MTTile> ParseTiles(JSONNode json, int count)
  {
    Dictionary < int, MTTile > tiles = new Dictionary < int, MTTile > ();
  
    for (int x=0 ; x<count ; x++)
    {
      JSONNode node = json[x.ToString()];
      node = node["objectgroup"];
      if (node  != null)
      {
        MTTile ti = new MTTile();

        ti.height = node["height"].AsInt;
        ti.width = node["width"].AsInt;
        ti.x = node["x"].AsInt;
        ti.y = node["y"].AsInt;
        ti.draworder = node["draworder"].Value;
        ti.name = node["name"].Value;
        ti.type = node["type"].Value;
        ti.opacity = node["opacity"].AsFloat;
        ti.visible = node["visible"].AsBool;   
        ti.objects = ParseObjects(node["objects"]);  

        tiles.Add(x, ti);
      }
    }
    return tiles;
  }

  public Dictionary<int, MTObject> ParseObjects(JSONNode node)
  {
    
    Dictionary<int, MTObject> objects = new Dictionary<int, MTObject>();

    foreach (JSONNode objectNode in node.AsArray)
    {
      MTObject obj = new MTObject();

      obj.height = objectNode["height"].AsFloat;
      obj.width = objectNode["width"].AsFloat;
      obj.name = objectNode["name"].Value;
      obj.type = objectNode["type"].Value;
      obj.visible = objectNode["visible"].AsBool;
      obj.rotation = objectNode["rotation"].AsInt;
      obj.x = objectNode["x"].AsFloat;
      obj.y = objectNode["y"].AsFloat;
   

      objects.Add(objectNode["id"].AsInt, obj);
    }

    return objects;
  }


}

#endif