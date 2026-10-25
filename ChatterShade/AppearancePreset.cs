using System;
using System.Numerics;

namespace ChatterShade;

[Serializable]
public class AppearancePreset
{
    public string Name { get; set; } = string.Empty;
    public bool StanadardDialogueEnabled { get; set; } = true;
    public bool DarkDialogueEnabled { get; set; } = true;
    public DialogueBackgroundMode StandardBackgroundMode { get; set; }
         = DialogueBackgroundMode.SolidColor;
    public DialogueBackgroundMode DarkBackgroundMode { get; set; }
         = DialogueBackgroundMode.SolidColor;
    public float StandardBackgroundOpacity { get; set; } = 1f;
    public float DarkBackgroundOpacity { get; set;} = 1f;

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
