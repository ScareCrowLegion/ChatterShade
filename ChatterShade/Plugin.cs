using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ChatterShade.Windows;
using Dalamud.Game.Addon.Lifecycle;
using Dalamud.Game.Addon.Lifecycle.AddonArgTypes;
using System;
using System.Numerics;
using FFXIVClientStructs.FFXIV.Client.Graphics;
using FFXIVClientStructs.FFXIV.Component.GUI;
using Lumina.Excel.Sheets;

namespace ChatterShade;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] 
    internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] 
    internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] 
    internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] 
    internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] 
    internal static IPlayerState PlayerState { get; private set; } = null!;
    [PluginService] 
    internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] 
    internal static IPluginLog Log { get; private set; } = null!;
    [PluginService]
    internal static IAddonLifecycle AddonLifecycle { get; private set; } = null!;

    private const string CommandName = "/cshade";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("ChatterShade");
    private ConfigWindow ConfigWindow { get; init; }

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        AddonLifecycle.RegisterListener(AddonEvent.PreDraw, "Talk", OnTalkPreDraw);

        AddonLifecycle.RegisterListener(AddonEvent.PostDraw, "Talk", OnTalkPostDraw);

        ConfigWindow = new ConfigWindow(this);

        WindowSystem.AddWindow(ConfigWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open the ChatterShade settings."
        });

        // Tell the UI system that we want our windows to be drawn through the window system
        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;

        // This adds a button to the plugin installer entry of this plugin which allows
        // toggling the display status of the configuration ui
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUi;

        // Adds another button doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleConfigUi;

        // Add a simple message to the log with level set to information
        // Use /xllog to open the log window in-game
        // Example Output: 00:57:54.959 | INF | [SamplePlugin] ===A cool log message from Sample Plugin===
        Log.Information("ChatterShade loaded.");
    }

    private unsafe void OnTalkPreDraw(AddonEvent type, AddonArgs args)
    {
        modifiedTextNode = 0;
        modifiedBackgroundNode = 0;

        if (!Configuration.Enabled || args.Addon.IsNull || !args.Addon.IsReady)
            return;

        var addon = (AtkUnitBase*)args.Addon.Address;
        var textNode = addon->GetTextNodeById(3);

        if (textNode == null)
            return;

        modifiedBackgroundNodeId = 13;
        var backgroundNode = addon->GetNodeById(modifiedBackgroundNodeId);

        if (backgroundNode == null || !backgroundNode->IsVisible())
        {
            modifiedBackgroundNodeId = 18;
            backgroundNode = addon->GetNodeById(modifiedBackgroundNodeId);
        }

        var useDarkDialogueColors =
            modifiedBackgroundNodeId == 18 &&
            backgroundNode != null &&
            backgroundNode->Type == NodeType.Image &&
            backgroundNode->IsVisible();

        var styleEnabled = useDarkDialogueColors
            ? Configuration.DarkDialogueEnabled
            : Configuration.StandardDialogueEnabled;

        if (!styleEnabled)
            return;

        originalTextColor = textNode->TextColor;
        modifiedTextNode = (nint)textNode;

        textNode->TextColor = ToByteColor(
            useDarkDialogueColors
               ? Configuration.DarkDialogueTextColor
               : Configuration.DialogueTextColor);

        if (backgroundNode != null && backgroundNode->Type == NodeType.Image && backgroundNode->IsVisible())
        {
            originalBackgroundColor = backgroundNode->Color;
            modifiedBackgroundNode = (nint)backgroundNode;

            var tint = ToByteColor(useDarkDialogueColors 
                ? Configuration.DarkDialogueBackgroundTint : Configuration.DialogueBackgroundTint);

            backgroundNode->SetAlpha(tint.A);
            backgroundNode->Color = tint;
            backgroundNode->DrawFlags |= 1;
        }
    }

    private unsafe void OnTalkPostDraw(AddonEvent type, AddonArgs args)
    {
        if (args.Addon.IsNull)
        {
            modifiedTextNode = 0;
            modifiedBackgroundNode = 0;
            return;
        }

        var addon = (AtkUnitBase*)args.Addon.Address;

        if (modifiedTextNode != 0)
        {
            var textNode = addon->GetTextNodeById(3);

            if (textNode != null && (nint)textNode == modifiedTextNode)
                textNode->TextColor = originalTextColor;
        }

        if (modifiedBackgroundNode != 0)
        {
            var backgroundNode = addon->GetNodeById(modifiedBackgroundNodeId);

            if (backgroundNode != null && (nint)backgroundNode == modifiedBackgroundNode)
            {
                backgroundNode->SetAlpha(originalBackgroundColor.A);
                backgroundNode->Color = originalBackgroundColor;
                backgroundNode->DrawFlags |= 1;
            }
        }

        modifiedTextNode = 0;
        modifiedBackgroundNode = 0;
    }

    private static ByteColor ToByteColor(Vector4 color)
    {
        return new ByteColor
        {
            R = (byte)Math.Clamp(color.X * 255f, 0f, 255f),
            G = (byte)Math.Clamp(color.Y * 255f, 0f, 255f),
            B = (byte)Math.Clamp(color.Z * 255f, 0f, 255f),
            A = (byte)Math.Clamp(color.W * 255f, 0f, 255f)
        };
    }


    public void Dispose()
    {
        AddonLifecycle.UnregisterListener(AddonEvent.PreDraw, "Talk", OnTalkPreDraw);
        AddonLifecycle.UnregisterListener(AddonEvent.PostDraw, "Talk", OnTalkPostDraw);
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleConfigUi;
        
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }


    private void OnCommand(string command, string args)
    {
        ConfigWindow.Toggle();
    }

    private nint modifiedTextNode;
    private ByteColor originalTextColor;
    private nint modifiedBackgroundNode;
    private ByteColor originalBackgroundColor;
    private uint modifiedBackgroundNodeId;
    public void ToggleConfigUi() => ConfigWindow.Toggle();
}
