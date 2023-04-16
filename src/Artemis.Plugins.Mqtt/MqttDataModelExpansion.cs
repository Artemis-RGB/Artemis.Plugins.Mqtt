﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Plugins.Mqtt.DataModels;
using Artemis.Plugins.Mqtt.DataModels.Dynamic;
using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;

namespace Artemis.Plugins.Mqtt;

public class MqttDataModelExpansion : Module<RootDataModel>
{
    private readonly List<MqttConnector> connectors = new();
    private readonly PluginSetting<StructureDefinitionNode> dynamicDataModelStructureSetting;

    private readonly PluginSetting<List<MqttConnectionSettings>> serverConnectionsSetting;

    public MqttDataModelExpansion(PluginSettings settings)
    {
        serverConnectionsSetting = settings.GetSetting("ServerConnections", new List<MqttConnectionSettings>());
        serverConnectionsSetting.PropertyChanged += OnSeverConnectionListChanged;

        dynamicDataModelStructureSetting = settings.GetSetting("DynamicDataModelStructure", StructureDefinitionNode.RootDefault);
        dynamicDataModelStructureSetting.PropertyChanged += OnDataModelStructureChanged;
    }

    public override List<IModuleActivationRequirement> ActivationRequirements { get; } = new();

    public override async void Enable()
    {
        DataModel.UpdateDataModel(dynamicDataModelStructureSetting.Value);
        DataModel.Statuses.UpdateConnectorList(serverConnectionsSetting.Value);
        await RestartConnectors();
    }

    public override async void Disable()
    {
        await StopConnectors();
    }

    public override void Update(double deltaTime)
    {
    }


    private Task RestartConnectors()
    {
        // Resize connectors to match number of setup servers
        // - Remove extraneous connectors if there are more connectors than there are server connections
        if (connectors.Count > serverConnectionsSetting.Value.Count)
        {
            var amountToRemove = connectors.Count - serverConnectionsSetting.Value.Count;
            foreach (var connector in connectors.Take(amountToRemove))
            {
                connector.MessageReceived -= OnMqttClientMessageReceived;
                connector.Connected -= OnMqttClientConnected;
                connector.Disconnected -= OnMqttClientDisconnected;
                connector.Dispose();
            }

            connectors.RemoveRange(0, amountToRemove);
        }

        // - Add new connectors if there are less connectors than there are server connections
        else if (connectors.Count < serverConnectionsSetting.Value.Count)
        {
            for (var i = connectors.Count; i < serverConnectionsSetting.Value.Count; i++)
            {
                var connector = new MqttConnector();
                connector.MessageReceived += OnMqttClientMessageReceived;
                connector.Connected += OnMqttClientConnected;
                connector.Disconnected += OnMqttClientDisconnected;
                connectors.Add(connector);
            }
        }


        // Calculate which topics should be listened to by which servers
        var serverTopicMap = new Dictionary<Guid?, HashSet<string>>();
        var nodesToSearch = new Queue<StructureDefinitionNode>();
        nodesToSearch.Enqueue(dynamicDataModelStructureSetting.Value);

        foreach (var server in serverConnectionsSetting.Value)
            serverTopicMap.Add(server.ServerId, new HashSet<string>());

        // - Not implemented as a recursive function because it then becomes a lot of hassle to merge dictionaries.
        while (nodesToSearch.TryDequeue(out var current))
            if (current.Children == null)
            {
                if (current.Server == null) // If null, listens to any server
                    foreach (var server in serverConnectionsSetting.Value)
                        serverTopicMap[server.ServerId].Add(current.Topic);
                else
                    serverTopicMap[current.Server].Add(current.Topic);
            }
            else
            {
                foreach (var child in current.Children)
                    nodesToSearch.Enqueue(child);
            }


        // Start each connector with relevant settings
        return Task.WhenAll(
            connectors.Select((connector, i) =>
                connector.Start(serverConnectionsSetting.Value[i], serverTopicMap[serverConnectionsSetting.Value[i].ServerId]))
        );
    }

    private Task StopConnectors()
    {
        return Task.WhenAll(
            connectors.Select(connector => connector.Stop())
        );
    }

    private void OnMqttClientMessageReceived(object sender, MqttApplicationMessageReceivedEventArgs e)
    {
        // Pass incoming messages to the root DataModel's HandleMessage method.
        DataModel.HandleMessage((sender as MqttConnector).ServerId, e.ApplicationMessage.Topic, e.ApplicationMessage.ConvertPayloadToString());
    }

    private void OnMqttClientConnected(object sender, MqttClientConnectedEventArgs e)
    {
        DataModel.Statuses[(sender as MqttConnector).ServerId].IsConnected = true;
    }

    private void OnMqttClientDisconnected(object sender, MqttClientDisconnectedEventArgs e)
    {
        DataModel.Statuses[(sender as MqttConnector).ServerId].IsConnected = false;
    }

    private void OnSeverConnectionListChanged(object sender, PropertyChangedEventArgs e)
    {
        RestartConnectors();
        DataModel.Statuses.UpdateConnectorList(serverConnectionsSetting.Value);
    }

    private async void OnDataModelStructureChanged(object sender, PropertyChangedEventArgs e)
    {
        // Rebuild the Artemis Data Model with the new structure
        DataModel.UpdateDataModel(dynamicDataModelStructureSetting.Value);

        // Restart the Mqtt client incase it needs to change which topics it's subscribed to
        await RestartConnectors();
    }

    protected override void Dispose(bool disposing)
    {
        connectors.ForEach(connector =>
        {
            connector.MessageReceived -= OnMqttClientMessageReceived;
            connector.Dispose();
        });
        connectors.Clear();
    }
}