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
   // ShowTutorial ();
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
      ChangeLevel("Tiledmaps/tutorial_ex_json");
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
      ChangeLevel("Tiledmaps/tutorial_ex_json");
    }
  }

  /*IEnumerator CoRoutine() {
    yield return new WaitForSeconds(10);
    tutorialText.text = ("Welcome to survival of the Zargs!");
    yield return new WaitForSeconds(5);
    tutorialText.text = ("Cooperate to repair the spaceship");
    yield return new WaitForSeconds(5);
    tutorialText.text = ("Press FOCUS to center the camera on your player");
    yield return new WaitForSeconds(5);
    tutorialText.text = ("Use the action buttons to vote when required");
    yield return new WaitForSeconds(5);
    tutorialText.text = "";
  }*/

  //action

  private void StartLevel()
  {
    current_level_.AddComponent<LevelScript>();
    MTLLoader loader = current_level_.AddComponent<MTLLoader>();
    loader.level_file = Resources.Load("Tiledmaps/test_3.json") as TextAsset;
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

  public void ChangeLevel(string lvl)//"Tiledmaps/tutorial_ex_json"
  {
    LevelScript ls = current_level_.GetComponent<LevelScript>();

    MTLLoader loader;
    if (ls != null)
    {
      loader = current_level_.GetComponent<MTLLoader>();
      loader.Clear();

      loader.level_file = Resources.Load(lvl) as TextAsset;
      Destroy(current_level_.GetComponent<LevelScript>());
      current_level_.AddComponent<LevelScript>();
    }
    else
    {
      current_level_.AddComponent<LevelScript>();
      loader = current_level_.AddComponent<MTLLoader>();
      state_ = GameState.PLAY;
      AirConsole.instance.Broadcast("GameStarts");
      GameObject.Find("WaitingScreen").SetActive(false);
      DisplayLabel("Use action buttons to interact.\nTry to escape the crashed ship.", 5);
      loader.level_file = Resources.Load(lvl) as TextAsset;
    }
       
    //loader.scale = 1.0f;
    GameObject.Find("Players").GetComponent<PlayersScript>().join_enabled_ = false;

    if (loader.level_file != null)
    {
      loader.Load();
    
      GameObject.Find("Players").GetComponent<PlayersScript>().MoveAllPlayers(new Vector3(0.0f, 2.95f, -8));
      (current_level_.GetComponent<LevelScript>()).FocusSomeone();
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


