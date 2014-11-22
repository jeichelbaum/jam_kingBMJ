using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Triangulator))]
public class Interaction : MonoBehaviour
{
  private Triangulator triangulator;
  private NGonMesh ngon;

  private List<GameObject> vertexColls = new List<GameObject>();

  private Vector2 lastMousePos;

  void Awake() {
    triangulator = GetComponent<Triangulator>();
    ngon = GetComponent<NGonMesh>();
  }

  void OnEnable() {
    //ngon.Changed += ngon_Changed;
  }

  void OnDisable() {
    //ngon.Changed -= ngon_Changed;
  }

  private void ngon_Changed()
  {
    for (int i = 0; i < vertexColls.Count; i++)
    {
      Destroy(vertexColls[i]);
    }

    vertexColls.Clear();

    for (int i = 0; i < ngon.vertices.Count; i++) {
      var go = (GameObject) GameObject.CreatePrimitive(PrimitiveType.Sphere);
      go.transform.localScale = Vector3.one*0.2f;
      go.AddComponent<VertexTag>().VertexIndex = i;
      go.transform.parent = transform;
      go.transform.position = ngon.vertices[i];
      go.tag = "VertexCollider";
      vertexColls.Add(go);
    }
  }

  void Update () {
    RaycastHit hit;
	  var ray = Camera.main.ScreenPointToRay(Input.mousePosition);

    bool didInteract = false;

	  if (Physics.Raycast(ray, out hit)) {
	    if (hit.collider.tag == "VertexCollider") {
        didInteract = DoHitVertex(hit);
	    } else {
        didInteract = DoHitFace(hit);
	    }
	  }

    if (!didInteract && Input.GetMouseButton(2)) {
      Rotate((Vector2)Input.mousePosition - lastMousePos);
    }

    lastMousePos = Input.mousePosition;
  }

  void Rotate(Vector2 mouseDelta) {
    transform.RotateAround(transform.position, Vector3.up, -mouseDelta.x * 0.4f);
    transform.RotateAround(transform.position, Vector3.right, mouseDelta.y);
  }


  bool DoHitVertex(RaycastHit hit)
  {
    return false;
  }

  bool DoHitFace(RaycastHit hit) {
    int ngonFace = triangulator.MapTriIndex(hit.triangleIndex);
    if (Input.GetMouseButtonDown(0))
    {
        ngon.FaceExtrude(ngonFace);
      return true;
    }
    else if (Input.GetMouseButtonDown(1))
    {
        ngon.VertexMerge(ngonFace);
    }

    return false;
  }
}


