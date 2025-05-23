using System.Dynamic;
using System.IO.Pipelines;
//toto funguje ten return ale ten strat pattern tu nie je ok, a conditions sa nespracovavaju do buducej query
public class ReturnObject
{
    public string returnStrat;
    public ICondition chainCondition;
    public ReturnObject(string returnStrat)
    {
        this.returnStrat = returnStrat;
    }
    public ReturnObject(string returnStrat, ICondition chainCondition)
    {
        this.returnStrat = returnStrat;
        this.chainCondition = chainCondition;
    }
    public string Get()
    {
        return returnStrat;
    }
}


public abstract class QueryReturnStrategy
{

    public abstract ReturnObject ReturnStrategy(List<MatchObject> nodes, Traversal traversal);
    public abstract ReturnObject ChainingStrategy(List<MatchObject> nodes, Traversal traversal);
}

public class TraversalReturnStrategy : QueryReturnStrategy
{
    public override ReturnObject ReturnStrategy(List<MatchObject> nodes, Traversal traversal)
    {
        string result = "RETURN path, [n IN nodes(path) | elementID(n) ] AS NeoIds";
        ReturnObject returnObject= new ReturnObject( result);
        return returnObject;
    }
    public override ReturnObject ChainingStrategy(List<MatchObject> nodes, Traversal traversal)
    {
        string result = "WITH  [n IN nodes(path) ] as nodes";
        ReturnObject returnObject= new ReturnObject( result, new SimpleCondition("", "in", new NeoVar("nodes")));
        return returnObject;
    }
}

public class NodeReturnStrategy : QueryReturnStrategy
{
    public override ReturnObject ReturnStrategy(List<MatchObject> nodes, Traversal traversal)
    {
        
        string result = "RETURN " + string.Join( ", ", nodes.Select( n => n.NeoVarToString() ) );
        
        return new ReturnObject( result );
    }
    public override ReturnObject ChainingStrategy(List<MatchObject> nodes, Traversal traversal)
    {
        string result = "WITH " + string.Join( " + ", nodes.Select( n => "collect(" + n.NeoVarToString() + ")" ) ) + "AS nodes";
        ReturnObject returnObject = new ReturnObject(result, new SimpleCondition("", "in", new NeoVar("nodes")));
        return returnObject;

    }
}
