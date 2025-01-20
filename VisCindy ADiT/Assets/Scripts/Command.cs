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
    public Command(GameObject gameObject, string command, string fromNode = "", string toNode = "", string nodeName="")
    {
        this.gameObject = gameObject;
        this.command = command;
        this.fromNode = fromNode;
        this.toNode = toNode;
        this.nodeName = nodeName;
    }
}
