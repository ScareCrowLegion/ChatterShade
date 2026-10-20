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
using System.Threading;
using System.Threading.Tasks;
using KamiToolKit;
using KamiToolKit.Nodes;
using KamiToolKit.Enums;
using System.IO;

namespace ChatterShade;

public sealed class Plugin : IAsyncDalamudPlugin
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
    [PluginService]
    internal static IFramework Framework { get; private set; } = null!;

    private const string CommandName = "/cshade";
    private bool commandRegistered;
    private bool toolkitInitializationStarted;
    private ImGuiImageNode? replacementBackgroundNode;
    private nint replacementBackgroundAddon;
    private uint replacementBackgroundNodeId;
    public Configuration Configuration { get; private set; } = null!;
    public readonly WindowSystem WindowSystem = new("ChatterShade");
    private ConfigWindow ConfigWindow { get; set; } = null!;
    private bool nativeBackgroundHiddenForDraw;

    public async Task LoadAsync(CancellationToken cancellationtoken)
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        toolkitInitializationStarted = true;

        await KamiToolKitLibrary.InitializeAsync(PluginInterface, "ChatterShade");
        Log.Information("ChatterShade: KamiToolKit initialization completed.");

        AddonLifecycle.RegisterListener(AddonEvent.PreDraw, "Talk", OnTalkPreDraw);

        AddonLifecycle.RegisterListener(AddonEvent.PostDraw, "Talk", OnTalkPostDraw);

        AddonLifecycle.RegisterListener(AddonEvent.PostUpdate, "Talk", OnTalkPostUpdate);

        AddonLifecycle.RegisterListener(AddonEvent.PreFinalize, "Talk", OnTalkPreFinalize);

        ConfigWindow = new ConfigWindow(this);

        WindowSystem.AddWindow(ConfigWindow);

        commandRegistered = CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
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
    }

    private unsafe void OnTalkPreDraw(AddonEvent type, AddonArgs args)
    {
        modifiedTextNode = 0;
        modifiedBackgroundNode = 0;
        modifiedNpcNameNode = 0;

        nativeBackgroundHiddenForDraw = false;

        if (!Configuration.Enabled || args.Addon.IsNull || !args.Addon.IsReady)
            return;

        var addon = (AtkUnitBase*)args.Addon.Address;
        var textNode = addon->GetTextNodeById(3);
        var npcNameNode = addon->GetTextNodeById(2);

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

        var useSolidBackground = (useDarkDialogueColors ? Configuration.DarkBackgroundMode : Configuration.StandardBackgroundMode) == DialogueBackgroundMode.SolidColor;

        originalTextColor = textNode->TextColor;
        modifiedTextNode = (nint)textNode;

        textNode->TextColor = ToByteColor(
            useDarkDialogueColors
               ? Configuration.DarkDialogueTextColor
               : Configuration.DialogueTextColor);
        
        // Temporary NPC name recoloring test.
        if (npcNameNode != null && npcNameNode->IsVisible())
        {
            originalNpcNameColor = npcNameNode->TextColor;
            modifiedNpcNameNode = (nint)npcNameNode;

            npcNameNode->TextColor = ToByteColor(Configuration.NpcNameColor);
        }

        if (backgroundNode != null && backgroundNode->Type == NodeType.Image && backgroundNode->IsVisible())
        {
            originalBackgroundColor = backgroundNode->Color;
            modifiedBackgroundNode = (nint)backgroundNode;

            var tint = ToByteColor(useDarkDialogueColors 
                ? Configuration.DarkDialogueBackgroundTint : Configuration.DialogueBackgroundTint);

            backgroundNode->SetAlpha(tint.A);
            backgroundNode->Color = tint;
            backgroundNode->DrawFlags |= 1;

            if (useSolidBackground &&
                replacementBackgroundNode != null &&
                replacementBackgroundAddon == args.Addon.Address &&
                replacementBackgroundNodeId == modifiedBackgroundNodeId &&
                replacementBackgroundNode.IsVisible)
            {
                backgroundNode->ToggleVisibility(false);
                nativeBackgroundHiddenForDraw = true;
            }
        }
    }

    private unsafe void OnTalkPostDraw(AddonEvent type, AddonArgs args)
    {
        if (args.Addon.IsNull)
        {
            modifiedTextNode = 0;
            modifiedNpcNameNode = 0;
            modifiedBackgroundNode = 0;
            nativeBackgroundHiddenForDraw = false;
            return;
        }

        var addon = (AtkUnitBase*)args.Addon.Address;

        if (modifiedTextNode != 0)
        {
            var textNode = addon->GetTextNodeById(3);

            if (textNode != null && (nint)textNode == modifiedTextNode)
                textNode->TextColor = originalTextColor;
        }

        // Restore original NPC name color.
        if (modifiedNpcNameNode != 0)
        {
            var npcNameNode = addon->GetTextNodeById(2);

            if (npcNameNode != null && (nint)npcNameNode == modifiedNpcNameNode)
            {
                npcNameNode->TextColor = originalNpcNameColor;
            }
        }
        if (modifiedBackgroundNode != 0)
        {
            var backgroundNode = addon->GetNodeById(modifiedBackgroundNodeId);

            if (backgroundNode != null && (nint)backgroundNode == modifiedBackgroundNode)
            {
                if (nativeBackgroundHiddenForDraw)
                    backgroundNode->ToggleVisibility(true);

                backgroundNode->SetAlpha(originalBackgroundColor.A);
                backgroundNode->Color = originalBackgroundColor;
                backgroundNode->DrawFlags |= 1;
            }
        }

        modifiedTextNode = 0;
        modifiedNpcNameNode = 0;
        modifiedBackgroundNode = 0;

        nativeBackgroundHiddenForDraw = false;
    }
    private unsafe void OnTalkPostUpdate(AddonEvent type, AddonArgs args)
    {
        if (replacementBackgroundNode != null)
            replacementBackgroundNode.IsVisible = false;

        if (!Configuration.Enabled ||
            args.Addon.IsNull ||
            !args.Addon.IsReady)
            return;

        var addon = (AtkUnitBase*)args.Addon.Address;

        uint backgroundNodeId = 13;
        var backgroundNode = addon->GetNodeById(backgroundNodeId);

        if (backgroundNode == null || !backgroundNode->IsVisible())
        {
            backgroundNodeId = 18;
            backgroundNode = addon->GetNodeById(backgroundNodeId);
        }

        if (backgroundNode == null ||
            backgroundNode->Type != NodeType.Image ||
            !backgroundNode->IsVisible())
            return;

        var useDarkDialogueColors = backgroundNodeId == 18;

        if (backgroundNode == null || backgroundNode->Type != NodeType.Image ||
            !backgroundNode->IsVisible())
            return;

        var styleEnabled = useDarkDialogueColors
            ? Configuration.DarkDialogueEnabled
            : Configuration.StandardDialogueEnabled;

        if (!styleEnabled)
            return;

        var backgroundMode = useDarkDialogueColors
            ? Configuration.DarkBackgroundMode
            : Configuration.StandardBackgroundMode;

        if (backgroundMode != DialogueBackgroundMode.SolidColor)
            return;

        if (replacementBackgroundNode != null &&
            replacementBackgroundNodeId != backgroundNodeId)
        {
            DisposeReplacementBackground();
        }

        if (replacementBackgroundNode == null)
        {
            replacementBackgroundNode = new ImGuiImageNode
            {
                IsVisible = false,
                FitTexture = true
            };

            replacementBackgroundAddon = args.Addon.Address;
            replacementBackgroundNodeId = backgroundNodeId;

            replacementBackgroundNode.AttachNode(
                backgroundNode,
                NodePosition.AfterTarget);

            var imagePath = Path.Combine(
                PluginInterface.AssemblyLocation.DirectoryName!,
                "Data",
                "dialogue-background.png");

            replacementBackgroundNode.TexturePath = imagePath;
        }

        if (replacementBackgroundAddon != args.Addon.Address)
            return;

        const float horizontalInset = 24f;
        const float topInset = 24f;
        const float bottomInset = 20f;

        replacementBackgroundNode.Position =
            new Vector2(backgroundNode->X + horizontalInset * backgroundNode->ScaleX,
                        backgroundNode->Y + topInset * backgroundNode->ScaleY);

        replacementBackgroundNode.Size =
            new Vector2(Math.Max(1f, backgroundNode->Width - horizontalInset * 2f),
                        Math.Max(1f, backgroundNode->Height - topInset - bottomInset));

        replacementBackgroundNode.Scale =
            new Vector2(backgroundNode->ScaleX, backgroundNode->ScaleY);

        replacementBackgroundNode.Origin =
            new Vector2(backgroundNode->OriginX, backgroundNode->OriginY);

        var panelColor = useDarkDialogueColors ? Configuration.DarkDialogueBackgroundTint : Configuration.DialogueBackgroundTint;
        panelColor.W = Math.Clamp(useDarkDialogueColors ? Configuration.DarkBackgroundOpacity : Configuration.StandardBackgroundOpacity, 0f, 1f);
        replacementBackgroundNode.Color = panelColor;

        replacementBackgroundNode.IsVisible = true;
        replacementBackgroundNode.MarkDirty();
    }

    private void OnTalkPreFinalize(AddonEvent type, AddonArgs args)
    {
        if (args.Addon.Address == replacementBackgroundAddon)
            DisposeReplacementBackground();
    }

    private void DisposeReplacementBackground()
    {
        replacementBackgroundNode?.Dispose();
        replacementBackgroundNode = null;
        replacementBackgroundAddon = 0;
        replacementBackgroundNodeId = 0;
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


    public async ValueTask DisposeAsync()
    {
        await Framework.RunOnFrameworkThread(() =>
        {
            AddonLifecycle.UnregisterListener(AddonEvent.PreDraw, "Talk", OnTalkPreDraw);
            AddonLifecycle.UnregisterListener(AddonEvent.PostDraw, "Talk", OnTalkPostDraw);
            AddonLifecycle.UnregisterListener(AddonEvent.PostUpdate, "Talk", OnTalkPostUpdate);
            AddonLifecycle.UnregisterListener(AddonEvent.PreFinalize, "Talk", OnTalkPreFinalize);

            DisposeReplacementBackground();
        });
        
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleConfigUi;
        
        WindowSystem.RemoveAllWindows();

        ConfigWindow?.Dispose();

        if (commandRegistered)
        {
            CommandManager.RemoveHandler(CommandName);
                commandRegistered = false;
        }
        if (toolkitInitializationStarted)
        {
            await KamiToolKitLibrary.DisposeAsync();
            toolkitInitializationStarted = false;
        }

    }


    private void OnCommand(string command, string args)
    {
        ConfigWindow.Toggle();
    }

    private nint modifiedTextNode;
    private ByteColor originalTextColor;
    private nint modifiedNpcNameNode;
    private ByteColor originalNpcNameColor;
    private nint modifiedBackgroundNode;
    private ByteColor originalBackgroundColor;
    private uint modifiedBackgroundNodeId;
    public void ToggleConfigUi() => ConfigWindow.Toggle();
}
