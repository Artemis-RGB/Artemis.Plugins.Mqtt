using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using Artemis.Plugins.Mqtt.DataModels;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;

namespace Artemis.Plugins.Mqtt.ViewModels;

public class StructureNodeViewModel : ViewModelBase
{
    private readonly IWindowService _windowService;
    private readonly StructureNodeViewModel? _parent;

    private string _label;
    private Guid? _server;
    private string _topic;
    private Type _type;
    
    public StructureNodeViewModel(IWindowService windowService, StructureNodeViewModel? parent, StructureDefinitionNode model)
    {
        _windowService = windowService;
        _parent = parent;
        
        _label = model.Label;
        _server = model.Server;
        _topic = model.Topic;
        _type = model.Type ?? throw new InvalidOperationException("Type cannot be null in model.");

        Children = model.IsGroup 
            ? new ObservableCollection<StructureNodeViewModel>(model.Children!.Select(c => new StructureNodeViewModel(windowService, this, c))) 
            : new ObservableCollection<StructureNodeViewModel>();
    }

    public StructureDefinitionNode ViewModelToModel()
    {
        var node = new StructureDefinitionNode(Label, Server, Topic, Type, IsGroup);

        if (Children.Any())
            node.Children!.AddRange(Children.Select(c => c.ViewModelToModel()));

        return node;
    }

    #region Properties

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

    public ObservableCollection<StructureNodeViewModel> Children { get; init; }

    public bool IsGroup => Children.Any();
    public bool IsValue => !IsGroup;

    #endregion

    #region Actions

    /// <summary>
    ///     Triggers a dialog to edit this ViewModel, and stores changes on confirm.
    /// </summary>
    public async Task EditNode()
    {
        var r = await _windowService.ShowDialogAsync<StructureNodeConfigurationDialogViewModel, StructureDefinitionNode?>(this);
        if (r == null)
            return;

        Label = r.Label;
        Server = r.Server;
        Topic = r.Topic;
        Type = r.Type ?? throw new InvalidOperationException();
    }

    /// <summary>
    ///     Trigers a dialog that asks the user to confirm to delete this structural node.
    /// </summary>
    public async Task DeleteNode()
    {
        if(_parent == null)
            throw new InvalidOperationException("Cannot delete root node.");
        
        if (!_parent.IsGroup)
            throw new InvalidOperationException("Cannot delete child node from node that does not support children.");
        
        if (!_parent.Children.Contains(this))
            throw new InvalidOperationException("Cannot delete node that is not a child of this node.");

        var result = await _windowService.ShowConfirmContentDialog(
            $"Delete {(IsGroup ? "Group" : "Value")}",
            "Are you sure you wish to delete this " + (IsGroup
                ? "group?" + Environment.NewLine + Environment.NewLine + $"This will also delete {Children.Count} child item(s)."
                : "value?"),
            "Delete",
            "Don't delete"
        );
        if (result)
            _parent.Children.Remove(this);
    }

    /// <summary>
    ///     Attempts to add a new child node ViewModel to this node's children collection.
    /// </summary>
    /// <param name="isGroup">If <c>true</c>, adds a new group node. If <c>false</c>, adds a new value node.</param>
    /// <exception cref="InvalidOperationException">If this node is a value-type node that does not support children.</exception>
    public async Task AddChildNode(bool isGroup)
    {
        if (!IsGroup)
            throw new InvalidOperationException("Cannot add a child item to an item that does not support children.");

        var child = new StructureDefinitionNode("", Guid.Empty, "", typeof(string), isGroup);
        var childVm = new StructureNodeViewModel(_windowService, this, child);
        
        var dialogResult = await _windowService.ShowDialogAsync<StructureNodeConfigurationDialogViewModel, StructureDefinitionNode?>(childVm);
        
        if (dialogResult is null)
            return;

        Children.Add(childVm);
    }

    #endregion
}