using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Lumina.Excel.Sheets;

namespace ChatterShade.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;

    private readonly Plugin plugin;
    private int livePreviewStyle = 0;
    private int selectedWorkspace = 0;
    private int selectedPresetIndex = -1;
    private string newPresetName = string.Empty;
    private string presetStatusMessage = string.Empty;

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

    public void Dispose() 
    { 

    }

    public override void PreDraw()
    {
        // Preserve the existing movable-window setting.
        if (configuration.IsConfigWindowMovable)
        {
            Flags &= ~ImGuiWindowFlags.NoMove;
        }
        else
        {
            Flags |= ImGuiWindowFlags.NoMove;
        }

        // Aetheric Studio color palette.
        ImGui.PushStyleColor(ImGuiCol.WindowBg,
            new Vector4(0.035f, 0.045f, 0.075f, 1.00f));
        ImGui.PushStyleColor(ImGuiCol.ChildBg,
            new Vector4(0.050f, 0.070f, 0.108f, 1.00f));
        ImGui.PushStyleColor(ImGuiCol.PopupBg,
            new Vector4(0.055f, 0.075f, 0.120f, 0.99f));
        ImGui.PushStyleColor(ImGuiCol.Border,
            new Vector4(0.200f, 0.310f, 0.420f, 0.85f));
        ImGui.PushStyleColor(ImGuiCol.BorderShadow,
            new Vector4(0.010f, 0.015f, 0.030f, 0.30f));

        // Primary and secondary text.
        ImGui.PushStyleColor(ImGuiCol.Text,
            new Vector4(0.890f, 0.920f, 0.970f, 1.00f));
        ImGui.PushStyleColor(ImGuiCol.TextDisabled,
            new Vector4(0.500f, 0.600f, 0.720f, 1.00f));

        // Dark title bar with an active aether-blue tint.
        ImGui.PushStyleColor(ImGuiCol.TitleBg,
            new Vector4(0.035f, 0.060f, 0.105f, 1.00f));
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive,
            new Vector4(0.055f, 0.150f, 0.220f, 1.00f));
        ImGui.PushStyleColor(ImGuiCol.TitleBgCollapsed,
            new Vector4(0.030f, 0.050f, 0.080f, 1.00f));

        // Input fields and interactive controls.
        ImGui.PushStyleColor(ImGuiCol.FrameBg,
            new Vector4(0.075f, 0.100f, 0.150f, 1.00f));
        ImGui.PushStyleColor(ImGuiCol.FrameBgHovered,
            new Vector4(0.100f, 0.210f, 0.290f, 1.00f));
        ImGui.PushStyleColor(ImGuiCol.FrameBgActive,
            new Vector4(0.120f, 0.290f, 0.370f, 1.00f));

        // Buttons.
        ImGui.PushStyleColor(ImGuiCol.Button,
            new Vector4(0.075f, 0.150f, 0.220f, 1.00f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered,
            new Vector4(0.100f, 0.280f, 0.370f, 1.00f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive,
            new Vector4(0.120f, 0.360f, 0.460f, 1.00f));

        // Navigation selections and collapsing section headers.
        ImGui.PushStyleColor(ImGuiCol.Header,
            new Vector4(0.075f, 0.180f, 0.270f, 0.95f));
        ImGui.PushStyleColor(ImGuiCol.HeaderHovered,
            new Vector4(0.100f, 0.290f, 0.390f, 1.00f));
        ImGui.PushStyleColor(ImGuiCol.HeaderActive,
            new Vector4(0.120f, 0.360f, 0.460f, 1.00f));

        // Separators and crystal-gold accents.
        ImGui.PushStyleColor(ImGuiCol.Separator,
            new Vector4(0.140f, 0.230f, 0.330f, 0.55f));
        ImGui.PushStyleColor(ImGuiCol.CheckMark,
            new Vector4(0.930f, 0.720f, 0.370f, 1.00f));
        ImGui.PushStyleColor(ImGuiCol.SliderGrab,
            new Vector4(0.580f, 0.680f, 0.770f, 1.00f));
        ImGui.PushStyleColor(ImGuiCol.SliderGrabActive,
            new Vector4(0.930f, 0.720f, 0.370f, 1.00f));

        // Scrollbars.
        ImGui.PushStyleColor(ImGuiCol.ScrollbarBg,
            new Vector4(0.025f, 0.040f, 0.065f, 0.60f));
        ImGui.PushStyleColor(ImGuiCol.ScrollbarGrab,
            new Vector4(0.150f, 0.250f, 0.350f, 0.80f));
        ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabHovered,
            new Vector4(0.210f, 0.360f, 0.460f, 0.90f));
        ImGui.PushStyleColor(ImGuiCol.ScrollbarGrabActive,
            new Vector4(0.820f, 0.630f, 0.300f, 1.00f));

        ImGui.PushStyleColor(ImGuiCol.TextSelectedBg,
            new Vector4(0.130f, 0.360f, 0.480f, 0.45f));

        // Rounded shapes and consistent control spacing.
        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 8f);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, 6f);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 4f);
        ImGui.PushStyleVar(ImGuiStyleVar.PopupRounding, 6f);
        ImGui.PushStyleVar(ImGuiStyleVar.ScrollbarRounding, 6f);
        ImGui.PushStyleVar(ImGuiStyleVar.GrabRounding, 4f);

        ImGui.PushStyleVar(
            ImGuiStyleVar.WindowPadding,
            new Vector2(12f, 12f));

        ImGui.PushStyleVar(
            ImGuiStyleVar.FramePadding,
            new Vector2(8f, 5f));

        ImGui.PushStyleVar(
            ImGuiStyleVar.ItemSpacing,
            new Vector2(9f, 6f));

        ImGui.PushStyleVar(
            ImGuiStyleVar.ItemInnerSpacing,
            new Vector2(6f, 4f));

        ImGui.PushStyleVar(ImGuiStyleVar.WindowBorderSize, 1f);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildBorderSize, 1f);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameBorderSize, 0f);
        ImGui.PushStyleVar(ImGuiStyleVar.PopupBorderSize, 1f);
    }

    public override void PostDraw()
    {
        // Restore every style entry pushed by PreDraw().
        ImGui.PopStyleVar(14);
        ImGui.PopStyleColor(28);
    }




    public override void Draw()
    {
        // ChatterShade workspace layout design.
        ImGui.BeginGroup();

        // Left Navigation sidebar.
        ImGui.BeginChild("ChatterShadeNavigation",
              new Vector2(160, 0),
              true);

        ImGui.Spacing();

        // ChatterShade brand heading.
        ImGui.PushStyleColor(
            ImGuiCol.Text,
            new Vector4(0.93f, 0.72f, 0.37f, 1.00f));

        ImGui.TextUnformatted("CHATTER");

        ImGui.PopStyleColor();

        ImGui.SameLine(0, 4f);
        ImGui.TextUnformatted("SHADE");

        ImGui.Spacing();

        // Subtle crystal-gold divider.
        ImGui.PushStyleColor(
            ImGuiCol.Separator,
            new Vector4(0.38f, 0.31f, 0.20f, 0.85f));

        ImGui.Separator();

        ImGui.PopStyleColor();

        ImGui.Spacing();
        ImGui.TextDisabled("WORKSPACE");
        ImGui.Spacing();

        DrawNavigationItem("Appearance", 0);
        DrawNavigationItem("Speakers", 1);
        DrawNavigationItem("Presets", 2);
        DrawNavigationItem("Settings", 3);

        ImGui.EndChild();

        ImGui.SameLine();

        // Right side workspace.

        ImGui.BeginChild("ChatterShadeWorkspace", 
              new Vector2(0, 0),
              false,
              ImGuiWindowFlags.AlwaysUseWindowPadding);

        // Global enable setting
        if (selectedWorkspace == 0)
        {
            DrawWorkspaceTitle("Appearance");

            ImGui.TextWrapped("Customize dialogue text, background and NPC name colors.");
            ImGui.Spacing();

            DrawSectionLabel("LIVE PREVIEW");
            ImGui.Spacing();

            DrawSectionLabel("PREVIEW MODE");
            ImGui.Spacing();

            var previewButtonWidth =
                (ImGui.GetContentRegionAvail().X -
                ImGui.GetStyle().ItemSpacing.X) * 0.5f;

            DrawPreviewModeButton(
                "Standard Dialogue",
                0,
                previewButtonWidth);

            ImGui.SameLine();

            DrawPreviewModeButton(
                "Dark / System",
                1,
                previewButtonWidth);

            ImGui.Spacing();

            var previewIsDark = livePreviewStyle == 1;

            var previewBackgroundTint = previewIsDark
                ? configuration.DarkDialogueBackgroundTint
                : configuration.DialogueBackgroundTint;

            var previewBackgroundMode = previewIsDark
                ? configuration.DarkBackgroundMode
                : configuration.StandardBackgroundMode;

            var previewBackgroundOpacity = previewIsDark
                ? configuration.DarkBackgroundOpacity
                : configuration.StandardBackgroundOpacity;

            var previewTextColor = previewIsDark
                ? configuration.DarkDialogueTextColor
                : configuration.DialogueTextColor;

            // NPC name customization applies to standard dialogue.
            var previewSpeakerColor = !previewIsDark && configuration.NpcNameColorEnabled
                ? configuration.NpcNameColor
                : new Vector4(0.89f, 0.92f, 0.97f, 1f);

            if (previewBackgroundMode == DialogueBackgroundMode.OriginalTexture)
            {
                // The original game texture cannot be reproduced by the sample panel.
                // Use full tint opacity while still displaying the configured tint
                previewBackgroundOpacity = 1f;
            }

            DrawDialoguePreview(
                "DialogueLivePreview",
                previewIsDark ? string.Empty : "Arielle",
                "The path ahead may be perilous, but we will face it together.",
                previewBackgroundTint,
                previewBackgroundOpacity,
                previewSpeakerColor,
                previewTextColor);

            ImGui.TextDisabled(
                previewIsDark
                    ? "System dialogue preview · no speaker name"
                    : "Standard dialogue preview · NPC name shown");

            ImGui.TextDisabled(
                "Illustrative sample; game textures and fonts may differ");

            ImGui.Spacing();
            ImGui.Separator();
            ImGui.Spacing();

            var enabled = configuration.Enabled;
            if (ImGui.Checkbox("Enable dialogue colors", ref enabled))
            {
                configuration.Enabled = enabled;
                configuration.Save();
            }

            ImGui.Spacing();
            ImGui.Separator();
            DrawSectionLabel("DIALOGUE APPEARANCE");
            ImGui.Spacing();

            // Standard Dialogue section
            if (DrawStyleCollapsingHeader(
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
                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.62f);
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
                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.62f);
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

                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.62f);
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

                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.62f);
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

                if (DrawStyledActionButton("Restore Original Appearance##Standard"))
                {
                    configuration.StandardDialogueEnabled = false;
                    configuration.Save();
                }

                ImGui.SameLine();

                if (DrawStyledActionButton("Use Dark Preset##Standard", true))
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
            if (DrawStyleCollapsingHeader(
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
                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.62f);
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
                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.62f);
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

                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.62f);
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

                    ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.62f);
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

                if (DrawStyledActionButton("Restore Original Appearance##Dark"))
                {
                    configuration.DarkDialogueEnabled = false;
                    configuration.Save();
                }

                ImGui.SameLine();

                if (DrawStyledActionButton("Use Dark Preset##Dark", true))
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
            DrawSectionLabel("NAME CUSTOMIZATION");
            ImGui.Spacing();

            // Independent NPC Name Customization section
            if (DrawStyleCollapsingHeader(
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
                ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X * 0.62f);
                if (ImGui.ColorEdit4(
                        "NPC name color",
                        ref npcNameColor,
                        ImGuiColorEditFlags.NoAlpha))
                {
                    configuration.NpcNameColor = npcNameColor;
                    configuration.Save();
                }

                ImGui.Spacing();

                if (DrawStyledActionButton("Restore Original Name##NPC"))
                {
                    configuration.NpcNameColorEnabled = false;
                    configuration.Save();
                }

                ImGui.SameLine();

                if (DrawStyledActionButton("Use White Preset##NPC", true))
                {
                    configuration.NpcNameColor = new Vector4(1f, 1f, 1f, 1f);
                    configuration.NpcNameColorEnabled = true;
                    configuration.Save();
                }
            }

            // End of NPC Name Customization section
            ImGui.Spacing();
            ImGui.Separator();

            ImGui.TextWrapped(
                "Custom dialogue backgrounds use a rounded style. " +
                "Disabling customization restores the original game " +
                "or installed UI mod appearance.");

            ImGui.Spacing();
            ImGui.Separator();
            DrawSectionLabel("GENERAL SETTINGS");
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
            DrawWorkspaceTitle("Speakers");

            DrawSectionLabel("INDIVIDUAL NPC PROFILES");
            ImGui.Spacing();

            ImGui.TextWrapped(
                "Manage individual NPC Speaker profiles and their custom name colors.");

            ImGui.Spacing();

            if (DrawStyledActionButton("Edit Speaker Profiles"))
            {
                plugin.OpenSpeakerProfileWindow();
            }
        }
        else if (selectedWorkspace == 2)
        {
            DrawWorkspaceTitle("Presets");

            DrawSectionLabel("APPEARANCE PRESETS");
            ImGui.Spacing();

            ImGui.TextWrapped("Save and reuse named combinations of dialogue colors, backgrounds, and NPC name styling.");

            ImGui.Spacing();
            ImGui.Separator();

            ImGui.TextDisabled("SAVE CURRENT APPEARANCE");
            ImGui.Spacing();

            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X);

            ImGui.InputTextWithHint("##NewAppearancePresetName", "Enter a preset name...", ref newPresetName, 64);

            if (DrawStyledActionButton("Save Current Appearance", true))
            {
                var trimmedName = newPresetName.Trim();

                configuration.AppearancePresets ??= new();

                var presets = configuration.AppearancePresets;

                if (string.IsNullOrWhiteSpace(trimmedName))
                {
                    presetStatusMessage = "Enter a name before saving a preset.";
                }
                else if (presets.Exists(preset =>
                             preset != null &&
                             string.Equals(
                                 preset.Name?.Trim(),
                                 trimmedName,
                                 StringComparison.OrdinalIgnoreCase)))
                {
                    presetStatusMessage = "A preset with that name already exists.";
                }
                else
                {
                    presets.Add(CreateAppearancePreset(trimmedName));

                    selectedPresetIndex = presets.Count - 1;
                    newPresetName = string.Empty;

                    configuration.Save();

                    presetStatusMessage = $"Saved preset '{trimmedName}'.";
                }
            }

            ImGui.Spacing();
            ImGui.Separator();

            ImGui.TextDisabled("SAVED PRESETS");
            ImGui.Spacing();

            configuration.AppearancePresets ??= new();

            var savedPresets = configuration.AppearancePresets;

            ImGui.BeginChild(
                "AppearancePresetList",
                new Vector2(0, 140),
                true);

            if (savedPresets.Count == 0)
            {
                ImGui.TextDisabled("No saved appearance presets yet.");
            }
            else
            {
                for (var i = 0; i < savedPresets.Count; i++)
                {
                    var preset = savedPresets[i];

                    if (preset == null)
                        continue;

                    var displayName = string.IsNullOrWhiteSpace(preset.Name)
                        ? "Unnamed Preset"
                        : preset.Name;

                    ImGui.PushID(i);

                    var isSelected = selectedPresetIndex == i;

                    if (isSelected)
                    {
                        ImGui.PushStyleColor(
                            ImGuiCol.Text,
                            new Vector4(0.95f, 0.78f, 0.48f, 1.00f));
                    }

                    if (ImGui.Selectable(
                        displayName,
                        isSelected,
                        ImGuiSelectableFlags.None,
                        new Vector2(ImGui.GetContentRegionAvail().X, 0)))
                    {
                        selectedPresetIndex = i;
                        presetStatusMessage = string.Empty;
                    }

                    if (isSelected)
                    {
                        ImGui.PopStyleColor();
                    }
                    ImGui.PopID();
                }
            }

            ImGui.EndChild();

            ImGui.Spacing();

            var selectedPresetIsVaild =
                selectedPresetIndex >= 0 &&
                selectedPresetIndex < savedPresets.Count &&
                savedPresets[selectedPresetIndex] != null;

            if (selectedPresetIsVaild)
            {
                var selectedPreset = savedPresets[selectedPresetIndex];

                ImGui.TextWrapped($"Selected preset: {selectedPreset.Name}");

                if (DrawStyledActionButton("Apply Selected Preset", true))
                {
                    ApplyAppearancePreset(selectedPreset);

                    presetStatusMessage = $"Applied preset '{selectedPreset.Name} '.";
                }

                ImGui.SameLine();

                if (DrawStyledActionButton("Update Selected Preset", true))
                {
                    ImGui.OpenPopup("Update Appearance Preset###UpdateAppearancePreset");
                }

                ImGui.Spacing();

                if (DrawStyledDangerButton("Delete Selected Preset"))
                {
                    ImGui.OpenPopup(
                        "Delete Appearance Preset###DeleteAppearancePreset");

                }
            }
            else
            {
                ImGui.TextDisabled
                    ("Select a saved preset to apply or delete it.");
            }

            if (ImGui.BeginPopupModal(
                "Delete Appearance###DeleteAppearancePreset",
                ImGuiWindowFlags.AlwaysAutoResize))
            {
                var canDelete =
                    selectedPresetIndex >= 0 &&
                    selectedPresetIndex < savedPresets.Count &&
                    savedPresets[selectedPresetIndex] != null;

                var presetName = canDelete
                    ? savedPresets[selectedPresetIndex].Name
                    : string.Empty;

                ImGui.TextWrapped(
                    canDelete
                        ? $"Are you sure you want to delete '{presetName}'?"
                        : "No valid preset is selected.");

                ImGui.Spacing();

                if (DrawStyledActionButton("Cancel##CancelDeleteAppearancePreset"))
                {
                    ImGui.CloseCurrentPopup();
                }

                ImGui.SameLine();

                if (DrawStyledActionButton("Delete Preset##ConfirmDeleteAppearancePreset"))
                {
                    if (canDelete)
                    {
                        savedPresets.RemoveAt(selectedPresetIndex);
                        selectedPresetIndex = -1;

                        configuration.Save();

                        presetStatusMessage =
                            $"Deleted preset '{presetName}'.";
                    }

                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }

            var viewport = ImGui.GetMainViewport();
            var updatePopupSize = new Vector2(440f, 100f);

            var updatePopupPosition = new Vector2(
                viewport.Pos.X + (viewport.Size.X - updatePopupSize.X) * 0.5f,
                viewport.Pos.Y + (viewport.Size.Y - updatePopupSize.Y) * 0.5f);

            ImGui.SetNextWindowPos(
                updatePopupPosition,
                ImGuiCond.Always);

            ImGui.SetNextWindowSize(
                updatePopupSize,
                ImGuiCond.Always);

            if (ImGui.BeginPopupModal("Update Appearance Preset###UpdateAppearancePreset",
                    ImGuiWindowFlags.AlwaysAutoResize))

            {
                var canUpdate =
                    selectedPresetIndex >= 0 &&
                    selectedPresetIndex < savedPresets.Count &&
                    savedPresets[selectedPresetIndex] != null;

                var presetName = canUpdate
                    ? savedPresets[selectedPresetIndex].Name ?? "Unnamed Preset"
                    : string.Empty;

                ImGui.TextWrapped(
                    canUpdate
                        ? $"Replace '{presetName}' with your current appearance?"
                        : "No valid preset is selecteda.");

                ImGui.Spacing();

                if (DrawStyledActionButton("Cancel##CancelUpdateAppearancePreset"))
                {
                    ImGui.CloseCurrentPopup();
                }

                ImGui.SameLine();

                if (DrawStyledActionButton("Update Preset##ConfirmUpdateAppearancePreset"))
                {
                    if (canUpdate)
                    {
                        savedPresets[selectedPresetIndex] =
                            CreateAppearancePreset(presetName);

                        configuration.Save();

                        presetStatusMessage =
                            $"Updated preset '{presetName}' with the current appearance.";
                    }

                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
            }

            if (!string.IsNullOrWhiteSpace(presetStatusMessage))
            {
                ImGui.Spacing();
                ImGui.Separator();

                ImGui.PushStyleColor(
                    ImGuiCol.Text,
                    new Vector4(0.93f, 0.72f, 0.37f, 1.00f));

                ImGui.TextWrapped(presetStatusMessage);

                ImGui.PopStyleColor();
            }
        }
        else if (selectedWorkspace == 3)
        {
            DrawWorkspaceTitle("Settings");
            ImGui.TextWrapped(
                "General plugin settings and maintenance options will be organized here.");
        }
        ImGui.EndChild(); 
        // End of ChatterSpace Workspace.

        ImGui.EndGroup();
    }

    private void DrawNavigationItem(string label, int workspace)
    {
        var isSelected = selectedWorkspace == workspace;

        var textColor = isSelected
            ? new Vector4(0.95f, 0.78f, 0.48F, 1.00f)
            : new Vector4(0.82f, 0.87f, 0.94f, 1.00f);

        ImGui.PushStyleColor(ImGuiCol.Text, textColor);

        if (ImGui.Selectable(
            $"{(isSelected ? "> " : " ")}{label}",
            isSelected,
            ImGuiSelectableFlags.None,
            new Vector2(ImGui.GetContentRegionAvail().X, 0)))
        {
            selectedWorkspace = workspace;
        }

        ImGui.PopStyleColor();
    }

    private static void DrawWorkspaceTitle(string title)
    {
        ImGui.Spacing();

        // Use the native font size for crisp text.
        ImGui.PushStyleColor(
            ImGuiCol.Text,
            new Vector4(0.60f, 0.84f, 0.98f, 1.00f));

        ImGui.TextUnformatted(title);

        ImGui.PopStyleColor();

        ImGui.Spacing();

        // Restrained crystal-gold divider.
        ImGui.PushStyleColor(
            ImGuiCol.Separator,
            new Vector4(0.38f, 0.31f, 0.20f, 0.85f));

        ImGui.Separator();

        ImGui.PopStyleColor();

        ImGui.Spacing();
    }

    private static bool DrawStyleCollapsingHeader(string label, ImGuiTreeNodeFlags flags)
    {
        // Midnight-blue panel with aether-blue interaction state.
        ImGui.PushStyleColor(
            ImGuiCol.Header,
            new Vector4(0.065f, 0.110f, 0.170f, 1.00f));

        ImGui.PushStyleColor(
            ImGuiCol.HeaderHovered,
            new Vector4(0.090f, 0.240f, 0.340f, 1.00f));

        ImGui.PushStyleColor(
            ImGuiCol.HeaderActive,
            new Vector4(0.120f, 0.310f, 0.420f, 1.00f));

        // Give the header a softer, panel-like shape.
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 5f);
        ImGui.PushStyleVar(ImGuiStyleVar.FramePadding,
            new Vector2(10f, 7f));

        var isExpanded = ImGui.CollapsingHeader(
            label, 
            flags | ImGuiTreeNodeFlags.Framed);

        // Restore the shared theme after drawing the header.
        ImGui.PopStyleVar(2);
        ImGui.PopStyleColor(3);

        return isExpanded;
    }

    private static bool DrawStyledActionButton(string label, bool useGoldAccent = false)
    {
        if (useGoldAccent)
        {
            // Crystal-gold action button.
            ImGui.PushStyleColor(
                ImGuiCol.Button,
                new Vector4(0.38f, 0.28f, 0.12f, 1.00f));

            ImGui.PushStyleColor(
                ImGuiCol.ButtonHovered,
                new Vector4(0.52f, 0.38f, 0.15f, 1.00f));

            ImGui.PushStyleColor(
                ImGuiCol.ButtonActive,
                new Vector4(0.65f, 0.47f, 0.19f, 1.00f));

            ImGui.PushStyleColor(
                ImGuiCol.Text,
                new Vector4(0.98f, 0.92f, 0.79f, 1.00f));
        }

        // Draw the button regardless of which style is selected.
        var clicked = ImGui.Button(label);

        // Restore the colors only when the gold style was applied.
        if (useGoldAccent)
        {
            ImGui.PopStyleColor(4);
        }

        // Return the button result on every code path.
        return clicked;
    }

    private static bool DrawStyledDangerButton(string label)
    {
        ImGui.PushStyleColor(
            ImGuiCol.Button,
            new Vector4(0.30f, 0.095f, 0.12f, 1.00f));

        ImGui.PushStyleColor(
            ImGuiCol.ButtonHovered,
            new Vector4(0.44f, 0.12f, 0.15f, 1.00f));

        ImGui.PushStyleColor(
            ImGuiCol.ButtonActive,
            new Vector4(0.56f, 0.15f, 0.18f, 1.00f));

        ImGui.PushStyleColor(
            ImGuiCol.Text,
            new Vector4(0.98f, 0.89f, 0.87f, 1.00f));

        var clicked = ImGui.Button(label);

        ImGui.PopStyleColor(4);

        return clicked;
    }
    private static void DrawSectionLabel(string label)
    {
        ImGui.PushStyleColor(
            ImGuiCol.Text,
            new Vector4(0.93f, 0.72f, 0.37f, 1.00f));

        ImGui.TextUnformatted(label);

        ImGui.PopStyleColor();
    }



    private static void DrawDialoguePreview(string childId, string speakerName, string dialogueText, Vector4 backgroundTint, float backgroundOpacity, Vector4 speakerColor, Vector4 dialogueTextColor)
    {
        // Preserve the configured background color and opacity.
        var previewBackground = backgroundTint;
        previewBackground.W = Math.Clamp(backgroundOpacity, 0f, 1f);

        // Give the sample dialogue a defined, aether-blue outline.
        ImGui.PushStyleColor(
            ImGuiCol.ChildBg,
            previewBackground);

        ImGui.PushStyleColor(
            ImGuiCol.Border,
            new Vector4(0.19f, 0.38f, 0.50f, 0.90f));

        // Use comfortable padding inside the preview panel.
        ImGui.PushStyleVar(
            ImGuiStyleVar.ChildRounding,
            6f);

        ImGui.PushStyleVar(
            ImGuiStyleVar.WindowPadding,
            new Vector2(14f, 10f));

        if (ImGui.BeginChild(
                childId,
                new Vector2(0f, 100f),
                true,
                ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse))
        {
            // Display the speaker only when the selected style has one.
            if (!string.IsNullOrWhiteSpace(speakerName))
            {
                ImGui.PushStyleColor(
                    ImGuiCol.Text,
                    speakerColor);

                ImGui.TextUnformatted(speakerName);

                ImGui.PopStyleColor();

                ImGui.Spacing();
            }

            // Render the dialogue using its configured text color.
            ImGui.PushStyleColor(
                ImGuiCol.Text,
                dialogueTextColor);

            ImGui.TextWrapped(dialogueText);

            ImGui.PopStyleColor();
        }

        ImGui.EndChild();

        // Restore the parent window's shared theme.
        ImGui.PopStyleVar(2);
        ImGui.PopStyleColor(2);
    }

    private bool DrawPreviewModeButton(string label, int style, float width)
    {
        var isSelected = livePreviewStyle == style;

        var buttonColor = isSelected
            ? new Vector4(0.38f, 0.28f, 0.12f, 1.00f)
            : new Vector4(0.075f, 0.150f, 0.220f, 1.00f);

        var hoveredColor = isSelected
            ? new Vector4(0.52f, 0.38f, 0.15f, 1.00f)
            : new Vector4(0.100f, 0.280f, 0.370f, 1.00f);

        var activeColor = isSelected
            ? new Vector4(0.65f, 0.47f, 0.19f, 1.00f)
            : new Vector4(0.120f, 0.360f, 0.460f, 1.00f);

        var textColor = isSelected
            ? new Vector4(0.98f, 0.92f, 0.79f, 1.00f)
            : new Vector4(0.890f, 0.920f, 0.970f, 1.00f);

        ImGui.PushStyleColor(ImGuiCol.Button, buttonColor);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, hoveredColor);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, activeColor);
        ImGui.PushStyleColor(ImGuiCol.Text, textColor);

        var clicked = ImGui.Button(
            label,
            new Vector2(width, 0f));

        ImGui.PopStyleColor(4);

        if (clicked)
            livePreviewStyle = style;

        return clicked;
    }

    private AppearancePreset CreateAppearancePreset(string name)
    {
        return new AppearancePreset
        {
            Name = name,

            StanadardDialogueEnabled = configuration.StandardDialogueEnabled,
            DarkDialogueEnabled = configuration.DarkDialogueEnabled,

            StandardBackgroundMode = configuration.StandardBackgroundMode,
            DarkBackgroundMode = configuration.DarkBackgroundMode,

            StandardBackgroundOpacity = configuration.StandardBackgroundOpacity,
            DarkBackgroundOpacity = configuration.DarkBackgroundOpacity,

            DialogueTextColor = configuration.DialogueTextColor,
            DarkDialogueTextColor = configuration.DarkDialogueTextColor,

            DialogueBackgroundTint = configuration.DialogueBackgroundTint,
            DarkDialogueBackgroundTint = configuration.DarkDialogueBackgroundTint,

            NpcNameColorEnabled = configuration.NpcNameColorEnabled,
            NpcNameColor = configuration.NpcNameColor,
        };
    }
    private void ApplyAppearancePreset(AppearancePreset preset)
    {
        configuration.StandardDialogueEnabled = preset.StanadardDialogueEnabled;
        configuration.DarkDialogueEnabled = preset.DarkDialogueEnabled;

        configuration.StandardBackgroundMode = preset.StandardBackgroundMode;
        configuration.DarkBackgroundMode = preset.DarkBackgroundMode;

        configuration.StandardBackgroundOpacity = preset.StandardBackgroundOpacity;
        configuration.DarkBackgroundOpacity = preset.DarkBackgroundOpacity;

        configuration.DialogueTextColor = preset.DialogueTextColor;
        configuration.DarkDialogueTextColor = preset.DarkDialogueTextColor;

        configuration.DialogueBackgroundTint = preset.DialogueBackgroundTint;
        configuration.DarkDialogueBackgroundTint = preset.DarkDialogueBackgroundTint;

        configuration.NpcNameColorEnabled = preset.NpcNameColorEnabled;
        configuration.NpcNameColor = preset.NpcNameColor;

        configuration.Save();
    }
}

