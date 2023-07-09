using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Artemis.Core.Modules;
using Swan;

namespace Artemis.Plugins.Mqtt.DataModels;

public class MqttNodeDataModel : DataModel
{
    private readonly string _path;
    
    public string Data { get; set; }

    public MqttNodeDataModel(string path)
    {
        _path = path;
        Data = "";
    }

    public void PropagateValue(string partialTopic, object data)
    {
        //if our path is the same as the partialTopic, we are the end of the line, update our own value
        if (partialTopic == _path)
        {
            //if we have a slash in the path, we can't have the same path. something went wrong.
            Debug.Assert(!partialTopic.Contains('/'));
            Data = data.ToString();
            return;
        }
        
        var parts = partialTopic.Split('/');

        if (parts.Length == 1)
        {
            //if we don't have a slash in the partialTopic, we don't have children.
            if (!TryGetDynamicChild(partialTopic, out DynamicChild<MqttNodeDataModel>? child))
                child = AddDynamicChild(partialTopic, new MqttNodeDataModel(partialTopic));
        
            child.Value.PropagateValue(partialTopic, data);
        }
        else
        {
            //if we have a slash in the partialTopic, we have children.

            var childTopic = parts[0];
            var remainingTopic = string.Join("/", parts[1..]);
            
            if (!TryGetDynamicChild(childTopic, out DynamicChild<MqttNodeDataModel>? child))
                child = AddDynamicChild(childTopic, new MqttNodeDataModel(childTopic));
        
            child.Value.PropagateValue(remainingTopic, data);
        }
    }
    
    public void PropagateValue(string[] topicParts, object data)
    {
        var self = topicParts[0];
        var rest = topicParts[1..];

        if (rest.Length == 0)
        {
            if (self == _path)
            {
                Data = data.ToString();

            }
            else
            {
                if (!TryGetDynamicChild<MqttNodeDataModel>(self, out var child))
                    child = AddDynamicChild(self, new MqttNodeDataModel(self));
                
                child.Value.PropagateValue(new string[] {self}, data);
            }
        }
        else
        {
            var childKey = rest[0];
            if (!TryGetDynamicChild<MqttNodeDataModel>(childKey, out var child))
                child = AddDynamicChild(childKey, new MqttNodeDataModel(childKey));
            
            child.Value.PropagateValue(rest, data);
        }
    }
}