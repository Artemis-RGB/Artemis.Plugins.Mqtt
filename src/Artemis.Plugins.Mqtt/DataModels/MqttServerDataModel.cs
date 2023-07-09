using System;
using System.Collections.Concurrent;
using Artemis.Core.Modules;
using Artemis.Plugins.Mqtt.DataModels.Dynamic;

namespace Artemis.Plugins.Mqtt.DataModels;

public class MqttServerDataModel : DataModel
{
    private readonly string _path;
    private readonly ConcurrentDictionary<string, DynamicChild> _allDynamicChildren;

    public MqttServerDataModel(string path, ConcurrentDictionary<string, DynamicChild> dict)
    {
        _path = path;
        _allDynamicChildren = dict;
    }

    public void PropagateValue(string topic, object data)
    {
        var parts = topic.Split('/');
        if (parts.Length == 1)
        {
            var id = $"{_path}_{topic}";
            if (!_allDynamicChildren.TryGetValue(id, out var dynamicChild))
            {
                dynamicChild = AddDynamicChild(id, data.ToString());
                _allDynamicChildren.TryAdd(id, dynamicChild);
            }

            if (dynamicChild is DynamicChild<string> stringChild)
            {
                stringChild.Value = data.ToString();
            }
        }
        else
        {
           var remainingTopic = string.Join('/', parts[1..]);
           var childDataModel = AddDynamicChild(parts[1], new MqttServerDataModel(remainingTopic, _allDynamicChildren), parts[1]);
           childDataModel.Value.PropagateValue(remainingTopic, data);
        }
    }
}