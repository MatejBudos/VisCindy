using System;
using System.Collections.Generic;
using System.Linq;

public class NeoNode{
    
    //graphid a neovar treba aby boli nastavene
    public string graphId{get; set;}
    public string nodeId{get; set;} //presunut do composite
    public string NeoVar {get; set;}
    public ICondition attributes{get; set;} // toto bude composite nie str


    public NeoNode( string nVar, string gid, ICondition attr = null ){
        this.NeoVar = nVar;
        this.graphId = gid;
        this.attributes = attr;
        
        
    }
    public string ToCypherMatchProperties()
    {
        var allProps = new Dictionary<string, string>
        {
            { "graphId", graphId.ToString() },

        };
        if (nodeId != null ){
            allProps["Id"] =  $"'{nodeId}'";
        }


        return "( " + this.NeoVar +" {" +  string.Join(", ", allProps.Select(kvp => $"{kvp.Key}: {kvp.Value}")) + "})";
    }
    public string ToCypherNodeConditions(){
        return attributes.ToQueryString( this.NeoVar );

    }
  
}