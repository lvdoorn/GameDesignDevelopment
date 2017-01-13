using UnityEngine;
using System.Collections;

using NDream.AirConsole;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;

enum GameState
{
  JOIN =0,
  PLAY =1
}

public class GameScript : MonoBehaviour
{
  private GameState state_ = GameState.JOIN;
  private GameObject current_level_;
  public Text tutorialText;

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
     // StartLevel();
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

  private void ShowTutorial()
  {
    //StartCoroutine(CoRoutine());
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
    //ShowTutorial ();
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

  public void StartExtendedTutorial()
  {
    current_level_.AddComponent<LevelScript>();
    MTLLoader loader = current_level_.AddComponent<MTLLoader>();
    loader.level_file = Resources.Load("Tiledmaps/tutorial_ex_json") as TextAsset;
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
      (current_level_.GetComponent<LevelScript>()).MessageToDebug("Use action buttons to interact."); 
      (current_level_.GetComponent<LevelScript>()).MessageToDebug("Try to escape the crashed ship. ");
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


