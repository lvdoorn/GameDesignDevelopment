using UnityEngine;
using System.Collections;

using NDream.AirConsole;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
using System.Collections.Generic;

public enum GameState
{
  JOIN,
  PLAY,
  PUZZLE,
  VOTE,
  INTERMISSION,
  DISCONNECTED,
  INTRO
}

public enum Fruits : byte
{
  NONE = 0,
  RED = 1,
  PURPLE = 2,
  BLUE = 4,
  ALL = RED | PURPLE | BLUE
}

public class GameScript : MonoBehaviour
{
  private GameObject current_level_;
  public float Scale { get; set; }
  private GameState state_;
  private CtrlState controller_state_;
  public GameState State {
    get {
      return state_;
    }
    set {
      state_ = value;
      if (AirConsole.instance.IsAirConsoleUnityPluginReady()) {
        switch (state_) {
          case GameState.JOIN:
            int number_of_players = GameObject.Find("Players").GetComponent<PlayersScript>().GetNumberOfPlayers();
            if (number_of_players < 3) {
              ControllerState = new CtrlState() {
                state = "Wait",
                text = "Waiting for " + (3 - number_of_players) + "\r\nmore player" + ((3 - number_of_players) == 1 ? "" : "s")
              };
            } else {
              ControllerState = new CtrlState() {
                state = "Join",
                action1 = "Start game"
              };
            }
            break;
          case GameState.PLAY:
            ControllerState = new CtrlState() {
              state = "Play",
              action1 = "Interact"
            };
            break;
          case GameState.PUZZLE:
            ControllerState = new CtrlState() {
              state = "Play"
            };
            break;
          case GameState.VOTE:
            ControllerState = new CtrlState() {
              state = "Vote",
              action1 = "Yes",
              action2 = "No"
            };
            break;
          case GameState.INTRO:
            ControllerState = new CtrlState() {
              state = "Wait",
              text = "zZz zzz zZz"
            };
            break;
          default:
            ControllerState = new CtrlState() {
              state = "Wait",
              text = "Please wait..."
            };
            break;
        }
      }
    }
  }
  public CtrlState ControllerState {
    get {
      return controller_state_;
    }
    set {
      controller_state_ = value;
      AirConsole.instance.SetCustomDeviceState(controller_state_);
    }
  }
  public Fruits CollectedFruits { get; set; }

  public struct InfoBoxMessage {
    public float time;
    public string text;
    public string image_name;
    public bool initialized;
  }

  public struct CtrlState {
    public string state;
    public string action1;
    public string action2;
    public string text;
  }
  
  private List<InfoBoxMessage> label_queue_ = new List<InfoBoxMessage>();
  private GameObject info_box_;
  private Text info_box_text_;

  void Awake()
  {
   // ShowTutorial ();
    //current_level_ = new GameObject();
    //current_level_.transform.SetParent(transform);
    
    info_box_ = transform.Find("UI/InfoBox").gameObject;
    info_box_text_ = info_box_.GetComponentInChildren<Text>(true);
    
    controller_state_ = new CtrlState();
    state_ = GameState.JOIN;
    CollectedFruits = Fruits.NONE;

    AirConsole.instance.onMessage += OnMessage;
    RefreshWaitingScreen(0);

    //StartIntro();

  }
  void Update()
  {
    if(State == GameState.INTERMISSION)
    {
      GameObject screen = GameObject.Find("Game").transform.FindChild("UI").FindChild("IntermissionScreen").gameObject;
      screen.transform.FindChild("IntermissionText").position += new Vector3(0, 1.0f, 0);
      if (screen.transform.FindChild("IntermissionText").position.y > 380  + screen.transform.FindChild("IntermissionText").GetComponent<Text>().preferredHeight +20.0f )
        EndIntermission();
    }
  }
  void OnGUI() {
    if (label_queue_.Count > 0) {
      if (Time.time < label_queue_[0].time) { // display queue element 0
        if (!label_queue_[0].initialized) {
          InfoBoxMessage ibm = label_queue_[0];
          ibm.initialized = true;
          label_queue_[0] = ibm;
          string text = label_queue_[0].text;
          info_box_text_.text = text;
          foreach (Image img in info_box_.GetComponentsInChildren<Image>(true)) {
            GameObject go = img.gameObject;
            if (go.name == label_queue_[0].image_name) {
              go.SetActive(true);
            } else {
              go.SetActive(false);
            }
          }
        }
      } else { // queue element 0 has expired -> remove from queue
        label_queue_.RemoveAt(0);
      }
      info_box_.SetActive(true);
    } else {
      info_box_.SetActive(false);
    }
  }	
  //ariconsole handler
  void OnMessage(int from, JToken data)
  {
    if (data["start"] != null)
    {
      int num_players = GameObject.Find("Players").GetComponent<PlayersScript>().GetNumberOfPlayers();
      if (num_players >= 3)
      {
        Debug.Log("received start");

        //StartWoods();
        StartIntro();

       // StartTutorial();
      }
    }
  }
  
