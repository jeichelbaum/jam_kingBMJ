using System;
using System.Runtime.Remoting.Contexts;
using UnityEngine;
using System.Collections.Generic;

public class NGonMesh : MonoBehaviour {

  public float extrudeOffset = 2f;

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


  public void VertexDemerge(int vertexIndex){
    
  }

  public void FaceExtrude(int faceIndex)
  {
      // calculate extrude dir
      Vector3 planeVec1 = vertices[faces[faceIndex][1]] - vertices[faces[faceIndex][0]];
      Vector3 planeVec2 = vertices[faces[faceIndex][2]] - vertices[faces[faceIndex][0]];
      Vector3 extrudeDir = Vector3.Cross(planeVec1, planeVec2).normalized * extrudeOffset;

      // create extruded verticies
      int[] newVertexIndicies = new int[faces[faceIndex].Length];
      for (int vertexIndex = 0; vertexIndex < faces[faceIndex].Length; vertexIndex++) { 
        Vector3 vertex = vertices[faces[faceIndex][vertexIndex]];
        newVertexIndicies[vertexIndex] = AddVertex(vertex + extrudeDir);
      }

    int n = faces[faceIndex].Length;
      // build face for each edge
      for (int vertexIndex = 0; vertexIndex < n; vertexIndex++) {

          CreateFace(new int[]{
              faces[faceIndex][vertexIndex],
              faces[faceIndex][(vertexIndex + 1)%n],
              newVertexIndicies[(vertexIndex + 1)%n],
              newVertexIndicies[vertexIndex]
          });  
      }

      CreateFace(newVertexIndicies);
      DeleteFace(faceIndex);

    ThrowChanged();
  }

  public void FaceDetrude(int faceIndex)
  { 
    
  }
}
