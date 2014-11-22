using UnityEditor;
using UnityEngine;
using System.Collections;

[CustomEditor(typeof(NGonMesh))]
public class NGonMeshInspector : Editor {

  private Vector3 moveDir;
  private float scale;
  private Vector3 rotAngles;

  public override void OnInspectorGUI() {
    NGonMesh myTarget = (NGonMesh)target;

    DrawDefaultInspector();
    if (GUILayout.Button("Update Mesh")) {
      myTarget.ThrowChanged();
    }

    if (GUILayout.Button("Make Face")) {
      myTarget.CreateBaseCube();
    }

    moveDir = EditorGUILayout.Vector3Field("Direction", moveDir);
    if (GUILayout.Button("Move")) {
      myTarget.Move(0, moveDir);
    }

    scale = EditorGUILayout.FloatField("Amount", scale);
    if (GUILayout.Button("Scale")) {
      myTarget.Scale(0, scale);
    }

    rotAngles = EditorGUILayout.Vector3Field("Angles", rotAngles);
    if (GUILayout.Button("Rotate")) {
      myTarget.Rotate(0, Quaternion.Euler(rotAngles));
    }

  }
}
