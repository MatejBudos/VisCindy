using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Command
{
    public GameObject gameObject;
    public string command;
    public Command(GameObject gameObject, string command)
    {
        this.gameObject = gameObject;
        this.command = command;
    }
}
