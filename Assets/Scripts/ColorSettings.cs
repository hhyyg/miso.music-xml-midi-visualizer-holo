using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorSettings
{
    public NoteColor C { get; set; }
    public NoteColor D { get; set; }
    public NoteColor E { get; set; }
    public NoteColor F { get; set; }
    public NoteColor G { get; set; }
    public NoteColor A { get; set; }
    public NoteColor B { get; set; }

    public static ColorSettings LoadColorSettingsByMaterials()
    {
        var settings = new ColorSettings();
        settings.C = LoadNoteColorByMaterial(Resources.Load<Material>("NoteSample/C"));
        settings.D = LoadNoteColorByMaterial(Resources.Load<Material>("NoteSample/D"));
        settings.E = LoadNoteColorByMaterial(Resources.Load<Material>("NoteSample/E"));
        settings.F = LoadNoteColorByMaterial(Resources.Load<Material>("NoteSample/F"));
        settings.G = LoadNoteColorByMaterial(Resources.Load<Material>("NoteSample/G"));
        settings.A = LoadNoteColorByMaterial(Resources.Load<Material>("NoteSample/A"));
        settings.B = LoadNoteColorByMaterial(Resources.Load<Material>("NoteSample/B"));
        return settings;
    }

    private static NoteColor LoadNoteColorByMaterial(Material material)
    {
        return new NoteColor() { MainColor = material.GetColor("_Color") };
    }
}
