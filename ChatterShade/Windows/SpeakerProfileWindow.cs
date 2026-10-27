using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace ChatterShade.Windows;

public class SpeakerProfileWindow : Window, IDisposable
{
    private readonly Configuration configuration;
    private int selectedProfileIndex = -1;
    private string speakerSearch = string.Empty;
    private bool isAddingProfile;

    private string editingSpeakerName = string.Empty;
    private Vector4 editingNameColor = Vector4.One;
    private bool editingProfileEnabled = true;
    private string profileSaveError = string.Empty;
    private Vector2 deletePopupCenter;

    public SpeakerProfileWindow(Plugin plugin)
        : base("Speaker Profile Editor###ChatterShadeSpeakerProfiles")
    {
        Flags = ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize;

        configuration = plugin.Configuration;
    }

    public void Dispose()
    {
    }

    public override void PreDraw()
    {
        // Midnight-navy surfaces.
        ImGui.PushStyleColor(ImGuiCol.WindowBg,
            new Vector4(0.035f, 0.045f, 0.075f, 1.00f));
        ImGui.PushStyleColor(ImGuiCol.ChildBg,
            new Vector4(0.050f, 0.070f, 0.10f, 1.00f));
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

        // Title bar.
        ImGui.PushStyleColor(ImGuiCol.TitleBg,
            new Vector4(0.035f, 0.060f, 0.105f, 1.00f));
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive,
            new Vector4(0.055f, 0.150f, 0.220f, 1.00f));
        ImGui.PushStyleColor(ImGuiCol.TitleBgCollapsed,
            new Vector4(0.030f, 0.050f, 0.080f, 1.00f));

        // Input fields.
        ImGui.PushStyleColor(ImGuiCol.FrameBg,
            new Vector4(0.075f, 0.100f, 0.150f, 1.00f));
        ImGui.PushStyleColor(ImGuiCol.FrameBgHovered,
            new Vector4(0.100f, 0.210f, 0.290f, 1.00f));
        ImGui.PushStyleColor(ImGuiCol.FrameBgHovered,
            new Vector4(0.120f, 0.290f, 0.370f, 1.00f));

