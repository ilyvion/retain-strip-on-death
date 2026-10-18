namespace RetainStripOnDeath;

/// <summary>
/// The mod's settings.
/// </summary>
public class Settings : ModSettings
{
    private static bool _alsoOutsideHomeArea = true;

    internal static bool AlsoOutsideHomeArea
    {
        get => _alsoOutsideHomeArea;
        set => _alsoOutsideHomeArea = value;
    }

    /// <inheritdoc/>
    public override void ExposeData()
    {
        base.ExposeData();

        Scribe_Values.Look(ref _alsoOutsideHomeArea, "alsoOutsideHomeArea", true);
    }

    /// <summary>
    /// Draws the mod's settings window.
    /// </summary>
    public static void DoSettingsWindowContents(Rect inRect)
    {
        var listingStandard = new Listing_Standard();
        listingStandard.Begin(inRect);

        listingStandard.CheckboxLabeled(
            "RetainStripOnDeath.AlsoOutsideHomeAreaLabel".Translate(),
            ref _alsoOutsideHomeArea,
            "RetainStripOnDeath.AlsoOutsideHomeAreaToolTip".Translate()
        );

        listingStandard.End();
    }
}
