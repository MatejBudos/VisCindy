using System;
using System.Collections.Generic;
using System.Linq;
public interface ICondition
{
    string ToQueryString( string nodeVar );
    public bool isEmpty();
}

public class ConditionBuilder
{
    private readonly CompositeCondition _root;
    private readonly Dictionary<string, CompositeCondition> _tagMap = new();

    public ConditionBuilder(string rootLogicalOperator)
    {
        _root = new CompositeCondition(rootLogicalOperator);
        _tagMap["root"] = _root;
    }

    public void AddChild(string tagName, string logicalOperator)
    {
        var composite = new CompositeCondition(logicalOperator);
        _tagMap[tagName] = composite;
    }

    public void AddToParent(string parentTag, ICondition child)
    {
        if (!_tagMap.ContainsKey(parentTag))
            throw new ArgumentException($"Parent tag '{parentTag}' not found.");

        _tagMap[parentTag].Add(child);
    }

    public CompositeCondition GetComposite(string tagName)
    {
        if (!_tagMap.ContainsKey(tagName))
            throw new ArgumentException($"Tag '{tagName}' not found.");

        return _tagMap[tagName];
    }

    public ICondition Build()
    {
        return _root;
    }
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
        if (Value is string)
            return $"{NeoVar}.{Field} {Operator} \"{Value}\"";

        //if (Value is NeoVar || Value is int)
        return $"{NeoVar}.{Field} {Operator} {Value.ToString()}";
    }
    public bool isEmpty()
    {
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


class Program
{
    static void Main()
    {
        // var builder = new ConditionBuilder("OR"); // root composite

        // // Create nested composites
        // builder.AddChild("and1", "AND");
        // builder.AddChild("or1", "OR");

        // // Add leaves to or1: (name = Jane OR name = John)
        // builder.AddToParent("or1", new SimpleCondition("name", "=", "Jane"));
        // builder.AddToParent("or1", new SimpleCondition("name", "=", "John"));

        // // Add age condition to and1
        // builder.AddToParent("and1", new SimpleCondition("age", ">", 30));

        // // Add or1 (name conditions) to and1
        // builder.AddToParent("and1", builder.GetComposite("or1"));

        // // Add and1 to root
        // builder.AddToParent("root", builder.GetComposite("and1"));

        // // Add active condition to root
        // builder.AddToParent("root", new SimpleCondition("a.active", "=", true));

        // // Now build the full condition tree and print it
        // var rootCondition = builder.Build();
        // Console.WriteLine(rootCondition.ToQueryString("a"));

        var builder = new ConditionBuilder("OR"); // root tag (not used here, but required)

        var simple1 = new SimpleCondition("age", "<", 18);
        // var simple2 = new SimpleCondition("age", ">", 70);

        
        builder.AddToParent("root", simple1);
        // builder.AddToParent("root", simple2);

        var rootCondition = builder.Build();
        Console.WriteLine(rootCondition.ToQueryString("a"));

    }
}
