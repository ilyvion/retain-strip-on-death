using DevTools.Testing;

namespace RetainStripOnDeath.Tests;

// Regression tests for the 0.1.1 fix for pawns dying without a map: the patch's state for such a
// pawn was left behind after Kill, so killing the same pawn again threw from the Kill prefix and
// the pawn didn't die.
[TestFixture(TestType.Playing)]
internal sealed class OffMapKillTests
{
    private Map map = null!;
    private HashSet<Thing> thingsBefore = [];
    private Pawn? pawn;

    [SetUp]
    public void SetUp()
    {
        map = Find.CurrentMap;
        thingsBefore = [.. map.listerThings.AllThings];
        Expect.IsEmpty(
            Rimworld_Pawn_Kill.ShouldStripCorpseOfPawn,
            "An earlier test leaked a strip entry"
        );
        Expect.IsEmpty(Rimworld_Pawn_Kill.PawnCorpse, "An earlier test leaked a corpse entry");

        // Factionless, so killing it sends no letter and changes no goodwill.
        pawn = PawnGenerator.GeneratePawn(
            new PawnGenerationRequest(PawnKindDefOf.Villager, faction: null)
        );
    }

    [TearDown]
    public void TearDown()
    {
        if (pawn != null)
        {
            if (pawn.Corpse is { Destroyed: false } corpse)
            {
                corpse.Destroy();
            }
            else if (pawn.Spawned)
            {
                pawn.Destroy();
            }
        }
        foreach (var thing in map.listerThings.AllThings.Except(thingsBefore).ToList())
        {
            if (!thing.Destroyed)
            {
                thing.Destroy();
            }
        }
        Rimworld_Pawn_Kill.ShouldStripCorpseOfPawn.Clear();
        Rimworld_Pawn_Kill.PawnCorpse.Clear();
        pawn = null;
    }

    [Test]
    public void PawnKilledOffMapLeavesNoStripState()
    {
        pawn!.Kill(null);

        Expect.IsTrue(pawn.Dead);
        Expect.IsFalse(Rimworld_Pawn_Kill.ShouldStripCorpseOfPawn.ContainsKey(pawn));
        Expect.IsEmpty(Rimworld_Pawn_Kill.PawnCorpse);
    }

    [Test]
    public void PawnKilledOffMapCanBeKilledAgainAfterResurrection()
    {
        pawn!.Kill(null);
        var corpse = pawn.Corpse;
        Expect.IsNotNull(corpse, "Killing an unspawned pawn made no corpse");
        if (corpse == null)
        {
            return;
        }
        _ = GenSpawn.Spawn(corpse, map.Center, map);
        Expect.IsTrue(ResurrectionUtility.TryResurrect(pawn));
        Expect.IsTrue(pawn.Spawned, "Resurrecting a spawned corpse didn't spawn the pawn");

        pawn.Kill(null);

        Expect.IsTrue(pawn.Dead);
        Expect.IsEmpty(Rimworld_Pawn_Kill.ShouldStripCorpseOfPawn);
        Expect.IsEmpty(Rimworld_Pawn_Kill.PawnCorpse);
    }

    [Test]
    public void PawnKilledOffMapCanBeKilledAgainAfterBeingSpawnedAlive()
    {
        pawn!.Kill(null);
        Expect.IsTrue(ResurrectionUtility.TryResurrect(pawn));
        Expect.IsFalse(pawn.Spawned, "Resurrecting an unspawned corpse spawned the pawn");
        _ = GenSpawn.Spawn(pawn, map.Center, map);

        pawn.Kill(null);

        Expect.IsTrue(pawn.Dead);
        Expect.IsEmpty(Rimworld_Pawn_Kill.ShouldStripCorpseOfPawn);
        Expect.IsEmpty(Rimworld_Pawn_Kill.PawnCorpse);
    }
}
