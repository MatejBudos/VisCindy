using UnityEngine;
using System.Collections.Generic; // Required for List

public class SieraHandler : MonoBehaviour
{
    public static SieraHandler Instance { get; private set; } // Singleton instance

    public GameObject vertexPrefab;
    public GameObject vertexHolder;
    public GameObject edgeHolder;

    [Header("Line Properties")]
    public Material lineMaterial;
    public float lineWidth = 0.05f;
    public Color lineColor = Color.black;
    public float lineZDepth = -1.0f; // Hardcoded Z axis for lines, adjust as needed
    public int lineSortingOrder = -1; // Sorting order for LineRenderer

    private bool isInEdgeMode = false;
    private GameObject firstSelectedVertexForEdge = null;
    private List<Edge> activeEdges = new List<Edge>(); // List to track active edges

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple SieraHandler instances detected. Destroying this one.", this);
            Destroy(gameObject);
        }
    }

    void Start()
    {
        
    }

    void Update()
    {
        // Update positions of all active edges
        for (int i = activeEdges.Count - 1; i >= 0; i--)
        {
            Edge edge = activeEdges[i];
            bool removeEdge = false;

            if (edge.lineRenderer == null) removeEdge = true;
            else if (edge.isSelfLoop && edge.startNode == null) removeEdge = true;
            else if (!edge.isSelfLoop && (edge.startNode == null || edge.endNode == null)) removeEdge = true;

            if (removeEdge)
            {
                if (edge.lineRenderer != null) Destroy(edge.lineRenderer.gameObject);
                activeEdges.RemoveAt(i);
                Debug.Log("Removed stale edge due to missing components.");
                continue;
            }
            edge.UpdatePosition(lineZDepth);
        }
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
    }

    public void OnAddEdgeButtonPressed()
    {
        isInEdgeMode = !isInEdgeMode;
        if (isInEdgeMode)
        {
            firstSelectedVertexForEdge = null;
            Debug.Log("Add Edge mode: ACTIVATED.");
        }
        else
        {
            firstSelectedVertexForEdge = null;
            Debug.Log("Add Edge mode: DEACTIVATED.");
        }
    }

    public void SelectVertex(GameObject vertex)
    {
        if (!isInEdgeMode)
        {
            Debug.Log(vertex.name + " clicked (Edge mode OFF).");
            return;
        }

        if (firstSelectedVertexForEdge == null)
        {
            firstSelectedVertexForEdge = vertex;
            Debug.Log(vertex.name + " selected as first vertex for edge.");
        }
        else
        {
            DrawAndStoreLine(firstSelectedVertexForEdge, vertex); // Changed method name
            firstSelectedVertexForEdge = null;
            isInEdgeMode = false;
            Debug.Log("Add Edge mode: DEACTIVATED (edge created/attempted).");
        }
    }

    void DrawAndStoreLine(GameObject startNode, GameObject endNode) // Renamed from DrawLine
    {
        GameObject lineObj = new GameObject("Edge_" + startNode.name + "_to_" + endNode.name);
        Transform parentForEdge = (edgeHolder != null) ? edgeHolder.transform : this.transform;
        lineObj.transform.SetParent(parentForEdge, false);

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.material = lineMaterial;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.startColor = lineColor;
        lr.endColor = lineColor;
        lr.useWorldSpace = true;
        lr.sortingOrder = lineSortingOrder; // Set sorting order

        Edge newEdge = new Edge(lr, startNode, endNode);
        activeEdges.Add(newEdge);

        // Initial position update using the new Edge instance's method
        newEdge.UpdatePosition(lineZDepth); 

        if (newEdge.isSelfLoop)
        {
            Debug.Log("Creating self-loop for " + startNode.name);
        }
        else
        {
            Debug.Log("Creating edge between " + startNode.name + " and " + endNode.name);
        }
    }
    
    // Public helper method to draw/update self-loop graphics
    public void DrawSelfLoopGraphic(LineRenderer lr, GameObject node, float zDepth)
    {
        if (lr == null || node == null) return;

        lr.loop = true;
        int segments = 16;
        lr.positionCount = segments; // Ensure positionCount is correct for updates

        float nodeRadius = GetNodeVisualRadius(node);
        Vector3 selfLoopCenterOffset = node.transform.up * (nodeRadius * 0.8f);
        Vector3 selfLoopCenter = node.transform.position + selfLoopCenterOffset; // This is world space center
        float selfLoopVisualRadius = nodeRadius * 0.2f;

        for (int i = 0; i < segments; i++)
        {
            float angle = ((float)i / segments) * Mathf.PI * 2.0f;
            Vector3 pointOnCircleInPlane = new Vector3(Mathf.Sin(angle) * selfLoopVisualRadius, Mathf.Cos(angle) * selfLoopVisualRadius, 0);
            
            // The selfLoopCenter is already in world space. Add the planar circle points to it.
            Vector3 worldPoint = selfLoopCenter + pointOnCircleInPlane;
            lr.SetPosition(i, new Vector3(worldPoint.x, worldPoint.y, zDepth));
        }
    }

    private float GetNodeVisualRadius(GameObject node)
    {
        RectTransform rt = node.GetComponent<RectTransform>();
        if (rt != null)
        {
            return (rt.rect.width / 2.0f) * ((rt.lossyScale.x + rt.lossyScale.y) / 2.0f);
        }
        Renderer rend = node.GetComponent<Renderer>();
        if (rend != null)
        {
            return rend.bounds.extents.x;
        }
        return 0.5f;
    }
}