using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(NGonMesh), typeof(MeshFilter), typeof(MeshRenderer))]
[RequireComponent(typeof(MeshCollider))]
[ExecuteInEditMode]
public class Triangulator : MonoBehaviour
{
  public class Triangle {
    public Vector3[] Vertices { get; private set; }
    public Vector3 Centroid { get; private set; }

    public Triangle(Vector3 v0, Vector3 v1, Vector3 v2) {
      Vertices = new Vector3[3];
      Vertices[0] = v0;
      Vertices[1] = v1;
      Vertices[2] = v2;
      Centroid = (v0 + v1 + v2) / 3;
    }

    public Vector3 Normal()
    {
      Vector3 cross = Vector3.Cross(Vertices[0] - Vertices[1], Vertices[0] - Vertices[2]);
      cross.Normalize();
      return cross;
    }

    public bool IsPointInTriangle(Vector3 _point) {
      if (IsPointPartOf(_point))
        return false;

      // Compute vectors        
      Vector3 dir0 = Vertices[2] - Vertices[0];
      Vector3 dir1 = Vertices[1] - Vertices[0];
      Vector3 dir2 = _point - Vertices[0];

      // Compute dot products
      float dot00 = Vector3.Dot(dir0, dir0);
      float dot01 = Vector3.Dot(dir0, dir1);
      float dot02 = Vector3.Dot(dir0, dir2);
      float dot11 = Vector3.Dot(dir1, dir1);
      float dot12 = Vector3.Dot(dir1, dir2);

      // Compute barycentric coordinates
      float invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
      float u = (dot11 * dot02 - dot01 * dot12) * invDenom;
      float v = (dot00 * dot12 - dot01 * dot02) * invDenom;

      // Check if point is in triangle
      return (u >= 0) && (v >= 0) && (u + v < 1);
    }

    public bool IsPointPartOf(Vector3 _point) {
      return _point == Vertices[0] || _point == Vertices[1] || _point == Vertices[2];
    }

    public bool IsWoundClockwise() {
      Vector3 cross = Vector3.Cross(Vertices[0] - Vertices[1], Vertices[0] - Vertices[2]);
      return Vector3.Dot(cross, Vector3.up) > 0;
    }

    public override string ToString() {
      return Vertices[0].ToString() + Vertices[1].ToString() + Vertices[2].ToString();
    }

    public float GetArea() {
      return Vector3.Cross(Vertices[0] - Vertices[1], Vertices[0] - Vertices[2]).magnitude/2;
    }
  }

  public NGonMesh NGon;
  public Mesh mesh;
  public MeshCollider meshColl;

  private List<int> triIndexMap = new List<int>(); 

  public void Awake() {
    GetComponent<MeshFilter>().sharedMesh = new Mesh();
    mesh = GetComponent<MeshFilter>().sharedMesh;
    NGon = GetComponent<NGonMesh>();
    meshColl = GetComponent<MeshCollider>();
    meshColl.sharedMesh = mesh;
  }

  public void OnEnable() {
    NGon.Changed += NGon_Changed;
  }

  public void OnDisable() {
    NGon.Changed -= NGon_Changed;
  }

  public int MapTriIndex(int index) {
    return triIndexMap[index];
  }

  void NGon_Changed() {
    var tris = new List<Triangle>();
    triIndexMap.Clear();
    for(int i = 0; i < NGon.faces.Count; i++) {
      var face = NGon.faces[i];
      var points = new List<Vector3>();
      foreach (var index in face) {
        points.Add(NGon.vertices[index]);
      }
      var newTris = Triangulate(points);
      for (int tri = 0; tri < newTris.Count; tri++) {
        triIndexMap.Add(i);
        tris.Add(newTris[tri]);
      }
    }

    var indices = new List<int>();
    var vertices = new List<Vector3>();
    var normals =  new List<Vector3>();
    var uvs = new List<Vector2>();
    var colors = new List<Color>();

    int triIndex = 0;
    foreach (var triangle in tris) {
      for (int i = 0; i < 3; i++) {
        indices.Add(indices.Count);
        vertices.Add(triangle.Vertices[i]);
        normals.Add(triangle.Normal());
        uvs.Add(Vector2.zero);
        colors.Add(NGon.FaceColors[MapTriIndex(triIndex)]);
      }
      triIndex++;
    }

    mesh.Clear();

    mesh.vertices = vertices.ToArray();
    mesh.triangles = indices.ToArray();
    mesh.colors = colors.ToArray();

    mesh.uv = uvs.ToArray();
    mesh.normals = normals.ToArray();

    meshColl.sharedMesh = mesh;
    meshColl.enabled = false;
    meshColl.enabled = true;
  }

