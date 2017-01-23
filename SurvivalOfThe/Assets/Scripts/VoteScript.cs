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

  private char[] solution_ = { '1', '3', '0', '6', '3', '9', '2' };

  void Start()
  {
    vote_ = GameObject.Find("UI").transform.GetChild(0).gameObject;
    game_ = GameObject.Find("Game").GetComponent<GameScript>();
    question_ = GameObject.Find("UI").transform.GetChild(0).GetChild(0).GetComponent<Text>();
    code_text_ = GameObject.Find("UI").transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<Text>();

    AirConsole.instance.onMessage += OnMessage;
  }

  void Update()
  {
    
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
      if (data["vote"] != null)
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
        game_.StartExtendedTutorial();
      }
    }
  }
}
 