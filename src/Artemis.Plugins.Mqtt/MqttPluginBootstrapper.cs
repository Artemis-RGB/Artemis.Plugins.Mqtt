using Artemis.Core;
using Artemis.Plugins.Mqtt.ViewModels;
using Artemis.UI.Shared;

namespace Artemis.Plugins.Mqtt;

public class MqttPluginBootstrapper : PluginBootstrapper
{
    public override void OnPluginEnabled(Plugin plugin)
    {
        plugin.ConfigurationDialog = new PluginConfigurationDialog<MqttPluginConfigurationViewModel>();
    }
}