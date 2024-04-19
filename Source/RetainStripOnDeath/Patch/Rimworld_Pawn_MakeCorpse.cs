using HarmonyLib;
using Verse;
using RimWorld;

namespace RetainStripOnDeath;

[HarmonyPatch(typeof(Pawn), nameof(Pawn.MakeCorpse), new[] { typeof(Building_Grave), typeof(bool), typeof(float) })]
internal static class Rimworld_Pawn_MakeCorpse
{
    private static void Postfix(ref Pawn __instance, Corpse? __result)
    {
        if (Rimworld_Pawn_Kill.ShouldStripCorpseOfPawn[__instance] && __result != null)
        {
            Rimworld_Pawn_Kill.PawnCorpse.Add(__instance, __result);
        }
        else
        {
            Rimworld_Pawn_Kill.ShouldStripCorpseOfPawn[__instance] = false;
        }
    }
}
