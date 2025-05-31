using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

public abstract class MatchObject
{
    public ICondition attributes { get; set; }
    public string NeoVar { get; set; }
    public abstract string ToCypherMatchProperties();
    public virtual void Add( MatchObject obj )
    {
        return;
    }
    public virtual string NeoVarToString()
    {
        return NeoVar;
    }
    public virtual string ToCypherConditions()
    {
        return attributes != null ? attributes.ToQueryString(this.NeoVar) : "";

    }
    public MatchObject(string nVar, ICondition attr = null)
    {
        attributes = attr;
        NeoVar = nVar;
    } 
}
public class NeoNode : MatchObject
{
    //graphid a neovar treba aby boli nastavene
    public string graphId { get; set; }
    public string nodeId { get; set; } //presunut do composite
    public NeoNode(string nVar, string gid, ICondition attr = null) : base(nVar, attr)
    {
        this.graphId = gid;
    }
    public override string ToCypherMatchProperties()
    {
        var allProps = new Dictionary<string, string>
        {
            { "graphId", graphId.ToString() },

        };
        if (nodeId != null)
        {
            allProps["Id"] = $"'{nodeId}'";
        }


        return "( " + this.NeoVar + " {" + string.Join(", ", allProps.Select(kvp => $"{kvp.Key}: {kvp.Value}")) + "})";
    }
}
public class NeoEdge : MatchObject
{
    public string min;
    public string max;
    public string relType;

    public NeoEdge(string nVar, string relType = null,
                    string min = null, string max = null,
                    ICondition attr = null) : base(nVar, attr)
    {
        this.relType = relType;
        this.min = min;
        this.max = max;
    }

    public override string ToCypherMatchProperties()
    {

        string relationShip = relType != null ? ":" + relType : "";
        string minMax = "";
        if (min != null && max != null)
        {
            minMax = "*" + min + ".." + max;
        }
        else if (min != null)
        {
            minMax = "*" + min; ;
        }
        return "[" + NeoVar + relationShip + minMax + "]";
    }
}

public class MatchPattern : MatchObject
{
    private List<MatchObject> elements = new();

    public MatchPattern(MatchObject n1, MatchObject e, MatchObject n2) : base(null, null)
    {
        elements.Add(n1);
        elements.Add(e);
        elements.Add(n2);
    }

    public override void Add(MatchObject obj)
    {
        elements.Add(obj);
    }

    public override string ToCypherMatchProperties()
    {
        // Return the combined pattern of nodes and relationships
        return string.Join("-", elements.Select(e => e.ToCypherMatchProperties()));
    }

    public override string ToCypherConditions()
    {
        // Combine all conditions with AND
        var conditions = elements
            .Select(e => e.ToCypherConditions())
            .Where(c => !string.IsNullOrWhiteSpace(c));

        return string.Join(" AND ", conditions);
    }
    public override string NeoVarToString()
    {
        var el = elements.Select(e => e is NeoNode ? e.NeoVarToString() : null).Where(c => !string.IsNullOrWhiteSpace(c));
        return string.Join(", ", el);
    }

}
