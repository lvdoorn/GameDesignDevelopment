using UnityEngine;
using System.Collections;

using NDream.AirConsole;
using Newtonsoft.Json.Linq;
using UnityEngine.UI;

public class VoteScript : MonoBehaviour
{
  private GameScript game_;
  private GameObject vote_;
  private Text question_;

  private Text code_text_;
  private char[] code_;
  private int selected_;

  private char[] solution_;
  
  [Header("Engine Code Number Objects")]
  public GameObject[] EngineNumbers;
  
  [Header("GameObjects to remove on success")]
  public GameObject[] RemoveOnSuccess;

  [Header("Tileset used to display numbers")]
  public Texture2D Tileset;

  void Start()
  {
    vote_ = GameObject.Find("UI").transform.GetChild(0).gameObject;
    game_ = GameObject.Find("Game").GetComponent<GameScript>();
    question_ = GameObject.Find("UI").transform.GetChild(0).GetChild(0).GetComponent<Text>();
    code_text_ = GameObject.Find("UI").transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<Text>();
    
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

  public void Init(string question, int number_of_players) {
    if (game_.State == GameState.VOTE || vote_.activeSelf)
      return;

    vote_.SetActive(true);
    game_.State = GameState.VOTE;

    code_ = new char[number_of_players - 1];
    for (int i = 0; i < number_of_players - 1; i++) {
      code_[i] = '0';
    }
    selected_ = 0;

    question_.text = question;
    code_text_.text = CodeToString();
    AirConsole.instance.Broadcast("BeginVote");
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

  IEnumerator FadeBackToGame() {
    yield return new WaitForSeconds(1);
    game_.State = GameState.PLAY;
    vote_.SetActive(false);
    AirConsole.instance.Broadcast("EndVote");
  }

  // Airconsole handler
  void OnMessage(int from, JToken data)
  {
    if (game_.State == GameState.VOTE)
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
      if (data["vote"] != null && (int)data["vote"] == 1)
      {
        for (int i = 0; i < code_.Length; i++) {
          if (code_[i] != solution_[i]) {
            game_.DisplayInfoBox("That code did not start the engine...", 2);
            StartCoroutine(FadeBackToGame());
            return;
          }
        }
        game_.DisplayInfoBox("The engine started !", 2);
        StartCoroutine(FadeBackToGame());
        foreach (GameObject go in RemoveOnSuccess) {
          Destroy(go);
        }
        game_.ShowIntermission("We should escape the space ship\r\n maybe.. \r\n if we don't wanna die");
      }
    }
  }
}
 