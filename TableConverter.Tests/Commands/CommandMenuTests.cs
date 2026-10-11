using System;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using TableConverter.Commands.DataModels;
using TableConverter.Commands.Interfaces;

namespace TableConverter.Tests.Commands;

/// <summary>
/// Covers how a workspace's main menu is put together: the groups, and the commands within them, run in
/// the order they were added, and a section starts a break rather than a new group.
/// </summary>
public class CommandMenuTests
{
    [Fact]
    public void Groups_And_Commands_Run_In_The_Order_They_Were_Added()
    {
        var menu = new CommandMenu()
            .Group("File", group => group
                .Add(Command("File.New"), Command("File.Open"))
                .Section()
                .Add(Command("File.Import")))
            .Group("Edit", group => group
                .Add(Command("Edit.Undo")));

        Assert.Equal(["File", "Edit"], menu.Groups.Select(group => group.Title));
        Assert.Equal(["File.New", "File.Open"], Names(menu.Groups[0], 0));
        Assert.Equal(["File.Import"], Names(menu.Groups[0], 1));
        Assert.Equal(["Edit.Undo"], Names(menu.Groups[1], 0));
    }

    [Fact]
    public void Starting_A_New_Block_Keeps_Its_Commands_In_The_Same_Group()
    {
        var menu = new CommandMenu()
            .Group("Tools", group => group
                .Add(Command("Tools.TrimWhitespace"))
                .Section()
                .Add(Command("Tools.SortByColumn")));

        var group = Assert.Single(menu.Groups);

        Assert.Equal(2, group.Sections.Count);
    }

    [Fact]
    public void A_Menu_Counts_Every_Command_It_Holds()
    {
        var menu = new CommandMenu()
            .Group("File", group => group.Add(Command("File.New"), Command("File.Open")))
            .Group("Edit", group => group.Add(Command("Edit.Undo")));

        Assert.Equal(3, menu.Count);
    }

    [Fact]
    public void A_Menu_With_No_Groups_Holds_Nothing()
    {
        Assert.Equal(0, new CommandMenu().Count);
    }

    private static string[] Names(CommandMenuGroup group, int section)
    {
        return [.. group.Sections[section].Commands.Select(command => command.Metadata.Name)];
    }

    private static ICommandInstance Command(string name)
    {
        return new FakeCommandInstance(name);
    }

    /// <summary>
    /// A command instance which carries only the metadata the menu reads, so the menu can be exercised
    /// without building the rest of the command infrastructure.
    /// </summary>
    private sealed class FakeCommandInstance(string name) : ICommandInstance
    {
        public ICommandMetadata Metadata { get; } =
            new CommandMetadata(name, name, $"{name} description", "TestIcon");

        public ICommand Command => throw new NotSupportedException();

        public IRelayCommand RelayCommand => throw new NotSupportedException();

        public ICommandHandlerBase Handler => throw new NotSupportedException();

        public ICommandContext Context => throw new NotSupportedException();

        public void RaiseCanExecuteChanged()
        {
        }
    }
}