        // Buttons.
        ImGui.PushStyleColor(ImGuiCol.Button,
            new Vector4(0.075f, 0.150f, 0.220f, 1.00f));
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered,
            new Vector4(0.100f, 0.280f, 0.370f, 1.00f));
        ImGui.PushStyleColor(ImGuiCol.ButtonActive,
            new Vector4(0.120f, 0.360f, 0.460f, 1.00f));

        // Selection and interactive headers.
        ImGui.PushStyleColor(ImGuiCol.Header,
            new Vector4(0.075f, 0.180f, 0.270f, 0.95f));
        ImGui.PushStyleColor(ImGuiCol.HeaderHovered,
            new Vector4(0.100f, 0.290f, 0.390f, 1.00f));
        ImGui.PushStyleColor(ImGuiCol.HeaderActive,
            new Vector4(0.120f, 0.360f, 0.460f, 1.00f));

        // Separators and accents.
        ImGui.PushStyleColor(ImGuiCol.Separator,
            new Vector4(0.140f, 0.230f, 0.330f, 0.55f));
        ImGui.PushStyleColor(ImGuiCol.CheckMark,
            new Vector4(0.930f, 0.720f, 0.370f, 1.00f));
        ImGui.PushStyleColor(ImGuiCol.SliderGrab,
            new Vector4(0.580f, 0.680f, 0.770f, 1.00f));
        ImGui.PushStyleColor(ImGuiCol.SliderGrabActive,
            new Vector4(0.930f, 0.720f, 0.370f, 1.00f));

        // Scrollbars and selected text.
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

        // Rounded surfaces and consistent spacing.
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
        ImGui.PopStyleVar(14);
        ImGui.PopStyleColor(28);
    }
    
    public override void Draw()
    {
        DrawEditorTitle("Saved Speaker Profiles");

        ImGui.Separator();
        ImGui.Spacing();

        ImGui.TextWrapped(
            "Select a saved NPC speaker to view their individual name color.");

        ImGui.Spacing();

        var profiles = configuration.SpeakerProfiles;

        if (profiles == null)
        {
            ImGui.TextDisabled(
                "Speaker profile collection is unavailable.");

            return;
        }

        if (profiles.Count == 0)
        {
            selectedProfileIndex = -1;

            ImGui.TextDisabled(
                "No speaker profiles have been created yet.");
        }

        DrawEditorSectionLabel("PROFILE LIBRARY");
        ImGui.TextDisabled($"Saved profiles: {profiles.Count}");
        ImGui.Spacing();

        if (DrawEditorButton("Add Speaker", useGoldAccent: true))
        {
            isAddingProfile = true;

            editingSpeakerName = string.Empty;
            editingNameColor = Vector4.One;
            editingProfileEnabled = true;

            profileSaveError = string.Empty;

            ImGui.OpenPopup("Speaker Profile###SpeakerProfilePopup");
        }

        ImGui.Spacing();

        ImGui.SetNextItemWidth(
            string.IsNullOrEmpty(speakerSearch)
            ? -1f
             : ImGui.GetContentRegionAvail().X
              - 55f
              - ImGui.GetStyle().ItemSpacing.X);

        ImGui.InputTextWithHint(
            "##SpeakerSearch",
            "Search speaker profiles...",
            ref speakerSearch,
            100);

        if (!string.IsNullOrEmpty(speakerSearch))
        {
            ImGui.SameLine();

            if (ImGui.Button("Clear##SpeakerSearch"))
            {
                speakerSearch = string.Empty;
            }
        }

        ImGui.Spacing();

        var matchingProfileCount = 0;

        var visibleProfileCount = 0;

        foreach (var profile in profiles)
        {
            if (profile == null)
                continue;

            var displayName = string.IsNullOrWhiteSpace(profile.SpeakerName)
                ? "Unnamed Speaker"
                : profile.SpeakerName;

            if (!string.IsNullOrWhiteSpace(speakerSearch) &&
                !displayName.Contains(
                    speakerSearch.Trim(),
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            visibleProfileCount++;
        }

        // Allow space for at least three rows and up to eight rows.
        var rowHeight = ImGui.GetFrameHeightWithSpacing();
        var displayedRows = Math.Clamp(visibleProfileCount, 3, 8);
        var profileListHeight = displayedRows * rowHeight
                               + ImGui.GetStyle().WindowPadding.Y * 2f;

        // Profile selection list.
        if (ImGui.BeginChild(
                "SpeakerProfileList",
                new Vector2(0, profileListHeight),
                true))
        {
            for (var i = 0; i < profiles.Count; i++)
            {
                var profile = profiles[i];

                if (profile == null)
                    continue;


                var displayName = string.IsNullOrWhiteSpace(profile.SpeakerName)
                    ? "Unnamed Speaker"
                    : profile.SpeakerName;

                // Filter before drawing any UI elements.
                if (!string.IsNullOrWhiteSpace(speakerSearch) &&
                    !displayName.Contains(
                        speakerSearch.Trim(),
                        StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Only matching profiles reach this point.
                matchingProfileCount++;

                ImGui.PushID(i);

                ImGui.ColorButton(
                    "##SpeakerColor",
                    profile.NameColor,
                    ImGuiColorEditFlags.NoTooltip,
                    new Vector2(22, 22));

                ImGui.SameLine();

                var isSelected = selectedProfileIndex == i;

                if (ImGui.Selectable(displayName, isSelected))
                {
                    selectedProfileIndex = i;
                }

                if (!profile.Enabled)
                {
                    ImGui.SameLine();
                    ImGui.TextDisabled("(Disabled)");
                }

                ImGui.PopID();
            }

            if (matchingProfileCount == 0 &&
                profiles.Count > 0 &&
                !string.IsNullOrWhiteSpace(speakerSearch))
            {
                ImGui.TextDisabled("No matching speaker profiles found.");
            }
        }

        ImGui.EndChild();

        ImGui.Spacing();
        ImGui.Separator();

        // Visibility Search Check.
        bool selectedProfileMatchesSearch =
            selectedProfileIndex >= 0 &&
            selectedProfileIndex < profiles.Count &&
            profiles[selectedProfileIndex] != null &&
            (string.IsNullOrWhiteSpace(speakerSearch) ||
             (profiles[selectedProfileIndex].SpeakerName ?? string.Empty)
                 .Contains(speakerSearch.Trim(), StringComparison.OrdinalIgnoreCase));

        // Selected profile information.
        if (selectedProfileMatchesSearch)
        {
            var selectedProfile = profiles[selectedProfileIndex];

            if (selectedProfile != null)
            {
                DrawEditorSectionLabel("SELECTED SPEAKER");
                ImGui.Spacing();

                ImGui.TextUnformatted(
                    $"Name: {selectedProfile.SpeakerName}");

                ImGui.TextUnformatted(
                    $"Status: {(selectedProfile.Enabled ? "Enabled" : "Disabled")}");

                ImGui.ColorButton(
                    "##SelectedSpeakerColor",
                    selectedProfile.NameColor,
                    ImGuiColorEditFlags.NoTooltip,
                    new Vector2(24, 24));

                ImGui.SameLine();
                ImGui.TextUnformatted("Saved name color");

                ImGui.Spacing();

                if (DrawEditorButton("Edit Selected"))
                {
                    isAddingProfile = false;

                    editingSpeakerName = selectedProfile.SpeakerName;
                    editingNameColor = selectedProfile.NameColor;
                    editingProfileEnabled = selectedProfile.Enabled;

                    profileSaveError = string.Empty;

                    ImGui.OpenPopup("Speaker Profile###SpeakerProfilePopup");
                }

                ImGui.SameLine();

                if (DrawEditorButton("Delete Selected", useGoldAccent: true))
                {
                    var editorPosition = ImGui.GetWindowPos();
                    var editorSize = ImGui.GetWindowSize();

                    deletePopupCenter = editorPosition + editorSize * 0.5f;

                    ImGui.OpenPopup("Delete Speaker Profile");
                }
            }
        }
        else
        {
            ImGui.TextDisabled(selectedProfileIndex >= 0 &&
                !string.IsNullOrWhiteSpace(speakerSearch)
                ? "The selected speaker is hidden by the current search."
                : "Select a speaker above to view their settings.");
        }

        var popupTitle = isAddingProfile
            ? "Add Speaker Profile###SpeakerProfilePopup"
            : "Edit Speaker Profile###SpeakerProfilePopup";

        if (ImGui.BeginPopupModal(
            popupTitle,
            ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.TextUnformatted(
                isAddingProfile ? "Add Speaker" : "Edit Speaker");

            ImGui.Separator();
            ImGui.Spacing();

            ImGui.InputText(
                "Speaker name",
                ref editingSpeakerName,
                100);

            ImGui.ColorEdit4(
                "NPC name color",
                ref editingNameColor,
                ImGuiColorEditFlags.NoAlpha);

            ImGui.Checkbox(
                "Profile enabled",
                ref editingProfileEnabled);

            ImGui.Spacing();
            ImGui.Separator();

            if (ImGui.Button("Cancel"))
            {
                ImGui.CloseCurrentPopup();
            }

            ImGui.SameLine();

            if (DrawEditorButton("Save Changes", useGoldAccent: true))
            {
                profileSaveError = string.Empty;

                var trimmedName = editingSpeakerName.Trim();

                if (profiles == null)
                {
                    profileSaveError = "Profile collection is unavailable.";
                }
                else if (string.IsNullOrWhiteSpace(trimmedName))
                {
                    profileSaveError = "Speaker name cannot be empty.";
                }
                else
                {
                    var duplicateExists = profiles.Exists(profile =>
                        profile != null &&
                        (!isAddingProfile &&
                         selectedProfileIndex >= 0 &&
                         selectedProfileIndex < profiles.Count
                            ? !ReferenceEquals(profile, profiles[selectedProfileIndex])
                            : true) && string.Equals(
                            profile.SpeakerName?.Trim(),
                            trimmedName,
                            StringComparison.OrdinalIgnoreCase));

                    if (duplicateExists)
                    {
                        profileSaveError =
                            "A profile with this speaker name already exists.";
                    }
                    else if (isAddingProfile)
                    {
                        // Create a new speaker profile.
                        var newProfile = new SpeakerProfile
                        {
                            SpeakerName = trimmedName,
                            NameColor = editingNameColor,
                            Enabled = editingProfileEnabled
                        };

                        profiles.Add(newProfile);
                        selectedProfileIndex = profiles.Count - 1;

                        configuration.Save();
                        ImGui.CloseCurrentPopup();
                    }
                    else if (selectedProfileIndex < 0 ||
                             selectedProfileIndex >= profiles.Count ||
                             profiles[selectedProfileIndex] == null)
                    {
                        profileSaveError =
                            "No valid speaker profile is selected.";
                    }
                    else
                    {
                        // Update the existing speaker profile.
                        var selectedProfile = profiles[selectedProfileIndex];

                        selectedProfile.SpeakerName = trimmedName;
                        selectedProfile.NameColor = editingNameColor;
                        selectedProfile.Enabled = editingProfileEnabled;

                        configuration.Save();
                        ImGui.CloseCurrentPopup();
                    }
                }
            }

            if (!string.IsNullOrEmpty(profileSaveError))
            {
                ImGui.Spacing();
                ImGui.TextWrapped(profileSaveError);
            }

            ImGui.EndPopup();
        }


        ImGui.SetNextWindowPos(
            deletePopupCenter,
            ImGuiCond.Always,
            new Vector2(0.5f, 0.5f));

        ImGui.SetNextWindowSize(
            new Vector2(390, 115),
            ImGuiCond.Always);

        if (ImGui.BeginPopupModal(
                "Delete Speaker Profile",
                ImGuiWindowFlags.NoResize))

        {

            if (profiles != null &&
                selectedProfileIndex >= 0 &&
                selectedProfileIndex < profiles.Count &&
                profiles[selectedProfileIndex] != null)
            {
                var profileToDelete = profiles[selectedProfileIndex];

                ImGui.TextWrapped(
                    $"Are you sure you want to delete the profile for " +
                    $"{profileToDelete.SpeakerName}?");

                ImGui.Spacing();

                ImGui.TextWrapped(
                    "This will remove the speaker's custom color settings.");

                ImGui.Separator();
                ImGui.Spacing();

                if (DrawEditorButton("Canel##Delete"))
                {
                    ImGui.CloseCurrentPopup();
                }

                ImGui.SameLine();

                if (DrawEditorButton("Delete Profile", useGoldAccent: true))
                {
                    profiles.RemoveAt(selectedProfileIndex);
                    selectedProfileIndex = -1;

                    configuration.Save();

                    ImGui.CloseCurrentPopup();
                }
            }
            else
            {
                ImGui.TextDisabled("No valid speaker profile selected.");

                if (ImGui.Button("Close##Delete"))
                    ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }
    }

    private static void DrawEditorTitle(string title)
    {
        ImGui.Spacing();

        ImGui.PushStyleColor(
            ImGuiCol.Text,
            new Vector4(0.60f, 0.84f, 0.98f, 1.00f));

        ImGui.TextUnformatted(title);

        ImGui.PopStyleColor();

        ImGui.Spacing();

        ImGui.PushStyleColor(
            ImGuiCol.Separator,
            new Vector4(0.38f, 0.31f, 0.20f, 0.85f));

        ImGui.Separator();

        ImGui.PopStyleColor();

        ImGui.Spacing();
    }

    private static void DrawEditorSectionLabel(string label)
    {
        ImGui.PushStyleColor(
            ImGuiCol.Text,
            new Vector4(0.93f, 0.72f, 0.37f, 1.00f));

        ImGui.TextUnformatted(label);

        ImGui.PopStyleColor();
    }

    private static bool DrawEditorButton(string label, bool useGoldAccent = false, bool useDangerAccent = false)
    {
        var buttonColor = useGoldAccent
            ? new Vector4(0.38f, 0.28f, 0.12f, 1.00f)
            : useDangerAccent
              ? new Vector4(0.30f, 0.095f, 0.12f, 1.00f)
              : new Vector4(0.075f, 0.15f, 0.220f, 1.00f);

        var hoveredColor = useGoldAccent
            ? new Vector4(0.52f, 0.38f, 0.15f, 1.00f)
            : useDangerAccent
               ? new Vector4(0.44f, 0.12f, 0.15f, 1.00f)
               : new Vector4(0.100f, 0.280f, 0.370f, 1.00f);

        var activeColor = useGoldAccent
            ? new Vector4(0.65f, 0.47f, 0.19f, 1.00f)
            : useDangerAccent
               ? new Vector4(0.56f, 0.15f, 0.18f, 1.00f)
               : new Vector4(0.120f, 0.360f, 0.460f, 1.00f);

        var textColor = useGoldAccent
            ? new Vector4(0.98f, 0.92f, 0.79f, 1.00f)
            : useDangerAccent
               ? new Vector4(0.98f, 0.89f, 0.87f, 1.00f)
               : new Vector4(0.890f, 0.920f, 0.970f, 1.00f);

        ImGui.PushStyleColor(ImGuiCol.Button, buttonColor);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, hoveredColor);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, activeColor);
        ImGui.PushStyleColor(ImGuiCol.Text, textColor);

        var clicked = ImGui.Button(label);

        ImGui.PopStyleColor(4);

        return clicked;
    }
}


