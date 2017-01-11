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


