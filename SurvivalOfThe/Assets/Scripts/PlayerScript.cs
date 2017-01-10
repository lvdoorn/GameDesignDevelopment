using UnityEngine;
using System.Collections;

using NDream.AirConsole;
using Newtonsoft.Json.Linq;

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


  void Start ()
  {
    rb_ = GetComponent<Rigidbody2D>();
    moved_ = false;
    anim_ = GetComponent<Animator>();
    //anim_.enabled = false;
    movement_ = "S";
    has_focus_ = false;
    offset = new Vector3(((float)gameObject.GetComponent<SpriteRenderer>().sprite.texture.width) / (gameObject.GetComponent<SpriteRenderer>().sprite.pixelsPerUnit * 10), 0, 0);

    AirConsole.instance.onMessage += OnMessage;
  }

  void Update()
  {
    bool m = false;
    
    // set the movement
    if (movement_ == "R")
    {
      rb_.AddForce(new Vector2(5, 0));
      gameObject.GetComponent<SpriteRenderer>().flipX = false;
      if(flipped_)
      {
        transform.position -= offset;
        gameObject.GetComponent<BoxCollider2D>().offset += new Vector2(offset.x, offset.y);
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
        gameObject.GetComponent<BoxCollider2D>().offset -= new Vector2(offset.x, offset.y);
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
      cam.transform.position = new Vector3(p.x,p.y, -10);// + new Vector2(50,50) ;
    }

    // animate it
    if (m && ( !moved_ ))
    {
      //anim_.Stop();
      anim_.Play("WalkSide");

      //anim_.enabled = true;
    }
    if(!m && moved_)
    {
      //anim_.Stop();
      anim_.Play("Idle");
      //anim_.enabled = false;
    }
    moved_ = m;


  }

  // Airconsole handler

  void OnMessage(int from, JToken data)
  {
   // Debug.Log((string)data["direction"]);
    if(from == id_)
    {
      if (data["direction"] != null)
      {
        string dir = (string)data["direction"];
        movement_ = dir;
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
      v -= offset / 2.0f;
    else
      v += offset / 2.0f;
    return v;

  }

}
 