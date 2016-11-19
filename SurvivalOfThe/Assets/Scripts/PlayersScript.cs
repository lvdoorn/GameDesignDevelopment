using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using NDream.AirConsole;
using Newtonsoft.Json.Linq;

public class PlayersScript : MonoBehaviour
{
  private List<GameObject> players_;

  public GameObject prefab;

  // Use this for initialization
  void Start()
  {
    int player_count = 3;
    players_ = new List<GameObject>();

    AirConsole.instance.onConnect += OnConnect;
    AirConsole.instance.onDisconnect += OnDisconnect;   
    
    SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
  }

  // Update is called once per frame
  void Update()
  {

  }

  void AddPlayer(int id)
  {
    GameObject clone = Instantiate(prefab, prefab.transform.position, prefab.transform.rotation) as GameObject;
    Debug.Log("inst");

    clone.GetComponent<SpriteRenderer>().enabled = true;
    clone.GetComponent<PlayerScript>().enabled = true;
    clone.GetComponent<PlayerScript>().setId(id);

    players_.Add(clone);

    SetFocus(id);
  }
  void RemovePlayer(int id)
  {
  
    for (int i = players_.Count - 1; i > -1; i--)
    {
      PlayerScript ps = players_[i].GetComponent<PlayerScript>();
      if (ps.getId() ==  id)
      {
        Destroy(players_[i] );
        players_.RemoveAt(i);
      }
    }
  }

  void OnConnect(int device_id)
  {
    AddPlayer(device_id);
  }

  void OnDisconnect(int device_id)
  {
    RemovePlayer(device_id);
  }
  void SetFocus(int id)
  {
    for (int i = players_.Count - 1; i > -1; i--)
    {
      PlayerScript ps = players_[i].GetComponent<PlayerScript>();
      if (ps.getId() == id)
        ps.has_focus_ = true;
      else
        ps.has_focus_ = false;

    }
  }
}
