using UnityEngine;

public class SliceBehaviour : MonoBehaviour {
     private Slicer slicer;

     private void Start() {
          MeshFilter f = GetComponent<MeshFilter>();
          slicer = new Slicer(f.mesh);
     }
}