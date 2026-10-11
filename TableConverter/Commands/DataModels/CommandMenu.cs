using System;
using System.Collections.Generic;
using System.Linq;
using TableConverter.Commands.Interfaces;

namespace TableConverter.Commands.DataModels;

/// <summary>
///     A block of commands shown together within a menu group. A separator is drawn between one block and
///     the next, so the blocks are what breaks a group up into runs of related commands.
/// </summary>
public sealed class CommandMenuSection
{
    private readonly List<ICommandInstance> _commands = [];

    /// <summary>The commands of the block, in the order they are shown.</summary>
    public IReadOnlyList<ICommandInstance> Commands => _commands;

    /// <summary>Adds commands to the end of the block.</summary>
    internal void Add(IEnumerable<ICommandInstance> commands) => _commands.AddRange(commands);
}

/// <summary>
///     A titled group of the main menu, such as "File", holding the commands shown under it.
/// </summary>
/// <remarks>
///     A group is built by adding commands to it and starting another block with <see cref="Section" />
///     wherever the group should be broken up. It is built once, as the workspace is initialised, and only
///     read afterwards.
/// </remarks>
public sealed class CommandMenuGroup(string title)
{
    private readonly List<CommandMenuSection> _sections = [new()];

    /// <summary>The title shown for the group in the menu.</summary>
    public string Title { get; } = title;

    /// <summary>The blocks of the group, in the order they are shown.</summary>
    public IReadOnlyList<CommandMenuSection> Sections => _sections;

    /// <summary>The number of commands the group holds.</summary>
    public int Count => _sections.Sum(section => section.Commands.Count);

    /// <summary>Adds commands to the block currently being built, in the order they are given.</summary>
    public CommandMenuGroup Add(params ICommandInstance[] commands)
    {
        _sections[^1].Add(commands);
        return this;
    }

    /// <summary>
    ///     Starts a new block, so commands added after it are shown apart from the ones before it.
    /// </summary>
    public CommandMenuGroup Section()
    {
        _sections.Add(new CommandMenuSection());
        return this;
    }
}

/// <summary>
///     The whole of a workspace's main menu, in the order it is shown.
/// </summary>
/// <remarks>
///     The menu is built by the workspace rather than described by each command, so where a command sits
///     and what order the groups run in is decided in one place. Groups and the commands within them are
///     shown in the order they are added.
/// </remarks>
public sealed class CommandMenu
{
    private readonly List<CommandMenuGroup> _groups = [];

    /// <summary>The groups of the menu, in the order they are shown.</summary>
    public IReadOnlyList<CommandMenuGroup> Groups => _groups;

    /// <summary>The total number of commands the menu holds.</summary>
    public int Count => _groups.Sum(group => group.Count);

    /// <summary>
    ///     Adds a group to the menu, built by the given actions. Groups are shown in the order they are
    ///     added.
    /// </summary>
    public CommandMenu Group(string title, Action<CommandMenuGroup> build)
    {
        var group = new CommandMenuGroup(title);
        build(group);
        _groups.Add(group);
        return this;
    }
}

