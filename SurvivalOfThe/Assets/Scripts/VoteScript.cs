using UnityEngine;
using System.Collections;

using NDream.AirConsole;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;
using System.Collections.Generic;

public class VoteScript : MonoBehaviour
{
  private GameScript game_;
  private GameObject vote_;
  private Text question_;

  private Text code_text_;
  private char[] code_;
  private int selected_;

  private char[] solution_;
  
  private GameObject[] EngineNumbers = new GameObject[8];
  private List<GameObject> RemoveOnSuccess = new List<GameObject>();

  [Header("Tileset used to display numbers")]
  public Texture2D Tileset;

  private string vote_type_ = "";

  void Start()
  {
    vote_ = transform.Find("Vote").gameObject;
    game_ = GameObject.Find("Game").GetComponent<GameScript>();
    question_ = transform.Find("Vote/Question").GetComponent<Text>();
    code_text_ = transform.Find("Vote/Code/Text").GetComponent<Text>();
    
    Transform objects = GameObject.Find("Game").transform.Find("tutorial/LevelLayer3/Objects");
    foreach (Transform obj in objects) {
      if (obj.name.StartsWith("door")) {
        RemoveOnSuccess.Add(obj.gameObject);
      } else if (obj.name.StartsWith("engine_number_")) {
        int number = int.Parse(obj.name.Substring(14, 1));
        EngineNumbers[number - 1] = obj.gameObject;
      }
    }
    
    int index = 0;
    solution_ = new char[EngineNumbers.Length];
    foreach (GameObject engine_number in EngineNumbers)
    {
      int number = Random.Range(0, 10);
      Material mat = engine_number.GetComponentInChildren<MeshRenderer>().material;
      
      Texture2D tex = new Texture2D(mat.mainTexture.width, mat.mainTexture.height);
      for (int x = 0; x < tex.width; x++) {
        for (int y = 0; y < tex.height; y++) {
          tex.SetPixel(x, y, Tileset.GetPixel((1 + number) * 32 - x - 1, Tileset.height - (index % 2 == 0 ? 0 : 3) * 32 - y - 1));
        }
      }
      tex.filterMode = FilterMode.Point;
      tex.wrapMode = TextureWrapMode.Clamp;
      tex.Apply();

      mat.mainTexture = tex;
      solution_[index] = (char)('0' + number);

      index++;
    }

    AirConsole.instance.onMessage += OnMessage;
  }

  public void Init(string question, int number_of_players)
  {
    if (game_.State == GameState.VOTE || vote_.activeSelf)
      return;

    vote_.SetActive(true);
    game_.State = GameState.VOTE;
    game_.ControllerState = new GameScript.CtrlState() {
      state = "Vote",
      action1 = "Submit",
      action2 = "Cancel"
    };

    code_ = new char[number_of_players - 1];
    for (int i = 0; i < number_of_players - 1; i++) {
      code_[i] = '0';
    }
    selected_ = 0;

    question_.text = question;
    code_text_.text = CodeToString();
    vote_type_ = "startengine";
  }
  public void Init(string question,  char [] code,  string vote_type)
  {
    if (game_.State == GameState.VOTE || vote_.activeSelf)
      return;

    vote_.SetActive(true);
    game_.State = GameState.VOTE;
    game_.ControllerState = new GameScript.CtrlState() {
      state = "Vote",
      action1 = "Submit",
      action2 = "Cancel"
    };

    solution_ = code;
    selected_ = 0;

    Debug.Log(code.Length);
    code_ = new char[code.Length];
    for (int i = 0; i < code.Length; i++)
    {
      code_[i] = '0';
    }

    question_.text = question;
    code_text_.text = CodeToString();
    vote_type_ = vote_type;
  }

  private string CodeToString() {
    string result = "";
    for (int i = 0; i < code_.Length; i++) {
      if (selected_ == i) {
        result += "<color=red>" + code_[i] + "</color>";
      } else {
        result += code_[i];
      }
    }
    return result;
  }

  void BackToGame() {
    vote_type_ = "";
    vote_.SetActive(false);
    game_.State = GameState.PLAY;
  }

  // Airconsole handler
  void OnMessage(int from, JToken data)
  {
    if (gameObject.activeSelf && game_.State == GameState.VOTE && vote_type_ != "")
    {
      if (data["direction"] != null)
      {
        string dir = (string)data["direction"];
        if (dir == "R") {
          if (selected_ + 1 < code_.Length) {
            selected_++;
            code_text_.text = CodeToString();
          }
        } else if (dir == "L") {
          if (selected_ > 0) {
            selected_--;
            code_text_.text = CodeToString();
          }
        } else if (dir == "U") {
          if (code_[selected_] < '9') {
            code_[selected_]++;
            code_text_.text = CodeToString();
          }
        } else if (dir == "D") {
          if (code_[selected_] > '0') {
            code_[selected_]--;
            code_text_.text = CodeToString();
          }
        }
      }
      if (data["vote"] != null)
      {
        if ((int)data["vote"] == 1) {
          for (int i = 0; i < code_.Length ; i++) {
            if (code_[i] != solution_[i]) {
              if(vote_type_ == "startengine")
                game_.DisplayInfoBox("That code did not start the engine...");
              else
                game_.DisplayInfoBox("That code did not work...");
              GameObject.Find("Game").GetComponent<GameScript>().PlaySound("wrong");
              BackToGame();
              return;
            }
          }

          // everything below will only be executed if the code was correct
          if (vote_type_ == "startengine") {
            foreach (GameObject go in RemoveOnSuccess) {
              Destroy(go);
            }
            BackToGame();
            game_.ShowIntermission("We should escape the space ship\r\n maybe.. \r\n if we don't wanna die");
            GameObject.Find("Game").GetComponent<GameScript>().PlaySound("correct");
          } else if (vote_type_ == "open_med_station") {
            game_.GetCurrentLevel().RemoveObject("med_station_door");
            GameObject.Find("Game").GetComponent<GameScript>().PlaySound("correct");
            BackToGame();
          }
        } else if ((int)data["vote"] == 2) {
          BackToGame();
        }
      }
    }
  }
}
 