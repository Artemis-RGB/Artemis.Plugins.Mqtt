using System;
using System.Collections.Concurrent;
using Artemis.Core.Modules;

namespace Artemis.Plugins.Mqtt.DataModels;

public class NodeDataModel : DataModel
{
    private readonly ConcurrentDictionary<string, DynamicChild> _allDynamicChildren;

    public NodeDataModel(ConcurrentDictionary<string, DynamicChild> cache)
    {
        _allDynamicChildren = cache;
    }
    internal void CreateStructure(StructureDefinitionNode dataModelStructure)
    {
        ClearDynamicChildren();
        _allDynamicChildren.Clear();
        
        if (dataModelStructure.Children == null)
            return;
        
        foreach (var childDefinition in dataModelStructure.Children)
        {
            var id = GetNodeId(childDefinition.Server ?? Guid.NewGuid(), childDefinition.Topic);
            DynamicChild dynamicChild;
            if (childDefinition.Type == null)
            {
                var folderNode = new NodeDataModel(_allDynamicChildren);
                folderNode.CreateStructure(childDefinition);
                dynamicChild = AddDynamicChild(id, folderNode, childDefinition.Label);
            }
            else
            {
                var type = childDefinition.Type;
                dynamicChild = type switch
                {
                    _ when type == typeof(bool) => AddDynamicChild<bool>(id, false, childDefinition.Label),
                    _ when type == typeof(int) => AddDynamicChild<int>(id, 0, childDefinition.Label),
                    _ when type == typeof(double) => AddDynamicChild<double>(id, 0, childDefinition.Label),
                    _ when type == typeof(string) => AddDynamicChild<string>(id, "", childDefinition.Label),
                    _ => throw new ArgumentOutOfRangeException(nameof(type), type, "Unsupported type")
                };
            }
            
            _allDynamicChildren.TryAdd(id, dynamicChild);
        }
    }

    private static string GetNodeId(Guid serverGuid, string topic)
    {
        return $"{serverGuid}_{topic}";
    }

    public void PropagateValue(Guid sourceServer, string topic, object data)
    {
        if (!_allDynamicChildren.TryGetValue(GetNodeId(sourceServer, topic), out var dynamicChild)) 
            return;
        
        switch (dynamicChild)
        {
            case DynamicChild<int> intChild:
                intChild.Value = int.TryParse(data.ToString(), out var intValue) ? intValue : 0;
                return;
            case DynamicChild<double> doubleChild:
                doubleChild.Value =  double.TryParse(data.ToString(), out var doubleValue) ? doubleValue : 0;
                return;
            case DynamicChild<bool> boolChild:
                boolChild.Value = string.Compare(data.ToString(), "true", StringComparison.OrdinalIgnoreCase) == 0;
                return;
            case DynamicChild<string> stringChild:
                stringChild.Value = data.ToString()!;
                return;
        }
    }
}