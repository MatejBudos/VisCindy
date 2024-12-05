using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class QueryPayload
{
    public string query;
}
public class DrawGraph : MonoBehaviour
{
    public GameObject graphPrefab;
    public GameObject spherePrefab;
    public GameObject linePrefab;
    public int pregeneratedNodes = 50;
    public int pregeneratedEdges = 50;
    private int counter = 0;

    public string apiUrl = "http://127.0.0.1:5000/api/";
    private Dictionary<string, NodeObject> _nodesDictionary = new Dictionary<string, NodeObject>();

    private bool _loadedFlag = true;

    private const string SPHERE_POOL_KEY = "Nodes";
    private const string LINE_POOL_KEY = "Lines";

    void Start()
    {
        // Initialize object pools
        ObjectPool.SharedInstance.CreatePool(spherePrefab, pregeneratedNodes, SPHERE_POOL_KEY);
        ObjectPool.SharedInstance.CreatePool(linePrefab, pregeneratedEdges, LINE_POOL_KEY);
    }

    void Update()
    {
        if (!_loadedFlag) return;

        foreach (KeyValuePair<string, NodeObject> node in _nodesDictionary)
        {
            foreach (KeyValuePair<string, GameObject> edge in node.Value.UIedges)
            {
                LineRenderer lineRenderer = edge.Value.GetComponent<LineRenderer>();
                lineRenderer.SetPosition(0, node.Value.UInode.transform.position);
                lineRenderer.SetPosition(1, _nodesDictionary[edge.Key].UInode.transform.position);
            }
        }
    }

    public void CreateGraphFromAPI()
    {
        StartCoroutine(GetGraphData());
    }

    IEnumerator GetGraphData()
    {
        string q = @"MATCH (n)
            OPTIONAL MATCH(n)-[r]->(m)
            WITH
            Id(n) AS id,
            collect(CASE
                WHEN m IS NOT NULL THEN { source: Id(n), target: Id(m), relationship: type(r)}
                ELSE NULL
            END) AS edges
            RETURN
            id,
            [edge IN edges WHERE edge IS NOT NULL] AS edges;";
        QueryPayload queryPayload = new QueryPayload { query = q };
        string jsonPayload = JsonUtility.ToJson(queryPayload);
        //Debug.Log(jsonPayload);
        //UnityWebRequest www = new UnityWebRequest(apiUrl + "query", "POST");
        //byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
        //www.uploadHandler = new UploadHandlerRaw(bodyRaw);
        //www.downloadHandler = new DownloadHandlerBuffer();
        //www.SetRequestHeader("Content-Type", "application/json");
        //yield return www.SendWebRequest();
        using (UnityWebRequest request = new UnityWebRequest(apiUrl + "query", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Query Response: " + request.downloadHandler.text);

                // Pass the response JSON to the next API
                StartCoroutine(SendToLayouterApi(request.downloadHandler.text));
            }
            else
            {
                Debug.LogError("Query API Error: " + request.error);
            }
        }
    

        //if (www.result != UnityWebRequest.Result.Success)
        //{
        //    Debug.LogError(www.error);
        //}
        //else
        //{
            
        //    string json = www.downloadHandler.text;
        //    Dictionary<string, NodeObject> forAdd = new Dictionary<string, NodeObject>();
        //    foreach (var node in ReadingJson.ReadJson(json))
        //    {
        //        Debug.Log("vrojor " + counter);
        //        _nodesDictionary.Add(counter.ToString(), node.Value);
        //        forAdd.Add(counter.ToString(), node.Value);
        //        counter++;
        //    }

        //    _loadedFlag = true;
        //    VisualizeGraph(forAdd);                        
        //}
    }

    private IEnumerator SendToLayouterApi(string jsonResponse)
    {

        using (UnityWebRequest request = new UnityWebRequest(apiUrl + "layouter", "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonResponse);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Layouter Response: " + request.downloadHandler.text);
                string json = request.downloadHandler.text;
                Dictionary<string, NodeObject> forAdd = new Dictionary<string, NodeObject>();
                foreach (var node in ReadingJson.ReadJson(json))
                {
                    Debug.Log("vrojor " + counter + " " + node.Key);                    
                    _nodesDictionary.Add(counter.ToString(), node.Value);
                    forAdd.Add(counter.ToString(), node.Value);
                    counter++;
                }

                _loadedFlag = true;
                VisualizeGraph(forAdd);
            }
            else
            {
                Debug.LogError("Layouter API Error: " + request.error);
            }
        }
    }

    private void VisualizeGraph(Dictionary<string, NodeObject> forAdd)
    {
        // Create spheres using object pool
        foreach (KeyValuePair<string, NodeObject> node in forAdd)
        {
            GameObject sphere = ObjectPool.SharedInstance.GetObject(SPHERE_POOL_KEY);
            if (sphere != null)
            {
                sphere.transform.position = new Vector3(node.Value.x, node.Value.y, node.Value.z);
                sphere.transform.SetParent(graphPrefab.transform);
                _nodesDictionary[node.Key].UInode = sphere;
            }
        }

        // Create edges using object pool
        foreach (KeyValuePair<string, NodeObject> node in forAdd)
        {
            foreach (string targetNode in node.Value.edges)
            {
                GameObject edge = ObjectPool.SharedInstance.GetObject(LINE_POOL_KEY);
                if (edge != null)
                {
                    edge.transform.SetParent(gameObject.transform);
                    if (!_nodesDictionary[node.Key].UIedges.ContainsKey(targetNode)){
                        _nodesDictionary[node.Key].UIedges.Add(targetNode, edge);
                    }
                    
                }
            }
        }
    }

    public void ResetGraph()
    {
        foreach (KeyValuePair<string, NodeObject> node in _nodesDictionary)
        {
            if (node.Value.UInode != null)
            {
                ObjectPool.SharedInstance.ReturnObject(node.Value.UInode, SPHERE_POOL_KEY);
            }

            foreach (KeyValuePair<string, GameObject> edge in node.Value.UIedges)
            {
                ObjectPool.SharedInstance.ReturnObject(edge.Value, LINE_POOL_KEY);
            }
            node.Value.UIedges.Clear();
        }

        _nodesDictionary.Clear();
        _loadedFlag = false;
    }
}
