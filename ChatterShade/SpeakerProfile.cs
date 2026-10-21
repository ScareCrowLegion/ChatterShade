using System;
using System.Numerics;


namespace ChatterShade;

[Serializable]
public class SpeakerProfile
{
    public string SpeakerName { get; set; } = string.Empty;

    public bool Enabled { get; set; } = true;

    public Vector4 NameColor { get; set; } =
        new(1f, 1f, 1f, 1f);
}
