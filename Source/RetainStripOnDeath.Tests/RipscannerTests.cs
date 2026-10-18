using DevTools.Testing;

namespace RetainStripOnDeath.Tests;

// Regression tests for GitHub issue #1: the ripscanner kills its occupant while holding it, off any
// map. Such a death used to leave the pawn's strip entry behind, so the next Kill of that pawn
// threw from the Kill prefix and aborted the scan, leaving no corpse and no subcore. A pawn loses
// its Strip designation on entering the scanner, so its corpse stays undesignated.
[TestFixture(TestType.Playing)]
[LoadIfBiotechActive]
internal sealed class RipscannerTests
{
    [Test]
    public static void RipscanningADesignatedPrisonerLeavesCorpseAndSubcore()
    {
        using var scene = new StripScene(Find.CurrentMap, alsoOutsideHomeArea: true);
        var scanner = SpawnRipscanner(scene);
        var pawn = scene.SpawnPawn(scene.Cell(3));
        scene.DesignateStrip(pawn);

        Ripscan(scene, scanner, pawn);

        StripScene.ExpectNoStripState();
    }

    [Test]
    public static void PawnKilledOffMapCanBeRipscannedAfterResurrection()
    {
        using var scene = new StripScene(Find.CurrentMap, alsoOutsideHomeArea: true);
        var scanner = SpawnRipscanner(scene);
        var pawn = scene.MakePawn();
        pawn.Kill(null);
        Expect.IsTrue(ResurrectionUtility.TryResurrect(pawn));
        _ = GenSpawn.Spawn(pawn, scene.Cell(3), Find.CurrentMap);

        Ripscan(scene, scanner, pawn);

        StripScene.ExpectNoStripState();
    }

    private static Building_SubcoreScanner SpawnRipscanner(StripScene scene)
    {
        var def = DefDatabase<ThingDef>.GetNamed("SubcoreRipscanner");
        var scanner = (Building_SubcoreScanner)
            GenSpawn.Spawn(
                ThingMaker.MakeThing(def, GenStuff.DefaultStuffFor(def)),
                scene.Cell(0),
                Find.CurrentMap
            );
        scanner.SetFaction(Faction.OfPlayer);
        scanner.GetComp<CompPowerTrader>().PowerOn = true;

        var start = scanner
            .GetGizmos()
            .OfType<Command_Action>()
            .First(command => command.defaultLabel == "SubcoreScannerStart".Translate());
        start.action();

        foreach (var ingredient in def.building.subcoreScannerFixedIngredients)
        {
            var thing = ThingMaker.MakeThing(ingredient.FixedIngredient);
            thing.stackCount = (int)ingredient.GetBaseCount();
            _ = scanner.GetDirectlyHeldThings().TryAdd(thing);
        }

        Expect.AreEqual(SubcoreScannerState.WaitingForOccupant, scanner.State);
        return scanner;
    }

    // Puts the pawn in the scanner as a prisoner and runs the scan to completion, checking that
    // the pawn's corpse and the subcore both end up on the map.
    private static void Ripscan(StripScene scene, Building_SubcoreScanner scanner, Pawn pawn)
    {
        var map = scanner.Map;
        var outputDef = scanner.def.building.subcoreScannerOutputDef;
        var subcoresBefore = map.listerThings.ThingsOfDef(outputDef).Count;
        pawn.guest.SetGuestStatus(Faction.OfPlayer, GuestStatus.Prisoner);

        scanner.TryAcceptPawn(pawn);
        Expect.ReferencesAreEqual(pawn, scanner.Occupant, "The scanner didn't take the pawn");
        scanner.fabricationTicksLeft = 1;
        scanner.DoTick();

        Expect.IsTrue(pawn.Dead, "The ripscanned pawn didn't die");
        Expect.IsNull(scanner.Occupant, "The pawn is still in the scanner");
        var corpse = pawn.Corpse;
        Expect.IsNotNull(corpse, "Ripscanning the pawn made no corpse");
        if (corpse != null)
        {
            Expect.IsTrue(corpse.Spawned, "The corpse wasn't dropped onto the map");
            Expect.IsFalse(scene.HasStrip(corpse));
        }
        Expect.AreEqual(
            subcoresBefore + 1,
            map.listerThings.ThingsOfDef(outputDef).Count,
            "The scan produced no subcore"
        );
    }
}
