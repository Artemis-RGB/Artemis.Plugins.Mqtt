using System;
using System.Collections.Concurrent;
using Artemis.Core.Modules;
using Artemis.Plugins.Mqtt.DataModels.Dynamic;

namespace Artemis.Plugins.Mqtt.DataModels;

public class MqttServerDataModel : DataModel
{
    private readonly Guid _serverGuid;
    private readonly ConcurrentDictionary<string, DynamicChild> _allDynamicChildren;

    public MqttServerDataModel(Guid serverGuid, ConcurrentDictionary<string, DynamicChild> dict)
    {
        _serverGuid = serverGuid;
        _allDynamicChildren = dict;
    }

    private static string GetNodeId(Guid serverGuid, string topic)
    {
        return $"{serverGuid}_{topic}";
    }

    public void PropagateValue(string topic, object data)
    {
        var id = GetNodeId(_serverGuid, topic);
        if (!_allDynamicChildren.TryGetValue(GetNodeId(default, topic), out var dynamicChild))
        {
            dynamicChild = AddDynamicChild(id, data.ToString());
            _allDynamicChildren.TryAdd(id, dynamicChild);
        }

        if (dynamicChild is DynamicChild<string> stringChild)
        {
            stringChild.Value = data.ToString();
        }
    }
}