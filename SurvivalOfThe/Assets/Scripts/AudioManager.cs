using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

  Dictionary<string, AudioClip> clips_;

  // Use this for initialization
  void Start ()
  {
    clips_ = new Dictionary<string, AudioClip>();
  }
	
	// Update is called once per frame
	void Update ()
  {
		
	}

  private AudioClip GetAudioClip(string name)
  {
    AudioClip ac;
    if(clips_.ContainsKey(name) )
    {
      ac = clips_[name];
    }
    else
    {
      AudioClip clip1 = (AudioClip)Resources.Load("Sound/" + name);
      clips_.Add(name, clip1);
      ac = clip1;
    }
    return ac;
  }

  public void PlaySingle(string sound)
  {
    AudioClip ac = GetAudioClip(sound);

    AudioSource src = gameObject.GetComponent<AudioSource>();
   
    src.clip = ac;
    
    src.Play();
  }
}
