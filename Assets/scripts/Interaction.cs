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
	  if (Physics.Raycast(ray, out hit)) {
	    if (hit.collider.tag == "VertexCollider") {
	      DoHitVertex(hit);
	    } else {
	      DoHitFace(hit);
	    }
	  }
	}

  void DoHitVertex(RaycastHit hit) {
    
  }

  void DoHitFace(RaycastHit hit) {
    int ngonFace = triangulator.MapTriIndex(hit.triangleIndex);
    if (Input.GetMouseButtonDown(0))
    {
        ngon.FaceExtrude(ngonFace);
    }
    else if (Input.GetMouseButtonDown(1))
    {
        ngon.VertexMerge(ngonFace);
    }
  }
}


