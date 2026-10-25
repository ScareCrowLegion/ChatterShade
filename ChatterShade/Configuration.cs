using Dalamud.Configuration;
using System.Numerics;
using System;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;
using System.Collections.Generic;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs;

namespace ChatterShade;

public enum DialogueBackgroundMode
{
    OriginalTexture = 0,
    SolidColor = 1,
}

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool IsConfigWindowMovable { get; set; } = true;
    public bool Enabled { get; set; } = true;
    public bool StandardDialogueEnabled { get; set; } = true;
    public bool DarkDialogueEnabled { get; set; } = true;
    public DialogueBackgroundMode StandardBackgroundMode { get; set; } = DialogueBackgroundMode.SolidColor;
    public DialogueBackgroundMode DarkBackgroundMode { get; set; } = DialogueBackgroundMode.SolidColor;
    public float StandardBackgroundOpacity { get; set; } = 1f;
    public float DarkBackgroundOpacity { get; set; } = 1f;
    public Vector4 DialogueTextColor { get; set; } =
        new(242f / 255f, 242f / 255f, 242f / 255f, 1f);
    public bool NpcNameColorEnabled { get; set; } = true;
    public List<SpeakerProfile> SpeakerProfiles { get; set; } = new();
    public List<AppearancePreset> AppearancePresets { get; set; } = new();
    public Vector4 NpcNameColor { get; set; } =
        new(1f, 1f, 1f, 1f);
    public Vector4 DialogueBackgroundTint { get; set; } =
        new(48f / 255f, 48f / 255f, 48f / 255f, 1f);

    public Vector4 DarkDialogueTextColor { get; set; } =
        new(242f / 255f, 242f / 255f, 242f / 255f, 1f);

    public Vector4 DarkDialogueBackgroundTint { get; set; } =
        new(48f / 255f, 48f / 255f, 48f / 255f, 1f);

    // The below exists just to make saving less cumbersome
    private const int CurrentConfigurationVersion = 2;

    public void Migrate()
    {
        // Do not overwrite configuration from a newer plugin version.
        if (Version > CurrentConfigurationVersion)
            return;

        // Already up to date.
        if (Version == CurrentConfigurationVersion)
            return;

        // Upgrade configuration created before version 1.
        if (Version < 1)
        {
            // Preserve all existing user-selected colors, mode,
            // opacity values, and individual speaker profiles.

            // Repair a missing speaker profile collection without
            // replacing any existing profiles.
            SpeakerProfiles ??= new List<SpeakerProfile>();

            Version = 1;
        }

        // Add preset storage without replacing existing configuration.
        if (Version < 2)
        {
            AppearancePresets ??= new List<AppearancePreset>();
        }
        Save();
    }

    public void Save()
    {
      Plugin.PluginInterface.SavePluginConfig(this);
    }
}
