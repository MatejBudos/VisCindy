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
    public void setCondiction(ICondition condition)
    {
        this.chainCondition = condition;
    }
}


public interface QueryReturnStrategy
{

    public ReturnObject ReturnStrategy(List<MatchObject> nodes);
    public ReturnObject ChainingStrategy(List<MatchObject> nodes);
}

public class TraversalReturnStrategy : QueryReturnStrategy
{
    public ReturnObject ReturnStrategy(List<MatchObject> nodes)
    {
        string result = "RETURN path, [n IN nodes(path) | elementID(n) ] AS NeoIds";
        ReturnObject returnObject= new ReturnObject( result);
        return returnObject;
    }
    public ReturnObject ChainingStrategy(List<MatchObject> nodes)
    {
        string result = "WITH  [n IN nodes(path) ] as nodes";
        ReturnObject returnObject= new ReturnObject( result, new SimpleCondition("", "in", new NeoVar("nodes")));
        return returnObject;
    }
}

public class NodeReturnStrategy : QueryReturnStrategy
{
    public ReturnObject ReturnStrategy(List<MatchObject> nodes)
    {
        
        string result = "RETURN " + string.Join( ", ", nodes.Select( n => n.NeoVarToString() ) );
        
        return new ReturnObject( result );
    }
    //pre match patterns nefunguje
    public ReturnObject ChainingStrategy(List<MatchObject> nodes)
    {
        string result = "WITH " + string.Join(" + ", nodes.Select(n => "collect(" + n.NeoVarToString() + ")")) + "AS nodes";

        ReturnObject returnObject = new ReturnObject(result, new SimpleCondition("", "in", new NeoVar("nodes")));

        /*
        ReturnObject returnObject = new ReturnObject(result);
        CompositeCondition condition = new CompositeCondition("AND");
        foreach (MatchObject obj in nodes)
        {
            //asi pridat aj match pattern
            if (obj is NeoNode)
            {
                condition.Add( new SimpleCondition( obj.NeoVarToString(), "in", new NeoVar("nodes")));
            }
        }
        returnObject.setCondiction(condition);
        */
        return returnObject;

    }
}
