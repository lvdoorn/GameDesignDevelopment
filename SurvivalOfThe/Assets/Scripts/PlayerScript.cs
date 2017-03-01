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
            gameObject.GetComponent<CircleCollider2D>().offset += new Vector2(offset.x, offset.y) / gameObject.transform.localScale.x;
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
            gameObject.GetComponent<CircleCollider2D>().offset -= new Vector2(offset.x, offset.y) / gameObject.transform.localScale.x;
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
          GetComponent<AudioSource>().Play();
          //anim_.enabled = true;
        }
        if (!m && moved_)
        {
          //anim_.Stop();
          anim_.Play("Idle");
          GetComponent<AudioSource>().Stop();
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
        if (it != "")
        {
          if (it == "fire_extinguisher")
          {
            GameObject.Find("Game").GetComponent<GameScript>().GetCurrentLevel().ExecuteIfInRange(gameObject,0.6f, "fire", "remove", "", "");
          }
          if (it == "dna_sampler")
          {
            GameObject.Find("Game").GetComponent<GameScript>().GetCurrentLevel().ExecuteIfInRange(gameObject, 0.6f, "sample_dna", "pickup", "sample_dna", "");
          }
          if (it == "dynamite")
          {
            GameObject.Find("Game").GetComponent<GameScript>().GetCurrentLevel().ExecuteIfInRange(gameObject, 0.6f, "exit", "remove", "", "synthetic_explosion_1");
          }
          if (it == "sample_dna")
          {
            GameObject.Find("Game").GetComponent<GameScript>().GetCurrentLevel().ExecuteIfInRange(gameObject, 0.6f, "dna_door", "condition_remove", "2", "");
          }
          if (it == "pickaxe")
          {
            GameObject.Find("Game").GetComponent<GameScript>().GetCurrentLevel().ExecuteIfInRange(gameObject, 0.6f, "special_crate", "remove_addletter", "doc_2", "trigger_letter|Dear new medic!\n\nThe code is 589413\nRegards,\nYour Supervisor");
            GameObject.Find("Game").GetComponent<GameScript>().GetCurrentLevel().ExecuteIfInRange(gameObject, 0.6f, "crate", "remove", "", "impactcrunch04");
          }
          if (it == "machete")
          {
            GameObject.Find("Game").GetComponent<GameScript>().GetCurrentLevel().ExecuteIfInRange(gameObject, 0.4f, "grass", "remove_this", "", "machete_cut");
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

  public bool addItem(string item)
  {
    int id = -1;
    bool exists = false;
    for(int x=0; x< 4; x++)
    {
      if(items_[x] == "" && id == -1)
      {
        id = x;
      }
      if (items_[x] == item)
        exists = true;
    }
    if(id != -1 && !exists)
    {
      items_[id] = item;
      var message = new
      {
        addItem = item,
        slot = id
      };
      AirConsole.instance.Message(id_, message);
      Debug.Log("sent item add");
      return true;
    }
    return false;
  }
  public void removeItem(string item)
  {
    for (int x = 0; x < 4; x++)
    {
      if (items_[x].Equals( item) )
      {
        items_[x] = "";
  
        var message = new
        {
          removeItem = item,
          slot = x
        };
        x = 4;
        AirConsole.instance.Message(id_, message);
        Debug.Log("sent item remove");
      }
    }
  }
  public bool HasItem(string item)
  {
    for(int x =0; x <items_.Length; x++)
    {
      if (items_[x] == item)
        return true;
    }

    return false;
  }
}
 