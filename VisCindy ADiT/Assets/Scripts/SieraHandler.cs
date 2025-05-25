using UnityEngine;
using System.Collections.Generic; // Required for List

public class SieraHandler : MonoBehaviour
{
    public GameObject vertexPrefab;
    public GameObject vertexHolder; // Parent for instantiated vertices
    public GameObject edgeHolder;   // Optional: Assign an empty GameObject (child of Canvas, sibling before VertexHolder) to keep edges organized

    [Header("Line Properties")]
    public Material lineMaterial;   // Assign a Material for the lines (e.g., Sprites/Default or Unlit/Color)
    public float lineWidth = 0.05f;
    public Color lineColor = Color.black;

    private bool isInEdgeMode = false;
    private GameObject firstSelectedVertexForEdge = null;

    void Start()
    {
        
    }

    public void AddNewNodeObject()
    {
        if (vertexPrefab == null || vertexHolder == null)
            return;

        GameObject vertex = Instantiate(vertexPrefab, vertexHolder.transform, false);
        if (!vertex.activeSelf)
        {
            vertex.SetActive(true);
        }
        // Ensure the new vertex has the VertexClickHandler and its sieraHandler field is assigned (usually done on the prefab)
    }

    // Called by the "Add Edge" UI Button
    public void OnAddEdgeButtonPressed()
    {
        isInEdgeMode = !isInEdgeMode; // Toggle edge mode
        if (isInEdgeMode)
        {
            firstSelectedVertexForEdge = null; // Clear previous selection when entering mode
            Debug.Log("Add Edge mode: ACTIVATED. Click first vertex, then second (or same again for self-loop).");
            // You could add UI feedback here (e.g., change button color)
        }
        else
        {
            firstSelectedVertexForEdge = null; // Clear selection when exiting mode manually
            Debug.Log("Add Edge mode: DEACTIVATED.");
            // Revert UI feedback if any
        }
    }

    // Called by VertexClickHandler when a vertex is clicked
    public void SelectVertex(GameObject vertex)
    {
        if (!isInEdgeMode)
        {
            Debug.Log(vertex.name + " clicked (Edge mode OFF). Handle parameter editing or other actions here.");
            // This is where you'd handle opening the node's submenu if not in edge mode
            return;
        }

        // --- In Edge Mode ---
        if (firstSelectedVertexForEdge == null)
        {
            firstSelectedVertexForEdge = vertex;
            Debug.Log(vertex.name + " selected as first vertex for edge.");
            // Optional: Add visual feedback to 'firstSelectedVertexForEdge'
        }
        else // A first vertex is already selected
        {
            if (firstSelectedVertexForEdge == vertex) // Clicked the same vertex again (self-loop)
            {
                Debug.Log("Creating self-loop for " + vertex.name);
                DrawLine(firstSelectedVertexForEdge, vertex);
            }
            else // Clicked a different, second vertex
            {
                Debug.Log("Creating edge between " + firstSelectedVertexForEdge.name + " and " + vertex.name);
                DrawLine(firstSelectedVertexForEdge, vertex);
            }
            // Reset for next edge or exit mode
            firstSelectedVertexForEdge = null;
            isInEdgeMode = false; // Automatically exit edge mode after an edge is drawn/attempted
            Debug.Log("Add Edge mode: DEACTIVATED (edge created/attempted).");
            // Optional: Revert visual feedback from 'firstSelectedVertexForEdge' and button
        }
    }

    void DrawLine(GameObject startNode, GameObject endNode)
    {
        GameObject lineObj = new GameObject("Edge_" + startNode.name + "_to_" + endNode.name);
        
        // Parent the line object
        Transform parentForEdge = (edgeHolder != null) ? edgeHolder.transform : this.transform;
        lineObj.transform.SetParent(parentForEdge, false);

        // If EdgeHolder is used and is a sibling BEFORE VertexHolder in the canvas hierarchy, lines will render behind vertices.
        // If parented to SieraMain directly, ensure SieraMain is on the canvas or rendering order is managed.

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.material = lineMaterial;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.startColor = lineColor;
        lr.endColor = lineColor;

        // LineRenderer uses world space by default. Node positions should be their world positions.
        lr.useWorldSpace = true; 

        if (startNode == endNode) // Self-loop: Draw a small circle on/near the node
        {
            lr.loop = true; // Connects the last point to the first for a closed shape
            int segments = 16; // Number of segments for the circular loop
            lr.positionCount = segments;

            float nodeRadius = GetNodeVisualRadius(startNode);
            // Place the self-loop slightly offset from the node's center, e.g., on its periphery
            // Using node's 'up' assuming it's oriented consistently on the canvas
            Vector3 selfLoopCenterOffset = startNode.transform.up * (nodeRadius * 0.8f); // Adjust multiplier as needed
            Vector3 selfLoopCenter = startNode.transform.position + selfLoopCenterOffset;
            float selfLoopVisualRadius = nodeRadius * 0.2f; // Radius of the small circle itself

            for (int i = 0; i < segments; i++)
            {
                float angle = ((float)i / segments) * Mathf.PI * 2.0f;
                // Create points for a circle in the XY plane relative to the selfLoopCenter
                // This assumes the canvas is largely aligned with the world XY plane or that the camera views it orthographically.
                // For a UI canvas, rotations of the LineRenderer object itself might be needed if nodes are rotated.
                Vector3 pointOnCircle = new Vector3(Mathf.Sin(angle) * selfLoopVisualRadius, Mathf.Cos(angle) * selfLoopVisualRadius, 0);
                
                // If your canvas or nodes have significant rotation, you might need to transform pointOnCircle
                // by the node's rotation or use the camera's plane.
                // For simple cases, adding world offset like this works.
                lr.SetPosition(i, selfLoopCenter + pointOnCircle);
            }
        }
        else // Regular edge between two different nodes
        {
            lr.positionCount = 2;
            lr.SetPosition(0, startNode.transform.position);
            lr.SetPosition(1, endNode.transform.position);
        }
    }

    private float GetNodeVisualRadius(GameObject node)
    {
        RectTransform rt = node.GetComponent<RectTransform>();
        if (rt != null)
        {
            // Assuming square/circular button, use width. Average local scale.
            return (rt.rect.width / 2.0f) * ((rt.lossyScale.x + rt.lossyScale.y) / 2.0f) ;
        }
        // Fallback for non-RectTransform or if more complex bounds are needed
        Renderer rend = node.GetComponent<Renderer>();
        if (rend != null)
        {
            return rend.bounds.extents.x; // Approximation
        }
        return 0.5f; // Default if no other info
    }
}