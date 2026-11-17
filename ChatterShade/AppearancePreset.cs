using System;
using System.Numerics;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace ChatterShade;

[Serializable]
public class AppearancePreset
{
    private bool standardDialogueEnabled = true;
    private bool standardDialogueEnabledWasSet;
    public string Name { get; set; } = string.Empty;
    public bool StandardDialogueEnabled
    {
        get => standardDialogueEnabled;
        set
        {
            standardDialogueEnabled = value;
            standardDialogueEnabledWasSet = true;
        }
    }

    // Accept the misspelled property from presets saved by older versions.
    // This Property is cleared after deserialization and is never written.
    // back into the saved configuration.
    [JsonProperty("StanadardDialogueEnabled", NullValueHandling = NullValueHandling.Ignore)]

    private bool? LegacyStandardDialogueEnabled { get; set; }

    [OnDeserialized]
    private void OnDeserialized(StreamingContext context)
    {
        if (!standardDialogueEnabledWasSet &&
            LegacyStandardDialogueEnabled.HasValue)
        {
            standardDialogueEnabled = LegacyStandardDialogueEnabled.Value;
        }

        // Prevent the legacy property from being serialized again.
        LegacyStandardDialogueEnabled = null;
    }

    public bool DarkDialogueEnabled { get; set; } = true;
    public DialogueBackgroundMode StandardBackgroundMode { get; set; } = DialogueBackgroundMode.SolidColor;
    public DialogueBackgroundMode DarkBackgroundMode { get; set; } = DialogueBackgroundMode.SolidColor;
    public float StandardBackgroundOpacity { get; set; } = 1f;
    public float DarkBackgroundOpacity { get; set; } = 1f;
    public Vector4 DialogueTextColor { get; set; }
        = new Vector4(242f / 255f, 242f / 255f, 242f / 255f, 1f);

    public Vector4 DarkDialogueTextColor { get; set; }
        = new Vector4(242f / 255f, 242f / 255f, 242f / 255f, 1f);

    public Vector4 DialogueBackgroundTint { get; set; } =
        new(48f / 255f, 48f / 255f, 48f / 255f, 1f);

    public Vector4 DarkDialogueBackgroundTint { get; set; } =
        new(48f / 255f, 48f / 255f, 48f / 255f, 1f);

    public bool NpcNameColorEnabled { get; set; } = true;

    public Vector4 NpcNameColor { get; set; } =
        new(1f, 1f, 1f, 1f);

}
