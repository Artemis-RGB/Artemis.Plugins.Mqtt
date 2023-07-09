using System.Linq;
using Artemis.Core.Modules;

namespace Artemis.Plugins.Mqtt.DataModels;

public class MqttDataModel : DataModel
{
    public StatusesDataModel Statuses { get; } = new();

    public NodeDataModel Root { get; } = new(new());

    public MqttServersDataModel Servers { get; } = new();
}