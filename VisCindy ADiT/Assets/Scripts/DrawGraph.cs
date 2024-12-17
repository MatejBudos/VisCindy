using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine.Networking;

[System.Serializable]
public class QueryPayload
{
    public string query;
}

public class DrawGraph : MonoBehaviour
{
    public GameObject graphPrefab;
    private int _counter = 0;
    private static readonly CookieContainer CookieContainer = new CookieContainer();
    private HttpClientHandler _handler = new HttpClientHandler
    {
        CookieContainer = CookieContainer,
        UseCookies = true
    };
    public string apiUrl = "http://127.0.0.1:5000/api/";
    private Dictionary<string, NodeObject> _nodesDictionary = new Dictionary<string, NodeObject>();

    private bool _loadedFlag = true;
    private string _responseData1;
    
    private List<GameObject> activeNodes = new List<GameObject>(); 
    private List<GameObject> activeEdges = new List<GameObject>();

    private const string SPHERE_POOL_KEY = "Nodes";
    private const string LINE_POOL_KEY = "Lines";
    
    

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
        ResetGraph();
        StartCoroutine(ProcessGraphData()); 
    }

    public void CreateGraphLayout(string layout)
    {
        ResetGraph();
        StartCoroutine(ProcessGraphDataLayout(layout));
    }

    private IEnumerator ProcessGraphDataLayout(string layout)
    {
        yield return StartCoroutine(LayoutGraph(layout));

        Dictionary<string, NodeObject> forAdd = new Dictionary<string, NodeObject>();
        foreach (var node in ReadingJson.ReadJson(_responseData1))
        {
            _nodesDictionary.Add(node.Key, node.Value);
            forAdd.Add(node.Key, node.Value);
            _counter++;
        }

        _loadedFlag = true;
        VisualizeGraph(forAdd);
    }
    
    private IEnumerator ProcessGraphData()
    {
        yield return StartCoroutine(GetGraphData());

        Dictionary<string, NodeObject> forAdd = new Dictionary<string, NodeObject>();
        foreach (var node in ReadingJson.ReadJson(_responseData1))
        {
            _nodesDictionary.Add(node.Key, node.Value);
            forAdd.Add(node.Key, node.Value);
            _counter++;
        }

        _loadedFlag = true;
        VisualizeGraph(forAdd);
    }

    private IEnumerator GetGraphData()
    {
        using (HttpClient client = new HttpClient(new HttpClientHandler
               {
                   CookieContainer = CookieContainer,
                   UseCookies = true
               }))
        {
            var response1 = client.GetAsync(apiUrl + "graph/1");
            yield return response1;

            if (response1.Result.IsSuccessStatusCode)
            {
                _responseData1 = response1.Result.Content.ReadAsStringAsync().Result;
                yield return _responseData1;
            }
            else
            {
                Console.WriteLine($"Error: {response1.Result.StatusCode} - {response1.Result.ReasonPhrase}");
            }

        }
    }

    private void VisualizeGraph(Dictionary<string, NodeObject> forAdd)
    {
        Queue<GameObject> spherePool = ObjectPool.SharedInstance.poolDictionary[SPHERE_POOL_KEY];
        Queue<GameObject> linePool = ObjectPool.SharedInstance.poolDictionary[LINE_POOL_KEY];
        
        foreach (KeyValuePair<string, NodeObject> node in forAdd)
        {
            GameObject sphere = ObjectPool.SharedInstance.GetObject(SPHERE_POOL_KEY);
            if (sphere != null)
            {
                sphere.SetActive(true);
                sphere.name = node.Key;
                sphere.transform.position = new Vector3(node.Value.x, node.Value.y, node.Value.z);
                sphere.transform.SetParent(graphPrefab.transform);
                _nodesDictionary[node.Key].UInode = sphere;
                activeNodes.Add(sphere);
            }
            else
            {
                Debug.LogWarning("Not enough spheres in the pool!");
            }
        }

        foreach (KeyValuePair<string, NodeObject> node in forAdd)
        {
            foreach (string targetNode in node.Value.edges)
            {
                if (linePool.Count > 0)
                {
                    GameObject edge = linePool.Dequeue();
                    edge.SetActive(true);
                    edge.transform.SetParent(graphPrefab.transform);

                    // Reset Line Renderer positions:
                    LineRenderer lr = edge.GetComponent<LineRenderer>(); 
                    lr.positionCount = 0; 
                    lr.positionCount = 2; 

                    if (!_nodesDictionary[node.Key].UIedges.ContainsKey(targetNode))
                    {
                        _nodesDictionary[node.Key].UIedges.Add(targetNode, edge);
                        activeEdges.Add(edge);
                    }
                }
                else
                {
                    Debug.LogWarning("Not enough lines in the pool!");
                }
            }
        }
    }
    
    public IEnumerator LayoutGraph(string layoutType)
    {
        using (HttpClient client = new HttpClient(new HttpClientHandler
               {
                   CookieContainer = CookieContainer,
                   UseCookies = true
               }))
        {
            var response1 = client.GetAsync(apiUrl + "layouter/" + layoutType);
            yield return response1;

            if (response1.Result.IsSuccessStatusCode)
            {
                _responseData1 = response1.Result.Content.ReadAsStringAsync().Result;
                yield return _responseData1;
            }
            else
            {
                Console.WriteLine($"Error: {response1.Result.StatusCode} - {response1.Result.ReasonPhrase}");
            }

        }
    }

    public void ResetGraph()
    {
        _nodesDictionary = new Dictionary<string, NodeObject>();
        foreach (GameObject node in activeNodes)
        {
            ObjectPool.SharedInstance.ReturnObject(node, SPHERE_POOL_KEY);
        }
        activeNodes.Clear();
        
        foreach (GameObject edge in activeEdges)
        {
            ObjectPool.SharedInstance.ReturnObject(edge, LINE_POOL_KEY);
        }
        activeEdges.Clear(); 
    }
}