using UnityEngine;
using System.Collections;

public class NGonMeshTest : MonoBehaviour {

    NGonMesh ngon;

	// Use this for initialization
	void Start () {
        ngon = new NGonMesh();

	}


    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(new Vector3(0,0,0), 1);
    }
}
