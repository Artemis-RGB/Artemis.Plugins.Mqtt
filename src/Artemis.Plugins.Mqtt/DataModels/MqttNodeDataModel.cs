using System;
using System.Collections.Concurrent;
using Artemis.Core.Modules;

namespace Artemis.Plugins.Mqtt.DataModels;

public class MqttNodeDataModel : DataModel
{
    private readonly string _path;

    public MqttNodeDataModel(string path)
    {
        _path = path;
    }

    public void PropagateValue(string totalTopic, object data)
    {
        var parts = totalTopic.Split('/');
        var isFolder = parts.Length > 1;
        
        var thisTopic = parts.Length == 0 ? totalTopic : parts[0];
        var remainingTopic = isFolder ? string.Join('/', parts[1..]) : null;

        if (isFolder)
        {
            //if we have a DynamicChild<string> with the same name, remove it
            if (TryGetDynamicChild(thisTopic, out DynamicChild<string>? sdm))
                RemoveDynamicChild(sdm);
            
            if (!TryGetDynamicChild(thisTopic, out DynamicChild<MqttNodeDataModel>? dm))
                dm = AddDynamicChild(thisTopic, new MqttNodeDataModel(thisTopic));

            dm.Value.PropagateValue(remainingTopic!, data);
        }
        else
        {
            if (!TryGetDynamicChild(thisTopic, out DynamicChild<string>? dm))
                dm = AddDynamicChild(thisTopic, data.ToString())!;

            dm.Value = data.ToString()!;
        }
    }
}