using UnityEngine;
using System.Collections;

using NDream.AirConsole;
using Newtonsoft.Json.Linq;


public class LevelScript : MonoBehaviour
{
  private PlayersScript players_;
  private int current_layer_ = -1;

	void Start ()
  {
    players_ = GameObject.Find("Players").GetComponent<PlayersScript>();

    AirConsole.instance.onMessage += OnMessage;
  }	
	void Update ()
  {
    /*
    if (Input.GetKeyDown("1"))
    {
      Debug.Log("kd1");
      SetFocus(1);
    }
    if (Input.GetKeyDown("2"))
    {
      Debug.Log("kd2");
      SetFocus(2);
    }
    if (Input.GetKeyDown("3"))
    {
      Debug.Log("kd3");
      SetFocus(3);
    }*/

  }
  public void Init()
  {
    players_ = GameObject.Find("Players").GetComponent<PlayersScript>();
  }

  // airconsole handlers

  void OnMessage(int from, JToken data)
  {
    if (data["focus"] != null)
    {
      Debug.Log((string)data["focus"]);
      int id = from;
      SetFocus(id);
    }
  }

  // actions 

  private void SwitchLayer(int layer_id)
  {
    if (layer_id != current_layer_)
    {
      Debug.Log("LevelLayer" + layer_id.ToString());
      players_.LayerSwitched(layer_id);
      foreach (Transform child in transform)
      {
        if (child.gameObject.name.Contains("LevelLayer" + layer_id.ToString()) )
        {
          child.gameObject.SetActive(false);
        }
        else
          child.gameObject.SetActive(true);
      }
    }
    current_layer_ = layer_id;
  }

  private void SetFocus(int player_id)
  {
    GameObject pl = players_.GetPlayer(player_id);
    SwitchLayer((pl.GetComponent<PlayerScript>()).layer_);
    players_.SetFocus(player_id);
  }

}
