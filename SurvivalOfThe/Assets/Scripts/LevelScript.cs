using UnityEngine;
using System.Collections;

using NDream.AirConsole;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using UnityEngine.UI;

public class LevelScript : MonoBehaviour
{
  private PlayersScript players_;
  private int current_layer_ = -1;

  private bool vote_mode = false;
  private string vote_event = "";

  private float debug_text_current = 0.0f;
  private float debug_text_executed = 0.0f;
  private float debug_text_wait = 5.0f;
  private List<string> debug_text_queue = new List<string>();

  void Start ()
  {
    players_ = GameObject.Find("Players").GetComponent<PlayersScript>();

    AirConsole.instance.onMessage += OnMessage;
  }	
	void Update ()
  {
    if(vote_mode)
    {
      int vote = players_.GatherVotes();
      if(vote != -1)
      {
        EndVoteMode(vote);
      }
      else
      {
        string yes = "Yes(Action1) [";
        foreach(int k in players_.VotesYes( ) )
        {
          yes += k.ToString() + ", ";
        }
        yes += "]";

        string no = "No(Action2) [";
        foreach (int k in players_.VotesNo())
        {
          no += k.ToString() + ", ";
        }
        no += "]";

        GameObject.Find("Game").transform.GetChild(1).GetChild(1).GetChild(1).GetComponent<Text>().text = yes;
        GameObject.Find("Game").transform.GetChild(1).GetChild(1).GetChild(2).GetComponent<Text>().text = no;
      }
    }
    if (Input.GetKeyDown(KeyCode.V))
      BeginVoteMode("disable_pipe_system","Test");

    debug_text_current = Time.time;
    if (debug_text_executed != 0.0f)
    {
      if (debug_text_current - debug_text_executed > debug_text_wait)
      {
        debug_text_executed = 0.0f;
        GameObject.Find("DebugTextText").GetComponent<Text>().text = "";
      }
    }
    else
    {
      if( debug_text_queue.Count >0 )
      {
        debug_text_executed = debug_text_current;
        GameObject.Find("DebugTextText").GetComponent<Text>().text = debug_text_queue[0];
        debug_text_queue.RemoveAt(0);
      }
    }
  }
  public void Init()
  {
    players_ = GameObject.Find("Players").GetComponent<PlayersScript>();

  }

  // airconsole handlers

  void OnMessage(int from, JToken data)
  {
    if (data["focus"] != null)
    {
      //Debug.Log((string)data["focus"]);
      int id = from;
      SetFocus(id);
    }
  }

  // actions 

  private void SwitchLayer(int layer_id)
  {
    if (layer_id != current_layer_)
    {
      Debug.Log("LevelLayer" + layer_id.ToString());
      players_.LayerSwitched(layer_id);
      foreach (Transform child in transform)
      {
        if (!child.gameObject.name.Contains("LevelLayer" + layer_id.ToString()))
        {
          Debug.Log("Switch off :" + child.gameObject.name);
          child.gameObject.SetActive(false);
        }
        else
        {
          Debug.Log("Switch on :" + child.gameObject.name);
          child.gameObject.SetActive(true);
        }
      }
    }
    current_layer_ = layer_id;
  }

  public void SetFocus(int player_id)
  {
    GameObject pl = players_.GetPlayer(player_id);
    if (pl != null)
    {
      SwitchLayer((pl.GetComponent<PlayerScript>()).layer_);
      players_.SetFocus(player_id);
    }
  }
  public void FocusSomeone()
  {
    int player_id = 0;

    while (player_id < 100)
    {
      GameObject pl = players_.GetPlayer(player_id);
      if (pl != null)
      {
        SwitchLayer((pl.GetComponent<PlayerScript>()).layer_);
        players_.SetFocus(player_id);
        player_id = 100;
      }
      player_id++;
    }
  }

  private void BeginVoteMode(string type, string question )
  {
    vote_mode = true;
    vote_event = type;

    GameObject.Find("Game").transform.GetChild(1).gameObject.SetActive(true);
    Vector3 v = GameObject.Find("MainCamera").transform.position;
    v.z = 0;
    GameObject.Find("Game").transform.GetChild(1).position =  v;
    GameObject.Find("Game").transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<Text>().text = question;
    GameObject.Find("Players").GetComponent<PlayersScript>().SetVoteMode();
  }

  private void EndVoteMode(int vote)
  {
    Debug.Log(vote);
    vote_mode = false;
    GameObject.Find("Game").transform.GetChild(1).gameObject.SetActive(false);
    if( vote_event == "disable_pipe_system"  )
    {
      if(vote == 0 )
      {
        RemoveObject("fire1");
        RemoveObject("fire2");
      }
    }
  }
  public void MessageToDebug(string msg)
  {
    debug_text_queue.Add(msg);
  }



  // scripted 

