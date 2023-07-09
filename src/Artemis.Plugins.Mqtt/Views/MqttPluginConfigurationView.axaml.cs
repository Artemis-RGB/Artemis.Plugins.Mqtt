using Artemis.Plugins.Mqtt.ViewModels;
using Avalonia.ReactiveUI;

namespace Artemis.Plugins.Mqtt.Views;

public partial class MqttPluginConfigurationView : ReactiveUserControl<MqttPluginConfigurationViewModel>
{
    public MqttPluginConfigurationView()
    {
        InitializeComponent();
    }
}