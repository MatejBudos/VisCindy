using System;
using System.Collections.Generic;
using System.Linq;
public interface ICondition
{
    string ToQueryString( string nodeVar );
    public bool isEmpty();
}


public class SimpleCondition : ICondition
{
    public string Field { get; }
    public string Operator { get; }
    public object Value { get; }

    public SimpleCondition(string field, string op, object value)
    {
        Field = field;
        Operator = op;
        Value = value;
    }

   public string ToQueryString(string NeoVar = "a")
    {
        string formattedValue = Value is string 
            ? $"\"{Value}\"" 
            : Value.ToString();

        return $"{NeoVar}.{Field} {Operator} {formattedValue}";
    }
    public bool isEmpty(){
        return false;
    }
}

public class CompositeCondition : ICondition
{
    public string LogicalOperator { get; } // "AND" or "OR"
    private readonly List<ICondition> _conditions = new();

    public CompositeCondition(string logicalOperator)
    {
        LogicalOperator = logicalOperator.ToUpper();
    }

    public void Add(ICondition condition)
    {
        _conditions.Add(condition);
    }

    public string ToQueryString( string NeoVar = "a" )
    {
        return "(" + string.Join($" {LogicalOperator} ", _conditions.Select(c => c.ToQueryString( NeoVar ))) + ")";
    }
    public bool isEmpty(){
        return _conditions.Count == 0;
    }
}
