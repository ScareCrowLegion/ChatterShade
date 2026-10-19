using Dalamud.Configuration;
using System.Numerics;
using System;
using FFXIVClientStructs.FFXIV.Client.Game.InstanceContent;

namespace ChatterShade;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool IsConfigWindowMovable { get; set; } = true;
    public bool Enabled { get; set; } = true;
    public bool StandardDialogueEnabled { get; set; } = true;
    public bool DarkDialogueEnabled { get; set; } = true;
    public Vector4 DialogueTextColor { get; set; } =
        new(242f / 255f, 242f / 255f, 242f / 255f, 1f);
    public Vector4 DialogueBackgroundTint { get; set; } =
        new(48f / 255f, 48f / 255f, 48f / 255f, 1f);

    public Vector4 DarkDialogueTextColor { get; set; } =
        new(242f / 255f, 242f / 255f, 242f / 255f, 1f);

    public Vector4 DarkDialogueBackgroundTint { get; set; } =
        new(48f / 255f, 48f / 255f, 48f / 255f, 1f);

    // The below exists just to make saving less cumbersome
    public void Save()
    {
        Plugin.PluginInterface.SavePluginConfig(this);
    }
}
