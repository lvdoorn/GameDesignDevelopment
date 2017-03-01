using UnityEngine;
using System.Linq;
using System.Collections.Generic;

using NDream.AirConsole;
using Newtonsoft.Json.Linq;

public class PlayersScript : MonoBehaviour
{
  private Dictionary<int, GameObject> players_;
  private Dictionary<int, KeyValuePair<string, GameObject>> disconnected_; //keep track of disconnected players so they can reconnect
  private GameState saved_state = GameState.PLAY;
  public GameObject prefab;

  void Awake()
  {
    players_ = new Dictionary<int, GameObject>();
    disconnected_ = new Dictionary<int, KeyValuePair<string, GameObject>>();

    AirConsole.instance.onConnect += OnConnect;
    AirConsole.instance.onDisconnect += OnDisconnect;
    AirConsole.instance.onMessage += OnMessage;

    SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
  }

  // player handling
  private GameObject InstantiatePlayer(int id) {
    GameObject clone = Instantiate(prefab, prefab.transform.position, prefab.transform.rotation) as GameObject;
    Debug.Log("Added Player "+id.ToString());

    clone.transform.position = new Vector3(1, 0.28f, 0);

    clone.GetComponent<PlayerScript>().setId(id);
    clone.GetComponent<PlayerScript>().Nickname = AirConsole.instance.GetNickname(id);

    clone.GetComponent<PlayerScript>().layer_ = 2;
    if( players_.Count == 1 )
      clone.GetComponent<SpriteRenderer>().color = Color.blue;
    if (players_.Count == 2)
      clone.GetComponent<SpriteRenderer>().color = Color.yellow;
    if (players_.Count == 3)
      clone.GetComponent<SpriteRenderer>().color = Color.red;
    if (players_.Count == 4)
      clone.GetComponent<SpriteRenderer>().color = Color.green;
    if (players_.Count == 5)
      clone.GetComponent<SpriteRenderer>().color = Color.magenta;
    if (players_.Count == 6)
      clone.GetComponent<SpriteRenderer>().color = Color.grey;
    if (players_.Count == 7)
      clone.GetComponent<SpriteRenderer>().color = Color.cyan;

    return clone;
  }
  
  void AddPlayer(int id)
  {
    GameObject clone = InstantiatePlayer(id);
    clone.GetComponent<SpriteRenderer>().enabled = true;
    clone.GetComponent<PlayerScript>().enabled = true;
    clone.GetComponent<AudioSource>().enabled = true;

    players_.Add(id,clone);
    AirConsole.instance.SetActivePlayers();

    if(players_.Count < 3)
      GameObject.Find("Game").GetComponent<GameScript>().RefreshWaitingScreen("Waiting for players", players_.Count+" players connected");
    else
      GameObject.Find("Game").GetComponent<GameScript>().RefreshWaitingScreen("Start", players_.Count + " players connected");
    
    GameObject.Find("Game").GetComponent<GameScript>().ShowStatusMessage(AirConsole.instance.GetNickname(id) + " has joined!", "controller_on");
    SetFocus(id);
  }

  void DisconnectPlayer(int id)
  {
    GameScript game = GameObject.Find("Game").GetComponent<GameScript>();
    string nickname = players_[id].GetComponent<PlayerScript>().Nickname;
    //Destroy(players_[id]);
    players_[id].GetComponent<SpriteRenderer>().enabled = false;
    players_[id].GetComponent<PlayerScript>().enabled = false;
    players_[id].GetComponent<AudioSource>().enabled = false;
    disconnected_.Add(id, new KeyValuePair<string, GameObject>(game.GetCurrentLevel().name, players_[id]));
    players_.Remove(id);

    if (disconnected_[id].Value.GetComponent<PlayerScript>().has_focus_) {
      disconnected_[id].Value.GetComponent<PlayerScript>().has_focus_ = false;
      if (players_.Count > 0) {
        players_.First().Value.GetComponent<PlayerScript>().has_focus_ = true;
      }
    }
    
    if (players_.Count < 3) {
      if (game.State != GameState.JOIN) {
        saved_state = game.State;
        game.State = GameState.JOIN;
        game.DisplayInfoBox("We have lost too many members of our crew!", 10, "Alien");
        game.transform.Find("UI/PauseScreen").gameObject.SetActive(true);
      }
      game.RefreshWaitingScreen("Waiting for players", players_.Count + " players connected");
    } else {
      game.RefreshWaitingScreen("Start", players_.Count + " players connected");
    }

    game.ShowStatusMessage(nickname + " has left!", "controller_off");
  }

