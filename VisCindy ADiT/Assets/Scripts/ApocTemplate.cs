using System.Collections.Generic;
//using Microsoft.MixedReality.Toolkit.Audio;
using System;
using System.Linq;
public abstract class Traversal
{
    //strart node treba definovat vzdy z nejakych matchnutych nodes
    public MatchObject startNode{ get; set; }
    
    public int maxLevel{ get; set; } = 10;
    public int minLevel{ get; set; } = 1;
    public string uniqueness{ get; set; } = "NODE_GLOBAL";

    public virtual string BuildQuery()
    {
        var config = new Dictionary<string, string>
        {
            { "minLevel", minLevel.ToString() },
            { "maxLevel", maxLevel.ToString() },
            { "uniqueness", $"\"{uniqueness}\"" }
        };

        foreach (var kvp in GetCustomConfig())
        {
            config[kvp.Key] = kvp.Value;
        }

        string configString = string.Join(", ", config.Select(kvp => $"{kvp.Key}: {kvp.Value}"));

       return
        "CALL apoc.path." + GetFunctionName() + "( " + startNode.NeoVar +  ", {" + configString + "})\n" +
        "YIELD path";

    }
    //hook methods
    protected abstract string GetFunctionName();
    protected abstract Dictionary<string, string> GetCustomConfig();

    
}

public class ExpandConfigTraversal : Traversal
{
    public string RelationshipFilter{get; set; }
    public int Limit { get; set; } = 1;
    public  List<MatchObject> TerminatorNodesParam { get; set; } = new List<MatchObject>();
    
  
    protected override string GetFunctionName() => "expandConfig";

    protected override Dictionary<string, string> GetCustomConfig()
    {
        var config = new Dictionary<string, string>
        {
            { "limit", Limit.ToString() },
            { "terminatorNodes", '[' + string.Join(", ", TerminatorNodesParam.Select(n => n.NeoVar )) + ']' },
        };
        if ( RelationshipFilter != null ){
            config["relationshipFilter"] = RelationshipFilter;
        }
        return config;
    }
    public void AddTerminatorNode( MatchObject node ){
        TerminatorNodesParam.Add( node );
    }
    public void RemoveTerminatorNode( string neoVar ){
        //toto sa bude dat aj inak/lepsie. zavisi ako to bude v unity spravene
        foreach( var neoNode in TerminatorNodesParam ){
            if ( neoNode.NeoVar.Equals( neoVar ) ){
                TerminatorNodesParam.Remove( neoNode );
            }
        }

    }
    
}

public class ExpandToXJumpsTraversal : Traversal
{
    public ExpandToXJumpsTraversal setJumps( int jumps ){
        this.maxLevel = jumps;
        this.minLevel = jumps;
        return this;
    }
    protected override string GetFunctionName() => "expandConfig";

    protected override Dictionary<string, string> GetCustomConfig() => new(); // žiadne ďalšie
}

public class SpanningTreeTraversal : Traversal
{
    protected override string GetFunctionName() => "spanningTree";

    protected override Dictionary<string, string> GetCustomConfig() => new(); // žiadne ďalšie

}


