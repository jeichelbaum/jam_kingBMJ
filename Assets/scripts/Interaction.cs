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

  private InteractionState state = InteractionState.Idle;

  private int extrudeFaceIndex = 0;
  private Vector3 extrudeDir = Vector3.zero;
  private Vector2 extrudeStartMousePos;

  private Vector3 CameraTargetPos;
  private Vector3 dampVel;

  private float overFaceTimer = 0;
  private int lastFaceIndex = -1;
  private Color faceColor;

  enum InteractionState {
    Idle,
    Extrude,
    Scale,
    Rotate
  }

  void Awake() {
    triangulator = GetComponent<Triangulator>();
    ngon = GetComponent<NGonMesh>();
    CameraTargetPos = Camera.main.transform.position;
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

    switch (state)
    {
      case InteractionState.Idle:
        RaycastHit hit;
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool didInteract = false;
        if (Physics.Raycast(ray, out hit))
        {
          if (hit.collider.tag == "VertexCollider")
          {
            didInteract = DoHitVertex(hit);
          }
          else
          {
            didInteract = DoHitFace(hit);
            overFaceTimer += Time.deltaTime;
          }
        }

      if (!didInteract && Input.GetMouseButton(2)) {
          Rotate((Vector2)Input.mousePosition - lastMousePos);
        }

        
        break;
      case InteractionState.Extrude:
        if (Input.GetMouseButtonUp(0))
        {
          state = InteractionState.Idle;
          break;
        }

        var prevDist = Vector3.Distance(lastMousePos, extrudeStartMousePos);
        var dist = Vector3.Distance(Input.mousePosition, extrudeStartMousePos);

        ngon.Move(extrudeFaceIndex, extrudeDir * (dist - prevDist) * 0.1f);
        break;
    }

    CameraTargetPos += (CameraTargetPos - transform.position).normalized*Input.GetAxis("Mouse ScrollWheel") * 10;

    Camera.main.transform.position = Vector3.SmoothDamp(Camera.main.transform.position, CameraTargetPos, ref dampVel,
      0.3f);

    lastMousePos = Input.mousePosition;
  }

  void Rotate(Vector2 mouseDelta) {
    transform.RotateAround(transform.position, Vector3.up, -mouseDelta.x * 0.4f);
    transform.RotateAround(transform.position, Vector3.right, mouseDelta.y);
  }


  bool DoHitVertex(RaycastHit hit) {
    return false;
  }

  bool DoHitFace(RaycastHit hit) {
    int ngonFace = triangulator.MapTriIndex(hit.triangleIndex);
    if (Input.GetMouseButtonDown(0)) {
      if (lastFaceIndex == ngonFace) {
        ngon.FaceColors[ngonFace] = faceColor;
      }
      extrudeFaceIndex = ngon.FaceExtrude(ngonFace, out extrudeDir);
      state = InteractionState.Extrude;
      extrudeStartMousePos = Input.mousePosition;
      return true;
    } else if (Input.GetMouseButtonDown(1)) {
      ngon.VertexMerge(ngonFace);
      lastFaceIndex = -1;
    } else {
      if (ngonFace != lastFaceIndex)
      {
        if (lastFaceIndex != -1)
        {
          ngon.FaceColors[lastFaceIndex] = faceColor;
        }

        overFaceTimer = 0;
        faceColor = ngon.FaceColors[ngonFace];
        lastFaceIndex = ngonFace;
      }

      ngon.FaceColors[ngonFace] = Color.Lerp(faceColor, Color.white, (Mathf.Sin(overFaceTimer * 2) + 1) * 0.5f);
      ngon.ThrowChanged();
    }


    return false;
  }
}


