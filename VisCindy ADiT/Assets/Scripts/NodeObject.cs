using System.Collections.Generic;
using UnityEngine;

public class NodeObject
{
    public string id;
    public float x, y, z;

    public List<string> edges = new List<string>();
    public List<string> edges_id = new List<string>();

    public List<string> neighbours= new List<string>();
    public GameObject UInode;
    public Dictionary<string, GameObject> UIedges = new Dictionary<string, GameObject>();

    public NodeObject(string name, float x, float y, float z)
    {
        id = name;
        this.x = x;
        this.y = y;
        this.z = z;
    }
}
