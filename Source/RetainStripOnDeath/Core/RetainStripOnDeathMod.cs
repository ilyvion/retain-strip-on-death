namespace RetainStripOnDeath;

/// <summary>
/// The Retain Strip on Death mod.
/// </summary>
public class RetainStripOnDeathMod : Mod
{
    private readonly ModContentPack content;

    /// <summary>
    /// Applies the mod's Harmony patches and loads its settings.
    /// </summary>
    public RetainStripOnDeathMod(ModContentPack content)
        : base(content)
    {
        this.content = content;

        new Harmony(Constants.Id).PatchAll();

        _ = GetSettings<Settings>();
    }

    /// <inheritdoc/>
    public override void DoSettingsWindowContents(Rect inRect)
    {
        base.DoSettingsWindowContents(inRect);
        Settings.DoSettingsWindowContents(inRect);
    }

    /// <inheritdoc/>
    public override string SettingsCategory() => content.Name;

    /// <summary>
    /// Logs a message prefixed with the mod's name.
    /// </summary>
    public static void Message(string msg) => Log.Message("[Retain Strip on Death] " + msg);

    /// <summary>
    /// Logs a warning prefixed with the mod's name.
    /// </summary>
    public static void Warning(string msg) => Log.Warning("[Retain Strip on Death] " + msg);

    /// <summary>
    /// Logs an error prefixed with the mod's name.
    /// </summary>
    public static void Error(string msg) => Log.Error("[Retain Strip on Death] " + msg);

    /// <summary>
    /// Logs an error prefixed with the mod's name, only once per <paramref name="key"/>.
    /// </summary>
    public static void ErrorOnce(string msg, int key) =>
        Log.ErrorOnce("[Retain Strip on Death] " + msg, key);

    /// <summary>
    /// Logs a message prefixed with the mod's name, followed by <paramref name="e"/> as an error.
    /// </summary>
    public static void Exception(string msg, Exception? e = null)
    {
        Message(msg);
        if (e != null)
        {
            Log.Error(e.ToString());
        }
    }
}
