using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Artemis.Core.Modules;
using Swan;

namespace Artemis.Plugins.Mqtt.DataModels;

public class MqttNodeDataModel : DataModel
{
    public string Data { get; set; }

    public MqttNodeDataModel()
    {
        Data = "";
    }

    public void PropagateValue(string[] topics, object data)
    {
        if (topics.Length == 0)
        {
            Data = data.ToString() ?? "";
            return;
        }

        var key = topics[0];
        var remainingPartialTopics = topics[1..]; 
        if (!TryGetDynamicChild<MqttNodeDataModel>(key, out var child))
            child = AddDynamicChild<MqttNodeDataModel>(key, new());
            
        child.Value.PropagateValue(remainingPartialTopics, data);
    }
}