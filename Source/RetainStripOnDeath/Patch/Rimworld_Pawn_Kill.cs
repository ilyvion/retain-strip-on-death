using HarmonyLib;
using Verse;
using System.Collections.Generic;
using RimWorld;

namespace RetainStripOnDeath;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
internal static class Rimworld_Pawn_Kill
{
    internal static Dictionary<Pawn, bool?> ShouldStripCorpseOfPawn = new();
    internal static Dictionary<Pawn, Corpse> PawnCorpse = new();

    private static void Prefix(ref Pawn __instance)
    {
        Map currentMap = __instance.Map;
        if (currentMap == null)
        {
            ShouldStripCorpseOfPawn.Add(__instance, null);
            return;
        }

        DesignationManager designationManager = currentMap.designationManager;
        if (designationManager == null)
        {
            ShouldStripCorpseOfPawn.Add(__instance, null);
            return;
        }

        if (designationManager.DesignationOn(__instance, DesignationDefOf.Strip) != null)
        {
            ShouldStripCorpseOfPawn.Add(__instance, true);
        }
        else
        {
            ShouldStripCorpseOfPawn.Add(__instance, false);
        }
    }

    private static void Postfix(ref Pawn __instance)
    {
        // Just as a safeguard against mistakes
        if (!ShouldStripCorpseOfPawn.TryGetValue(__instance, out var shouldStripCorpseOfPawn))
        {
            RetainStripOnDeathMod.ErrorOnce("Ended up in Kill.Postfix without a value for 'shouldStripCorpseOfPawn'. This shouldn't be possible and is a bug.", 0x420 + 0x69 + 0x120188);
            return;
        }

        // shouldStripCorpseOfPawn being null means we couldn't figure out their pre-death designation.
        // This probably means they died off map or pre-map (such as at map generation) at the start of a game.
        if (!shouldStripCorpseOfPawn.HasValue)
        {
            return;
        }

        if (shouldStripCorpseOfPawn.Value)
        {
            var corpse = PawnCorpse[__instance];
            var corpseMap = corpse.Map;
            var corpsePosition = corpse.Position;

            if (!Settings.AlsoOutsideHomeArea && corpsePosition.InBounds(corpseMap) && !corpseMap.areaManager.Home[corpsePosition])
            {
                // This corpse is outside the home area, and the setting to retain stripping
                // outside the home area is off, so do nothing
                // (i.e. let the game do what it does by default)
            }
            else if (corpse.AnythingToStrip())
            {
                // Mark corpse for stripping if pawn was marked for stripping and has anything to strip
                corpseMap.designationManager?.AddDesignation(new Designation(corpse, DesignationDefOf.Strip));
            }
            PawnCorpse.Remove(__instance);
        }
        ShouldStripCorpseOfPawn.Remove(__instance);
    }
}
