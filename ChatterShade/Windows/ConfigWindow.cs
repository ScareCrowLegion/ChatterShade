using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace ChatterShade.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;

    public ConfigWindow(Plugin plugin) : base("ChatterShade###ChatterShadeConfig")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(420, 520);
        SizeCondition = ImGuiCond.Always;

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
        var enabled = configuration.Enabled;
        if (ImGui.Checkbox("Enable dialogue colors", ref enabled))
        {
            configuration.Enabled = enabled;
            configuration.Save();
        }

        ImGui.Spacing();

        ImGui.TextUnformatted("Standard dialogue");

        var standaradEnabled = configuration.StandardDialogueEnabled;
        if (ImGui.Checkbox("Customize standard dialogue", ref standaradEnabled))
        {
            configuration.StandardDialogueEnabled = standaradEnabled;
            configuration.Save();
        }

        var standardMode = (int)configuration.StandardBackgroundMode;
        if (ImGui.Combo("Background mode##Standard", ref standardMode, "Original texture\0Solid color\0"))
        {
            configuration.StandardBackgroundMode = (DialogueBackgroundMode)standardMode;
            configuration.Save();
        }

        var textColor = configuration.DialogueTextColor;
        if (ImGui.ColorEdit4("Dialogue text", ref textColor, ImGuiColorEditFlags.NoAlpha))
        {
            configuration.DialogueTextColor = textColor;
            configuration.Save();
        }

        var backgroundTint = configuration.DialogueBackgroundTint;

        var standardBackgroundLabel = configuration.StandardBackgroundMode == DialogueBackgroundMode.SolidColor
                ? "Background color##StandardBackground"
                : "Background tint##StandardBackground";

        if (ImGui.ColorEdit4(standardBackgroundLabel, ref backgroundTint, ImGuiColorEditFlags.NoAlpha))
        {
            configuration.DialogueBackgroundTint = backgroundTint;
            configuration.Save();
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

            configuration.StandardBackgroundMode = DialogueBackgroundMode.SolidColor;
            configuration.StandardDialogueEnabled = true;
            configuration.Save();
        }

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.TextUnformatted("Dark dialogue");

        var darkEnabled = configuration.DarkDialogueEnabled;
        if (ImGui.Checkbox("Customize dark dialogue", ref darkEnabled))
        {
            configuration.DarkDialogueEnabled = darkEnabled;
            configuration.Save();
        }

        var darkMode = (int)configuration.DarkBackgroundMode;
        if (ImGui.Combo("Background mode##Dark", ref darkMode, "Original texture\0Solid color\0"))
        {
            configuration.DarkBackgroundMode = (DialogueBackgroundMode)darkMode;
            configuration.Save();
        }

        var darkTextColor = configuration.DarkDialogueTextColor;
        if (ImGui.ColorEdit4("Dialogue text##Dark", ref darkTextColor, ImGuiColorEditFlags.NoAlpha))
        {
            configuration.DarkDialogueTextColor = darkTextColor;
            configuration.Save();
        }

        var darkBackgroundTint = configuration.DarkDialogueBackgroundTint;

        var darkBackgroundLabel = configuration.DarkBackgroundMode == DialogueBackgroundMode.SolidColor
             ? "Background color##DarkBackground"
             : "Background tint##DarkBackground";

        if (ImGui.ColorEdit4(darkBackgroundLabel, ref darkBackgroundTint, ImGuiColorEditFlags.NoAlpha))
        {
            configuration.DarkDialogueBackgroundTint = darkBackgroundTint;
            configuration.Save();
        }

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

            configuration.DarkBackgroundMode = DialogueBackgroundMode.SolidColor;
            configuration.DarkDialogueEnabled = true;
            configuration.Save();
        }

        ImGui.TextWrapped("Original texture applies a tint to your current UI texture. Solid color replaces the background.");
        ImGui.Spacing();

        var movable = configuration.IsConfigWindowMovable;
        if (ImGui.Checkbox("Movable Config Window", ref movable))
        {
            configuration.IsConfigWindowMovable = movable;
            configuration.Save();
        }
    }
}
