using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine.Networking;
using System.Text; // Required for List
using UnityEngine;
using UnityEngine.UI;


public class SieraHandler : MonoBehaviour
{
    public static SieraHandler Instance { get; private set; } // Singleton instance

    public GameObject vertexPrefab;
    public GameObject vertexHolder;
    public GameObject edgeHolder;
    
    [Header("API Configuration")]
    [Tooltip("Base URL for the API. Example: http://127.0.0.1:5000/api/")]
    public string baseApiUrl = "http://127.0.0.1:5000/api/";
    [Tooltip("The identifier/name of the graph whose properties you want to fetch.")]
    public string graphIdentifierForProperties = "myDefaultGraph";
    
    private static readonly CookieContainer SieraCookieContainer = new CookieContainer();
    private HttpClientHandler _sieraHttpClientHandler;
    private HttpClient _sieraHttpClient;
    
    [Header("Graph Export Configuration")]
    [Tooltip("Assign the TMP_InputField from your 'Limit' UI panel here.")]
    public TMP_InputField limitValueInputField; 

    [Header("Line Properties")]
    public Material lineMaterial;
    public float lineWidth = 0.05f;
    public Color lineColor = Color.black;
    public float lineZDepth = -1.0f; // Hardcoded Z axis for lines, adjust as needed
    public int lineSortingOrder = -1; // Sorting order for LineRenderer

    private bool isInEdgeMode = false;
    private GameObject firstSelectedVertexForEdge = null;
    private List<Edge> activeEdges = new List<Edge>(); 
    private int nextVertexIdCounter = 0;
    
    public List<string> LastFetchedNodeProperties { get; private set; } = new List<string>();
    public static event System.Action OnGraphNodePropertiesAvailable; 


    [Header("Edge UI Configuration")]
    [Tooltip("Prefab for the UI element (button/submenu) to show above edges.")]
    public GameObject edgeUIPrefab; // Assign your inactive UI prefab here
    [Tooltip("Parent transform for instantiated edge UI elements. Should be under your main Canvas.")]
    public Transform edgeUIHolderTransform; // Assign an empty GameObject from your Canvas hierarchy here
    [Tooltip("Small Z-offset for the edge UI relative to the edge line's Z plane (e.g., -0.01 to be slightly in front).")]
    public float edgeUI_Z_OffsetFromLine = -0.01f;

