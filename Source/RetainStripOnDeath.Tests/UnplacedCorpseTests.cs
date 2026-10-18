using DevTools.Testing;

namespace RetainStripOnDeath.Tests;

// Guards against a NullReferenceException escaping Pawn.Kill when a designated pawn's corpse has
// no map by the time the Kill postfix runs.
[TestFixture(TestType.Playing)]
internal sealed class UnplacedCorpseTests
{
    [TestCase(true)]
    [TestCase(false)]
    public static void DesignatedPawnDiesWhenItsCorpseCannotBePlaced(bool alsoOutsideHomeArea)
    {
        using var scene = new StripScene(Find.CurrentMap, alsoOutsideHomeArea);
        var cell = scene.Cell(0);
        scene.SetHome(cell, false);
        var pawn = scene.SpawnPawn(cell);
        scene.DesignateStrip(pawn);

        using (
            new ScopedMethodHook(
                AccessTools.Method(
                    typeof(GenPlace),
                    nameof(GenPlace.TryPlaceThing),
                    [
                        typeof(Thing),
                        typeof(IntVec3),
                        typeof(Map),
                        typeof(ThingPlaceMode),
                        typeof(Action<Thing, int>),
                        typeof(Predicate<IntVec3>),
                        typeof(Rot4?),
                        typeof(int),
                    ]
                ),
                prefix: new HarmonyMethod(FailCorpsePlacement)
            )
        )
        {
            pawn.Kill(null);
        }

        Expect.IsTrue(pawn.Dead);
        StripScene.ExpectNoStripState();
    }

    [TestCase(true)]
    [TestCase(false)]
    public static void DesignatedPawnDiesWhenItsCorpseIsDestroyedDuringKill(
        bool alsoOutsideHomeArea
    )
    {
        using var scene = new StripScene(Find.CurrentMap, alsoOutsideHomeArea);
        var cell = scene.Cell(0);
        scene.SetHome(cell, false);
        var pawn = scene.SpawnPawn(cell);
        scene.DesignateStrip(pawn);

        using (
            new ScopedMethodHook(
                AccessTools.Method(typeof(Scenario), nameof(Scenario.Notify_PawnDied)),
                postfix: new HarmonyMethod(DestroyCorpse)
            )
        )
        {
            pawn.Kill(null);
        }

        Expect.IsTrue(pawn.Dead);
        Expect.IsTrue(pawn.Corpse is null or { Destroyed: true }, "The corpse wasn't destroyed");
        StripScene.ExpectNoStripState();
    }

    private static bool FailCorpsePlacement(Thing thing, ref bool __result)
    {
        if (thing is Corpse)
        {
            __result = false;
            return false;
        }
        return true;
    }

    private static void DestroyCorpse(Corpse corpse)
    {
        if (!corpse.Destroyed)
        {
            corpse.Destroy();
        }
    }
}
