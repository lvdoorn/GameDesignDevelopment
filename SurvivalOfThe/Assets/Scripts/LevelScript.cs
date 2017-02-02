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

  void Start ()
  {
    players_ = GameObject.Find("Players").GetComponent<PlayersScript>();

    AirConsole.instance.onMessage += OnMessage;
  }
  public void Reset()
  {
    current_layer_ = -1;

    vote_mode = false;
    vote_event = "";
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
        string yes = "Yes [";
        foreach(int k in players_.VotesYes( ) )
        {
          yes += k.ToString() + ", ";
        }
        yes += "]";

        string no = "No [";
        foreach (int k in players_.VotesNo())
        {
          no += k.ToString() + ", ";
        }
        no += "]";

        GameObject.Find("Game").transform.GetChild(1).GetChild(1).GetChild(2).GetComponent<Text>().text = yes;
        GameObject.Find("Game").transform.GetChild(1).GetChild(1).GetChild(3).GetComponent<Text>().text = no;
      }
    }
    if (Input.GetKeyDown(KeyCode.V))
      BeginVoteMode("disable_pipe_system","Test","test text");

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

  private void BeginVoteMode(string type, string question, string addiional )
  {
    vote_mode = true;
    vote_event = type;

    GameObject.Find("Game").transform.GetChild(1).gameObject.SetActive(true);
    Vector3 v = GameObject.Find("MainCamera").transform.position;
    v.z = 0;
    GameObject.Find("Game").transform.GetChild(1).position =  v;
    GameObject.Find("Game").transform.GetChild(1).GetChild(1).GetChild(0).GetComponent<Text>().text = question;
    GameObject.Find("Game").transform.GetChild(1).GetChild(1).GetChild(1).GetComponent<Text>().text = addiional;
    GameObject.Find("Players").GetComponent<PlayersScript>().SetVoteMode();
    AirConsole.instance.Broadcast("BeginVote");
  }

  private void EndVoteMode(int vote)
  {
    AirConsole.instance.Broadcast("EndVote");
    Debug.Log(vote);
    vote_mode = false;
    GameObject.Find("Game").transform.GetChild(1).gameObject.SetActive(false);
    if( vote_event == "disable_pipe_system"  )
    {
      if(vote == 0 )
      {
        MessageToDebug("Shuting down extended life support systems...");
        MessageToDebug("3rd deck stasis pots malfunction...");
        MessageToDebug("1st deck stasis pots malfunction...");
        RemoveObject("fire1");
        RemoveObject("fire2");
        RemoveObject("animal_stasis_pot_1");
        RemoveObject("animal_stasis_pot_2");
        RemoveObject("animal_stasis_pot_3");
        RemoveObject("animal_stasis_pot_4");
        GameObject.Find("tutorial_ex").transform.GetChild(0).GetChild(2).FindChild("dead_animal_warning").gameObject.GetComponent<ObjectScript>().trigger_text = "Oh my god the stasis pots....They are dead| ... We killed them";
      }
    }
  }
  public void MessageToDebug(string msg)
  {
    Debug.Log("Display " + msg);
    GameObject.Find("Game").GetComponent<GameScript>().DisplayInfoBox(msg,3);
  }

  // scripted 

  public void TriggerObject(Vector3 player_position)
  {
    float scale = GameObject.Find("Game").GetComponent<GameScript>().Scale;
    Vector2 player_position_2d = new Vector2(player_position.x, player_position.y);
    GameObject lobjs = GameObject.Find("LevelLayer" + current_layer_.ToString()).transform.FindChild("Objects").gameObject;
    if (lobjs != null)
    {
      foreach (Transform child in lobjs.transform)
      {
        if (child.childCount > 0)
        {
          float d = Vector2.Distance(new Vector2(child.GetChild(0).position.x, child.GetChild(0).position.y), new Vector2(player_position.x, player_position.y) );
          if (d < (scale > 1.0f ? 0.4f : 0.3f))
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
            if (action.StartsWith("hint")) {
              string[] parts = action.Split(':');
              string hint = parts[1];
              int seconds = parts.Length > 2 ? int.Parse(parts[2]) : 5;
              string img = parts.Length > 3 ? parts[3] : "Info";
              GameObject.Find("Game").GetComponent<GameScript>().DisplayInfoBox(hint, seconds, img);
            }
            if (action.StartsWith("vote")) {
              string[] parts = action.Split(':');
              if (parts[1] == "startengine") {
                int number_of_players = GameObject.Find("Players").GetComponent<PlayersScript>().PlayerCount();
                VoteScript vote_script = GameObject.Find("UI").GetComponent<VoteScript>();
                vote_script.Init("Enter the start sequence for the engine ...\nUse the DIRECTION keys to enter the code.\nSubmit the code with the ACTION key.", number_of_players);
              }
            }
            string triggerVote = child.gameObject.GetComponent<ObjectScript>().trigger_vote;
            if (triggerVote != "")
            {
              string[] parts = triggerVote.Split('|');
              if(parts.Length > 2)
              {
                BeginVoteMode(parts[0], parts[1], parts[2]);
              }
              else
              BeginVoteMode(parts[0], parts[1], "");
            }
            if (child.gameObject.GetComponent<ObjectScript>().trigger_audio != "")
            {
              if (child.gameObject.GetComponent<ObjectScript>().trigger_audio == "once")
              {
                child.gameObject.GetComponent<AudioSource>().Play();
              }
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
    GameObject lobjs = gameObject.transform.FindChild("LevelLayer" + current_layer_.ToString()).transform.FindChild("Objects").gameObject;
    if (lobjs != null)
    {
      foreach (Transform child in lobjs.transform)
      {
        if (child.childCount > 0)
        {
          Vector2 tv = obj.GetComponent<BoxCollider2D>().offset;
          float d = Vector2.Distance( new Vector2( child.GetChild(0).position.x, child.GetChild(0).position.y), new Vector2(obj.transform.position.x, obj.transform.position.y) + tv);
          //Debug.Log(d);
          // one time 
          if ((d < 0.3f) && child.gameObject.GetComponent<ObjectScript>().wasOutside(player_id))
          {
            child.gameObject.GetComponent<ObjectScript>().PlayerWasInside(player_id);

            int switch_layer = child.gameObject.GetComponent<ObjectScript>().switch_layer;
            string t = child.gameObject.GetComponent<ObjectScript>().turn_off;
            string text_trigger = child.gameObject.GetComponent<ObjectScript>().trigger_text;
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

              if (child.gameObject.GetComponent<ObjectScript>().trigger_audio != "")
              {
                if (child.gameObject.GetComponent<ObjectScript>().trigger_audio == "once")
                {
                  child.gameObject.GetComponent<AudioSource>().Play();
                  Debug.Log("Play Audio ");
                }
              }
            }
           
            if (text_trigger != "")
            {
              Debug.Log("TextTrigger");
              string[] parts = text_trigger.Split('|');

              for(int x=0; x< parts.Length; x++)
              {
                MessageToDebug(parts[x]);
              }
              child.gameObject.GetComponent<ObjectScript>().trigger_text = "";
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
           /*   if (child.gameObject.GetComponent<ObjectScript>().trigger_audio != "")
              {
                if (child.gameObject.GetComponent<ObjectScript>().trigger_audio == "once")
                {
                  child.gameObject.GetComponent<AudioSource>().Play();
                  Debug.Log("Play Audio ");
                }
              }*/
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
    for( int x=0; x < gameObject.transform.childCount; x++)
    {
      Transform ll = gameObject.transform.GetChild(x);
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
          float d = Vector2.Distance(new Vector2(child.GetChild(0).position.x, child.GetChild(0).position.y), new Vector2(obj.transform.position.x, obj.transform.position.y) + tv);

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
