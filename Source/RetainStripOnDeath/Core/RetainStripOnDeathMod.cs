using HarmonyLib;
using Verse;
using System;
using UnityEngine;

namespace RetainStripOnDeath;

public class RetainStripOnDeathMod : Mod
{
    private ModContentPack content;

    public RetainStripOnDeathMod(ModContentPack content) : base(content)
    {
        this.content = content;

        new Harmony(Constants.Id).PatchAll();

        GetSettings<Settings>();
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
        base.DoSettingsWindowContents(inRect);
        Settings.DoSettingsWindowContents(inRect);
    }

    public override string SettingsCategory()
    {
        return content.Name;
    }

    public static void Message(string msg)
    {
        Log.Message("[Retain Strip on Death] " + msg);
    }

    public static void Warning(string msg)
    {
        Log.Warning("[Retain Strip on Death] " + msg);
    }

    public static void Error(string msg)
    {
        Log.Error("[Retain Strip on Death] " + msg);
    }

    public static void Exception(string msg, Exception? e = null)
    {
        Message(msg);
        if (e != null)
        {
            Log.Error(e.ToString());
        }
    }
}
