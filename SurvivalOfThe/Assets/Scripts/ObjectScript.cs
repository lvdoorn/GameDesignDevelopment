using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectScript : MonoBehaviour
{
  public string action = "";
  public int switch_layer = -1;
  public string trigger_vote = "";
  public string trigger_text = "";
  public string turn_off = "";
  public string trigger_audio = "";

  public List<GameObject> tmp_objects = new List<GameObject>();

  public bool was_outside = false;
  public List<int> inside = new List<int>();


  void Start()
  {
  }

  void Update()
  {
  }
  public void PlayerWasOutside(int id)
  {
    inside.Remove(id);
  }
  public void PlayerWasInside(int id)
  {
    inside.Add(id);
  }
  public bool wasOutside(int id)
  {
    return !inside.Contains(id);
  }
  public bool someoneInside()
  {
    Debug.Log("inside" +inside.Count);
    return ( (inside.Count-1) > 0);
  }
}