  //action

  public void StartTutorial()
  {
    ChangePositions(
      new Vector3(-1.75f, 2.5f, 3),
      new Vector3(-0.25f, 3.5f, 3),
      new Vector3(-0.25f, 1.5f, 3),
      new Vector3(-2.25f, 3.5f, 3),
      new Vector3(-2.25f, 1.5f, 3),
      new Vector3(1.75f, 3.5f, 3),
      new Vector3(1.75f, 1.5f, 3),
      new Vector3(-4.0f, 3.5f, 3),
      new Vector3(-4.0f, 1.5f, 3)
    );
    ChangeLevel("tutorial");
   // ShowIntermission("A long time ago in a galaxy far, far away....\n\n....a spaceship crashed....");
    DisplayInfoBox("Welcome to Survival of the Zargs! Use the ACTION buttons on your device to interact.", 20, "Alien");
  }
  public void StartMiningStation()
  {
    GameObject.Find("MainCamera").GetComponent<Camera>().backgroundColor = new Color(0, 0, 0);
    ChangePositions(new Vector3(-0.0078125f, 4.0f, 0) );
    ChangeLevel("mining_station");
   // ShowIntermission("The crew of the space ship has found a cave. It seems to have been used as a mining station. As they enter with the intent to recover fuel the door closes behind them...");
  }
  public void StartJungle() {
    ChangePositions(new Vector3(0f, -3.725f, -0.125f));
    ChangeLevel("jungle");
    // ShowIntermission("...");
  }


  public void StartWoods(float x = 2.7f, float y = -0.212f, float z = 0f) {
    ChangePositions(new Vector3(x, y, z));
    ChangeLevel("woods");
    GameObject.Find("MainCamera").GetComponent<Camera>().backgroundColor = new Color(0, 0.0f,0.0f);
    //  GameObject.Find("MainCamera").GetComponent<Camera>().backgroundColor = new Color(0,92/255.0f,9 / 255.0f);

  }

  public void StartIntro()
  {
    State = GameState.INTRO;
    //GameObject.Find("MainCamera").SetActive(false);

    GameObject canvas = GameObject.Find("WaitingScreen").transform.FindChild("Text").gameObject;
    canvas.transform.FindChild("IntroText").gameObject.SetActive(true);
   // canvas.transform.FindChild("WaitingScreenText1").gameObject.SetActive(false);
   // canvas.transform.FindChild("WaitingScreenText2").gameObject.SetActive(false);
   // canvas.transform.FindChild("WaitingScreenText3").gameObject.SetActive(false);
  //  canvas.transform.FindChild("WaitingScreenText4").gameObject.SetActive(false);


    GameObject.Find("Game").transform.FindChild("IntroCamera").gameObject.SetActive(true);
    GameObject.Find("Game").transform.FindChild("IntroCamera").gameObject.GetComponent<IntroCameraScript>().Activate();
    GameObject.Find("Game").transform.FindChild("Intro").gameObject.SetActive(true);
  }
  public void EndIntro()
  {
    GameObject canvas = GameObject.Find("WaitingScreen").transform.FindChild("Text").gameObject;
    GameObject.Find("Game").transform.FindChild("MainCamera").gameObject.SetActive(true);
    GameObject.Find("Game").transform.FindChild("IntroCamera").gameObject.SetActive(false);
    GameObject.Find("Game").transform.FindChild("Intro").gameObject.SetActive(false);
    canvas.transform.FindChild("IntroText").gameObject.SetActive(false);
    StartTutorial();
  }

