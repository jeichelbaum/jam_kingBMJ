using System;
using UnityEngine;

public class NGonMesh {
  public event Action Changed;

  public Vector3[] vertices;
  public int[][] faces;
}
