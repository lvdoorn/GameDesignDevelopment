using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using NDream.AirConsole;
using Newtonsoft.Json.Linq;

public class PlayersScript : MonoBehaviour
{
  private Dictionary<int, GameObject> players_;

  public GameObject prefab;

  public bool join_enabled_ = true;

  void Awake()
  {
    players_ = new Dictionary<int, GameObject>();

    AirConsole.instance.onConnect += OnConnect;
    AirConsole.instance.onDisconnect += OnDisconnect;
    AirConsole.instance.onMessage += OnMessage;

    SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
  }

  // player handling

  void AddPlayer(int id)
  {
    GameObject clone = Instantiate(prefab, prefab.transform.position, prefab.transform.rotation) as GameObject;
    Debug.Log("Added Player "+id.ToString());

    clone.transform.position = new Vector3(1, 0.28f, 0);
    clone.GetComponent<SpriteRenderer>().enabled = true;
    clone.GetComponent<PlayerScript>().enabled = true;
    clone.GetComponent<PlayerScript>().setId(id);

    clone.GetComponent<PlayerScript>().layer_ = 2;   

    players_.Add(id,clone);

    if(players_.Count < 3)
      GameObject.Find("Game").GetComponent<GameScript>().RefreshWaitingScreen("Waiting for players", players_.Count+" players connected");
    else
      GameObject.Find("Game").GetComponent<GameScript>().RefreshWaitingScreen("Start", players_.Count + " players connected");

    SetFocus(id);
  }

  void RemovePlayer(int id)
  {
    Destroy(players_[id]);
    players_.Remove(id);

    if (players_.Count < 3)
      GameObject.Find("Game").GetComponent<GameScript>().RefreshWaitingScreen("Waiting for players", players_.Count + " players connected");
    else
      GameObject.Find("Game").GetComponent<GameScript>().RefreshWaitingScreen("Start", players_.Count + " players connected");
  }

  public GameObject GetPlayer(int id)
  {
    return (players_.ContainsKey(id) ? players_[id] : null );
  }
  public GameObject GetFirstPlayer()
  {
    for(int x=0; x <100;x++)
    {
      GameObject g = GetPlayer(x);
      if(g != null)
      {
        return g;
      }
    }
    return null;
  }

  public void MoveAllPlayers(Vector3 v)
  {
    foreach( KeyValuePair<int,GameObject> ko in players_)
    {
      ko.Value.transform.position = new Vector3(v.x, v.y, -8);
      PlayerScript ps = ko.Value.GetComponent<PlayerScript>();
      ps.layer_ = (int)v.z;
      Debug.Log("Move Player " + ko.Key.ToString());
    }
  }
  public void MoveAllPlayers(Vector3[] vs) {
    if (vs.Length == 0)
      return;

    int index = 0;
    foreach (GameObject player in players_.Values) {
      if (index >= vs.Length) {
        index -= vs.Length;
      }
      player.transform.position = new Vector3(vs[index].x, vs[index].y, -8);
      PlayerScript ps = player.GetComponent<PlayerScript>();
      ps.layer_ = (int)vs[index].z;
      index++;
    }
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
  void OnMessage(int from, JToken data)
  {
    if (GameObject.Find("Game").GetComponent<GameScript>().State == GameState.PLAY)
    {
      if (data["action"] != null)
      {
        if (((int)data["action"]) == 1)
        {
          Debug.Log("received action1");
          GameObject player = players_[from];
          Vector3 pos = player.transform.position;
          LevelScript ls = GameObject.Find("Game").GetComponent<GameScript>().GetCurrentLevel();
          if (ls != null)
          {
            ls.TriggerObject(pos);
          }
        }
      }
    }
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
  public int GatherVotes()
  {
    int v1 = 0;
    int v2 = 0;
    foreach (KeyValuePair<int, GameObject> player in players_)
    {
      int v = player.Value.GetComponent<PlayerScript>().getVote();
      if (v == -1)
        return -1;
      if (v == 0)
        v1++;
      if (v == 1)
        v2++;
    }
    if (v1 > v2)
      return 0;
    else
      return 1;
  }
  public List<int> VotesYes()
  {
    List<int> tl = new List<int>();
    foreach (KeyValuePair<int, GameObject> player in players_)
    {
      int v = player.Value.GetComponent<PlayerScript>().getVote();
      if (v == 0)
        tl.Add(player.Key);        
    }
    return tl;
  }
  public List<int> VotesNo()
  {
    List<int> tl = new List<int>();
    foreach (KeyValuePair<int, GameObject> player in players_)
    {
      int v = player.Value.GetComponent<PlayerScript>().getVote();
      if (v == 1)
        tl.Add(player.Key);
    }
    return tl;
  }

  public void SetVoteMode()
  {
    foreach (KeyValuePair<int, GameObject> player in players_)
    {
      player.Value.GetComponent<PlayerScript>().resetVote();
    }
  }
  public void RefreshVisibility()
  {
    int active_idx =0;
    int layer_id = 0;
    foreach (KeyValuePair<int, GameObject> player in players_)
    {
      if (player.Value.GetComponent<PlayerScript>().hasFocus() )
      {
        active_idx = player.Key;
        layer_id = player.Value.GetComponent<PlayerScript>().layer_;
      }
    }
    LayerSwitched(layer_id);
  }

  public int PlayerCount() {
    return players_.Count;
  }

}
