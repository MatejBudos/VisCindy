using System;
using System.IO;
using System.Collections.Generic;
using System.Text.Json;
using System.Diagnostics;
using UnityEngine;



public interface ICondition
{
    string ToQueryString(string nodeVar);
    bool isEmpty();
}

public class SimpleCondition : ICondition
{
    public string Attribute { get; }
    public string Operator { get; }
    public object Value { get; }

    public SimpleCondition(string attribute, string op, object value)
    {
        Attribute = attribute;
        Operator = op;
        Value = value;
    }

    public string ToQueryString(string NeoVar)
    {
        if (Value is string)
            return $"{NeoVar}.{Attribute} {Operator} \"{Value}\"";
        return $"{NeoVar}.{Attribute} {Operator} {Value}";
    }

    public bool isEmpty() => false;
}

public class CompositeCondition : ICondition
{
    public string LogicalOperator { get; }
    private readonly List<ICondition> _conditions = new();

    public CompositeCondition(string logicalOperator)
    {
        LogicalOperator = logicalOperator.ToUpper();
    }

    public void Add(ICondition condition) => _conditions.Add(condition);

    public string ToQueryString(string NeoVar)
    {
        return "(" + string.Join($" {LogicalOperator} ", _conditions.ConvertAll(c => c.ToQueryString(NeoVar))) + ")";
    }

    public bool isEmpty() => _conditions.Count == 0;
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

    public void CreateSimple(string tag, string attribute, string op, object value)
    {
        if (!_tagMap.ContainsKey(tag))
            _tagMap[tag] = new SimpleCondition(attribute, op, value);
    }

    public void CreateComposite(string tag, string logicalOperator)
    {
        if (!_tagMap.ContainsKey(tag))
            _tagMap[tag] = new CompositeCondition(logicalOperator);
    }

    public void AddToParent(string parentTag, string childTag)
    {
        if (!_tagMap.ContainsKey(parentTag))
            throw new ArgumentException($"Parent tag '{parentTag}' not found.");
        if (!_tagMap.ContainsKey(childTag))
            throw new ArgumentException($"Child tag '{childTag}' not found.");

        if (_tagMap[parentTag] is CompositeCondition parent)
            parent.Add(_tagMap[childTag]);
        else
            throw new InvalidOperationException($"Parent '{parentTag}' is not a composite condition.");
    }

    public void AddToRoot(string childTag) => AddToParent("root", childTag);

    public ICondition GetCondition(string tag) => _tagMap[tag];

    public Dictionary<string, ICondition> GetAllConditions() => _tagMap;

    public ICondition Build() => _root;
}

public static class ConditionParser
{
    public static ICondition LoadDataRows(List<RowData> conditions)
    {
        NormalizeParents(conditions);
        var childrenMap = BuildParentChildMap(conditions);

        var builder = new ConditionBuilder("OR");
        CreateAllSimpleConditions(conditions, builder);

        var memo = new Dictionary<string, string>();
        foreach (var cond in conditions)
        {
            if (cond.parent == "root")
            {
                var finalTag = BuildNode(cond.tag, childrenMap, builder, memo);
                builder.AddToRoot(finalTag);
            }
        }

        return builder.Build();
    }


    private static void NormalizeParents(List<RowData> conditions)
    {
        foreach (var cond in conditions)
            if (string.IsNullOrEmpty(cond.parent))
                cond.parent = "root";
    }

    private static Dictionary<string, List<RowData>> BuildParentChildMap(List<RowData> conditions)
    {
        var map = new Dictionary<string, List<RowData>>();
        foreach (var cond in conditions)
        {
            if (!map.ContainsKey(cond.parent))
                map[cond.parent] = new List<RowData>();
            map[cond.parent].Add(cond);
        }
        return map;
    }

    private static void CreateAllSimpleConditions(
        List<RowData> conditions,
        ConditionBuilder builder)
    {
        foreach (var cond in conditions)
            builder.CreateSimple(cond.tag, cond.attribute, cond.@operatorValue, cond.value);
    }

    private static string BuildNode(
        string tag,
        Dictionary<string, List<RowData>> childrenMap,
        ConditionBuilder builder,
        Dictionary<string, string> memo)
    {
        if (memo.ContainsKey(tag))
            return memo[tag];

        if (!childrenMap.ContainsKey(tag))
        {
            memo[tag] = tag;
            return tag;
        }

        var children = childrenMap[tag];
        var firstBuilt = BuildNode(children[0].tag, childrenMap, builder, memo);

        string composite0 = $"{tag}_cmp_0";
        builder.CreateComposite(composite0, children[0].logic);
        builder.AddToParent(composite0, tag);
        builder.AddToParent(composite0, firstBuilt);

        string current = composite0;

        for (int i = 1; i < children.Count; i++)
        {
            var child = children[i];
            var builtChild = BuildNode(child.tag, childrenMap, builder, memo);

            string compositeN = $"{tag}_cmp_{i}";
            builder.CreateComposite(compositeN, child.logic);
            builder.AddToParent(compositeN, current);
            builder.AddToParent(compositeN, builtChild);

            current = compositeN;
        }

        memo[tag] = current;
        return current;
    }
}

