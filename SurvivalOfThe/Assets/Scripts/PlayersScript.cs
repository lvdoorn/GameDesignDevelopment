using UnityEngine;
using System.Linq;
using System.Collections.Generic;

using NDream.AirConsole;
using Newtonsoft.Json.Linq;

public class PlayersScript : MonoBehaviour
{
  private Dictionary<int, GameObject> players_;
  private List<int> disconnected_;
  private Dictionary<int, int> players_number_;
  private GameState saved_state = GameState.PLAY;
  public GameObject prefab;

  void Awake()
  {
    players_ = new Dictionary<int, GameObject>();
    players_number_ = new Dictionary<int, int>();
    disconnected_ = new List<int>();

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
    clone.GetComponent<PlayerScript>().layer_ = 2;
        
    clone.GetComponent<SpriteRenderer>().enabled = true;
    clone.GetComponent<PlayerScript>().enabled = true;
    clone.GetComponent<AudioSource>().enabled = true;
    
    clone.GetComponent<PlayerScript>().setId(id);
    clone.GetComponent<PlayerScript>().Nickname = AirConsole.instance.GetNickname(id);

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

    players_.Add(id, clone);
    players_number_.Add(id, players_.Count-1);

    if(players_.Count < 3)
      GameObject.Find("Game").GetComponent<GameScript>().RefreshWaitingScreen("Waiting for players", players_.Count+" players connected");
    else
      GameObject.Find("Game").GetComponent<GameScript>().RefreshWaitingScreen("Start", players_.Count + " players connected");
    
    GameObject.Find("Game").GetComponent<GameScript>().ShowStatusMessage(AirConsole.instance.GetNickname(id) + " has joined", "controller_on");
    SetFocus(id);
  }

  void RemovePlayer(int id)
  {
    string nickname = players_[id].GetComponent<PlayerScript>().Nickname;
    Destroy(players_[id]);
    int number = players_number_[id];
    players_.Remove(id);
    players_number_.Remove(id);
    
    // shift player numbers
    foreach (int dev in new List<int>(players_number_.Keys)) {
      if (players_number_[dev] > number) {
        players_number_[dev]--;
      }
    }

    GameScript game = GameObject.Find("Game").GetComponent<GameScript>();
    if (players_.Count < 3)
      game.RefreshWaitingScreen("Waiting for players", players_.Count + " players connected");
    else
      game.RefreshWaitingScreen("Start", players_.Count + " players connected");

    game.ShowStatusMessage(nickname + " has left", "controller_off");
  }

  void DisconnectPlayer(int id)
  {
    GameScript game = GameObject.Find("Game").GetComponent<GameScript>();
    disconnected_.Add(id);
    
    saved_state = game.State;
    game.State = GameState.DISCONNECTED;
    game.transform.Find("UI/PauseScreen").gameObject.SetActive(true);
    game.ShowStatusMessage(players_[id].GetComponent<PlayerScript>().Nickname + " has disconnected!", "controller_off");
  }

  void ReconnectPlayer(int id) {
    GameScript game = GameObject.Find("Game").GetComponent<GameScript>();
    disconnected_.Remove(id);
    
    if (disconnected_.Count == 0) {
      game.transform.Find("UI/PauseScreen").gameObject.SetActive(false);
      game.State = saved_state;
    }
    game.ShowStatusMessage(players_[id].GetComponent<PlayerScript>().Nickname + " has reconnected!", "controller_on");
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
    return players_number_[i];
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
    GameScript game = GameObject.Find("Game").GetComponent<GameScript>();
    if (game.State == GameState.JOIN && players_.Count < 8) {
      AddPlayer(device_id);
    } else if (game.State == GameState.DISCONNECTED && disconnected_.Contains(device_id)) {
      ReconnectPlayer(device_id);
    } else {
      GameObject.Find("Game").GetComponent<GameScript>().ShowStatusMessage(AirConsole.instance.GetNickname(device_id) + " is spectating", "controller_on");
    }
  }

  void OnDisconnect(int device_id)
  {
    if (GameObject.Find("Game").GetComponent<GameScript>().State == GameState.JOIN) {
      if (players_.ContainsKey(device_id)) {
        RemovePlayer(device_id);
      }
    } else {
      if (players_.ContainsKey(device_id)) {
        DisconnectPlayer(device_id);
      }
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
