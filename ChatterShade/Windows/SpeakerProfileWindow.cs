using System;
using System.Numerics;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace ChatterShade.Windows;

public class SpeakerProfileWindow : Window, IDisposable
{
    private readonly Configuration configuration;
    private int selectedProfileIndex = -1;
    private bool isAddingProfiles;

    private string editingSpeakerName = string.Empty;
    private Vector4 editingNameColor = Vector4.One;
    private bool editingProfileEnabled = true;
    private string profileSaveError = string.Empty;

    public SpeakerProfileWindow(Plugin plugin)
        : base("Speaker Profile Editor###ChatterShadeSpeakerProfiles")
    {
        Size = new Vector2(650, 450);
        SizeCondition = ImGuiCond.FirstUseEver;

        Flags = ImGuiWindowFlags.NoCollapse;

        configuration = plugin.Configuration;
    }

    public void Dispose()
    {
    }


    public override void Draw()
    {
        ImGui.TextUnformatted("Saved Speaker Profiles");

        ImGui.Separator();
        ImGui.Spacing();

        ImGui.TextWrapped(
            "Select a saved NPC speaker to view their individual name color.");

        ImGui.Spacing();

        var profiles = configuration.SpeakerProfiles;

        if (profiles == null || profiles.Count == 0)
        {
            selectedProfileIndex = -1;

            ImGui.TextDisabled(
                "No speaker profiles have been created yet.");

            return;
        }

        ImGui.TextUnformatted($"Saved profiles: {profiles.Count}");
        ImGui.Spacing();

        // Profile selection list.
        if (ImGui.BeginChild(
                "SpeakerProfileList",
                new Vector2(0, 220),
                true))
        {
            for (var i = 0; i < profiles.Count; i++)
            {
                var profile = profiles[i];

                if (profile == null)
                    continue;

                ImGui.PushID(i);

                ImGui.ColorButton(
                    "##SpeakerColor",
                    profile.NameColor,
                    ImGuiColorEditFlags.NoTooltip,
                    new Vector2(22, 22));

                ImGui.SameLine();

                var displayName = string.IsNullOrWhiteSpace(profile.SpeakerName)
                    ? "Unnamed Speaker"
                    : profile.SpeakerName;

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
        }

        ImGui.EndChild();

        ImGui.Spacing();
        ImGui.Separator();

        // Selected profile information.
        if (selectedProfileIndex >= 0 &&
            selectedProfileIndex < profiles.Count)
        {
            var selectedProfile = profiles[selectedProfileIndex];

            if (selectedProfile != null)
            {
                ImGui.TextUnformatted("Selected Speaker");
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

                if (ImGui.Button("Edit Selected"))
                {
                    isAddingProfiles = false;

                    editingSpeakerName = selectedProfile.SpeakerName;
                    editingNameColor = selectedProfile.NameColor;
                    editingProfileEnabled = selectedProfile.Enabled;

                    profileSaveError = string.Empty;

                    ImGui.OpenPopup("Edit Speaker Profile");
                }    
            }
        }
        else
        {
            ImGui.TextDisabled(
                "Select a speaker above to view their settings.");
        }

        if (ImGui.BeginPopupModal(
            "Edit Speaker Profile",
            ImGuiWindowFlags.AlwaysAutoResize))
        {
            ImGui.TextUnformatted("Edit Speaker");

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

            if (ImGui.Button("Save Changes"))
            {
                // Profile Error Check System
                profileSaveError = string.Empty;

                var trimmedName = editingSpeakerName.Trim();

                if (profiles == null)
                {
                    profileSaveError = "Profile collection is unavailable.";
                }
                else if (selectedProfileIndex < 0 ||
                         selectedProfileIndex >= profiles.Count)
                {
                    profileSaveError = "No valid speaker profile is selected.";
                }
                else if (profiles[selectedProfileIndex] == null)
                {
                    profileSaveError = "The selected speaker profile is unavailable.";
                }
                else if (string.IsNullOrWhiteSpace(trimmedName))
                {
                    profileSaveError = "Speaker name cannot be empty.";
                }
                else
                {
                    var duplicateExists = profiles.Exists(profile =>
                        profile != null &&
                        !ReferenceEquals(profile, profiles[selectedProfileIndex]) &&
                        string.Equals(
                            profile.SpeakerName?.Trim(),
                            trimmedName,
                            StringComparison.OrdinalIgnoreCase));

                    if (duplicateExists)
                    {
                        profileSaveError = "A profile with this speaker name already exists.";
                    }
                    else
                    {
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
    }
}