  private float GetArea(List<Vector3> _points) {
    float area = 0;
    float an, ax, ay, az; // abs value of normal and its coords
    int coord; // coord to ignore: 1=x, 2=y, 3=z
    int i, j, k; // loop indices
    int n = _points.Count;

    if (_points.Count < 3) return 0; // a degenerate polygon

    Vector3 N = Vector3.Cross(_points[0] - _points[1], _points[0] - _points[2]);

    // select largest abs coordinate to ignore for projection
    ax = (N.x > 0 ? N.x : -N.x); // abs x-coord
    ay = (N.y > 0 ? N.y : -N.y); // abs y-coord
    az = (N.z > 0 ? N.z : -N.z); // abs z-coord

    coord = 3; // ignore z-coord
    if (ax > ay)
    {
      if (ax > az) coord = 1; // ignore x-coord
    }
    else if (ay > az) coord = 2; // ignore y-coord

    // compute area of the 2D projection
    switch (coord)
    {
      case 1:
        for (i = 1, j = 2, k = 0; i < n; i++, j++, k++)
          area += (_points[i].y*(_points[j].z - _points[k].z));
        break;
      case 2:
        for (i = 1, j = 2, k = 0; i < n; i++, j++, k++)
          area += (_points[i].z*(_points[j].x - _points[k].x));
        break;
      case 3:
        for (i = 1, j = 2, k = 0; i < n; i++, j++, k++)
          area += (_points[i].x*(_points[j].y - _points[k].y));
        break;
    }
    switch (coord)
    {
        // wrap-around term
      case 1:
        area += (_points[n].y*(_points[1].z - _points[n - 1].z));
        break;
      case 2:
        area += (_points[n].z*(_points[1].x - _points[n - 1].x));
        break;
      case 3:
        area += (_points[n].x*(_points[1].y - _points[n - 1].y));
        break;
    }

    // scale to get area before projection
    an = Mathf.Sqrt(ax*ax + ay*ay + az*az); // length of normal vector
    switch (coord)
    {
      case 1:
        area *= (an/(2*N.x));
        break;
      case 2:
        area *= (an/(2*N.y));
        break;
      case 3:
        area *= (an/(2*N.z));
        break;
    }
    return area;
  }

  private List<Triangle> Triangulate(List<Vector3> _points) {
    var points = new LinkedList<Vector3>();
    foreach (var point in _points) {
      points.AddLast(point);
    }

    var triangles = new List<Triangle>();
    LinkedListNode<Vector3> listNode = points.First;
    while (points.Count > 3) {
      bool isEar = true;

      Vector3 prev = (listNode.Previous ?? points.Last).Value;
      Vector3 curr = listNode.Value;
      Vector3 next = (listNode.Next ?? points.First).Value;

      if (IsConvex(prev, curr, next)) {
        LinkedListNode<Vector3> checker = points.First;
        while (checker.Next != null) {
          var current = new Triangle((listNode.Previous ?? points.Last).Value, listNode.Value, (listNode.Next ?? points.First).Value);
          if (current.IsPointInTriangle(checker.Value)) {
            isEar = false;
            break;
          }
          checker = checker.Next ?? points.First;
        }
      } else {
        isEar = false;
      }

      if (isEar) {
        triangles.Add(new Triangle((listNode.Previous ?? points.Last).Value, listNode.Value, (listNode.Next ?? points.First).Value));
        points.Remove(listNode);
        listNode = points.First;
      } else
        listNode = listNode.Next;
    }

    triangles.Add(new Triangle(points.First.Value, points.First.Next.Value, points.Last.Value));

    return triangles;
  }

  private bool IsConvex(Vector3 _prev, Vector3 _curr, Vector3 _next) {
    Vector3 diff = _next - _prev;
    Vector3 cross = Vector3.Cross(_prev - _curr, _next - _curr);
    Vector3 perp = Vector3.Cross(cross, diff);
    float d = Vector3.Dot(_curr - _prev, perp);
    return d > 0;
  }
}
