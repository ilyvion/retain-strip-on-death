using HarmonyLib;
using Verse;
using RimWorld;

namespace RetainStripOnDeath;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.MakeCorpse), new[] { typeof(Building_Grave), typeof(bool), typeof(float) })]
internal static class Rimworld_Pawn_MakeCorpse
{
    private static void Postfix(ref Pawn __instance, Corpse? __result)
    {
        // Just as a safeguard against mistakes
        if (!Rimworld_Pawn_Kill.ShouldStripCorpseOfPawn.TryGetValue(__instance, out var shouldStripCorpseOfPawn))
        {
            RetainStripOnDeathMod.Message($"Ended up in MakeCorpse.Postfix without a value for 'shouldStripCorpseOfPawn' for {__instance}.\nThis probably means they're spawned in directly _as a corpse_ and should be nothing to worry about, but we're making this note about it, just in case it was unexpected.");
            return;
        }

        // shouldStripCorpseOfPawn being null means we couldn't figure out their pre-death designation.
        // This probably means they died off map or pre-map (such as at map generation) at the start of a game.
        if (!shouldStripCorpseOfPawn.HasValue)
        {
            return;
        }

        if (shouldStripCorpseOfPawn.Value && __result != null)
        {
            Rimworld_Pawn_Kill.PawnCorpse.Add(__instance, __result);
        }
        else
        {
            Rimworld_Pawn_Kill.ShouldStripCorpseOfPawn[__instance] = false;
        }
    }
}
