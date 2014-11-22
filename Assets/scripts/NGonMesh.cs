using System;
using System.Runtime.Remoting.Contexts;
using UnityEngine;
using System.Collections.Generic;

public class NGonMesh : MonoBehaviour {

  public float extrudeOffset = 2f;

  public event Action Changed;

  public List<Vector3> vertices = new List<Vector3>();
  public List<List<int>> faces = new List<List<int>>();

  public void ThrowChanged() {
    if (Changed != null) {
      Changed();
    }
  }

  public void CreateOneFace() {
    faces.Add(new List<int>(vertices.Count));

    for (int i = 0; i < vertices.Count; i++) {
        faces[0].Add(i);
    }

    ThrowChanged();
  }


  public int AddVertex(Vector3 vertex)
  {
      vertices.Add(vertex);
      return vertices.Count - 1;
  }

  public void CreateFace(List<int> vertexIndicies) {
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
  
	return center / faces[face].Count;
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
      for (int v = faces[faceToDelete].Count - 1; v >= 0; v--)
      {
          int vertexIndexToDelete = faces[faceToDelete][v];

          // check all vertexIndicies and delete them
          for (int faceIndex = faces.Count-1; faceIndex >= 0 ; faceIndex--)
          {
              for (int vertexIndex = faces[faceIndex].Count-1; vertexIndex >= 0 ; vertexIndex--)
              {
                  if (faces[faceIndex][vertexIndex] == vertexIndexToDelete)
                  {
                      faces[faceIndex].RemoveAt(vertexIndex);
                      AddUniqueIndex(faces[faceIndex], newVertexIndex);
                  }
              }
          }
      }


      // delete face
      DeleteFace(faceToDelete);

      ThrowChanged();
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
      List<int> newVertexIndicies = new List<int>(faces[faceIndex].Count);
      for (int vertexIndex = 0; vertexIndex < faces[faceIndex].Count; vertexIndex++) { 
        Vector3 vertex = vertices[faces[faceIndex][vertexIndex]];
        newVertexIndicies.Add(AddVertex(vertex + extrudeDir));
      }

    int n = faces[faceIndex].Count;
      // build face for each edge
      for (int vertexIndex = 0; vertexIndex < n; vertexIndex++) {
          CreateFace(new List<int>{
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
      List<int> containedFaces = new List<int>();
      List<int> baseVerticies = new List<int>();

      // get containing faces
      //Debug.Log("fIndex " + faceIndex + " faces: " + faces.Count);
      foreach (int vertexIndex in faces[faceIndex])
      {
          AddContainingFaces(containedFaces, vertexIndex);
      }
      Debug.Log("#connected faces: " + containedFaces.Count);

      // remove verticies of faceToDelete and create face from base verticies
      /*foreach (int vertexIndex in faces[faceIndex])
      {
          baseVerticies.Remove(vertexIndex);
      }
      CreateFace(baseVerticies.ToArray());


      // delete all faces that are attached to faceToDelete
      foreach (int faceToDelete in containedFaces)
      {
          foreach (int vertexIndex in faces[faceToDelete])
          {
              AddUniqueIndex(baseVerticies, vertexIndex);
          }

          DeleteFace(faceToDelete);
      }*/


      ThrowChanged();
  }

  private void AddUniqueIndex(List<int> list, int index) {
      if (!list.Contains(index))
      {
          list.Add(index);
      }
  }

  private void AddContainingFaces(List<int> containedFaces, int vertexIndex)
  {
      for (int faceIndex = 0; faceIndex < faces.Count; faceIndex++) {
          foreach (int vIndex in faces[faceIndex]) {
              if (vertexIndex == vIndex) {
                  AddUniqueIndex(containedFaces, faceIndex);   
              }
          }
      }
  }
}
