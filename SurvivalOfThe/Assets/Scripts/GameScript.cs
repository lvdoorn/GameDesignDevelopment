using UnityEngine;
using System.Collections;

using NDream.AirConsole;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;

enum GameState
{
  JOIN = 0,
  PLAY = 1
}

public class GameScript : MonoBehaviour
{
  private GameState state_ = GameState.JOIN;
  private GameObject current_level_;
  public Text tutorialText;
  public Text waitingScreenText;
  public Animation animation;

  void Start()
  {
    current_level_ = new GameObject();
    current_level_.transform.SetParent(transform);

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

  //ariconsole handler
  void OnMessage(int from, JToken data)
  {
    if (data["start"] != null)
    {
      StartLevel();
    }
  }

  private void ShowTutorial()
  {
    StartCoroutine(Example());
  }

  IEnumerator Example() {
    tutorialText.text = ("Welcome to survival of the Zargs!");
    yield return new WaitForSeconds(5);
    tutorialText.text = ("Cooperate to repair the spaceship");
    yield return new WaitForSeconds(5);
    tutorialText.text = ("Press FOCUS to center the camera on your player");
    yield return new WaitForSeconds(5);
    tutorialText.text = ("Use the action buttons to vote when required");
  }

  //action

  private void StartLevel()
  {
    ShowTutorial();
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
  

  public void RefreshWaitingScreen(string text)
  {
    waitingScreenText.text = text;
  }
}


