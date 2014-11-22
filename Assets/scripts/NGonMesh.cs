using System;
using System.Runtime.Remoting.Contexts;
using UnityEngine;

public class NGonMesh : MonoBehaviour {

  public event Action Changed;

  public Vector3[] vertices;
  public int[][] faces;

  public void ThrowChanged() {
    if (Changed != null) {
      Changed();
    }
  }

  public void CreateOneFace() {
    faces = new int[1][];
    faces[0] = new int[vertices.Length];
    for (int i = 0; i < vertices.Length; i++) {
      faces[0][i] = i;
    }
    ThrowChanged();
  }

  private Vector3 GetCenter(int face){
	Vector3 center = new Vector3(0,0,0);
	foreach(int vIndex in faces[face]){
		center += vertices[vIndex];
	}
  
	return center / faces[face].Length;
  }

  public void Scale(int face, float scale) {    
    Vector3 center = GetCenter(face);

    foreach (int vIndex in faces[face])
    {
        Vector3 offset = vertices[vIndex] - center;
        vertices[vIndex] = center + offset * scale;
    }

    if (Changed != null) {
        Changed();
    }
  }

  public void Rotate(int face, Quaternion rotation) {
      Vector3 center = GetCenter(face);

      foreach (int vIndex in faces[face])
      {
          Vector3 offset = vertices[vIndex] - center;
          vertices[vIndex] = center + offset;
      }

      if (Changed != null)
      {
          Changed();
      }
  }

  public int TriangleToNgon(int triangle) {
      for (int faceIndex = 0; faceIndex < faces.Length; faceIndex++ )
      {
          foreach (int triIndex in faces[faceIndex])
          {
              if (triangle == triIndex)
              {
                  return faceIndex;
              }
          }
      }
      
      return -1;
  }
}
