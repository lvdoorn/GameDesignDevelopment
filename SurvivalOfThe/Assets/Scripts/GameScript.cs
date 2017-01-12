using UnityEngine;
using System.Collections;

using NDream.AirConsole;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
using System.Collections.Generic;

enum GameState
{
  JOIN =0,
  PLAY =1
}

public class GameScript : MonoBehaviour
{
  private GameState state_ = GameState.JOIN;
  private GameObject current_level_;
  
  private List<KeyValuePair<float, string>> label_queue_ = new List<KeyValuePair<float, string>>();
  private GameObject info_box_;
  private Text info_box_text_;

  void Start()
  {
    current_level_ = new GameObject();
    current_level_.transform.SetParent(transform);

    info_box_ = GameObject.Find("UI").transform.GetChild(0).gameObject;
    info_box_text_ = info_box_.GetComponentInChildren<Text>(true);

    AirConsole.instance.onMessage += OnMessage;
    RefreshWaitingScreen("Waiting for players");

  }
  void Update()
  {
    if (Input.GetKeyDown("p"))
    {
      StartLevel();
    }
  }

  void OnGUI() {
    if (label_queue_.Count > 0) {
      if (Time.time < label_queue_[0].Key) { // display queue element 0
        string text = label_queue_[0].Value;
        info_box_text_.text = text;
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
      StartExtendedTutorial();
    }
  }

  //action

  private void StartLevel()
  {
    current_level_.AddComponent<LevelScript>();
    MTLLoader loader = current_level_.AddComponent<MTLLoader>();
    loader.level_file = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("assets/Tiledmaps/test_3.json");
    //loader.scale = 1.0f;
    GameObject.Find("Players").GetComponent<PlayersScript>().join_enabled_ = false;

    if (loader.level_file != null)
    {
      loader.Load();
      (current_level_.GetComponent<LevelScript>()).FocusSomeone();
      state_ = GameState.PLAY;
      AirConsole.instance.Broadcast("GameStarts");
      GameObject.Find("WaitingScreen").SetActive(false);
    



    }
    else
      Debug.Log("Couldn't load file");
  }

  public void StartExtendedTutorial()
  {
    current_level_.AddComponent<LevelScript>();
    MTLLoader loader = current_level_.AddComponent<MTLLoader>();
    loader.level_file = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("assets/Tiledmaps/tutorial_ex.json");
    //loader.scale = 1.0f;
    GameObject.Find("Players").GetComponent<PlayersScript>().join_enabled_ = false;

    if (loader.level_file != null)
    {
      loader.Load();
      (current_level_.GetComponent<LevelScript>()).FocusSomeone();
      state_ = GameState.PLAY;
      AirConsole.instance.Broadcast("GameStarts");
      GameObject.Find("WaitingScreen").SetActive(false);
      GameObject.Find("Players").GetComponent<PlayersScript>().MoveAllPlayers(new Vector3(0.0f, 2.95f, 0));
      DisplayLabel("The crew of *some funny spaceship name* has gathered...\nCaptain X needs the help of his crew to start the engine!", 10);
      DisplayLabel("Use the FOCUS button to center the screen on your character.\nBEWARE: only one player can be focused at any time!", 30);
    }
    else
      Debug.Log("Couldn't load file");
  }
  

  public void RefreshWaitingScreen(string text)
  {
    GameObject.Find("WaitingScreenText").GetComponent<Text>().text = text;
  }

  public LevelScript GetCurrentLevel()
  {
    return current_level_.GetComponent<LevelScript>();
  }

  public void DisplayLabel(string text, float seconds = 60) {
    float end_time;
    if (label_queue_.Count > 0) {
      // display the new label until *seconds* seconds after the last label in the queue 
      end_time = label_queue_[label_queue_.Count - 1].Key + seconds;
    } else {
      // display the new label until *now* + *seconds*
      end_time = Time.time + seconds;
    }
    label_queue_.Add(new KeyValuePair<float, string>(end_time, text));
  }
}


