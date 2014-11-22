using System;
using System.Runtime.Remoting.Contexts;
using UnityEngine;
using System.Collections.Generic;

public class NGonMesh : MonoBehaviour {

  public event Action Changed;

  public List<Vector3> vertices = new List<Vector3>();
  public List<int[]> faces = new List<int[]>();

  public void ThrowChanged() {
    if (Changed != null) {
      Changed();
    }
  }

  public void CreateOneFace() {
    faces.Add(new int[vertices.Count]);

    for (int i = 0; i < vertices.Count; i++) {
      faces[0][i] = i;
    }

    ThrowChanged();
  }


  public int AddVertex(Vector3 vertex)
  {
      vertices.Add(vertex);
      return vertices.Count - 1;
  }

  public void CreateFace(int[] vertexIndicies) {
      faces.Add(vertexIndicies);
  }


  public void DeleteFace(int face) {
      faces.RemoveAt(face);
  }




  private Vector3 GetCenter(int face){
	Vector3 center = new Vector3(0,0,0);
	foreach(int vIndex in faces[face]){
		center += vertices[vIndex];
	}
  
	return center / faces[face].Length;
  }

  public void Move(int face, Vector3 direction) {
      foreach (int vIndex in faces[face])
      {
          vertices[vIndex] += direction;
      }

      ThrowChanged();
  }

  public void Scale(int face, float scale) {    
    Vector3 center = GetCenter(face);

    foreach (int vIndex in faces[face])
    {
        Vector3 offset = vertices[vIndex] - center;
        vertices[vIndex] = center + offset * scale;
    }

    ThrowChanged();
  }

  public void Rotate(int face, Quaternion rotation) {
      Vector3 center = GetCenter(face);

      foreach (int vIndex in faces[face])
      {
          Vector3 offset = vertices[vIndex] - center;
          offset = rotation * offset;
          vertices[vIndex] = center + offset;
      }

      ThrowChanged();
  }



  public int TriangleToNgon(int triangle) {
      for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++ )
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


  public void VertexMerge(int faceToDelete) {
      // add center of face as vertex to verticies and store the index of the new vertex
      int newVertexIndex = AddVertex(GetCenter(faceToDelete));

      // loop over all vertexIndicies that will be deleted
      foreach (int vertexIndexToDelete in faces[faceToDelete])
      {
          // check all vertexIndicies and replace if vertex will be deleted
          for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++)
          {
              for (int vertexIndex = 0; vertexIndex < faces[faceIndex].Length; vertexIndex++)
              {
                  if (faces[faceIndex][vertexIndex] == vertexIndexToDelete)
                  {
                      // replace vertexIndex with new vertexIndex
                      faces[faceIndex][vertexIndex] = newVertexIndex;
                  }
              }
          }
      }

      // delete face
      DeleteFace(faceToDelete);
  }
}
