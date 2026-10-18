namespace RetainStripOnDeath;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
internal static class Rimworld_Pawn_Kill
{
    internal static Dictionary<Pawn, bool?> ShouldStripCorpseOfPawn = [];
    internal static Dictionary<Pawn, Corpse> PawnCorpse = [];

    private static void Prefix(ref Pawn __instance)
    {
        var pawn = __instance;
        ShouldStripCorpseOfPawn[pawn] = ClassifyStripDesignation(
            pawn.Map,
            designationManager =>
                designationManager.DesignationOn(pawn, DesignationDefOf.Strip) != null
        );
    }

    /// <summary>
    /// Whether a pawn on <paramref name="map"/> has a Strip designation, as told by
    /// <paramref name="hasStrip"/>, or null if there is no map or designation manager to ask.
    /// </summary>
    internal static bool? ClassifyStripDesignation(
        Map? map,
        Func<DesignationManager, bool> hasStrip
    )
    {
        var designationManager = map?.designationManager;
        return designationManager == null ? null : hasStrip(designationManager);
    }

    private static void Postfix(ref Pawn __instance)
    {
        // Just as a safeguard against mistakes
        if (!ShouldStripCorpseOfPawn.TryGetValue(__instance, out var shouldStripCorpseOfPawn))
        {
            RetainStripOnDeathMod.ErrorOnce(
                "Ended up in Kill.Postfix without a value for 'shouldStripCorpseOfPawn'. This shouldn't be possible and is a bug.",
                0x420 + 0x69 + 0x120188
            );
            return;
        }

        // shouldStripCorpseOfPawn being null means we couldn't figure out their pre-death designation.
        // This probably means they died off map or pre-map (such as at map generation) at the start of a game.
        if (!shouldStripCorpseOfPawn.HasValue)
        {
            _ = ShouldStripCorpseOfPawn.Remove(__instance);
            return;
        }

        // Kill skips making a corpse for races without a corpse def, so there may be none
        if (shouldStripCorpseOfPawn.Value && PawnCorpse.TryGetValue(__instance, out var corpse))
        {
            var corpseMap = corpse.Map;
            var corpsePosition = corpse.Position;
            var inBounds = corpseMap != null && corpsePosition.InBounds(corpseMap);

            if (
                ShouldDesignateCorpse(
                    corpseSpawned: corpseMap != null,
                    Settings.AlsoOutsideHomeArea,
                    inBounds,
                    inHomeArea: inBounds && corpseMap!.areaManager.Home[corpsePosition],
                    corpse.AnythingToStrip
                )
            )
            {
                // Mark corpse for stripping if pawn was marked for stripping and has anything to strip
                corpseMap!.designationManager?.AddDesignation(
                    new Designation(corpse, DesignationDefOf.Strip)
                );
            }
            _ = PawnCorpse.Remove(__instance);
        }
        _ = ShouldStripCorpseOfPawn.Remove(__instance);
    }

    /// <summary>
    /// Whether the corpse of a pawn that was marked for stripping should be marked for stripping too.
    /// </summary>
    internal static bool ShouldDesignateCorpse(
        bool corpseSpawned,
        bool alsoOutsideHomeArea,
        bool inBounds,
        bool inHomeArea,
        Func<bool> anythingToStrip
    )
    {
        if (!corpseSpawned)
        {
            // The corpse couldn't be placed or was destroyed during Kill, so there's nothing
            // to designate
            return false;
        }

        if (!alsoOutsideHomeArea && inBounds && !inHomeArea)
        {
            // This corpse is outside the home area, and the setting to retain stripping
            // outside the home area is off, so do nothing
            // (i.e. let the game do what it does by default)
            return false;
        }

        return anythingToStrip();
    }
}
