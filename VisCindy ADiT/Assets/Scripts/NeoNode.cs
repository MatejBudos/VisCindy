using System;
using System.Collections.Generic;
using System.Linq;

public class NeoNode{
    
    public int graphId{get; set;}
    public string nodeId{get; set;}
    public string NeoVar {get; set;} = "a";
    public Dictionary<string,string> attributes{get; set;}


    public NeoNode( string nVar, int gid, string nid, Dictionary<string, string> attr){
        this.graphId = gid;
        this.nodeId = nid;
        this.attributes = attr;
        this.NeoVar = nVar;
        
    }
    public string ToCypherMatchProperties()
    {
    var allProps = new Dictionary<string, string>
    {
        { "graphId", graphId.ToString() },
        { "nodeId", $"'{nodeId}'" }
    };

    if (attributes != null)
    {
        foreach (var kvp in attributes)
        {
            allProps[kvp.Key] = $"'{kvp.Value}'";
        }
    }

    return "( " + this.NeoVar +" {" +  string.Join(", ", allProps.Select(kvp => $"{kvp.Key}: {kvp.Value}")) + "})";
    }
  
}