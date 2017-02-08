using UnityEngine;
using System.Collections;

using NDream.AirConsole;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

public class PlayerScript : MonoBehaviour
{

  private Rigidbody2D rb_;
  private bool moved_;
  private Animator anim_;
  private int id_;
  private string movement_;
  public bool has_focus_;
  public int layer_ = 0;
  private bool flipped_ = false;
  private Vector3 offset;

  private int selected_answer = -1;
  public bool holds_trigger = false;

  private string [] items_ = new string[4];

  private GameScript game_;

  void Start()
  {
    rb_ = GetComponent<Rigidbody2D>();
    moved_ = false;
    anim_ = GetComponent<Animator>();
    //anim_.enabled = false;
    movement_ = "S";
    has_focus_ = false;
    offset = new Vector3(((float)gameObject.GetComponent<SpriteRenderer>().sprite.texture.width * gameObject.transform.localScale.x) / (gameObject.GetComponent<SpriteRenderer>().sprite.pixelsPerUnit * 10), 0, 0);
    game_ = GameObject.Find("Game").GetComponent<GameScript>();

    AirConsole.instance.onMessage += OnMessage;

    for (int x = 0; x < 4; x++)
    {
      items_[x] = "";
    }
  }


  void Update()
  {
    bool m = false;

    LevelScript ls = game_.GetCurrentLevel();
    if (ls != null)
    {
      if (!ls.IsInVoteMode() && game_.State == GameState.PLAY)
      {
        // set the movement
        if (movement_ == "R")
        {
          rb_.AddForce(new Vector2(5, 0));
          gameObject.GetComponent<SpriteRenderer>().flipX = false;
          if (flipped_)
          {
            transform.position -= offset;
            gameObject.GetComponent<BoxCollider2D>().offset += new Vector2(offset.x, offset.y) / gameObject.transform.localScale.x;
          }

          flipped_ = false;
          m = true;
        }
        if (movement_ == "L")
        {
          rb_.AddForce(new Vector2(-5, 0));
          gameObject.GetComponent<SpriteRenderer>().flipX = true;
          if (!flipped_)
          {
            transform.position += offset;
            gameObject.GetComponent<BoxCollider2D>().offset -= new Vector2(offset.x, offset.y) / gameObject.transform.localScale.x;
          }

          flipped_ = true;
          m = true;
        }
        if (movement_ == "U")
        {
          rb_.AddForce(new Vector2(0, 5));
          m = true;
        }
        if (movement_ == "D")
        {
          rb_.AddForce(new Vector2(0, -5));
          m = true;
        }

        // if focused -> move camera
        if (has_focus_)
        {
          Camera cam = GameObject.Find("MainCamera").GetComponent<Camera>();
          Vector3 p = getCenteredPosition();
          cam.transform.position = new Vector3(p.x, p.y, -10);// + new Vector2(50,50) ;
        }
        if (moved_)
        {
          GameObject.Find("Game").GetComponent<GameScript>().GetCurrentLevel().CheckMoveTrigger(this.gameObject);
        }

        // animate it
        if (m && (!moved_))
        {
          //anim_.Stop();
          anim_.Play("WalkSide");

          //anim_.enabled = true;
        }
        if (!m && moved_)
        {
          //anim_.Stop();
          anim_.Play("Idle");
          //anim_.enabled = false;
        }
        moved_ = m;
      }
    }


  }

  // Airconsole handler

  void OnMessage(int from, JToken data)
  {
    // Debug.Log((string)data["direction"]);
    if (from == id_)
    {

      if (data["direction"] != null)
      {
        string dir = (string)data["direction"];
        movement_ = dir;
      }
      if (data["action"] != null)
      {
        Debug.Log(data["action"]);
        if (((int)data["action"]) == 1)
        {
          selected_answer = 0;
        }
        if (((int)data["action"]) == 2)
        {
          selected_answer = 1;
        }
      }
      if (data["vote"] != null)
      {
        Debug.Log(data["vote"]);
        if (((int)data["vote"]) == 1)
        {
          selected_answer = 0;
        }
        if (((int)data["vote"]) == 2)
        {
          selected_answer = 1;
        }
      }
      if (data["itemUsed"] != null)
      {
        Debug.Log("used item");
        Debug.Log(items_[(int)data["itemUsed"]]);
        string it = items_[(int)data["itemUsed"]];
        if (it != ""  )
        {
          if (it ==  "fire_extinguisher")
          {
            GameObject.Find("Game").GetComponent<GameScript>().GetCurrentLevel().ExtinguishFires(gameObject.transform.position);
          }
        }
      }
    }
  }

  // publics 

    // id getter setter
  public void setId(int id)
  {
    id_ = id;
  }
  public int getId()
  {
    return id_;
  }

  public Vector3 getCenteredPosition()
  {
    Vector3 v = transform.position;
    if (flipped_)
      v -= offset  / 2.0f;
    else
      v += offset / 2.0f;
    return v;

  }
  public void setLayer(int layer)
  {
    layer_ = layer;
  }
  public bool hasFocus()
  {
    return has_focus_;
  }

  public void resetVote()
  {
    selected_answer = -1;
  }
  public int getVote()
  {
    return selected_answer;
  }

  public void addItem(string item)
  {
    for(int x=0; x< 4; x++)
    {
      if(items_[x] == "")
      {
        items_[x] = item;
        var message = new
        {
          addItem = item,
          slot = x
        };
        AirConsole.instance.Message(id_, message);
        Debug.Log("sent item add");
        x = 4;
      }
    }
  }
  public void removeItem(string item)
  {
    for (int x = 0; x < 4; x++)
    {
      if (items_[x] == item)
      {
        items_[x] = "";
        x = 4;
      }
    }
  }
}
 