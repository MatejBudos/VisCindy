using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
public class CypherQueryBuilder
{

    //TODO: pridat edges do buildovania
    private List<MatchObject> MatchNodes = new List<MatchObject>();
    private Traversal _traversal;
    //private List<string> _matchClauses = new();
    private List<string> finalWhereCondition = new List<string>();
    private List<string> _returnClauses = new();
    private QueryReturnStrategy _queryReturnStrategy = new NodeReturnStrategy();

    public CypherQueryBuilder SetNeoNode(MatchObject node)
    {
        MatchNodes.Add(node);
        return this;
    }
    public CypherQueryBuilder SetNeoNode(List<MatchObject> nodes)
    {
        foreach (var node in nodes){
            MatchNodes.Add(node);
        }
        return this;
    }

    //mozno by sa same mohlo volat
    public CypherQueryBuilder AddWhereCondition()
    {
        foreach( MatchObject node in MatchNodes ){
            if (node?.attributes != null && !node.attributes.isEmpty())
                finalWhereCondition.Add(node.ToCypherConditions());
        }
        return this;
       
    }

    public CypherQueryBuilder SetTraversal(Traversal traversal)
    {
        _traversal = traversal;
        SetReturnStrategy(new TraversalReturnStrategy());
        return this;
    }


    public CypherQueryBuilder SetReturnStrategy( QueryReturnStrategy strategy )
    {
        this._queryReturnStrategy = strategy;
        return this;
    }

    public string Build()
    {
        List<string> queryParts = new List<string>();
        queryParts.Add("MATCH");
        string matchNodesString = string.Join(",", MatchNodes.Select(node => node.ToCypherMatchProperties()));
        queryParts.Add(matchNodesString);


        if( finalWhereCondition.Count != 0 )
            queryParts.Add("WHERE");
            queryParts.Add( string.Join(" AND ",finalWhereCondition) );

        if (_traversal != null)
            queryParts.Add(_traversal.BuildQuery());
            
        
        if (_queryReturnStrategy != null)
            queryParts.Add(_queryReturnStrategy.ReturnStrategy(MatchNodes).Get());

        return string.Join("\n", queryParts);
    }
}
