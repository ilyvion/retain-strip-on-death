using HarmonyLib;
using Verse;
using System.Collections.Generic;
using RimWorld;

namespace RetainStripOnDeath;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.Kill))]
internal static class Rimworld_Pawn_Kill
{
    internal static Dictionary<Pawn, bool> ShouldStripCorpseOfPawn = new();
    internal static Dictionary<Pawn, Corpse> PawnCorpse = new();

    private static void Prefix(ref Pawn __instance)
    {
        if (Find.CurrentMap.designationManager.DesignationOn(__instance, DesignationDefOf.Strip) != null)
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
        if (ShouldStripCorpseOfPawn[__instance])
        {
            var corpse = PawnCorpse[__instance];

            if (!Settings.AlsoOutsideHomeArea && corpse.Position.InBounds(corpse.Map) && !corpse.Map.areaManager.Home[corpse.Position])
            {
                // This corpse is outside the home area, and the setting to retain stripping
                // outside the home area is off, so do nothing
                // (i.e. let the game do what it does by default)
            }
            else if (corpse.AnythingToStrip())
            {
                // Mark corpse for stripping if pawn was marked for stripping and has anything to strip
                Find.CurrentMap.designationManager.AddDesignation(new Designation(corpse, DesignationDefOf.Strip));
            }
            PawnCorpse.Remove(__instance);
        }
        ShouldStripCorpseOfPawn.Remove(__instance);
    }
}
