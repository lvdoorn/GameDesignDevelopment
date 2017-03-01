using NDream.AirConsole;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleScript : MonoBehaviour
{

  GameObject[][] sprites_;
  bool[] positions_switched;
  int [] [] rotated;

  public Texture2D mining_texture_ = null;
  public Texture2D bridge_texture_ = null;

  public string type_ = "";

  void Start()
  {
    AirConsole.instance.onMessage += OnMessage;
  }

  void Update()
  {
  }


  public void Init(int num_players, string type)
  {
    type_ = type;
    sprites_ = new GameObject[num_players][];
    positions_switched = new bool[num_players];
    rotated = new int[num_players] [];

    Texture2D tex_used = mining_texture_;
    if (type == "mining_station")
      tex_used = mining_texture_;

    if (type == "bridge")
      tex_used = bridge_texture_;

    int tile_width = (int)tex_used.width / num_players;
    int tile_height = (int)tex_used.height / 2;
    Debug.Log(tile_width);
    float rat =  (float)tile_width / (float)tile_height;

    Vector3 off = new Vector3(-150, 50, 0);

    // split image
    for (int x = 0; x < num_players; x++)
    {
      sprites_[x] = new GameObject[2];
      rotated[x] = new int[2];

      Rect r1 = new Rect((int)tile_width * x, (int)tile_height, (int)tile_width, (int)tile_height);
      Sprite s1_1 = Sprite.Create(tex_used, r1, new Vector2(0.5f, 0.5f));

      GameObject canvas = new GameObject("canvas", typeof(RectTransform));
      Image img = canvas.AddComponent<Image>();
      img.sprite = s1_1;
      sprites_[x][0] = canvas;
      canvas.transform.SetParent(gameObject.transform);

      Rect r2 = new Rect((int)tile_width * x, (int)0, (int)tile_width, (int)tile_height);
      Sprite s2_1 = Sprite.Create(tex_used, r2, new Vector2(0.5f, 0.5f));

      GameObject canvas2 = new GameObject("canvas", typeof(RectTransform));
      Image img2 = canvas2.AddComponent<Image>();
      img2.sprite = s2_1;
      sprites_[x][1] = canvas2;
      canvas2.transform.SetParent(gameObject.transform);

      canvas.GetComponent<RectTransform>().sizeDelta = new Vector2(tile_width, tile_height);
      canvas2.GetComponent<RectTransform>().sizeDelta = new Vector2(tile_width, tile_height);

      canvas.GetComponent<RectTransform>().localPosition = off;
      canvas2.GetComponent<RectTransform>().localPosition = off - new Vector3(0, img2.rectTransform.rect.height *rat +5, 0);
      Debug.Log(img2.rectTransform.rect.width);

      canvas.transform.localScale = new Vector3(1, rat, 1);
      canvas2.transform.localScale = new Vector3(1, rat, 1);

      rotated[x][0] = 0;
      rotated[x][1] = 0;

      img2.color = Color.gray;

      off += new Vector3(img2.rectTransform.rect.width+5, 0, 0);
    }


    for (int x = 0; x < num_players; x++)
    {
      bool switch_it = Random.Range(0, 2) == 1 ? true : false;
      int rotate_it1 = Random.Range(0, 4);
      int rotate_it2 = Random.Range(0, 4);

      rotated[x][0] = rotate_it1;
      rotated[x][1] = rotate_it2;

      sprites_[x][0].transform.Rotate(Vector3.forward * 90 * rotated[x][0]);
      sprites_[x][1].transform.Rotate(Vector3.forward * 90 * rotated[x][1]);


      positions_switched[x] = switch_it;
      Debug.Log(switch_it);
      if (switch_it)
      {
        GameObject c1 = sprites_[x][0];
        GameObject c2 = sprites_[x][1];

        Sprite s1 = c1.GetComponent<Image>().sprite;
        c1.GetComponent<Image>().sprite = c2.GetComponent<Image>().sprite;
        c2.GetComponent<Image>().sprite = s1;

        Quaternion r1 = c1.transform.rotation;
        c1.transform.rotation = c2.transform.rotation;
        c2.transform.rotation = r1;
      }


    }
  }
  // Airconsole handler
  void OnMessage(int from, JToken data)
  {

    int player_number = GameObject.Find("Players").GetComponent<PlayersScript>().GetPlayerNumber(from); 
    if (data["direction"] != null)
    {
      string dir = (string)data["direction"];
      if (dir == "R")
      {
        Rotate(player_number, false);
        if (IsCompleted())
        {
          GameObject.Find("Game").gameObject.GetComponent<GameScript>().GetCurrentLevel().EndPuzzle();
        }
      }
      else if (dir == "L")
      {
        Rotate(player_number, true);
        if (IsCompleted())
        {
          GameObject.Find("Game").gameObject.GetComponent<GameScript>().GetCurrentLevel().EndPuzzle();
        }
      }
      else if (dir == "U")
      {
        SwitchTile(player_number);
        if(IsCompleted())
        {
          GameObject.Find("Game").gameObject.GetComponent<GameScript>().GetCurrentLevel().EndPuzzle();
        }
      }
      else if (dir == "D")
      {
        SwitchTile(player_number);
        if (IsCompleted())
        {
          GameObject.Find("Game").gameObject.GetComponent<GameScript>().GetCurrentLevel().EndPuzzle();
        }
      }
    }
  }

  private void SwitchTile(int x)
  {
    positions_switched[x] =! positions_switched[x];
    bool switch_it = positions_switched[x];
   
    GameObject c1 = sprites_[x][0];
    GameObject c2 = sprites_[x][1];

    Sprite s1 = c1.GetComponent<Image>().sprite;
    c1.GetComponent<Image>().sprite = c2.GetComponent<Image>().sprite;
    c2.GetComponent<Image>().sprite = s1;

    Quaternion r1 = c1.transform.rotation;
    c1.transform.rotation = c2.transform.rotation;
    c2.transform.rotation = r1;

    
  }
  private void Rotate(int x, bool left)
  {
    int target = 0;
    if (positions_switched[x])
      target = 1;
    if (left)
    {
      rotated[x][target] = rotated[x][target] + 1;
      if (rotated[x][target] > 3)      
        rotated[x][target] = 0;
      
      sprites_[x][0].transform.Rotate(Vector3.forward * (90 ));
      Debug.Log(rotated[x][target]);
    }
    else
    {      
      if (rotated[x][target] >0)
        rotated[x][target] = rotated[x][target] - 1;
      else
        rotated[x][target] = rotated[x][target] =3;
      sprites_[x][0].transform.Rotate(Vector3.forward * (-90 ));
      Debug.Log(rotated[x][target]);
    }
   
  }

  public bool IsCompleted()
  {
    for(int x=0; x < positions_switched.Length; x++)
    {
      if(     (rotated[x][0] != 0) ||      (rotated[x][1] != 0))
        Debug.Log(x+"not aligned" + rotated[x][0]+ " "+rotated[x][1] );
  // Debug.Log("0"+rotated[x][0]);
     // Debug.Log("1" + rotated[x][1]);
      if (positions_switched[x])
        return false;
      if (rotated[x][0] != 0)
        return false;
      if (rotated[x][1] != 0)
        return false;
   
    }
    return true;
  }
}
