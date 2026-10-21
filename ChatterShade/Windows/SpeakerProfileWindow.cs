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

        ImGui.TextUnformatted($"Saved profiles: {profiles.Count}");
        ImGui.Spacing();

        if (ImGui.Button("Add Speaker"))
        {
            isAddingProfile = true;

            editingSpeakerName = string.Empty;
            editingNameColor = Vector4.One;
            editingProfileEnabled = true;

            profileSaveError = string.Empty;

            ImGui.OpenPopup("Speaker Profile###SpeakerProfilePopup");
        }

        ImGui.Spacing();

        ImGui.SetNextItemWidth(-1f);
        ImGui.InputTextWithHint(
            "##SpeakerSearch",
            "Search speaker profiles...",
            ref speakerSearch,
            100);

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
                    isAddingProfile = false;

                    editingSpeakerName = selectedProfile.SpeakerName;
                    editingNameColor = selectedProfile.NameColor;
                    editingProfileEnabled = selectedProfile.Enabled;

                    profileSaveError = string.Empty;

                    ImGui.OpenPopup("Speaker Profile###SpeakerProfilePopup");
                }

                ImGui.SameLine();

                if (ImGui.Button("Delete Selected"))
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
            ImGui.TextDisabled(
                "Select a speaker above to view their settings.");
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

            if (ImGui.Button("Save Changes"))
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

            if (selectedProfileIndex >= 0 &&
                selectedProfileIndex < profiles.Count)
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

                if (ImGui.Button("Cancel##Delete"))
                {
                    ImGui.CloseCurrentPopup();
                }

                ImGui.SameLine();

                if (ImGui.Button("Delete Profile"))
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
}

