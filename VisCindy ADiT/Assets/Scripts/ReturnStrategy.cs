using System.Collections.Generic;
using System.Dynamic;
using System.Linq;

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
      
        string result = @"
            WITH
            Id(v0) AS id,
            elementId(v0) as NeoId,
            collect(CASE
                WHEN v1 IS NOT NULL THEN {source: Id(v0), target: Id(v1), relationship: type(e1), NeoId: elementId(e1)}
                ELSE null
            END) AS edges
          RETURN
            id, NeoId,
            [edge IN edges WHERE edge IS NOT NULL] AS edges;";
        
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
