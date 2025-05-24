using System.Collections.Generic;
using UnityEngine;

public class NodeObject
{
    public string id;
    public float x, y, z;

    public List<string> edges;
    public List<string> edges_id = new List<string>();

    public List<string> neighbours = new List<string>();
    public GameObject UInode;
    public Dictionary<string, GameObject> UIedges;

    public NodeObject(string name, float x, float y, float z)
    {
        id = name;
        this.x = x;
        this.y = y;
        this.z = z;
		this.UIedges = new Dictionary<string, GameObject>();
		this.edges = new List<string>();	
    }
    
    public bool isEqualTo(NodeObject nodeObject)
	{
    	return nodeObject.edges.Count == this.edges.Count;
	}

}