  void ReconnectPlayer(int id) {
    GameScript game = GameObject.Find("Game").GetComponent<GameScript>();
    string nickname = disconnected_[id].Value.GetComponent<PlayerScript>().Nickname;
    disconnected_[id].Value.GetComponent<SpriteRenderer>().enabled = true;
    disconnected_[id].Value.GetComponent<PlayerScript>().enabled = true;
    disconnected_[id].Value.GetComponent<AudioSource>().enabled = true;
    if (game.GetCurrentLevel().name != disconnected_[id].Key && players_.Count > 0) {
      MovePlayer(disconnected_[id].Value, players_.First().Value);
    }
    players_.Add(id, disconnected_[id].Value);
    disconnected_.Remove(id);
    
    if (players_.Count >= 3 && game.State == GameState.JOIN) {
      game.transform.Find("UI/PauseScreen").gameObject.SetActive(false);
      game.State = saved_state;
    }
    game.ShowStatusMessage(nickname + " has rejoined!", "controller_on");
  }

  public GameObject GetPlayer(int id)
  {
    return (players_.ContainsKey(id) ? players_[id] : null );
  }
  public GameObject GetFirstPlayer()
  {
    if (players_.Count == 0) {
      return null;
    }
    return players_.First().Value;
  }
  public int GetPlayerNumber(int i)
  {
    return AirConsole.instance.ConvertDeviceIdToPlayerNumber(i);
  }

  public void MoveAllPlayers(Vector3 v)
  {
    foreach( KeyValuePair<int,GameObject> ko in players_)
    {
      MovePlayer(ko.Value, v);
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
      MovePlayer(player, vs[index]);
      index++;
    }
  }
  public void MovePlayer(GameObject player, Vector3 position) {
    player.transform.position = new Vector3(position.x, position.y, -8);
    PlayerScript ps = player.GetComponent<PlayerScript>();
    ps.layer_ = (int)position.z;
  }
  public void MovePlayer(GameObject movedPlayer, GameObject targetPlayer) {
    Vector3 position = targetPlayer.transform.position;
    position.z = targetPlayer.GetComponent<PlayerScript>().layer_;
    MovePlayer(movedPlayer, position);
  }

  // airconsole handlers
  void OnConnect(int device_id)
  {
    if (players_.Count < 8) {
      if (disconnected_.ContainsKey(device_id)) {
        ReconnectPlayer(device_id);
      } else {
        AddPlayer(device_id);
        if (players_.Count > 0) {
          MovePlayer(players_[device_id], players_.First().Value);
        }
      }
    } else {
      GameObject.Find("Game").GetComponent<GameScript>().ShowStatusMessage(AirConsole.instance.GetNickname(device_id) + " is spectating", "controller_on");
    }
  }

  void OnDisconnect(int device_id)
  {
    if (players_.ContainsKey(device_id)) {
      DisconnectPlayer(device_id);
    }
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

  public bool HaveItem(string item)
  {
    foreach (KeyValuePair<int, GameObject> player in players_)
    {
      if (player.Value.GetComponent<PlayerScript>().HasItem(item))
        return true;
    }
    return false;
  }
  public int GetNumberOfPlayers()
  {
    return players_.Count;
  }

}
