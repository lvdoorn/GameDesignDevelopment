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
  public string item = "";

  public List<GameObject> tmp_objects = new List<GameObject>();

  public bool was_outside = false;
  public List<int> inside = new List<int>();

  public List<float> activations = new List<float>();
  public List<int> activation_ids = new List<int>();


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
  public bool IsMultipleTriggered(int number, int player_id)
  {
    float now = Time.time;

    float delta = 1.0f;

    int count = 0;

    for( int x=0; x < activations.Count;x++ )
    {
      if ( ( (now - activations[x]) < delta )  && (activation_ids[x] != player_id))
        count++;
    }

    for (int x = 0; x < activations.Count; x++)
    {
     if( ( (now - activations[x]) > delta )  || (activation_ids[x] == player_id))
     {
        activations.RemoveAt(x);
        activation_ids.RemoveAt(x);
      }
    }

    if (count >= number-1)
      return true;

    activations.Add(now);
    activation_ids.Add(player_id);
    return false;
  }
  private bool PlayerTriggered(int player_id)
  {
    for (int x = 0; x < activations.Count; x++)
    {
      if (activation_ids[x] == player_id)
        return true;
    }

    return false;
  }
}
