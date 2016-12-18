using UnityEngine;
using System.Collections;

using NDream.AirConsole;
using Newtonsoft.Json.Linq;

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
    }
    else
      Debug.Log("Couldn't load file");
  }
}


