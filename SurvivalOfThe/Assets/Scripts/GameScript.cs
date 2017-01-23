﻿using UnityEngine;
using System.Collections;

using NDream.AirConsole;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
using System.Collections.Generic;

public enum GameState
{
  JOIN =0,
  PLAY =1,
  VOTE =2
}

public class GameScript : MonoBehaviour
{
  private GameObject current_level_;
  public float Scale { get; set; }
  public GameState State { get; set; }

  public struct InfoBoxMessage {
    public float time;
    public string text;
    public string image_name;
    public bool initialized;
  }

  private List<InfoBoxMessage> label_queue_ = new List<InfoBoxMessage>();
  private GameObject info_box_;
  private Text info_box_text_;

  void Start()
  {
   // ShowTutorial ();
    //current_level_ = new GameObject();
    //current_level_.transform.SetParent(transform);
    
    info_box_ = GameObject.Find("UI").transform.GetChild(1).gameObject;
    info_box_text_ = info_box_.GetComponentInChildren<Text>(true);

    State = GameState.JOIN;

    AirConsole.instance.onMessage += OnMessage;
    RefreshWaitingScreen("Waiting for players" , "0 players connected");

  }
  void Update()
  {
    if (Input.GetKeyDown("p"))
    {
      StartTutorial();
      //StartExtendedTutorial();
    }
  }
  void OnGUI() {
    if (label_queue_.Count > 0) {
      if (Time.time < label_queue_[0].time) { // display queue element 0
        if (!label_queue_[0].initialized) {
          InfoBoxMessage ibm = label_queue_[0];
          ibm.initialized = true;
          label_queue_[0] = ibm;
          string text = label_queue_[0].text;
          info_box_text_.text = text;
          foreach (Image img in info_box_.GetComponentsInChildren<Image>(true)) {
            GameObject go = img.gameObject;
            if (go.name == label_queue_[0].image_name) {
              go.SetActive(true);
            } else {
              go.SetActive(false);
            }
          }
        }
      } else { // queue element 0 has expired -> remove from queue
        label_queue_.RemoveAt(0);
      }
      info_box_.SetActive(true);
    } else {
      info_box_.SetActive(false);
    }
  }	
  //ariconsole handler
  void OnMessage(int from, JToken data)
  {
    if (data["start"] != null)
    {
      Debug.Log("received start");
      StartTutorial();
      //StartExtendedTutorial();
    }
  }
  
  //action

  public void StartTutorial()
  {
    ChangeLevel("tutorial", 10.0f / 4.0f);
    ChangePositions(
      new Vector3(-8.5f, 9.0f, -8),
      new Vector3(-4.5f, 10.0f, -8),
      new Vector3(-7.5f, 10.0f, -8),
      new Vector3(-1.5f, 10.0f, -8),
      new Vector3(4.5f, 10.0f, -8),
      new Vector3(10.5f, 10.0f, -8),
      new Vector3(7.5f, 10.0f, -8),
      new Vector3(1.5f, 10.0f, -8)
    );
    DisplayInfoBox("Welcome to Survival of the Zargs! Use the ACTION buttons on your device to interact.", 10, "Alien");
  }

  public void StartExtendedTutorial()
  {
    ChangeLevel("tutorial_ex");
    ChangePositions(new Vector3(0.0f, 2.75f, -8));
    DisplayInfoBox("Use action buttons to interact.\nTry to escape the crashed ship.", 5, "Alien");
  }

  public void ChangePositions(params Vector3[] vs) {
    if (vs.Length == 1) {
      GameObject.Find("Players").GetComponent<PlayersScript>().MoveAllPlayers(vs[0]);
    } else {
      GameObject.Find("Players").GetComponent<PlayersScript>().MoveAllPlayers(vs);
    }
  }

  public void ChangeLevel(string lvl, float scale = 1.0f)
  {
    if (current_level_ != null) {
      current_level_.SetActive(false);
    }
    
    current_level_ = gameObject.transform.FindChild(lvl).gameObject;
    current_level_.SetActive(true);


    GameObject.Find("Players").GetComponent<PlayersScript>().join_enabled_ = false;
    State = GameState.PLAY;

    AirConsole.instance.Broadcast("GameStarts");
 
    gameObject.transform.FindChild("WaitingScreen").gameObject.SetActive(false);

    current_level_.GetComponent<LevelScript>().Init();
    current_level_.GetComponent<LevelScript>().FocusSomeone();
  }
  

  public void RefreshWaitingScreen(string text, string text2)
  {
    GameObject.Find("WaitingScreenText").GetComponent<Text>().text = text;
    GameObject.Find("WaitingScreenText2").GetComponent<Text>().text = text2;
  }

  public LevelScript GetCurrentLevel()
  {
    if(current_level_ != null)
      return current_level_.GetComponent<LevelScript>();

     return null;    
  }

  public void DisplayInfoBox(string text, float seconds = 5, string image_name = "Info") {
    float end_time;
    if (label_queue_.Count > 0) {
      // display the new label until *seconds* seconds after the last label in the queue 
      end_time = label_queue_[label_queue_.Count - 1].time + seconds;
    } else {
      // display the new label until *now* + *seconds*
      end_time = Time.time + seconds;
    }
    label_queue_.Add(new InfoBoxMessage() { time = end_time, text = text, image_name = image_name });
  }
}


