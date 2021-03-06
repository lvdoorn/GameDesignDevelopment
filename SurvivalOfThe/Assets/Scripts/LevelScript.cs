﻿using UnityEngine;
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

  private bool letter_shown_ = false;

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
        yes += string.Join(", ", players_.VotesYes().ToArray());
        yes += "]";

        string no = "No [";
        no += string.Join(", ", players_.VotesNo().ToArray());
        no += "]";

        GameObject.Find("Game").transform.GetChild(1).GetChild(1).GetChild(2).GetComponent<Text>().text = yes;
        GameObject.Find("Game").transform.GetChild(1).GetChild(1).GetChild(3).GetComponent<Text>().text = no;
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
    if (letter_shown_ && data["direction"] != null)
      HideLetter();
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
    GameObject.Find("Game").GetComponent<GameScript>().State = GameState.VOTE;
  }

  private void EndVoteMode(int vote)
  {
    GameObject.Find("Game").GetComponent<GameScript>().State = GameState.PLAY;
    vote_mode = false;
    GameObject.Find("Game").transform.GetChild(1).gameObject.SetActive(false);
    if ( vote_event == "disable_pipe_system"  )
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
        GameObject.Find("Game").GetComponent<GameScript>().PlaySound("InterfaceDeck");
        AirConsole.instance.Broadcast(new { vibrate = 500 });
        GameObject.Find("tutorial").transform.GetChild(0).GetChild(2).FindChild("dead_animal_warning").gameObject.GetComponent<ObjectScript>().trigger_text = "Oh my god the stasis pots....They are dead| ... We killed them";
      }
    } else if (vote_event.StartsWith("collect_fruit_")) {
      if (vote == 0) {
        // 2 fruit kinds are enough, but the players may decide to collect all 3
        string msg = "Yum yum! ";
        GameScript game = GameObject.Find("Game").GetComponent<GameScript>();
        Fruits fruit = Fruits.NONE;
        string selector = "";
        if (vote_event.EndsWith("red")) {
          fruit = Fruits.RED;
          selector = "red_";
        } else if (vote_event.EndsWith("purple")) {
          fruit = Fruits.PURPLE;
          selector = "purple_";
        } else if (vote_event.EndsWith("blue")) {
          fruit = Fruits.BLUE;
          selector = "blue_";
        }

        // display infos
        bool removeBorders = false;
        game.CollectedFruits |= fruit;
        if (game.CollectedFruits == Fruits.ALL) { // collected ALL fruits
          msg += "Now we definitely have enough food for our trip. Let's go!";
        } else if ((game.CollectedFruits & ~fruit) != 0) { // collected 2 fruits
          msg += "I guess we could continue our trip with that food...\r\nBut.. maybe.. we should collect some more?";
          removeBorders = true;
        } else { // collected first fruit
          msg += "Great! But we definitely need MORE food!";
        }
        game.DisplayInfoBox(msg, -1, "Player");
        
        // transform fruit trees and remove borders
        Transform objects = game.GetCurrentLevel().transform.GetChild(0).GetChild(2);
        for (int i = 0; i < objects.childCount; i++) {
          GameObject obj = objects.GetChild(i).gameObject;
          if (obj.name.StartsWith(selector)) {
            if (obj.name.EndsWith("_left") || obj.name.EndsWith("_right")) {
              Destroy(obj);
            }
          } else if (removeBorders && obj.name == "border") {
            Destroy(obj);
          }
        }
      } else {
        GameObject.Find("Game").GetComponent<GameScript>().DisplayInfoBox("But they look so tasty!", -1, "Player");
      }
    }
  }
  public void BeginPuzzle(string name)
  {
    int number_of_players = GameObject.Find("Players").GetComponent<PlayersScript>().PlayerCount();
    GameObject.Find("Game").GetComponent<GameScript>().State = GameState.PUZZLE;
    PuzzleScript puzzle_script = GameObject.Find("Game").transform.FindChild("UI").FindChild("Puzzle").GetComponent<PuzzleScript>();
    //if(name != "")
   // {
   //   Sprite sp = Resources.Load<Sprite>("Sprites/"+name);
   //   GameObject.Find("Game").transform.FindChild("UI").FindChild("Puzzle").GetComponent<Image>().sprite = sp;
   // }
    puzzle_script.Init( number_of_players, name);
    GameObject.Find("Game").transform.FindChild("UI").FindChild("Puzzle").gameObject.SetActive(true);
  }
  public void EndPuzzle()
  {
    PuzzleScript puzzle_script = GameObject.Find("Game").transform.FindChild("UI").FindChild("Puzzle").GetComponent<PuzzleScript>();
    GameObject.Find("Game").GetComponent<GameScript>().State = GameState.PLAY;
    GameObject.Find("Game").transform.FindChild("UI").FindChild("Puzzle").gameObject.SetActive(false);
    GameObject.Find("Game").GetComponent<GameScript>().PlaySound("correct");
    if (puzzle_script.type_ == "mining_station")
    {
      RemoveObject("puzzle_door");
    }
    if (puzzle_script.type_ == "bridge")
    {
      RemoveObject("bridge_over");
      RemoveObject("bridge_over2");
      RemoveObject("river_cover_1");
      RemoveObject("river_cover_2");
      RemoveObject("river_cover_3");
      RemoveObject("plank_1");
      RemoveObject("plank_2");
      RemoveObject("plank_3");
    }
  }

  public void ShowLetter(string text)
  {

    text = text.Replace("\\n", "\n");
    GameObject.Find("Game").transform.FindChild("UI").FindChild("Letter").gameObject.SetActive(true);
    GameObject.Find("Game").transform.FindChild("UI").FindChild("Letter").GetChild(0).GetComponent<Text>().text = text;
    letter_shown_ = true;
  }
  public void HideLetter()
  {
    GameObject.Find("Game").transform.FindChild("UI").FindChild("Letter").gameObject.SetActive(false);
    letter_shown_ = false;
  }


  public void MessageToDebug(string msg, string icon = "Info", int seconds = -1)
  {
    Debug.Log("Display " + msg);
    GameObject.Find("Game").GetComponent<GameScript>().DisplayInfoBox(msg,seconds,icon);
  }

  // scripted 

  /*public void ExtinguishFires(Vector3 player_position)
  {
    Debug.Log("ExtinguishFires");
    float scale = GameObject.Find("Game").GetComponent<GameScript>().Scale;
    Vector2 player_position_2d = new Vector2(player_position.x, player_position.y);
    GameObject lobjs = GameObject.Find("LevelLayer" + current_layer_.ToString()).transform.FindChild("Objects").gameObject;
    if (lobjs != null)
    {
      foreach (Transform child in lobjs.transform)
      {
        float d = Vector2.Distance(new Vector2(child.position.x, child.position.y), new Vector2(player_position.x, player_position.y));
        if (d <  0.6f)
        {
          Debug.Log(child.gameObject.name);
          string objname = child.gameObject.name;       
          if (objname.StartsWith("fire"))
          {
            RemoveObject(objname);
          }
        }      
      }
    }
    else
      Debug.Log("couldn't find obj");
  }*/

  public void ExecuteIfInRange(GameObject player, float dist, string name_filter, string action, string add, string add2)
  {
    Debug.Log(action);
    float scale = GameObject.Find("Game").GetComponent<GameScript>().Scale;
    Vector3 player_position = player.transform.position;
    Vector2 player_position_2d = new Vector2(player_position.x, player_position.y);
    Vector2 player_offset = new Vector2(0.1f, -0.1f);
    GameObject lobjs = GameObject.Find("LevelLayer" + current_layer_.ToString()).transform.FindChild("Objects").gameObject;
    if (lobjs != null)
    {
      foreach (Transform child in lobjs.transform)
      {
        
        float d = Vector2.Distance(new Vector2(child.position.x, child.position.y), new Vector2(player_position.x, player_position.y) +player_offset );
        if (d < dist)
        {
          Debug.Log(child.gameObject.name);
          string objname = child.gameObject.name;
          if (objname.StartsWith(name_filter))
          {
            if (action == "pickup")
            {
              player.GetComponent<PlayerScript>().addItem(add);
            }
            if (action == "remove_this")
            {
              Destroy(child.gameObject);
              if(add2 != "")
              {
                GameObject.Find("Game").GetComponent<GameScript>().PlaySound(add2);
              }
            }
            if (action == "remove")
            {
              RemoveObject(objname);
              if(add2 != "")
              {
                GameObject.Find("Game").GetComponent<GameScript>().PlaySound(add2);
              }
            }
            if (action == "remove_addletter")
            {
              RemoveObject(objname);
              GameObject.Find("LevelLayer" + current_layer_.ToString()).transform.FindChild("Objects").FindChild(add).gameObject.GetComponent<ObjectScript>().action = add2;
              GameObject.Find("LevelLayer" + current_layer_.ToString()).transform.FindChild("Objects").FindChild(add).gameObject.GetComponent<ObjectScript>().PlayerWasOutside(player.GetComponent<PlayerScript>().getId());
              GameObject.Find("Game").GetComponent<GameScript>().PlaySound("impactcrunch04");
            }
            if (action == "condition_remove")
            {
              string obj_action = child.gameObject.GetComponent<ObjectScript>().action;
              string[] parts = obj_action.Split('|');
              Debug.Log(obj_action);
              if(parts.Length> 0)
              {
                if(parts[0] == "condition_remove")
                {
                  List<int> ids = child.gameObject.GetComponent<ObjectScript>().IsMultipleTriggered(int.Parse(parts[2]), player.GetComponent<PlayerScript>().getId());
                  if (ids.Count >0 )
                  {
                    RemoveObject(objname);
                    for(int x=0; x< ids.Count;x++ )
                    {
                      PlayerScript s = players_.GetPlayer(ids[x]).GetComponent<PlayerScript>();
                      s.removeItem("prybar");
                    }                    
                  }
                  player.GetComponent<PlayerScript>().removeItem("sample_dna");
                
                }
              }
            }
          }
        }
      }
    }
    else
      Debug.Log("couldn't find obj");
    
  }

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
              int seconds = parts.Length > 2 ? int.Parse(parts[2]) : -1;
              string img = parts.Length > 3 ? parts[3] : "Info";
              GameObject.Find("Game").GetComponent<GameScript>().DisplayInfoBox(hint, seconds, img);
            }
            if (action.StartsWith("vote"))
            {
              string[] parts = action.Split(':');
              if (parts[1] == "startengine")
              {
                int number_of_players = GameObject.Find("Players").GetComponent<PlayersScript>().PlayerCount();
                VoteScript vote_script = GameObject.Find("UI").GetComponent<VoteScript>();
                vote_script.Init("Enter the start sequence for the engine ...\nUse the DIRECTION keys to enter the code.\nSubmit the code by pressing SUBMIT.", number_of_players);
              }
              if (parts[1] == "open_med_station")
              {
                VoteScript vote_script = GameObject.Find("UI").GetComponent<VoteScript>();
                char [] key = { '5','8','9','4','1','3'};// 589413
                vote_script.Init("Code required", key, "open_med_station");
         
              }
            }
            if (action.StartsWith("trigger_puzzle"))
            {
              BeginPuzzle("mining_station");
            }
            if (action.StartsWith("trigger_puzzle2"))
            {
              BeginPuzzle("bridge");
            }
            if (action.StartsWith("trigger_letter"))
            {
              string[] parts = action.Split('|');
              ShowLetter(parts[1]);
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
      GameScript game = GameObject.Find("Game").GetComponent<GameScript>();
      foreach (Transform child in lobjs.transform)
      {
        if (child.childCount > 0)
        {
          Vector2 tv = obj.GetComponent<CircleCollider2D>().offset;
          float d = Vector2.Distance( new Vector2( child.GetChild(0).position.x, child.GetChild(0).position.y), new Vector2(obj.transform.position.x, obj.transform.position.y) + tv);
          //Debug.Log(d);
          // one time 
          if ((d < 0.3f) && child.gameObject.GetComponent<ObjectScript>().wasOutside(player_id))
          {
            child.gameObject.GetComponent<ObjectScript>().PlayerWasInside(player_id);

            int switch_layer = child.gameObject.GetComponent<ObjectScript>().switch_layer;
            string t = child.gameObject.GetComponent<ObjectScript>().turn_off;
            string text_trigger = child.gameObject.GetComponent<ObjectScript>().trigger_text;
            string item_trigger = child.gameObject.GetComponent<ObjectScript>().item;
            string action = child.gameObject.GetComponent<ObjectScript>().action;

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
              TempDisableObjects(obj, switch_layer);
            }

            if (t != "")
            {
              List<GameObject> objs = new List<GameObject>();
              string[] parts = t.Split('|');
              foreach (string p in parts)
              {
                objs.Add(GameObject.Find(p));
              }
              MessageToDebug("Holding that.. I need to stay here", "Player");
              MessageToDebug("To deactivate that permanently we need a main console", "Player");



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
            if (item_trigger != "")
            {

              if (obj.GetComponent<PlayerScript>().addItem(item_trigger))
              {
                Destroy(child.gameObject);
                MessageToDebug("Thats mine now", "Player");
                game.PlaySound("life_pickup");
                if(item_trigger == "fuel")
                  game.fuel_in_inventory_ = true;
              }
            }

            if (text_trigger != "")
            {
              Debug.Log("TextTrigger");
              string[] parts = text_trigger.Split('|');

              for (int x = 0; x < parts.Length; x++)
              {
                MessageToDebug(parts[x]);
              }
              //child.gameObject.GetComponent<ObjectScript>().trigger_text = "";
            }
            if (action == "changeLevelToWoods") {
              game.StartWoods();
            }
            if (action == "changeLevelMiningStation")
            {
              if (players_.HaveItem("fuel"))
              {
                game.StartWoods(-4.5f, -1.375f, 0f);
                game.ShowIntermission("Now that the fuel has been found the space ship will be able to fly.");
              }
              else
              {
                MessageToDebug("We still need the fuel", "Player");
              }
            }

            if (action == "changeLevelToMiningStation")
            {
              game.StartMiningStation();
              if (!players_.HaveItem("fuel"))
                game.ShowIntermission("There! A station built into a mountain. Maybe we can find fuel there.");
            }
            if (action == "changeLevelToJungle")
            {
              game.StartJungle();
              if (!game.HaveFruits())
                game.ShowIntermission("This place looks beautiful...\r\nMaybe we can find some food here.");
            }
            if (action == "exitGame")
            {
              if (!game.HaveFruits())
                Debug.Log("no fruits");
              if (!game.fuel_in_inventory_)
                Debug.Log("no fuel");


              if (game.fuel_in_inventory_ && game.HaveFruits())
              {
                if ((game.CollectedFruits & Fruits.RED) != 0)
                {
                  game.ShowEnd(false);
                }
                else
                {
                  game.ShowEnd(true);
                }
              }
            }
            if (action == "changeLevelJungle") {
              game.StartWoods(-0.75f, 4.125f, 0f); 
              game.ShowIntermission("Now that we have collected delicious fruits, we won't starve on our continuing trip.");
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

  private void TempDisableObjects(GameObject obj, int layer)
  {
    int player_id = obj.GetComponent<PlayerScript>().getId();
    GameObject lobjs = transform.FindChild("LevelLayer" + layer.ToString()).transform.FindChild("Objects").gameObject;
    if (lobjs != null)
    {
      foreach (Transform child in lobjs.transform)
      {
        if (child.childCount > 0)
        {
          Vector2 tv = obj.GetComponent<CircleCollider2D>().offset;
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