  public void ShowIntermission(string text)
  {
    GetCurrentLevel().gameObject.SetActive(false);
    GameObject game = GameObject.Find("Game");
    GameObject screen = GameObject.Find("Game").transform.FindChild("UI").FindChild("IntermissionScreen").gameObject;

    screen.SetActive(true);

    Vector3 v = GameObject.Find("MainCamera").transform.position;
    v.z = 0;
    Vector3 tp = screen.transform.position;
    tp.y = -120.0f;
    screen.transform.FindChild("IntermissionText").position = tp;

    screen.transform.FindChild("IntermissionText").GetComponent<Text>().text = text;
    State = GameState.INTERMISSION;
  }

  public void EndIntermission()
  {
    GetCurrentLevel().gameObject.SetActive(true);
    GameObject screen = GameObject.Find("Game").transform.FindChild("UI").FindChild("IntermissionScreen").gameObject;

    screen.SetActive(false);
    State = GameState.PLAY;
  }

  public void ChangePositions(params Vector3[] vs) {
    if (vs.Length == 1) {
      GameObject.Find("Players").GetComponent<PlayersScript>().MoveAllPlayers(vs[0]);
    } else {
      GameObject.Find("Players").GetComponent<PlayersScript>().MoveAllPlayers(vs);
    }
  }

  public void ChangeLevel(string lvl, float scale = 1.0f)
  {
    if (current_level_ != null) {
      current_level_.SetActive(false);
    }
    
    current_level_ = gameObject.transform.FindChild(lvl).gameObject;
    current_level_.SetActive(true);

    State = GameState.PLAY;
 
    gameObject.transform.FindChild("WaitingScreen").gameObject.SetActive(false);

    current_level_.GetComponent<LevelScript>().Init();
    current_level_.GetComponent<LevelScript>().FocusSomeone();
  }
  

  public void RefreshWaitingScreen(int number_of_players)
  {
    State = GameState.JOIN; // updates controller displays
    
    var text = (number_of_players < 3 ? "Waiting for players" : "Start");
    var text2 = number_of_players + " player" + (number_of_players == 1 ? "" : "s") + " connected";

    transform.Find("WaitingScreen/Text/WaitingScreenText1").gameObject.GetComponent<Text>().text = text;
    transform.Find("WaitingScreen/Text/WaitingScreenText2").gameObject.GetComponent<Text>().text = text2;
  }

  public LevelScript GetCurrentLevel()
  {
    if(current_level_ != null)
      return current_level_.GetComponent<LevelScript>();

     return null;    
  }

  public void DisplayInfoBox(string text, float seconds = 5, string image_name = "Info") {
    float end_time;
    if (label_queue_.Count > 0) {
      // display the new label until *seconds* seconds after the last label in the queue 
      end_time = label_queue_[label_queue_.Count - 1].time + seconds;
    } else {
      // display the new label until *now* + *seconds*
      end_time = Time.time + seconds;
    }
    label_queue_.Add(new InfoBoxMessage() { time = end_time, text = text, image_name = image_name });
  }

  public void ShowStatusMessage(string msg, string sound) {
    StartCoroutine(DoShowStatusMessage(msg, sound));
  }

  private IEnumerator DoShowStatusMessage(string msg, string sound) {
    GameObject template = transform.Find("UI/StatusMessage").gameObject;
    GameObject status = Instantiate<GameObject>(template, template.transform.parent);
    status.transform.Find("Message").GetComponent<Text>().text = msg;
    status.SetActive(true);
    PlaySound(sound);
    yield return new WaitForSeconds(5);
    Destroy(status);
  }

  public void PlaySound(string name)
  {
    GameObject obj = GameObject.Find("MainCamera");
    if (obj != null)
      GameObject.Find("MainCamera").GetComponent<AudioManager>().PlaySingle(name);
    else
      GameObject.Find("IntroCamera").GetComponent<AudioManager>().PlaySingle(name);
  }
}


