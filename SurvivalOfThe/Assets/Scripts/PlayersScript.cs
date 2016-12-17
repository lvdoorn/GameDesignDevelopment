﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using NDream.AirConsole;
using Newtonsoft.Json.Linq;

public class PlayersScript : MonoBehaviour
{
  private Dictionary<int, GameObject> players_;

  public GameObject prefab;

  public bool join_enabled_ = true;

  void Start()
  {
    int player_count = 3;
    players_ = new Dictionary<int, GameObject>();

    AirConsole.instance.onConnect += OnConnect;
    AirConsole.instance.onDisconnect += OnDisconnect;   
    
    SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
  }

  void Update()
  {
  }

  // player handling

  void AddPlayer(int id)
  {
    GameObject clone = Instantiate(prefab, prefab.transform.position, prefab.transform.rotation) as GameObject;
    Debug.Log("Added Player "+id.ToString());

    clone.GetComponent<SpriteRenderer>().enabled = true;
    clone.GetComponent<PlayerScript>().enabled = true;
    clone.GetComponent<PlayerScript>().setId(id);
    clone.GetComponent<PlayerScript>().layer_ = 0;
    if (id == 1)
      clone.GetComponent<PlayerScript>().layer_ = 1;    

    players_.Add(id,clone);

    SetFocus(id);
  }

  void RemovePlayer(int id)
  {
    Destroy(players_[id]);
    players_.Remove(id);
  }

  public GameObject GetPlayer(int id)
  {
    return players_[id];
  }

  // airconsole handlers
  void OnConnect(int device_id)
  {
    if(join_enabled_)
      AddPlayer(device_id);
  }

  void OnDisconnect(int device_id)
  {
    RemovePlayer(device_id);
  }


  // publics 

  public void SetFocus(int id)
  {
    PlayerScript ps;
    foreach (KeyValuePair<int,GameObject> player in players_ )
    {
      ps = player.Value.GetComponent<PlayerScript>();   
      ps.has_focus_ = false;
    }
    ps = players_[id].GetComponent<PlayerScript>();
    ps.has_focus_ = true;    
  }

  public void LayerSwitched(int layer_id)
  {
    foreach(KeyValuePair<int, GameObject> player in players_)
    {
      PlayerScript ps = player.Value.GetComponent<PlayerScript>();
      if(ps.layer_ == layer_id)
      {
        ps.enabled = true;
        ps.gameObject.SetActive(true);
      }
      else
      {
        ps.enabled = false;
        ps.gameObject.SetActive(false);
      }
    }
  }

}
