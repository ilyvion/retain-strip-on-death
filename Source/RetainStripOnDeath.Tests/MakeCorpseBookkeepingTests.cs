using DevTools.Testing;

namespace RetainStripOnDeath.Tests;

[TestFixture(TestType.Playing)]
internal sealed class MakeCorpseBookkeepingTests
{
    private static Corpse? MakeCorpse(Pawn pawn) => pawn.MakeCorpse(null, false, 0f);

    [Test]
    public static void CorpseMadeOutsideKillLeavesNoStripState()
    {
        using var scene = new StripScene(Find.CurrentMap, alsoOutsideHomeArea: true);
        var pawn = scene.MakePawn();

        var corpse = MakeCorpse(pawn);

        Expect.IsNotNull(corpse);
        StripScene.ExpectNoStripState();
    }

    [Test]
    public static void UnknownDesignationIsLeftAlone()
    {
        using var scene = new StripScene(Find.CurrentMap, alsoOutsideHomeArea: true);
        var pawn = scene.MakePawn();
        Rimworld_Pawn_Kill.ShouldStripCorpseOfPawn[pawn] = null;

        _ = MakeCorpse(pawn);

        Expect.IsTrue(Rimworld_Pawn_Kill.ShouldStripCorpseOfPawn.TryGetValue(pawn, out var entry));
        Expect.IsFalse(entry.HasValue);
        Expect.IsEmpty(Rimworld_Pawn_Kill.PawnCorpse);
    }

    [Test]
    public static void DesignatedPawnsCorpseIsRecorded()
    {
        using var scene = new StripScene(Find.CurrentMap, alsoOutsideHomeArea: true);
        var pawn = scene.MakePawn();
        Rimworld_Pawn_Kill.ShouldStripCorpseOfPawn[pawn] = true;

        var corpse = MakeCorpse(pawn);

        Expect.IsNotNull(corpse);
        Expect.AreEqual<bool?>(true, Rimworld_Pawn_Kill.ShouldStripCorpseOfPawn[pawn]);
        Expect.IsTrue(Rimworld_Pawn_Kill.PawnCorpse.TryGetValue(pawn, out var recorded));
        Expect.ReferencesAreEqual(corpse, recorded);
    }

    [Test]
    public static void UndesignatedPawnsCorpseIsNotRecorded()
    {
        using var scene = new StripScene(Find.CurrentMap, alsoOutsideHomeArea: true);
        var pawn = scene.MakePawn();
        Rimworld_Pawn_Kill.ShouldStripCorpseOfPawn[pawn] = false;

        _ = MakeCorpse(pawn);

        Expect.AreEqual<bool?>(false, Rimworld_Pawn_Kill.ShouldStripCorpseOfPawn[pawn]);
        Expect.IsEmpty(Rimworld_Pawn_Kill.PawnCorpse);
    }

    [Test]
    [WarningsAllowed("We can't make corpse because the pawn is in a ThingOwner")]
    public static void DesignatedPawnWithoutACorpseIsNoLongerToBeStripped()
    {
        using var scene = new StripScene(Find.CurrentMap, alsoOutsideHomeArea: true);
        var pawn = scene.MakePawn();
        var carrier = scene.MakePawn();
        Expect.IsTrue(carrier.inventory.innerContainer.TryAdd(pawn));
        Rimworld_Pawn_Kill.ShouldStripCorpseOfPawn[pawn] = true;

        var corpse = MakeCorpse(pawn);

        Expect.IsNull(corpse);
        Expect.AreEqual<bool?>(false, Rimworld_Pawn_Kill.ShouldStripCorpseOfPawn[pawn]);
        Expect.IsEmpty(Rimworld_Pawn_Kill.PawnCorpse);
    }

    // Kill skips MakeCorpse entirely for a race without a corpse def, so no corpse is recorded
    // for a designated pawn of such a race.
    [Test]
    public static void DesignatedPawnOfARaceWithoutCorpsesDies()
    {
        using var scene = new StripScene(Find.CurrentMap, alsoOutsideHomeArea: true);
        var pawn = scene.SpawnPawn(scene.Cell(0));
        scene.DesignateStrip(pawn);

        var race = pawn.def.race;
        var corpseDef = race.corpseDef;
        race.corpseDef = null;
        try
        {
            pawn.Kill(null);
        }
        finally
        {
            race.corpseDef = corpseDef;
        }

        Expect.IsTrue(pawn.Dead);
        Expect.IsNull(pawn.Corpse);
        StripScene.ExpectNoStripState();
    }
}
