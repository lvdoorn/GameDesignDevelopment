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
      //Debug.Log((string)data["focus"]);
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
        if (!child.gameObject.name.Contains("LevelLayer" + layer_id.ToString()))
        {
          Debug.Log("Switch off :" + child.gameObject.name);
          child.gameObject.SetActive(false);
        }
        else
        {
          Debug.Log("Switch on :" + child.gameObject.name);
          child.gameObject.SetActive(true);
        }
      }
    }
    current_layer_ = layer_id;
  }

  public void SetFocus(int player_id)
  {
    GameObject pl = players_.GetPlayer(player_id);
    if (pl != null)
    {
      SwitchLayer((pl.GetComponent<PlayerScript>()).layer_);
      players_.SetFocus(player_id);
    }
  }
  public void FocusSomeone()
  {
    int player_id = 0;

    while (player_id < 100)
    {
      GameObject pl = players_.GetPlayer(player_id);
      if (pl != null)
      {
        SwitchLayer((pl.GetComponent<PlayerScript>()).layer_);
        players_.SetFocus(player_id);
        player_id = 100;
      }
      player_id++;
    }
  }

  private void FocusInput( string type )
  {

  }
  private void DefocusInput()
  {

  }



  // scripted 

  public void TriggerObject(Vector3 player_position)
  {
    GameObject lobjs = GameObject.Find("LevelLayer" + current_layer_.ToString()).transform.FindChild("Objects").gameObject;
    if (lobjs != null)
    {
      foreach (Transform child in lobjs.transform)
      {        
        float d = Vector3.Distance(child.GetChild(0).position, player_position);
        if(d < 0.3f)
        {
          Debug.Log(child.gameObject.name);
          string action = child.gameObject.GetComponent<ObjectScript>().action;
          Debug.Log(action);
          if ( action.StartsWith("destroy") )
          {
            string[] parts = action.Split(':');
            RemoveObject(parts[1]);
          }
        }
      }
    }
    else
      Debug.Log("couldn't find obj");
  }

  public void RemoveObject(string name)
  {
    Debug.Log("Destroying "+name);
    Destroy( GameObject.Find(name) );
  }


}
