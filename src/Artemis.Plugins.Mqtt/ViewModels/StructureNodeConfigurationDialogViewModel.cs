using System;
using System.Collections.Generic;
using System.Reactive;
using Artemis.Core;
using Artemis.Plugins.Mqtt.DataModels;
using Artemis.UI.Shared;
using ReactiveUI;
using ReactiveUI.Validation.Extensions;

namespace Artemis.Plugins.Mqtt.ViewModels;

/// <summary>
///     ViewModel for single node edit dialog.
/// </summary>
public class StructureNodeConfigurationDialogViewModel : DialogViewModelBase<StructureDefinitionNode?>
{
    private string _label;
    private Guid? _server;
    private string _topic;
    private Type _type;

    public StructureNodeConfigurationDialogViewModel(PluginSettings pluginSettings, StructureNodeViewModel poco)
    {
        _label = poco.Label;
        _server = poco.Server;
        _topic = poco.Topic;
        _type = poco.Type;
        IsGroup = poco.IsGroup;
        ServerConnectionsSetting = pluginSettings.GetSetting<List<MqttConnectionSettings>>("ServerConnections");

        Save = ReactiveCommand.Create(ExecuteSave);
        Cancel = ReactiveCommand.Create(ExecuteCancel);
        this.ValidationRule(vm => vm.Label, label => !string.IsNullOrWhiteSpace(label), "Label cannot be empty");
        this.ValidationRule(vm => vm.Server, server => server != null && server != Guid.Empty, "Server cannot be empty");
        this.ValidationRule(vm => vm.Topic, topic => !string.IsNullOrWhiteSpace(topic), "Topic cannot be empty");

        if (!IsGroup)
        {
            this.ValidationRule(vm => vm.Type, type => type != null, "Type cannot be empty");
        }
    }

    public ReactiveCommand<Unit, Unit> Save { get; }

    public ReactiveCommand<Unit, Unit> Cancel { get; }

    public string Label
    {
        get => _label;
        set => RaiseAndSetIfChanged(ref _label, value);
    }

    public Guid? Server
    {
        get => _server;
        set => RaiseAndSetIfChanged(ref _server, value);
    }

    public string Topic
    {
        get => _topic;
        set => RaiseAndSetIfChanged(ref _topic, value);
    }

    public Type Type
    {
        get => _type;
        set => RaiseAndSetIfChanged(ref _type, value);
    }

    public bool IsGroup { get; }
    public bool IsValue => !IsGroup;

    public IEnumerable<Type> SupportedValueTypes { get; } = new[] { typeof(string), typeof(bool), typeof(int), typeof(double) };

    public PluginSetting<List<MqttConnectionSettings>> ServerConnectionsSetting { get; }

    public void ExecuteSave()
    {
        if (!HasErrors)
            Close(new(Label, Server, Topic, Type, IsGroup));
    }

    public void ExecuteCancel()
    {
        Close(null);
    }
}