  public void TriggerObject(Vector3 player_position)
  {
    GameObject lobjs = GameObject.Find("LevelLayer" + current_layer_.ToString()).transform.FindChild("Objects").gameObject;
    if (lobjs != null)
    {
      foreach (Transform child in lobjs.transform)
      {
        if (child.childCount > 0)
        {
          float d = Vector3.Distance(child.GetChild(0).position, player_position);
          if (d < 0.3f)
          {
            Debug.Log(child.gameObject.name);
            string action = child.gameObject.GetComponent<ObjectScript>().action;
            Debug.Log(action);
            if (action.StartsWith("destroy"))
            {
              string[] parts = action.Split(':');
              string[] obj_parts = parts[1].Split('|');
          
              for ( int x=0; x< obj_parts.Length; x++ )
               RemoveObject(obj_parts[x]);
              MessageToDebug("That did something");
            }
            string triggerVote = child.gameObject.GetComponent<ObjectScript>().trigger_vote;
            if (triggerVote != "")
            {
              string[] parts = triggerVote.Split('|');
              BeginVoteMode(parts[0], parts[1]);
            }
          }
        }
      }
    }
    else
      Debug.Log("couldn't find obj");
  }
  public void CheckMoveTrigger(GameObject obj)
  {
    int player_id = obj.GetComponent<PlayerScript>().getId();
    GameObject lobjs = GameObject.Find("LevelLayer" + current_layer_.ToString()).transform.FindChild("Objects").gameObject;
    if (lobjs != null)
    {
      foreach (Transform child in lobjs.transform)
      {
        if (child.childCount > 0)
        {
          Vector2 tv = obj.GetComponent<BoxCollider2D>().offset;
          float d = Vector3.Distance(child.GetChild(0).position  , obj.transform.position + new Vector3(tv.x, tv.y, 0));
          //Debug.Log(d);
          // one time 
          if ((d < 0.3f) && child.gameObject.GetComponent<ObjectScript>().wasOutside(player_id))
          {
            child.gameObject.GetComponent<ObjectScript>().PlayerWasInside(player_id);

            int switch_layer = child.gameObject.GetComponent<ObjectScript>().switch_layer;
            string t = child.gameObject.GetComponent<ObjectScript>().turn_off;
            if (switch_layer != -1)
            {
              PlayerScript ps = obj.GetComponent<PlayerScript>();
              ps.setLayer(switch_layer);
            
              if (ps.hasFocus())
              {
                SetFocus(ps.getId());
              }
              else
              {
                players_.RefreshVisibility();
              }
              TempDisableObjects(obj);
            }
            if (t != "")
            {
              List<GameObject> objs = new List<GameObject>();
              string[] parts = t.Split('|');
              foreach (string p in parts)
              {
                objs.Add(GameObject.Find(p));
              }
              MessageToDebug("Holding that.. I need to stay here");
              MessageToDebug("To deactivate that permanently we need a main console");

              foreach (GameObject p in objs)
              {
                if (p != null)
                {
                  p.SetActive(false);
                  child.gameObject.GetComponent<ObjectScript>().tmp_objects.Add(p);
                }
              }              
            }
          }

          if (d >0.5f)
          {
            if(!child.gameObject.GetComponent<ObjectScript>().wasOutside(player_id))
            {
              Debug.Log("OUTSIDE");
              if (!child.gameObject.GetComponent<ObjectScript>().someoneInside())
              {
                foreach (GameObject p in child.gameObject.GetComponent<ObjectScript>().tmp_objects)
                {
                  if (p != null)
                    p.SetActive(true);
                }
                child.gameObject.GetComponent<ObjectScript>().tmp_objects.Clear();
              }
            }

            child.gameObject.GetComponent<ObjectScript>().was_outside = true;
            child.gameObject.GetComponent<ObjectScript>().PlayerWasOutside(player_id);
          }
     
        }
       
      }
    }
    else
      Debug.Log("couldn't find obj");
  }

  public void RemoveObject(string name)
  {
    for( int x=0; x < GameObject.Find("Level").transform.childCount; x++)
    {
      Transform ll = GameObject.Find("Level").transform.GetChild(x);
      Transform o = ll.FindChild("Objects").FindChild(name);
      if (o != null)
      {
        Debug.Log("Destroying " + name);
        Destroy(o.gameObject);
      }
    }   
  }
  private void TempDisableObjects(GameObject obj)
  {
    int player_id = obj.GetComponent<PlayerScript>().getId();
    GameObject lobjs = GameObject.Find("LevelLayer" + current_layer_.ToString()).transform.FindChild("Objects").gameObject;
    if (lobjs != null)
    {
      foreach (Transform child in lobjs.transform)
      {
        if (child.childCount > 0)
        {
          Vector2 tv = obj.GetComponent<BoxCollider2D>().offset;
          float d = Vector3.Distance(child.GetChild(0).position, obj.transform.position + new Vector3(tv.x, tv.y, 0));

          if ((d < 0.5f))
          {
            child.gameObject.GetComponent<ObjectScript>().PlayerWasInside(player_id);
          }
        }
      }
    }
  }

  public bool IsInVoteMode()
  {
    return vote_mode;
  }
 

}
