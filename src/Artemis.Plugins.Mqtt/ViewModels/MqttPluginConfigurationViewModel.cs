using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using Artemis.Core;
using Artemis.Plugins.Mqtt.DataModels;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using ReactiveUI;

namespace Artemis.Plugins.Mqtt.ViewModels;

/// <summary>
///     ViewModel for the main MQTT plugin configuration view.
/// </summary>
public class MqttPluginConfigurationViewModel : PluginConfigurationViewModel
{
    private readonly IWindowService _windowService;
    private readonly PluginSetting<StructureDefinitionNode> _dynamicDataModelStructureSetting;

    private readonly PluginSetting<List<MqttConnectionSettings>> _serverConnectionsSetting;

    public MqttPluginConfigurationViewModel(Plugin plugin, PluginSettings settings, IWindowService windowService) : base(plugin)
    {
        _windowService = windowService;

        _serverConnectionsSetting = settings.GetSetting("ServerConnections", new List<MqttConnectionSettings>());
        ServerConnections = new ObservableCollection<MqttConnectionSettings>(_serverConnectionsSetting.Value!);

        _dynamicDataModelStructureSetting = settings.GetSetting("DynamicDataModelStructure", StructureDefinitionNode.RootDefault);
        DynamicDataModelStructureRoot = new StructureNodeViewModel(windowService, null, _dynamicDataModelStructureSetting.Value!);
        
        AddServerConnection = ReactiveCommand.Create(ExecuteAddServerConnection);
        EditServerConnection = ReactiveCommand.Create<MqttConnectionSettings>(ExecuteEditServerConnection);
        DeleteServerConnection = ReactiveCommand.Create<MqttConnectionSettings>(ExecuteDeleteServerConnection);
    }

    public ObservableCollection<MqttConnectionSettings> ServerConnections { get; }
    public StructureNodeViewModel DynamicDataModelStructureRoot { get; }
    
    public ReactiveCommand<Unit, Unit> AddServerConnection { get; }
    public ReactiveCommand<MqttConnectionSettings, Unit> EditServerConnection { get; }
    
    public ReactiveCommand<MqttConnectionSettings, Unit> DeleteServerConnection { get; }

    public async void ExecuteAddServerConnection()
    {
        var settings = new MqttConnectionSettings();
        if (await _windowService.ShowDialogAsync<ServerConnectionDialogViewModel, DialogResult>(settings) == DialogResult.Save)
            ServerConnections.Add(settings);
    }

    public async void ExecuteEditServerConnection(MqttConnectionSettings settings)
    {
        if (await _windowService.ShowDialogAsync<ServerConnectionDialogViewModel, DialogResult>(settings) == DialogResult.Remove)
            ServerConnections.Remove(settings);
    }

    public async void ExecuteDeleteServerConnection(MqttConnectionSettings settings)
    {
        var result = await _windowService.ShowConfirmContentDialog(
            "Delete Connection",
            "Are you sure you wish to delete this connection?",
            "Delete",
            "Don't delete");
        if (result)
            ServerConnections.Remove(settings);
    }

    public void Save()
    {
        _serverConnectionsSetting.Value = ServerConnections.ToList();
        _serverConnectionsSetting.Save();

        _dynamicDataModelStructureSetting.Value = DynamicDataModelStructureRoot.ViewModelToModel();
        _dynamicDataModelStructureSetting.Save();
    }
}