    [Header("APOC Integration")]
    [Tooltip("Assign the GameObject that has the ApocUIController script (e.g., your APOC panel).")]
    public ApocUIController apocUIControllerRef;
    [Tooltip("Assign the Toggle UI element that enables/disables APOC export.")]
    public Toggle toggleAPOC;
    
    
    
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
            return; // Important to return if destroying, to prevent further execution
        }

        // Initialize HttpClient
        _sieraHttpClientHandler = new HttpClientHandler
        {
            CookieContainer = SieraCookieContainer,
            UseCookies = true
        };
        _sieraHttpClient = new HttpClient(_sieraHttpClientHandler, disposeHandler: false);
    }
    
    void OnDestroy()
    {
        _sieraHttpClient?.Dispose();
        _sieraHttpClient = null;
        _sieraHttpClientHandler?.Dispose();
        _sieraHttpClientHandler = null;

        if (Instance == this)
        {
            Instance = null;
        }
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

    private void Start()
    {
        TriggerFetchGraphProperties();
    }

    public void AddNewNodeObject()
    {
        if (vertexPrefab == null)
        {
            Debug.LogError("AddNewNodeObject: Vertex Prefab is not assigned in SieraHandler!", this);
            return;
        }
        if (vertexHolder == null)
        {
            Debug.LogError("AddNewNodeObject: Vertex Holder is not assigned in SieraHandler!", this);
            return;
        }

        GameObject newVertexInstance = Instantiate(vertexPrefab, vertexHolder.transform, false);
        string newVertexDisplayName = "v" + nextVertexIdCounter; // e.g., "v0", "v1"

        // 1. Set visual name on the vertex circle
        Transform visualTextTransform = newVertexInstance.transform.Find("Text (TMP)");
        if (visualTextTransform != null)
        {
            TextMeshProUGUI visualNameLabel = visualTextTransform.GetComponent<TextMeshProUGUI>();
            if (visualNameLabel != null)
            {
                visualNameLabel.text = newVertexDisplayName; // Sets "v0", "v1" visually
            }
            else
            {
                Debug.LogWarning($"AddNewNodeObject: Could not find TextMeshProUGUI component on 'Text (TMP)' child of instantiated vertex '{newVertexInstance.name}'.", newVertexInstance);
            }
        }
        else
        {
            Debug.LogWarning($"AddNewNodeObject: Could not find 'Text (TMP)' child GameObject in instantiated vertex prefab '{newVertexInstance.name}'. Cannot set visual vertex name.", newVertexInstance);
        }

        // 2. Optional: Set the GameObject's name for easier identification in the Hierarchy
        newVertexInstance.name = $"Vertex_{newVertexDisplayName}";

        // 3. Update the label within the VertexController (for exported vertexLabel)
        // This assumes VertexController is on a child (like DropdownMenu) and has 'vertexLabelTextComponent' assigned.
        VertexController vc = newVertexInstance.GetComponentInChildren<VertexController>(true);
        if (vc != null)
        {
            if (vc.vertexLabelTextComponent != null)
            {
                vc.vertexLabelTextComponent.text = newVertexDisplayName; // THIS IS THE CRUCIAL LINE
            }
            else
            {
                // If not assigned, GetVertexLabel() might try a dynamic find, but explicit setting is safer.
                Debug.LogWarning($"AddNewNodeObject: VertexController on '{newVertexInstance.name}' does not have 'vertexLabelTextComponent' assigned. Exported vertexLabel might not reflect '{newVertexDisplayName}' unless found dynamically.", vc);
                // You could also directly set a field on vc if GetVertexLabel() reads from that instead of a TMP component.
                // For example, if VertexController had `public void SetExportLabel(string label)`, you'd call that.
                // But since GetVertexLabel() reads from vertexLabelTextComponent, updating that is the current way.
            }
            // Note: The PersistentId for the vertex (the GUID) is handled by VertexController.Awake()
        }
        else
        {
            Debug.LogWarning($"AddNewNodeObject: Could not find VertexController component in children of instantiated vertex '{newVertexInstance.name}'. Exported label will not be set here.", newVertexInstance);
        }
        

        nextVertexIdCounter++; // Increment for the next vertex

        if (!newVertexInstance.activeSelf)
        {
            newVertexInstance.SetActive(true);
        }

        Debug.Log($"Added new vertex: '{newVertexInstance.name}' with display name '{newVertexDisplayName}'.");
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
    // The constructor of GraphExportData already sets graphData.limit to "None" by default
    GraphExportData graphData = new GraphExportData();

    Debug.Log("--- [EXPORT] Starting GetGraphDataForExport ---");

    // 1. Export Vertices
    if (vertexHolder != null)
    {
        Debug.Log($"[EXPORT] Processing VertexHolder: '{vertexHolder.name}'. It has {vertexHolder.transform.childCount} children.", this);
        for (int i = 0; i < vertexHolder.transform.childCount; i++)
        {
            Transform vertexTransform = vertexHolder.transform.GetChild(i);
            // Debug.Log($"[EXPORT] Checking child {i}: '{vertexTransform.name}' under VertexHolder.", vertexTransform); // Optional: very verbose

            VertexController vc = vertexTransform.GetComponentInChildren<VertexController>(true); // Search in children, include inactive
            if (vc != null)
            {
                // GetExportData in VertexController now uses GetDerivedExportIdAndLabel() internally
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
        Debug.LogWarning("[EXPORT] VertexHolder not assigned in SieraHandler. Cannot export vertex data.", this);
    }
    Debug.Log($"[EXPORT] Vertices processed for export: {graphData.vertices.Count}");

    // 2. Export Edges
    if (activeEdges == null)
    {
        Debug.LogWarning("[EXPORT] SieraHandler.activeEdges list is null. No edges will be exported.", this);
        activeEdges = new List<Edge>(); // Ensure it's not null to prevent further errors, though this indicates an issue.
    }

    Debug.Log($"[EXPORT] Processing {activeEdges.Count} active edges for export.");
    foreach (Edge edge in activeEdges)
    {
        if (edge == null || edge.lineRenderer == null)
        {
            Debug.LogWarning("[EXPORT] Encountered a null or improperly initialized edge in activeEdges list. Skipping.", this);
            continue;
        }

        if (edge.startNode != null && edge.endNode != null)
        {
            VertexController startVC = edge.startNode.GetComponentInChildren<VertexController>(true);
            VertexController endVC = edge.endNode.GetComponentInChildren<VertexController>(true);

            if (startVC != null && endVC != null)
            {
                // Initialize EdgeExportData (constructor sets defaults for UI fields)
                EdgeExportData edgeExport = new EdgeExportData
                {
                    fromVertexId = startVC.GetDerivedExportIdAndLabel(), // Using "vX" name from VertexController
                    toVertexId = endVC.GetDerivedExportIdAndLabel(),   // Using "vX" name from VertexController
                    edgeName = edge.lineRenderer.gameObject.name
                };

                // Try to get data from the Edge's UI instance
                if (edge.uiElementInstance != null)
                {
                    EdgeUIDataController uiController = edge.uiElementInstance.GetComponent<EdgeUIDataController>();
                    if (uiController != null)
                    {
                        EdgeUIDataController.EdgeUIValues uiData = uiController.GetCurrentValues();
                        edgeExport.relationshipType = uiData.RelationshipType;
                        edgeExport.minValue =  uiData.MinValue == "None" ? null : uiData.MinValue;
                        edgeExport.maxValue = uiData.MaxValue == "None" ? null : uiData.MaxValue;
                        // Debug.Log($"[EXPORT] Edge '{edgeExport.edgeName}' UI Data: Rel='{uiData.RelationshipType}', Min='{uiData.MinValue}', Max='{uiData.MaxValue}'");
                    }
                    else
                    {
                        Debug.LogWarning($"[EXPORT] Edge '{edge.lineRenderer.gameObject.name}' has a uiElementInstance but no EdgeUIDataController script found on it. Exporting default UI data for this edge.", edge.uiElementInstance);
                    }
                }
                else if (!edge.isSelfLoop) // Only warn if UI is missing for non-self-loops
                {
                    Debug.LogWarning($"[EXPORT] Edge '{edge.lineRenderer.gameObject.name}' has no uiElementInstance. Exporting default UI data for this edge.", edge.lineRenderer.gameObject);
                }
                // For self-loops, uiElementInstance is typically null/inactive, so default values in EdgeExportData are used.

                graphData.edges.Add(edgeExport);
                Debug.Log($"[EXPORT] Added Edge: From ID '{edgeExport.fromVertexId}' To ID '{edgeExport.toVertexId}' (Name: '{edgeExport.edgeName}', Rel: '{edgeExport.relationshipType}')");

            }
            else
            {
                if (startVC == null) Debug.LogWarning($"[EXPORT] Could not find VertexController on start node '{edge.startNode.name}' for edge '{edge.lineRenderer.gameObject.name}'.", edge.startNode);
                if (endVC == null) Debug.LogWarning($"[EXPORT] Could not find VertexController on end node '{edge.endNode.name}' for edge '{edge.lineRenderer.gameObject.name}'.", edge.endNode);
            }
        }
        else
        {
            Debug.LogWarning($"[EXPORT] Edge '{edge.lineRenderer.gameObject.name}' has a null startNode or endNode. Skipping this edge.", edge.lineRenderer.gameObject);
        }
    }
    Debug.Log($"[EXPORT] Edges processed and added to export list: {graphData.edges.Count}");

    // 3. Add the Limit value
    if (limitValueInputField != null)
    {
        if (!string.IsNullOrEmpty(limitValueInputField.text))
        {
            graphData.limit = limitValueInputField.text;
        }
        // If the input field is empty, graphData.limit will remain "None" (its default from GraphExportData constructor)
        Debug.Log($"[EXPORT] Limit value for export: '{graphData.limit}' (Read from input field: '{limitValueInputField.text}')");
    }
    else
    {
        Debug.LogWarning("[EXPORT] SieraHandler.limitValueInputField is NOT assigned in the Inspector! 'limit' will use default value ('None').", this);
        // graphData.limit will be "None" as set in its constructor
    }
    
    // 4. Add APOC settings if toggle is on
    if (toggleAPOC != null)
    {
        graphData.toggleAPOCState = toggleAPOC.isOn; // << SETTING THE TOGGLE STATE
        Debug.Log($"[EXPORT] APOC Toggle State: {graphData.toggleAPOCState}");

        if (graphData.toggleAPOCState) // Check the state we just set
        {
            if (apocUIControllerRef != null)
            {
                graphData.apoc = apocUIControllerRef.GetApocSettings();
                Debug.Log("[EXPORT] APOC toggle is ON. APOC settings included in export.");
            }
            else
            {
                Debug.LogWarning("[EXPORT] APOC toggle is ON, but ApocUIControllerRef is not assigned in SieraHandler. APOC settings will be null.");
                graphData.apoc = null;
            }
        }
        else
        {
            Debug.Log("[EXPORT] APOC toggle is OFF. APOC settings will be null in export.");
            graphData.apoc = null; // Ensure APOC data is null if toggle is off
        }
    }
    else
    {
        Debug.LogWarning("[EXPORT] SieraHandler.toggleAPOC is NOT assigned in the Inspector! 'toggleAPOCState' will be default (false) and APOC settings will be null.", this);
        graphData.toggleAPOCState = false; // Default if toggle itself is not assigned
        graphData.apoc = null;
    }

    Debug.Log("--- [EXPORT] Finished GetGraphDataForExport ---");
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
        Dictionary<string,MatchObject> matchObjects = ConnectMatchObjects(graphData, tmpVertices);

        Apoc apoc = graphData.apoc;
        CypherQueryBuilder cypherQueryBuilder = new NodeQueryBuilder();

    
   

        if (wasApocUsed(graphData.toggleAPOCState))
        {
            cypherQueryBuilder = new TraversalQueryBuilder();
            Traversal traversal = new ExpandConfigTraversal();



            if (apoc.TraversalType == "Paths")
            {
                traversal.AddTerminatorNode(matchObjects[apoc.end]);
            }

            if (apoc.TraversalType == "Spanning")
            {
                traversal = new SpanningTreeTraversal();
            }
            if (apoc.TraversalType == "Jumps")
            {
                traversal = new ExpandToXJumpsTraversal();
            }

            traversal.startNode = matchObjects[apoc.start];
            if (apoc.min != null && apoc.min != "")
            {
                traversal.minLevel = apoc.min;
            }
            if (apoc.max != null && apoc.max != "")
            {
                traversal.maxLevel = apoc.max;
            }
            traversal.uniqueness = apoc.unique;
            traversal.RelationshipFilter = apoc.relFilter;
            cypherQueryBuilder.SetTraversal(traversal);
        }

        
        string query = cypherQueryBuilder.SetNeoNode(matchObjects.Values.ToList()).AddWhereCondition().Build();
        Debug.Log( query );
        QueryPayload qp = new QueryPayload(query,false);
        if (DrawGraph.Instance != null)
        {
            DrawGraph.Instance.SetQueryPayload(qp);
        }
        else
        {
            Debug.LogError("DrawGraph.Instance nie je nájdená! QueryPayload nemohol byť odovzdaný.");
        }

    }
    private bool wasApocUsed(bool apoc)
    {
        return apoc;
    }


  public async Task SendQueryAsync(string query, bool apoc)
    {
        QueryPayload qp = new QueryPayload(query,false);
        string jsonPayload = JsonConvert.SerializeObject(qp);

        // Ensure baseApiUrl ends with a slash for correct URL concatenation.
        string trimmedBaseApiUrl = baseApiUrl.TrimEnd('/');
        string endpoint = "layouter/query";
        string apiUrl = $"{trimmedBaseApiUrl}/{endpoint}";


        if (_sieraHttpClient == null)
        {
            Debug.LogError("[API] HttpClient is not initialized in SendQueryAsync. Ensure Awake() has run correctly.", this);
            return;
        }

        Debug.Log($"[API] Sending query to: {apiUrl}\nPayload: {jsonPayload}");

        try
        {
            HttpContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

            HttpResponseMessage responseMessage = await _sieraHttpClient.PostAsync(apiUrl, content);
            
            // Ensure the response content is disposed after reading
            using (HttpContent responseContent = responseMessage.Content)
            {
                string responseBody = await responseContent.ReadAsStringAsync();

                if (responseMessage.IsSuccessStatusCode)
                {
                    Debug.Log($"[API] Query sent successfully to {apiUrl}. Status: {responseMessage.StatusCode}.\nResponse: {responseBody}");
                    // TODO: Process the responseBody if needed (e.g., parse JSON, update UI)
                }
                else
                {
                    Debug.LogError($"[API] Failed to send query to {apiUrl}. Status: {responseMessage.StatusCode} - {responseMessage.ReasonPhrase}\nResponse Body: {responseBody}");
                }
            }
        }
        catch (HttpRequestException e)
        {
            // This catches network errors (DNS resolution, connection refused, etc.)
            Debug.LogError($"[API] HttpRequestException when sending query to {apiUrl}: {e.Message}", this);
            if (e.InnerException != null)
            {
                Debug.LogError($"[API] Inner Exception: {e.InnerException.Message}", this);
            }
        }
        catch (Exception ex)
        {
            // This catches other errors (e.g., issues during request setup, an unexpected null somewhere)
            Debug.LogError($"[API] General exception in SendQueryAsync for {apiUrl}: {ex.ToString()}", this);
        }
    }

    private Dictionary<string, MatchObject> GraphVerticesToMatchObjects(GraphExportData graphData)
    {
        string graphId = "1"; // Consider making this dynamic or configurable
        Dictionary<string, MatchObject> tmpVertices = new Dictionary<string, MatchObject>();

        foreach (VertexExportData vertex in graphData.vertices)
        {
            MatchObject v = new NeoNode(vertex.id, graphId); // Assuming NeoNode is one of your classes

            // Pass vertex.rowsData (which is List<RowData>) directly
            ICondition conditions = DataRowsToICondition(vertex.rowsData); // << MODIFIED: Pass the list
            Debug.Log(conditions.ToQueryString(""));
            v.attributes = conditions; // Assuming MatchObject has an 'attributes' field of type ICondition

            tmpVertices[vertex.id] = v;
        }
        return tmpVertices;
    }


    private ICondition DataRowsToICondition(List<RowData> rowsDataList)
    {
        if (rowsDataList == null || rowsDataList.Count == 0)
        {
            return null;
        }
        Debug.Log(rowsDataList);
        ICondition rootCondition = ConditionParser.LoadDataRows(rowsDataList);

    
        return rootCondition;
    }

    private Dictionary<string, MatchObject> ConnectMatchObjects(GraphExportData graphData, Dictionary<string, MatchObject> Vertices)
    {
        Dictionary<string,MatchObject> matchObjects = new Dictionary<string,MatchObject>();
        HashSet<string> connectedNodes = new HashSet<string>();
        foreach (EdgeExportData edge in graphData.edges)
        {

            // MatchObject e  = new NeoEdge(
            //     edge.edgeName, edge.relationshipType, edge.minValue, edge.maxValue
            // );
            MatchObject e  = new NeoEdge(
                "e1", edge.relationshipType, edge.minValue, edge.maxValue
            );

            MatchObject fromNode = Vertices[edge.fromVertexId];
            MatchObject toNode = Vertices[edge.toVertexId];
            MatchObject pattern = new MatchPattern(fromNode, e, toNode);
            connectedNodes.Add(edge.fromVertexId);
            connectedNodes.Add(edge.toVertexId);
            matchObjects[fromNode.NeoVar + e.NeoVar + toNode.NeoVar] = pattern;
        }

        foreach (var vertex in Vertices)
        {
            if (connectedNodes.Contains(vertex.Key))
            { continue; }
            matchObjects[vertex.Key] = vertex.Value;
        }
        return matchObjects;
    }
    
    [ContextMenu("Fetch Graph Properties from API (Log JSON)")]
    public void TriggerFetchGraphProperties()
    {
        if (string.IsNullOrEmpty(graphIdentifierForProperties))
        {
            Debug.LogError("[API] 'Graph Identifier For Properties' is not set in SieraHandler Inspector.", this);
            return;
        }
        if (_sieraHttpClient == null)
        {
            Debug.LogError("[API] HttpClient is not initialized. Ensure Awake() has run correctly.", this);
            // Optionally re-initialize here if appropriate, but Awake should handle it.
            // Awake(); // Be careful with calling Awake manually.
            return;
        }
        StartCoroutine(FetchGraphPropertiesCoroutine(graphIdentifierForProperties));
    }

    private IEnumerator FetchGraphPropertiesCoroutine(string graphName)
    {
        // Ensure baseApiUrl ends with a slash if the "properties" endpoint doesn't start with one.
        string trimmedBaseApiUrl = baseApiUrl.TrimEnd('/');
        string endpoint = "properties"; // The specific endpoint for graph properties
        string fullApiUrl = $"{trimmedBaseApiUrl}/{endpoint}/{Uri.EscapeDataString(graphName)}";

        Debug.Log($"[API] Fetching graph properties from: {fullApiUrl}");

        Task<HttpResponseMessage> getTask = null;
        try
        {
            getTask = _sieraHttpClient.GetAsync(fullApiUrl);
        }
        catch (Exception ex)
        {
            Debug.LogError($"[API] Exception when trying to initiate GetAsync for '{graphName}': {ex.ToString()}", this);
            LastFetchedNodeProperties.Clear(); // Clear any old properties
            OnGraphNodePropertiesAvailable?.Invoke(); // Notify that an attempt was made (and failed)
            yield break; // Exit coroutine
        }

        // Wait for the asynchronous HTTP GET task to complete
        yield return new WaitUntil(() => getTask.IsCompleted);

        string responseData = null;

        try
        {
            if (getTask.IsFaulted)
            {
                Debug.LogError($"[API] Task faulted for graph '{graphName}': {getTask.Exception?.ToString()}", this);
                if (getTask.Exception?.InnerException != null)
                {
                    Debug.LogError($"[API] Inner Exception: {getTask.Exception.InnerException?.ToString()}", this);
                }
                LastFetchedNodeProperties.Clear();
                OnGraphNodePropertiesAvailable?.Invoke();
            }
            else if (getTask.IsCanceled)
            {
                Debug.LogError($"[API] Task to fetch properties for graph '{graphName}' was canceled.", this);
                LastFetchedNodeProperties.Clear();
                OnGraphNodePropertiesAvailable?.Invoke();
            }
            else // Task completed (successfully or with an HTTP error status code)
            {
                HttpResponseMessage responseMessage = getTask.Result;
                using (responseMessage) // Ensure the HttpResponseMessage is disposed
                {
                    Task<string> readTask = responseMessage.Content.ReadAsStringAsync();
                    new WaitUntil(() => readTask.IsCompleted);

                    if (readTask.IsFaulted)
                    {
                        Debug.LogError($"[API] Failed to read response content for graph '{graphName}': {readTask.Exception?.ToString()}", this);
                        LastFetchedNodeProperties.Clear();
                        OnGraphNodePropertiesAvailable?.Invoke();
                    }
                    else
                    {
                        responseData = readTask.Result;
                        if (responseMessage.IsSuccessStatusCode)
                        {
                            Debug.Log($"[API] Success! Response JSON for graph '{graphName}':\n{responseData}");
                            try
                            {
                                JObject jsonData = JObject.Parse(responseData); // Using Newtonsoft.Json.Linq
                                Debug.Log($"[API] Successfully parsed JSON with Newtonsoft. Root keys found: {string.Join(", ", jsonData.Properties().Select(p => p.Name))}");

                                // Extract and store node_properties
                                JArray nodePropsJsonArray = jsonData["node_properties"] as JArray;
                                if (nodePropsJsonArray != null)
                                {
                                    LastFetchedNodeProperties = nodePropsJsonArray.ToObject<List<string>>(); // Using Newtonsoft.Json
                                    Debug.Log($"[API] Extracted {LastFetchedNodeProperties.Count} node properties: {string.Join(", ", LastFetchedNodeProperties)}");
                                }
                                else
                                {
                                    Debug.LogWarning("[API] 'node_properties' key not found or not an array in JSON response. Node properties list will be empty.");
                                    LastFetchedNodeProperties.Clear();
                                }
                                OnGraphNodePropertiesAvailable?.Invoke(); // Fire event AFTER processing
                            }
                            catch (JsonReaderException jsonEx)
                            {
                                Debug.LogError($"[API] Failed to parse JSON response with Newtonsoft: {jsonEx.Message}\nRaw data was: {responseData}", this);
                                LastFetchedNodeProperties.Clear();
                                OnGraphNodePropertiesAvailable?.Invoke(); // Still invoke, so UI can clear if needed
                            }
                        }
                        else
                        {
                            Debug.LogError($"[API] HTTP Error fetching properties for graph '{graphName}': {responseMessage.StatusCode} - {responseMessage.ReasonPhrase}\nResponse Body: {responseData}", this);
                            LastFetchedNodeProperties.Clear();
                            OnGraphNodePropertiesAvailable?.Invoke(); // Notify to clear/reset dropdowns
                        }
                    }
                }
            }
        }
        catch (Exception e) // Catch any other synchronous exceptions that might occur during result processing
        {
            Debug.LogError($"[API] General exception during API call result processing for graph '{graphName}': {e.ToString()}", this);
            LastFetchedNodeProperties.Clear();
            OnGraphNodePropertiesAvailable?.Invoke();
        }
    }
    
}