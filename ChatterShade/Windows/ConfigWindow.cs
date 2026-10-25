using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace ChatterShade.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;

    private readonly Plugin plugin;

    private int selectedWorkspace = 0;

    public ConfigWindow(Plugin plugin) : base("ChatterShade###ChatterShadeConfig")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse;

        Size = new Vector2(760, 600);
        SizeCondition = ImGuiCond.FirstUseEver;

        this.plugin = plugin;
        configuration = plugin.Configuration;

        //Normalize alpha saved by eariler version while preserving RGB.
        if (configuration.DialogueTextColor.W != 1f ||
            configuration.DialogueBackgroundTint.W != 1f)
        {
            var textColor = configuration.DialogueTextColor;
            textColor.W = 1f;
            configuration.DialogueTextColor = textColor;

            var backgroundTint = configuration.DialogueBackgroundTint;
            backgroundTint.W = 1f;
            configuration.DialogueBackgroundTint = backgroundTint;

            configuration.Save();
        }
    }

    public void Dispose() { }

    public override void PreDraw()
    {
        // Flags must be added or removed before Draw() is being called, or they won't apply
        if (configuration.IsConfigWindowMovable)
        {
            Flags &= ~ImGuiWindowFlags.NoMove;
        }
        else
        {
            Flags |= ImGuiWindowFlags.NoMove;
        }
    }



    public override void Draw()
    {
        // Aetheric Studio workspace layout design.
        ImGui.BeginGroup();

        // Left Navigation sidebar.
        ImGui.BeginChild("ChatterShadeNavigation",
              new Vector2(160, 0),
              true);

        ImGui.Spacing();
        ImGui.TextUnformatted("AETHERIC STUDIO");
        ImGui.Separator();
        ImGui.Spacing();
        ImGui.TextDisabled("WORKSPACE");
        ImGui.Spacing();

        ImGui.PushStyleColor(ImGuiCol.Header, new Vector4(0.12f, 0.34f, 0.46f, 0.70f));
        ImGui.PushStyleColor(ImGuiCol.HeaderHovered, new Vector4(0.16f, 0.42f, 0.58f, 0.85f));
        ImGui.PushStyleColor(ImGuiCol.HeaderActive, new Vector4(0.18f, 0.46f, 0.62f, 1.0f));
        if (ImGui.Selectable($"{(selectedWorkspace == 0 ? "> " : " ")}Appearance", selectedWorkspace == 0,
                 ImGuiSelectableFlags.None, new Vector2(ImGui.GetContentRegionAvail().X, 0)))
        {
            selectedWorkspace = 0;
        }

        if (ImGui.Selectable($"{(selectedWorkspace == 1 ? "> " : " ")}Speakers", selectedWorkspace == 1,
                 ImGuiSelectableFlags.None, new Vector2(ImGui.GetContentRegionAvail().X, 0)))
        {
            selectedWorkspace = 1;
        }

        if (ImGui.Selectable($"{(selectedWorkspace == 2 ? "> " : " ")}Presets", selectedWorkspace == 2,
                 ImGuiSelectableFlags.None, new Vector2(ImGui.GetContentRegionAvail().X, 0)))
        {
            selectedWorkspace = 2;
        }

        if (ImGui.Selectable($"{(selectedWorkspace == 3 ? "> " : " ")}Settings", selectedWorkspace == 3,
                 ImGuiSelectableFlags.None, new Vector2(ImGui.GetContentRegionAvail().X, 0)))
        {
            selectedWorkspace = 3;
        }

        ImGui.PopStyleColor(3);

        ImGui.EndChild();

        ImGui.SameLine();

        // Right side workspace.

        ImGui.BeginChild("ChatterShadeWorkspace", 
              new Vector2(0, 0),
              false);

        // Global enable setting
        if (selectedWorkspace == 0)
        {
            ImGui.TextUnformatted("Appearance");
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.TextWrapped("Customize dialogue text, background and NPC name colors.");
            ImGui.Spacing();

            var enabled = configuration.Enabled;
            if (ImGui.Checkbox("Enable dialogue colors", ref enabled))
            {
                configuration.Enabled = enabled;
                configuration.Save();
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.TextDisabled("DIALOGUE APPEARANCE");
            ImGui.Spacing();

            // Standard Dialogue section
            if (ImGui.CollapsingHeader(
                    "Standard Dialogue",
                    ImGuiTreeNodeFlags.DefaultOpen))
            {
                var standardEnabled = configuration.StandardDialogueEnabled;
                if (ImGui.Checkbox("Customize standard dialogue", ref standardEnabled))
                {
                    configuration.StandardDialogueEnabled = standardEnabled;
                    configuration.Save();
                }

                var standardMode = (int)configuration.StandardBackgroundMode;
                if (ImGui.Combo(
                        "Background mode##Standard",
                        ref standardMode,
                        "Original texture\0Solid color\0"))
                {
                    configuration.StandardBackgroundMode =
                        (DialogueBackgroundMode)standardMode;
                    configuration.Save();
                }

                var textColor = configuration.DialogueTextColor;
                if (ImGui.ColorEdit4(
                        "Dialogue text",
                        ref textColor,
                        ImGuiColorEditFlags.NoAlpha))
                {
                    configuration.DialogueTextColor = textColor;
                    configuration.Save();
                }

                var backgroundTint = configuration.DialogueBackgroundTint;

                var standardBackgroundLabel =
                    configuration.StandardBackgroundMode == DialogueBackgroundMode.SolidColor
                        ? "Background color##StandardBackground"
                        : "Background tint##StandardBackground";

                if (ImGui.ColorEdit4(
                        standardBackgroundLabel,
                        ref backgroundTint,
                        ImGuiColorEditFlags.NoAlpha))
                {
                    configuration.DialogueBackgroundTint = backgroundTint;
                    configuration.Save();
                }

                if (configuration.StandardBackgroundMode == DialogueBackgroundMode.SolidColor)
                {
                    var opacity = configuration.StandardBackgroundOpacity * 100f;

                    if (ImGui.SliderFloat(
                            "Background opacity##Standard",
                            ref opacity,
                            0f,
                            100f,
                            "%.0f%%"))
                    {
                        configuration.StandardBackgroundOpacity = opacity / 100f;
                        configuration.Save();
                    }
                }

                ImGui.Spacing();

                if (ImGui.Button("Restore Original Appearance##Standard"))
                {
                    configuration.StandardDialogueEnabled = false;
                    configuration.Save();
                }

                ImGui.SameLine();

                if (ImGui.Button("Use Dark Preset##Standard"))
                {
                    configuration.DialogueTextColor =
                        new Vector4(242f / 255f, 242f / 255f, 242f / 255f, 1f);

                    configuration.DialogueBackgroundTint =
                        new Vector4(48f / 255f, 48f / 255f, 48f / 255f, 1f);

                    configuration.StandardBackgroundMode =
                        DialogueBackgroundMode.SolidColor;

                    configuration.StandardBackgroundOpacity = 1f;
                    configuration.StandardDialogueEnabled = true;

                    configuration.Save();
                }
            }

            ImGui.Spacing();
            ImGui.Separator();

            // Dark / System Dialogue section
            if (ImGui.CollapsingHeader(
                    "Dark / System Dialogue",
                    ImGuiTreeNodeFlags.DefaultOpen))
            {
                var darkEnabled = configuration.DarkDialogueEnabled;
                if (ImGui.Checkbox("Customize dark dialogue", ref darkEnabled))
                {
                    configuration.DarkDialogueEnabled = darkEnabled;
                    configuration.Save();
                }

                var darkMode = (int)configuration.DarkBackgroundMode;
                if (ImGui.Combo(
                        "Background mode##Dark",
                        ref darkMode,
                        "Original texture\0Solid color\0"))
                {
                    configuration.DarkBackgroundMode =
                        (DialogueBackgroundMode)darkMode;
                    configuration.Save();
                }

                var darkTextColor = configuration.DarkDialogueTextColor;
                if (ImGui.ColorEdit4(
                        "Dialogue text##Dark",
                        ref darkTextColor,
                        ImGuiColorEditFlags.NoAlpha))
                {
                    configuration.DarkDialogueTextColor = darkTextColor;
                    configuration.Save();
                }

                var darkBackgroundTint = configuration.DarkDialogueBackgroundTint;

                var darkBackgroundLabel =
                    configuration.DarkBackgroundMode == DialogueBackgroundMode.SolidColor
                        ? "Background color##DarkBackground"
                        : "Background tint##DarkBackground";

                if (ImGui.ColorEdit4(
                        darkBackgroundLabel,
                        ref darkBackgroundTint,
                        ImGuiColorEditFlags.NoAlpha))
                {
                    configuration.DarkDialogueBackgroundTint = darkBackgroundTint;
                    configuration.Save();
                }

                if (configuration.DarkBackgroundMode == DialogueBackgroundMode.SolidColor)
                {
                    var opacity = configuration.DarkBackgroundOpacity * 100f;

                    if (ImGui.SliderFloat(
                            "Background opacity##Dark",
                            ref opacity,
                            0f,
                            100f,
                            "%.0f%%"))
                    {
                        configuration.DarkBackgroundOpacity = opacity / 100f;
                        configuration.Save();
                    }
                }

                ImGui.Spacing();

                if (ImGui.Button("Restore Original Appearance##Dark"))
                {
                    configuration.DarkDialogueEnabled = false;
                    configuration.Save();
                }

                ImGui.SameLine();

                if (ImGui.Button("Use Dark Preset##Dark"))
                {
                    configuration.DarkDialogueTextColor =
                        new Vector4(242f / 255f, 242f / 255f, 242f / 255f, 1f);

                    configuration.DarkDialogueBackgroundTint =
                        new Vector4(48f / 255f, 48f / 255f, 48f / 255f, 1f);

                    configuration.DarkBackgroundMode =
                        DialogueBackgroundMode.SolidColor;

                    configuration.DarkBackgroundOpacity = 1f;
                    configuration.DarkDialogueEnabled = true;

                    configuration.Save();
                }
            }

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.TextDisabled("NAME CUSTOMIZATION");
            ImGui.Spacing();

            // Independent NPC Name Customization section
            if (ImGui.CollapsingHeader(
                    "NPC Name Customization",
                    ImGuiTreeNodeFlags.DefaultOpen))
            {
                var npcNameEnabled = configuration.NpcNameColorEnabled;
                if (ImGui.Checkbox("Customize NPC names", ref npcNameEnabled))
                {
                    configuration.NpcNameColorEnabled = npcNameEnabled;
                    configuration.Save();
                }

                var npcNameColor = configuration.NpcNameColor;
                if (ImGui.ColorEdit4(
                        "NPC name color",
                        ref npcNameColor,
                        ImGuiColorEditFlags.NoAlpha))
                {
                    configuration.NpcNameColor = npcNameColor;
                    configuration.Save();
                }

                ImGui.Spacing();

                if (ImGui.Button("Restore Original Name##NPC"))
                {
                    configuration.NpcNameColorEnabled = false;
                    configuration.Save();
                }

                ImGui.SameLine();

                if (ImGui.Button("Use White Preset##NPC"))
                {
                    configuration.NpcNameColor = new Vector4(1f, 1f, 1f, 1f);
                    configuration.NpcNameColorEnabled = true;
                    configuration.Save();
                }
            }

            // End of NPC Name Customization section
            ImGui.Spacing();
            ImGui.Separator();

            ImGui.Spacing();
            ImGui.Separator();

            ImGui.TextWrapped(
                "Custom dialogue backgrounds use a rounded style. " +
                "Disabling customization restores the original game " +
                "or installed UI mod appearance.");

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.TextDisabled("GENERAL SETTINGS");
            ImGui.Spacing();

            var movable = configuration.IsConfigWindowMovable;
            if (ImGui.Checkbox("Movable Config Window", ref movable))
            {
                configuration.IsConfigWindowMovable = movable;
                configuration.Save();
            }
        }

        if (selectedWorkspace == 1)
        {
            ImGui.TextUnformatted("Speakers");
            ImGui.Separator();
            ImGui.Spacing();

            ImGui.TextDisabled("INDIVIDUAL NPC PROFILES");
            ImGui.Spacing();

            ImGui.TextWrapped(
                "Manage individual NPC Speaker profiles and their custom name colors.");

            ImGui.Spacing();

            if (ImGui.Button("Edit Speaker Profiles"))
            {
                plugin.OpenSpeakerProfileWindow();
            }
        }
        else if (selectedWorkspace == 2)
        {
            ImGui.TextUnformatted("Presets");
            ImGui.Separator();
            ImGui.Spacing();
            ImGui.TextWrapped("Named appearance presets are planned for a future update.");
        }
        else if (selectedWorkspace == 3)
        {
            ImGui.TextUnformatted("Settings");
            ImGui.Separator();
            ImGui.Spacing();
            ImGui.TextWrapped(
                "General plugin settings and maintenance options will be organized here.");
        }
        ImGui.EndChild(); // End of ChatterSpace Workspace.
        ImGui.EndGroup();
    }
}

