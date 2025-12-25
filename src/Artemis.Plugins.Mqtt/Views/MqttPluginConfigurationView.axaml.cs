using Artemis.Plugins.Mqtt.ViewModels;
using ReactiveUI.Avalonia;

namespace Artemis.Plugins.Mqtt.Views;

public partial class MqttPluginConfigurationView : ReactiveUserControl<MqttPluginConfigurationViewModel>
{
    public MqttPluginConfigurationView()
    {
        InitializeComponent();
    }
}