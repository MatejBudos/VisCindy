using System.Collections.Generic;
//using Microsoft.MixedReality.Toolkit.Audio;
using System;
using System.Linq;
public abstract class Traversal
{
    public NeoNode startNode{ get; set; }
    //public NeoNode endNode{ set; get;}
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
        "CALL apoc.path." + GetFunctionName() + "($startNode, {" + configString + "})\n" +
        "YIELD path\n" +
        "RETURN path";

    }
    protected abstract string GetFunctionName();
    protected abstract Dictionary<string, string> GetCustomConfig();
    
}

public class ExpandConfigTraversal : Traversal
{
    public string RelationshipFilter { get; set; } = "\"KNOWS|WORKS_WITH\"";
    public int Limit { get; set; } = 1;
    public string TerminatorNodesParam { get; set; } = "endNodes";

    protected override string GetFunctionName() => "expandConfig";

    protected override Dictionary<string, string> GetCustomConfig() => new()
    {
        { "relationshipFilter", RelationshipFilter },
        { "limit", Limit.ToString() },
        { "terminatorNodes", $"${TerminatorNodesParam}" }
    };
    
}

public class ExpandToXJumpsTraversal : Traversal
{
    protected override string GetFunctionName() => "expandConfig";

    protected override Dictionary<string, string> GetCustomConfig() => new(); // žiadne ďalšie
}

public class SpanningTreeTraversal : Traversal
{
    protected override string GetFunctionName() => "spanningTree";

    protected override Dictionary<string, string> GetCustomConfig() => new(); // žiadne ďalšie

}


