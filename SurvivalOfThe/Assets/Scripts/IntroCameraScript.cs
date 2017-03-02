using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class IntroCameraScript : MonoBehaviour
{

  private bool reached_ = false;
  private List<string> lines_ = new List<string>();
  private float last_;
  private int line_count=0;
  private GameObject canvas_;
  private bool active_ = false;

	// Use this for initialization
	void Start ()
  {
    lines_.Insert(0, "Waking crew...");
    lines_.Insert(0, "Engine offline.");
    lines_.Insert(0, "Initialize emergency landing ...");
    lines_.Insert(0, "Collision warning. Planet ahead.");
    lines_.Insert(0, "Navigation malfunction!");
    lines_.Insert(0,"Warning!");


    canvas_ = GameObject.Find("WaitingScreen").transform.FindChild("Canvas").gameObject;
  }
  public void Activate()
  {
    active_ = true;
  }
	
	// Update is called once per frame
	void Update ()
  {
    if (active_)
    {
      if (!reached_)
      {
        if (Fade())
        {
          if (transform.position.z > -12.0f)
          {
            transform.position -= new Vector3(0, 0, 0.005f);
          }
          else
          {
            GameObject canvas = GameObject.Find("WaitingScreen").transform.FindChild("Canvas").gameObject;
            canvas.transform.FindChild("IntroText").gameObject.SetActive(true);
            reached_ = true;
            last_ = Time.time;
            NextLine();
          }
        }
      }
      else
      {
        float delta = Time.time - last_;

        if (delta > 3.0f)
        {
          last_ = Time.time;
          NextLine();
        }

      }
    }
	}

  private bool Fade()
  {
    Color w1 = canvas_.transform.FindChild("WaitingScreenText1").gameObject.GetComponent<Text>().color;

    if (w1.a <= 0.0f)
      return true;

    Color w2 = canvas_.transform.FindChild("WaitingScreenText2").gameObject.GetComponent<Text>().color;
    Color w3 = canvas_.transform.FindChild("WaitingScreenText3").gameObject.GetComponent<Text>().color;
    Color w4 = canvas_.transform.FindChild("WaitingScreenText4").gameObject.GetComponent<Text>().color;

    w1.a -= 0.01f;
    canvas_.transform.FindChild("WaitingScreenText1").gameObject.GetComponent<Text>().color = w1;

    w2.a -= 0.01f;
    canvas_.transform.FindChild("WaitingScreenText2").gameObject.GetComponent<Text>().color = w2;

    w3.a -= 0.01f;
    canvas_.transform.FindChild("WaitingScreenText3").gameObject.GetComponent<Text>().color = w3;

    w4.a -= 0.01f;
    canvas_.transform.FindChild("WaitingScreenText4").gameObject.GetComponent<Text>().color = w4;
   

    return false;
  }

  private void NextLine()
  {
    if (lines_.Count > 0)
    {
      GameObject canvas = GameObject.Find("WaitingScreen").transform.FindChild("Canvas").gameObject;
      canvas.transform.FindChild("IntroText").gameObject.GetComponent<Text>().text += lines_[0]+"\r\n";
      lines_.RemoveAt(0);
      line_count++;
    }
    else
    {
      GameObject.Find("Game").GetComponent<GameScript>().EndIntro();
    }
  }
}
