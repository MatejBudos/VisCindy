using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Command
{
    public GameObject gameObject;
    public string command;
    public string nodeName;
    public string fromNode;
    public string toNode;
    public NodeObject nodeObject;
    public Command(GameObject gameObject, string command, string fromNode = "", string toNode = "", string nodeName="", NodeObject node = null)
    {
        this.nodeObject = node;
        this.gameObject = gameObject;
        this.command = command;
        this.fromNode = fromNode;
        this.toNode = toNode;
        this.nodeName = nodeName;
    }
}
