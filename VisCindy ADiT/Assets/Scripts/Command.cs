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
    public NodeObject firstNode;
    public NodeObject secondNode;
    public NodeObject nodeObject;
    public Command(GameObject gameObject, string command,NodeObject firstNode = null,NodeObject secondNode = null,string nodeName="", string fromNode = "", string toNode = "" , NodeObject node = null)
    {
        this.nodeObject = node;
        this.gameObject = gameObject;
        this.command = command;
        this.firstNode = firstNode;
        this.secondNode = secondNode;
        this.fromNode = fromNode;
        this.toNode = toNode;
        this.nodeName = nodeName;
    }
}
