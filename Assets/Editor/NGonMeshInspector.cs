using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(NGonMesh))]
public class NGonMeshInspector : Editor {

  public override void OnInspectorGUI() {
    NGonMesh myTarget = (NGonMesh)target;

    DrawDefaultInspector();
    if (GUILayout.Button("Update Mesh")) {
      myTarget.ThrowChanged();
    }

    if (GUILayout.Button("Make Face")) {
      myTarget.CreateOneFace();
    }
  }
}
