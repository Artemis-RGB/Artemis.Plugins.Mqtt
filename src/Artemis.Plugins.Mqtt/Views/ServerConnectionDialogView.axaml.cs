using Artemis.Plugins.Mqtt.ViewModels;
using Artemis.UI.Shared;

namespace Artemis.Plugins.Mqtt.Views;

public partial class ServerConnectionDialogView : ReactiveAppWindow<ServerConnectionDialogViewModel>
{
    public ServerConnectionDialogView()
    {
        InitializeComponent();
    }
}