using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using System; // Required for List

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

    [Header("Edge UI Configuration")]
    [Tooltip("Prefab for the UI element (button/submenu) to show above edges.")]
    public GameObject edgeUIPrefab; // Assign your inactive UI prefab here
    [Tooltip("Parent transform for instantiated edge UI elements. Should be under your main Canvas.")]
    public Transform edgeUIHolderTransform; // Assign an empty GameObject from your Canvas hierarchy here
    [Tooltip("Small Z-offset for the edge UI relative to the edge line's Z plane (e.g., -0.01 to be slightly in front).")]
    public float edgeUI_Z_OffsetFromLine = -0.01f;

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
        for (int i = activeEdges.Count - 1; i >= 0; i--)
        {
            Edge edge = activeEdges[i];
            bool removeEdge = false;

            if (edge.lineRenderer == null) removeEdge = true;
            else if (edge.isSelfLoop && edge.startNode == null) removeEdge = true;
            else if (!edge.isSelfLoop && (edge.startNode == null || edge.endNode == null)) removeEdge = true;

            if (removeEdge)
            {
                edge.DestroyUIElement(); // << ADDED: Destroy associated UI
                if (edge.lineRenderer != null) Destroy(edge.lineRenderer.gameObject);
                activeEdges.RemoveAt(i);
                Debug.Log("Removed stale edge and its UI due to missing components.");
                continue;
            }
            // Pass the Z offset for the UI when updating the edge position
            edge.UpdatePosition(lineZDepth, edgeUI_Z_OffsetFromLine); // << MODIFIED
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

    void DrawAndStoreLine(GameObject startNode, GameObject endNode)
    {
        GameObject lineObj = new GameObject("Edge_" + startNode.name + "_to_" + endNode.name);
        Transform parentForEdgeLine = (edgeHolder != null) ? edgeHolder.transform : this.transform;
        lineObj.transform.SetParent(parentForEdgeLine, false);

        LineRenderer lr = lineObj.AddComponent<LineRenderer>();
        lr.material = lineMaterial;
        lr.startWidth = lineWidth;
        lr.endWidth = lineWidth;
        lr.startColor = lineColor;
        lr.endColor = lineColor;
        lr.useWorldSpace = true;
        lr.sortingOrder = lineSortingOrder;

        // Create the Edge object, passing the UI prefab and its designated parent
        Edge newEdge = new Edge(lr, startNode, endNode, edgeUIPrefab, edgeUIHolderTransform);
        activeEdges.Add(newEdge);

        // Initial position update for both line and its UI (if any)
        newEdge.UpdatePosition(lineZDepth, edgeUI_Z_OffsetFromLine);

        if (newEdge.isSelfLoop)
        {
            Debug.Log("Creating self-loop for " + startNode.name + " (Edge UI typically not shown).");
        }
        else
        {
            Debug.Log("Creating edge between " + startNode.name + " and " + endNode.name + (newEdge.uiElementInstance != null ? " with UI." : " without UI (check prefab)."));
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

    /// <summary>
    /// Collects all data from the current graph (vertices and edges) into a serializable format.
    /// </summary>
    /// <returns>A GraphExportData object representing the entire graph.</returns>
    public GraphExportData GetGraphDataForExport()
    {
        GraphExportData graphData = new GraphExportData();

        // 1. Export Vertices
        if (vertexHolder != null)
        {
            // Iterate through all child GameObjects of VertexHolder
            // Assumes each direct child with a VertexController is a main vertex.
            for (int i = 0; i < vertexHolder.transform.childCount; i++)
            {
                Transform vertexTransform = vertexHolder.transform.GetChild(i);
                VertexController vc = vertexTransform.GetComponentInChildren<VertexController>(true); // Search in children, include inactive
                if (vc != null)
                {
                    // Debug.Log($"[EXPORT] Found VertexController on '{vc.gameObject.name}' (child of '{vertexTransform.name}'). ID: '{vc.PersistentId}'. Adding to export list.", vc);
                    // The rest of vc.GetExportData() will be called...
                    graphData.vertices.Add(vc.GetExportData());
                }
                else
                {
                    Debug.LogWarning($"[EXPORT] Child '{vertexTransform.name}' of VertexHolder (or its children) does NOT have a VertexController component. Skipping this vertex.", vertexTransform);
                }
            }
        }
        else
        {
            Debug.LogWarning("VertexHolder not assigned in SieraHandler. Cannot export vertex data.", this);
        }

        // 2. Export Edges
        // The 'activeEdges' list (List<Edge>) is already managed by SieraHandler
        foreach (Edge edge in activeEdges)
        {
            if (edge.startNode != null && edge.endNode != null)
            {
                VertexController startVC = edge.startNode.GetComponentInChildren<VertexController>(true);
                VertexController endVC = edge.endNode.GetComponentInChildren<VertexController>(true);

                if (startVC != null && endVC != null)
                {
                    EdgeExportData edgeExport = new EdgeExportData
                    {
                        fromVertexId = startVC.PersistentId, // Use the PersistentId from VertexController
                        toVertexId = endVC.PersistentId,     // Use the PersistentId from VertexController
                        edgeName = edge.lineRenderer.gameObject.name // Optional: for debugging
                        // Populate other edge properties if you add them to EdgeExportData and your Edge class
                    };
                    graphData.edges.Add(edgeExport);
                }
                else
                {
                    Debug.LogWarning($"Could not find VertexController on start or end node for edge '{edge.lineRenderer?.gameObject.name}'. Start: {edge.startNode?.name}, End: {edge.endNode?.name}", edge.lineRenderer);
                }
            }
            else
            {
                Debug.LogWarning($"Edge '{edge.lineRenderer?.gameObject.name}' has a null startNode or endNode.", edge.lineRenderer);
            }
        }
        return graphData;
    }

    /// <summary>
    /// Example method to get the graph data, convert it to JSON, and print it to the console.
    /// This also demonstrates where you might save to PlayerPrefs or send to a backend.
    /// </summary>
    [ContextMenu("Export Graph Data to JSON (Log to Console)")]
    public void ExportAndPrintGraphJson()
    {
        GraphExportData graphData = GetGraphDataForExport();
        string jsonPayload = JsonUtility.ToJson(graphData, true); // 'true' for pretty printing (easier to read)

        Debug.Log("--- SERIALIZED GRAPH DATA (JSON) ---");
        Debug.Log(jsonPayload);
        Debug.Log("------------------------------------");

        // Now you can use 'jsonPayload' or 'graphData'
        // 1. Save to Shared Preferences (PlayerPrefs for Unity)
        // PlayerPrefs.SetString("MyGraphData", jsonPayload);
        // PlayerPrefs.Save(); // Important to actually save it
        // Debug.Log("Graph data saved to PlayerPrefs under key 'MyGraphData'.");

        // 2. Send to a backend class
        // YourBackendConnectorClass.Instance.SendData(jsonPayload);
        // or
        // YourBackendConnectorClass.Instance.SendData(graphData); // If it takes the object directly

        // For now, we've logged it. Implement actual saving/sending as needed.
    }


    public void SaveButtonClick()
    {
        GraphExportData graphData = Instance.GetGraphDataForExport();
        string jsonPayload = JsonUtility.ToJson(graphData, true); // 'true' for pretty printing (easier to read)

        Debug.Log("--- SERIALIZED GRAPH DATA (JSON) ---");
        Debug.Log(jsonPayload);
        Debug.Log("------------------------------------");

        //prvy krok do buildera
        var tmpVertices = GraphVerticesToMatchObjects(graphData);
        List<MatchObject> matchObjects = ConnectMatchObjects(graphData, tmpVertices);


        CypherQueryBuilder cypherQueryBuilder = new NodeQueryBuilder();
        Debug.Log(cypherQueryBuilder.SetNeoNode(matchObjects).AddWhereCondition().Build());

    }

    private Dictionary<string, MatchObject> GraphVerticesToMatchObjects(GraphExportData graphData)
    {
        // spravit text pole pre graph id a dat do graphData
        string graphId = "1";
        Dictionary<string, MatchObject> tmpVertices = new Dictionary<string, MatchObject>();
        foreach (VertexExportData vertex in graphData.vertices)
        {
            MatchObject v = new NeoNode(vertex.id, graphId);
            //iconditions
            ICondition conditions = DataRowsToICondition(vertex.rowsData);
            v.attributes = conditions;

            tmpVertices[vertex.id] = v;
        }
        return tmpVertices;
    }


    private ICondition DataRowsToICondition(object exportedConditions)
    {
        return null;
    }

    private List<MatchObject> ConnectMatchObjects(GraphExportData graphData, Dictionary<string, MatchObject> Vertices)
    {
        List<MatchObject> matchObjects = new List<MatchObject>();
        HashSet<string> connectedNodes = new HashSet<string>();
        foreach (EdgeExportData edge in graphData.edges)
        {
            //este attr etc pridat
            MatchObject e = new NeoEdge(edge.edgeName);

            MatchObject fromNode = Vertices[edge.fromVertexId];
            MatchObject toNode = Vertices[edge.toVertexId];
            MatchObject pattern = new MatchPattern(fromNode, e, toNode);
            connectedNodes.Add(edge.fromVertexId);
            connectedNodes.Add(edge.toVertexId);
            matchObjects.Add(pattern);
        }

        foreach (var vertex in Vertices)
        {
            if (connectedNodes.Contains(vertex.Key))
            { continue; }
            matchObjects.Add(vertex.Value);
        }
        return matchObjects;
    }
    
}