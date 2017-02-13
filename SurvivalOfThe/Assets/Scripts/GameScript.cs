using UnityEngine;
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
  private bool in_intermission = false;

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
    if(in_intermission)
    {
      GameObject screen = GameObject.Find("Game").transform.FindChild("UI").FindChild("IntermissionScreen").gameObject;
      screen.transform.FindChild("IntermissionText").position += new Vector3(0, 1.0f, 0);
      if (screen.transform.FindChild("IntermissionText").position.y > 380  + screen.transform.FindChild("IntermissionText").GetComponent<Text>().preferredHeight +20.0f )
        EndIntermission();
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
     // StartMiningStation();
    }
  }
  
  //action

  public void StartTutorial()
  {
    ChangePositions(
      new Vector3(-1.75f, 2.5f, 3),
      new Vector3(-0.25f, 3.5f, 3),
      new Vector3(-0.25f, 1.5f, 3),
      new Vector3(-2.25f, 3.5f, 3),
      new Vector3(-2.25f, 1.5f, 3),
      new Vector3(1.75f, 3.5f, 3),
      new Vector3(1.75f, 1.5f, 3),
      new Vector3(-4.0f, 3.5f, 3),
      new Vector3(-4.0f, 1.5f, 3)
    );
    ChangeLevel("tutorial");
    ShowIntermission("A long time ago in a galaxy far, far away....\n\n....a spaceship crashed....");
    DisplayInfoBox("Welcome to Survival of the Zargs! Use the ACTION buttons on your device to interact.", 20, "Alien");
  }
  public void StartMiningStation()
  {
    ChangePositions(new Vector3(-0.0078125f, 4.0f, 0) );
    ChangeLevel("mining_station");
    ShowIntermission("The crew of the space ship has found a cave. It seems to have been used as a mining station. As they enter with the intent to recover fuel the door closes behind them...");
  }

  public void ShowIntermission(string text)
  {
    GetCurrentLevel().gameObject.SetActive(false);
    GameObject game = GameObject.Find("Game");
    GameObject screen = GameObject.Find("Game").transform.FindChild("UI").FindChild("IntermissionScreen").gameObject;

    screen.SetActive(true);

    Vector3 v = GameObject.Find("MainCamera").transform.position;
    v.z = 0;
    Vector3 tp = screen.transform.position;
    tp.y = -120.0f;
    screen.transform.FindChild("IntermissionText").position = tp;

    screen.transform.FindChild("IntermissionText").GetComponent<Text>().text = text;
    in_intermission = true;
  }

  public void EndIntermission()
  {
    GetCurrentLevel().gameObject.SetActive(true);
    GameObject screen = GameObject.Find("Game").transform.FindChild("UI").FindChild("IntermissionScreen").gameObject;

    screen.SetActive(false);
    in_intermission = false;
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


