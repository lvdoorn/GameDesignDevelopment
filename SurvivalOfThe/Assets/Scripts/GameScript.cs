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

  void Start()
  {
   // ShowTutorial ();
    //current_level_ = new GameObject();
    //current_level_.transform.SetParent(transform);

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
    GetCurrentLevel().DisplayInfoBox("Welcome to Survival of the Zargs! Use the ACTION buttons on your device to interact.", 10);
  }

  public void StartExtendedTutorial()
  {
    ChangeLevel("tutorial_ex");
    ChangePositions(new Vector3(0.0f, 2.75f, -8));
    GetCurrentLevel().DisplayInfoBox("Use action buttons to interact.\nTry to escape the crashed ship.", 5);
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
}


