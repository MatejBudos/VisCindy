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
    private readonly Dictionary<string, ICondition> _tagMap = new();

    public ConditionBuilder(string rootLogicalOperator)
    {
        _root = new CompositeCondition(rootLogicalOperator);
        _tagMap["root"] = _root;
    }

    public void CreateSimple(string tag, string field, string op, object value)
    {
        _tagMap[tag] = new SimpleCondition(field, op, value);
    }

    public void CreateComposite(string tag, string logicalOperator)
    {
        _tagMap[tag] = new CompositeCondition(logicalOperator);
    }

    public void AddToParent(string parentTag, string childTag)
    {
        if (!_tagMap.ContainsKey(parentTag))
            throw new ArgumentException($"Parent tag '{parentTag}' not found.");
        if (!_tagMap.ContainsKey(childTag))
            throw new ArgumentException($"Child tag '{childTag}' not found.");

        if (_tagMap[parentTag] is CompositeCondition parent)
        {
            parent.Add(_tagMap[childTag]);
        }
        else
        {
            throw new InvalidOperationException($"Parent '{parentTag}' is not a composite condition.");
        }
    }

    
    public void AddToRoot(string childTag)
    {
        AddToParent("root", childTag);
    }

    public String Build(string label)
    {
        return _root.ToQueryString(label);
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

    public string ToQueryString(string NeoVar)
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

    public CompositeCondition(string logicalOperator )
    {
        LogicalOperator = logicalOperator.ToUpper();
    }

    public void Add(ICondition condition)
    {
        _conditions.Add(condition);
    }

    public string ToQueryString( string NeoVar)
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
        var builder = new ConditionBuilder("OR");

        // Create all nodes
        builder.CreateSimple("ageCond", "age", ">", 30);
        builder.CreateSimple("name1", "name", "=", "Jane");
        builder.CreateSimple("name2", "name", "=", "John");
        builder.CreateSimple("activeCond", "active", "=", true);

        builder.CreateComposite("or1", "OR");
        builder.CreateComposite("and1", "AND");

        // Build nested structure
        builder.AddToParent("or1", "name1");
        builder.AddToParent("or1", "name2");

        builder.AddToParent("and1", "ageCond");
        builder.AddToParent("and1", "or1");

        // Attach to root
        builder.AddToRoot("and1");
        builder.AddToRoot("activeCond");

        // Print final condition
        var final = builder.Build("a");
        Console.WriteLine(final);

        //------------------------------------------

        var builder1 = new ConditionBuilder("OR");

        builder1.CreateSimple("age", "age", ">", "30");

        builder1.AddToRoot("age");

        Console.WriteLine(builder1.Build("peter"));

    }
}
