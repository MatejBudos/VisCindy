using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;

public interface CypherQueryBuilder
{
    public CypherQueryBuilder SetNeoNode(MatchObject node);
    public CypherQueryBuilder SetNeoNode(List<MatchObject> nodes);
    public CypherQueryBuilder AddWhereCondition();
    public CypherQueryBuilder SetTraversal(Traversal traversal);
    public CypherQueryBuilder Chaining();
    public CypherQueryBuilder Returning();
    public string Build();

}

public class NodeQueryBuilder : CypherQueryBuilder
{
    private List<MatchObject> MatchNodes = new List<MatchObject>();
    private List<string> finalWhereCondition = new List<string>();
    private QueryReturnStrategy _queryReturnStrategy = new NodeReturnStrategy();
    private bool isChaining = false;
    public CypherQueryBuilder SetNeoNode(MatchObject node)
    {
        MatchNodes.Add(node);
        return this;
    }
    public CypherQueryBuilder SetNeoNode(List<MatchObject> nodes)
    {
        foreach (var node in nodes)
        {
            MatchNodes.Add(node);
        }
        return this;
    }
    //mozno by sa same mohlo volat
    public CypherQueryBuilder AddWhereCondition()
    {
        foreach (MatchObject node in MatchNodes)
        {
            if (node.hasAttributes())
            {
                finalWhereCondition.Add(node.ToCypherConditions());
            }
                
        }
        return this;
    }
    public CypherQueryBuilder SetTraversal(Traversal traversal)
    {
        return this;
    }
    public CypherQueryBuilder Chaining()
    {
        this.isChaining = true;
        return this;

    }
    public CypherQueryBuilder Returning()
    {
        this.isChaining = false;
        return this;

    }
    public string Build()
    {
        List<string> queryParts = new List<string>();
        queryParts.Add("MATCH");
        string matchNodesString = string.Join(",", MatchNodes.Select(node => node.ToCypherMatchProperties()));
        queryParts.Add(matchNodesString);

        if (finalWhereCondition.Count != 0)
        {
            queryParts.Add("WHERE");
            queryParts.Add(string.Join(" AND ", finalWhereCondition));
        }
           

        if (isChaining == true)
            queryParts.Add(_queryReturnStrategy.ChainingStrategy(MatchNodes).Get());
        else
            queryParts.Add(_queryReturnStrategy.ReturnStrategy(MatchNodes).Get());
        return string.Join("\n", queryParts);
    }
}

public class TraversalQueryBuilder : CypherQueryBuilder
{
    //TODO: pridat edges do buildovania
    private List<MatchObject> MatchNodes = new List<MatchObject>();
    private Traversal _traversal;
    //private List<string> _matchClauses = new();
    private List<string> finalWhereCondition = new List<string>();
    private QueryReturnStrategy _queryReturnStrategy = new TraversalReturnStrategy();
    private bool isChaining = false;
    public CypherQueryBuilder SetNeoNode(MatchObject node)
    {
        MatchNodes.Add(node);
        return this;
    }
    public CypherQueryBuilder SetNeoNode(List<MatchObject> nodes)
    {
        foreach (var node in nodes)
        {
            MatchNodes.Add(node);
        }
        return this;
    }
    //mozno by sa same mohlo volat
    public CypherQueryBuilder AddWhereCondition()
    {
        foreach (MatchObject node in MatchNodes)
        {
            if (node.hasAttributes())
                finalWhereCondition.Add(node.ToCypherConditions());
        }
        return this;
    }
    public CypherQueryBuilder SetTraversal(Traversal traversal)
    {
        _traversal = traversal;
        return this;
    }
    public CypherQueryBuilder Chaining()
    {
        this.isChaining = true;
        return this;
    }
    public CypherQueryBuilder Returning()
    {
        this.isChaining = false;
        return this;
    }
    public string Build()
    {
        List<string> queryParts = new List<string>();
        queryParts.Add("MATCH");
        string matchNodesString = string.Join(",", MatchNodes.Select(node => node.ToCypherMatchProperties()));
        queryParts.Add(matchNodesString);
        if (finalWhereCondition.Count != 0)
        {
            queryParts.Add("WHERE");
            queryParts.Add(string.Join(" AND ", finalWhereCondition));
        }
          

        queryParts.Add(_traversal.BuildQuery());
        if (isChaining == true)
            queryParts.Add(_queryReturnStrategy.ChainingStrategy(MatchNodes).Get());
        else
            queryParts.Add(_queryReturnStrategy.ReturnStrategy(MatchNodes).Get());

        return string.Join("\n", queryParts);
    }
}

