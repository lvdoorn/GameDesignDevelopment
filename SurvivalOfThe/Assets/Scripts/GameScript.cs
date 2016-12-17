using UnityEngine;
using System.Collections;

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

  }
  void Update()
  {
    if (Input.GetKeyDown("p"))
    {
      StartLevel();
    }
  }

  private void StartLevel()
  {
    current_level_.AddComponent<LevelScript>();
    MTLLoader loader = current_level_.AddComponent<MTLLoader>();
    loader.level_file = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>("assets/Tiledmaps/test_2.json");
    GameObject.Find("Players").GetComponent<PlayersScript>().join_enabled_ = false;
    if (loader.level_file != null)
    {
      loader.Load();
    }
    else
      Debug.Log("Couldn't load file");
  }
}
