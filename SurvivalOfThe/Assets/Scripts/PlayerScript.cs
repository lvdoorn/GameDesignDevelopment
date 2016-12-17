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


  void Start ()
  {
    rb_ = GetComponent<Rigidbody2D>();
    moved_ = false;
    anim_ = GetComponent<Animator>();
    anim_.enabled = false;
    movement_ = "S";
    has_focus_ = false;

    AirConsole.instance.onMessage += OnMessage;
  }

  void Update()
  {
    bool m = false;

    // set the movement
    if (movement_ == "R")
    {
      rb_.AddForce(new Vector2(5, 0));
      m = true;
    }
    if (movement_ == "L")
    {
      rb_.AddForce(new Vector2(-5, 0));
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
      Vector3 p = gameObject.transform.position;
      cam.transform.position = new Vector3(p.x, p.y, -10);// + new Vector2(50,50) ;
    }

    // animate it
    if (m && ( !moved_ ))
    {
      anim_.enabled = true;
    }
    if(!m && moved_)
    {
      anim_.enabled = false;
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

}
 