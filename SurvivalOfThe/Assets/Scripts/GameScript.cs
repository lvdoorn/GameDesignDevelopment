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

  void Start()
  {
   // ShowTutorial ();
    current_level_ = new GameObject();
    current_level_.transform.SetParent(transform);

    AirConsole.instance.onMessage += OnMessage;
    RefreshWaitingScreen("Waiting for players");

  }
  void Update()
  {
    if (Input.GetKeyDown("p"))
    {
      ChangeLevel("Tiledmaps/tutorial_ex_json");
      ChangePositions(new Vector3(0.0f, 2.95f, -8));
    }
  }

  //ariconsole handler
  void OnMessage(int from, JToken data)
  {
    if (data["start"] != null)
    {
      Debug.Log("received start");
      ChangeLevel("Tiledmaps/tutorial_ex_json");
      ChangePositions(new Vector3(0.0f, 2.95f, -8));
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

  public void StartTutorial() {
    ChangeLevel("Tiledmaps/tutorial_json", 10.0f / 4.0f);
    ChangePositions(
      new Vector3(-8.5f, 9.0f, 0),
      new Vector3(-4.5f, 10.0f, 0),
      new Vector3(-7.5f, 10.0f, 0),
      new Vector3(-1.5f, 10.0f, 0),
      new Vector3(4.5f, 10.0f, 0),
      new Vector3(10.5f, 10.0f, 0),
      new Vector3(7.5f, 10.0f, 0),
      new Vector3(1.5f, 10.0f, 0)
    );
    GetCurrentLevel().DisplayInfoBox("Welcome to Survival of the Zargs! Use the ACTION buttons on your device to interact.", 10);
  }

  public void ChangePositions(params Vector3[] vs) {
    if (vs.Length == 1) {
      GameObject.Find("Players").GetComponent<PlayersScript>().MoveAllPlayers(vs[0]);
    } else {
      GameObject.Find("Players").GetComponent<PlayersScript>().MoveAllPlayers(vs);
    }
  }

  public void ChangeLevel(string lvl, float scale = 1.0f)//"Tiledmaps/tutorial_ex_json"
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
      GetCurrentLevel().DisplayInfoBox("Use action buttons to interact.\nTry to escape the crashed ship.", 5);
      loader.level_file = Resources.Load(lvl) as TextAsset;
    }
       
    loader.scale = scale;
    GameObject.Find("Players").GetComponent<PlayersScript>().join_enabled_ = false;

    if (loader.level_file != null)
    {
      loader.Load();
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
}


