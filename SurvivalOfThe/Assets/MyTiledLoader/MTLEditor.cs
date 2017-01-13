using UnityEngine;
using System.Collections;

#if UNITY_EDITOR 
using UnityEditor;

[CustomEditor(typeof(MTLLoader))]
public class LoaderEditor : Editor
{
  public override void OnInspectorGUI()
  {
    DrawDefaultInspector();

    MTLLoader myScript = (MTLLoader)target;
    if (GUILayout.Button("Load"))
    {
      myScript.Load();
    }
    if (GUILayout.Button("Clear"))
    {
      myScript.Clear();
    }
  }
}
#endif