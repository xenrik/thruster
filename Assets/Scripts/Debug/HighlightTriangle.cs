// This script draws a debug line around mesh triangles
// as you move the mouse over them.
using UnityEngine;
using System.Collections;

public class HighlightTriangle : MonoBehaviour {
    private Camera cam;

    private Vector3 lastPointA;
    private Vector3 lastPointB;
    private Vector3 lastPointC;

    void Start() {
        cam = GetComponent<Camera>();
    }

    private void OnDrawGizmos() {
        if (!isActiveAndEnabled) {
            return;
        }

        /*
        RaycastHit hit;
        //Ray ray = cam.ScreenPointToRay(Input.mousePosition);
		Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);
        if (!Physics.Raycast(ray, out hit))
            return;

        MeshCollider meshCollider = hit.collider as MeshCollider;
        if (meshCollider == null || meshCollider.sharedMesh == null) {
            //Debug.Log("No collider, or no mesh");
            return;
        }

        Mesh mesh = meshCollider.sharedMesh;
        Vector3[] vertices = mesh.vertices;
        int[] triangles = mesh.triangles;
        Vector3 localA = vertices[triangles[hit.triangleIndex * 3 + 0]];
        Vector3 localB = vertices[triangles[hit.triangleIndex * 3 + 1]];
        Vector3 localC = vertices[triangles[hit.triangleIndex * 3 + 2]];
        Transform hitTransform = hit.collider.transform;
        Vector3 worldA = hitTransform.TransformPoint(localA);
        Vector3 worldB = hitTransform.TransformPoint(localB);
        Vector3 worldC = hitTransform.TransformPoint(localC);

        Debug.DrawLine(worldA, worldB, Color.red);
        Debug.DrawLine(worldB, worldC, Color.red);
        Debug.DrawLine(worldC, worldA, Color.red);

        if (Event.current.modifiers == EventModifiers.Alt && (!lastPointA.Equals(worldA) || !lastPointB.Equals(worldB) || !lastPointC.Equals(worldC))) {
            Debug.Log($"World: [{worldA.ToString("F2")},{worldB.ToString("F2")},{worldC.ToString("F2")}] - " +
                $"Local: [{localA.ToString("F2")},{localB.ToString("F2")},{localC.ToString("F2")}]");

            lastPointA = worldA;
            lastPointB = worldB;
            lastPointC = worldC;
        }
        */
    }
}