using System;
using System.Collections.Generic;
using Artemis.Core.Modules;

namespace Artemis.Plugins.Mqtt.DataModels;

public class MqttServersDataModel : DataModel
{
    private readonly Dictionary<Guid, DynamicChild<MqttServerDataModel>> _servers = new();

    internal void CreateServers(IEnumerable<MqttConnectionSettings> servers)
    {
        ClearDynamicChildren();
        _servers.Clear();

        foreach (var server in servers)
        {
            var id = server.ServerId.ToString();
            _servers.Add(
                server.ServerId,
                AddDynamicChild(id, new MqttServerDataModel(id), server.DisplayName)
            );
        }
    }

    public void PropagateValue(Guid sourceServer, string topic, object data)
    {
        if (!_servers.TryGetValue(sourceServer, out var server))
            return;

        server.Value.PropagateValue(topic, data);
    }
}