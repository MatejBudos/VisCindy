using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine.UI;

[System.Serializable]
public class QueryPayload
{
    public string query;
}


public class DrawGraph : MonoBehaviour
{
    public GameObject graphPrefab;
    private int _counter = 0;
    private Queue<GameObject> linePool;
    private Queue<GameObject> spherePool;
    private static readonly CookieContainer CookieContainer = new CookieContainer();
    private Stack<Command> undo = new Stack<Command>();
    private Stack<Command> redo = new Stack<Command>();
    private string firstSelectedKey = null;
    private string secondSelectedKey = null;
    public bool isAddEdgeMode = false;
    public bool isRemoveEdgeMode = false;
    public bool isRemoveNodeMode = false;
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
        spherePool = ObjectPool.SharedInstance.poolDictionary[SPHERE_POOL_KEY];
        linePool = ObjectPool.SharedInstance.poolDictionary[LINE_POOL_KEY];

        foreach (KeyValuePair<string, NodeObject> node in forAdd)
        {
            GameObject sphere = ObjectPool.SharedInstance.GetObject(SPHERE_POOL_KEY);
            if (sphere != null)
            {
                sphere.SetActive(true);
                sphere.name = node.Key;
                sphere.transform.position = new Vector3(node.Value.x, node.Value.y, node.Value.z);
                sphere.transform.SetParent(graphPrefab.transform);
                VertexSelector vertexSelector = sphere.AddComponent<VertexSelector>();
                vertexSelector.drawGraph = this;
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

    public void AddNode()
    {        
        GameObject sphere = ObjectPool.SharedInstance.GetObject(SPHERE_POOL_KEY);
        if (sphere != null)
        {
            sphere.SetActive(true);            
            sphere.name = _counter.ToString();
            sphere.transform.position = new Vector3(0,0,0);
            sphere.transform.SetParent(graphPrefab.transform);
            VertexSelector vertexSelector = sphere.AddComponent<VertexSelector>();
            vertexSelector.drawGraph = this;
            _nodesDictionary.Add(_counter.ToString(), new NodeObject(_counter.ToString(),0,0,0));
            _nodesDictionary[_counter.ToString()].UInode = sphere;
            _counter++;
            activeNodes.Add(sphere);
            redo.Clear();
            undo.Push(new Command(sphere, "AddNode"));
        }
        else
        {
            Debug.LogWarning("Not enough spheres in the pool!");
        }
    }

    public void AddEdge()
    {
        firstSelectedKey = null;
        secondSelectedKey = null;
        isAddEdgeMode = true;
    }

    public void RemoveEdge()
    {
        firstSelectedKey = null;
        secondSelectedKey = null;
        isRemoveEdgeMode = true;
        
    }

    public void RemoveNode()
    {
        firstSelectedKey = null;
        secondSelectedKey = null;
        isRemoveNodeMode = true;
    }

    public void OnVertexSelected(string vertexKey)
    {
        if (firstSelectedKey == null)
        {
            firstSelectedKey = vertexKey;
            Debug.Log($"First vertex selected: {firstSelectedKey}");
            if (isRemoveNodeMode)
            {
                DisableNode(firstSelectedKey);
                isRemoveNodeMode = false;
            }
        }
        else if (secondSelectedKey == null && firstSelectedKey != vertexKey)
        {
            secondSelectedKey = vertexKey;
            Debug.Log($"Second vertex selected: {secondSelectedKey}");

            if (isAddEdgeMode)
            {
                CreateEdge(firstSelectedKey, secondSelectedKey);
                isAddEdgeMode = false;
            } else if (isRemoveEdgeMode)
            {
                DisableEdge(firstSelectedKey, secondSelectedKey);
                isRemoveEdgeMode = false;
            }                        
        }
        else
        {
            Debug.LogWarning("Invalid selection. Please select a different second vertex.");
        }
    }

    private void DisableEdge(string fromNode, string toNode)
    {
        if(_nodesDictionary.ContainsKey(fromNode) && _nodesDictionary[fromNode].UIedges.ContainsKey(toNode))
        {
            Debug.Log("Edge succesfully disabled");
            GameObject edge = _nodesDictionary[fromNode].UIedges[toNode];            
            SetVisibilityEdge(edge,false);
            undo.Push(new Command(edge,"RemoveEdge"));
            redo.Clear();
        } else if (_nodesDictionary.ContainsKey(toNode) && _nodesDictionary[toNode].UIedges.ContainsKey(fromNode))
        {
            Debug.Log("Edge succesfully disabled");
            GameObject edge = _nodesDictionary[toNode].UIedges[fromNode];
            SetVisibilityEdge(edge, false);
            undo.Push(new Command(edge, "RemoveEdge"));
            redo.Clear();
        }
    }

    private void DisableNode(string nodeKey)
    {
        if (_nodesDictionary.ContainsKey(nodeKey))
        {
            GameObject node = _nodesDictionary[nodeKey].UInode;
            SetVisibilityNode(node, false);
            foreach (KeyValuePair<string,NodeObject> vrchol in _nodesDictionary)
            {
                foreach (KeyValuePair<string, GameObject> edge in _nodesDictionary[vrchol.Key].UIedges)
                {
                    if(vrchol.Key.Equals(nodeKey) || edge.Key.Equals(nodeKey))
                    {
                        SetVisibilityEdge(edge.Value, false);
                    }                    
                }
            }
            Command command = new Command(node, "RemoveNode");
            command.nodeName = nodeKey;
            undo.Push(command);
            redo.Clear();
        }
    }

    // Function to create the edge between two vertices
    private void CreateEdge(string fromNode, string toNode)
    {
        if (_nodesDictionary.ContainsKey(fromNode) && _nodesDictionary.ContainsKey(toNode) && !fromNode.Equals(toNode) && !_nodesDictionary[fromNode].UIedges.ContainsKey(toNode) && !_nodesDictionary[toNode].UIedges.ContainsKey(fromNode))
        {
            GameObject edge = ObjectPool.SharedInstance.GetObject(LINE_POOL_KEY);
            if (edge != null)
            {
                edge.SetActive(true);
                edge.transform.SetParent(graphPrefab.transform);

                LineRenderer lr = edge.GetComponent<LineRenderer>();
                lr.SetPosition(0, _nodesDictionary[fromNode].UInode.transform.position);
                lr.SetPosition(1, _nodesDictionary[toNode].UInode.transform.position);

                // Add the edge to the dictionary and activeEdges list
                if (!_nodesDictionary[fromNode].UIedges.ContainsKey(toNode))
                {
                    _nodesDictionary[fromNode].UIedges.Add(toNode, edge);
                    activeEdges.Add(edge);
                    Debug.Log($"Edge created between {fromNode} and {toNode}");

                    redo.Clear();
                    undo.Push(new Command(edge, "AddEdge"));

                    // Reset Line Renderer positions:
                    LineRenderer lrenderer = edge.GetComponent<LineRenderer>();
                    lrenderer.positionCount = 0;
                    lrenderer.positionCount = 2;
                }
                else
                {
                    Debug.Log("Edge already exists between these nodes!");
                    ObjectPool.SharedInstance.ReturnObject(edge, LINE_POOL_KEY); // Return to pool
                }
            }
            else
            {
                Debug.Log("Not enough lines in the pool!");
            }
        }
        else
        {
            Debug.Log($"Invalid nodes: {fromNode} or {toNode} not found in the graph!");
        }
    }

    public void SetVisibilityNode(GameObject sphere, bool active)
    {
        sphere.SetActive(active);        
    }

    public void SetVisibilityEdge(GameObject edge, bool active)
    {
        edge.SetActive(active);
    }


    public void Undo()
    {
        if (undo.Count != 0)
        {
            Command aktualCommand = undo.Pop();
            if (aktualCommand.command.Equals("AddNode"))
            {
                SetVisibilityNode(aktualCommand.gameObject, false);
                redo.Push(aktualCommand);
            } else if (aktualCommand.command.Equals("AddEdge"))
            {
                SetVisibilityNode(aktualCommand.gameObject, false);
                redo.Push(aktualCommand);
            } else if (aktualCommand.command.Equals("RemoveNode"))
            {
                SetVisibilityNode(aktualCommand.gameObject, true);
                foreach (KeyValuePair<string, NodeObject> vrchol in _nodesDictionary)
                {
                    foreach (KeyValuePair<string, GameObject> edge in _nodesDictionary[vrchol.Key].UIedges)
                    {
                        if (vrchol.Key.Equals(aktualCommand.nodeName) || edge.Key.Equals(aktualCommand.nodeName))
                        {
                            SetVisibilityEdge(edge.Value, true);
                        }
                    }
                }
                redo.Push(aktualCommand);
            } else if (aktualCommand.command.Equals("RemoveEdge"))
            {
                redo.Push(aktualCommand);
                SetVisibilityEdge(aktualCommand.gameObject,true);
            }
        }
    }

    public void Redo()
    {
        if(redo.Count != 0)
        {
            Command aktualCommand = redo.Pop();
            if (aktualCommand.command.Equals("AddNode"))
            {
                SetVisibilityNode(aktualCommand.gameObject, true);
                undo.Push(aktualCommand);
            } else if (aktualCommand.command.Equals("AddEdge"))
            {
                SetVisibilityEdge(aktualCommand.gameObject, true);
                undo.Push(aktualCommand);
            } else if (aktualCommand.command.Equals("RemoveNode"))
            {
                SetVisibilityNode(aktualCommand.gameObject, false);
                foreach (KeyValuePair<string, NodeObject> vrchol in _nodesDictionary)
                {
                    foreach (KeyValuePair<string, GameObject> edge in _nodesDictionary[vrchol.Key].UIedges)
                    {
                        if (vrchol.Key.Equals(aktualCommand.nodeName) || edge.Key.Equals(aktualCommand.nodeName))
                        {
                            SetVisibilityEdge(edge.Value, false);
                        }
                    }
                }
                undo.Push(aktualCommand);
            }
            else if (aktualCommand.command.Equals("RemoveEdge"))
            {
                undo.Push(aktualCommand);
                SetVisibilityEdge(aktualCommand.gameObject, false);
            }
        }                
    }
}