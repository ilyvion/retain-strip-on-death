using DevTools.Testing;

namespace RetainStripOnDeath.Tests;

[TestFixture(TestType.Playing)]
internal sealed class StripOnDeathTests
{
    [TestCase(true, true, true)]
    [TestCase(true, false, true)]
    [TestCase(false, true, true)]
    [TestCase(false, false, false)]
    public static void DesignatedPawnsCorpseIsDesignatedPerHomeAreaSetting(
        bool alsoOutsideHomeArea,
        bool inHomeArea,
        bool expectStrip
    )
    {
        using var scene = new StripScene(Find.CurrentMap, alsoOutsideHomeArea);
        var cell = scene.Cell(0);
        scene.SetHome(cell, inHomeArea);
        var pawn = scene.SpawnPawn(cell);
        scene.DesignateStrip(pawn);

        var corpse = StripScene.Kill(pawn);

        if (corpse != null)
        {
            Expect.AreEqual(expectStrip, scene.HasStrip(corpse));
        }
        StripScene.ExpectNoStripState();
    }

    [TestCase(true, true)]
    [TestCase(true, false)]
    [TestCase(false, true)]
    [TestCase(false, false)]
    public static void UndesignatedPawnsCorpseIsNotDesignated(
        bool alsoOutsideHomeArea,
        bool inHomeArea
    )
    {
        using var scene = new StripScene(Find.CurrentMap, alsoOutsideHomeArea);
        var cell = scene.Cell(0);
        scene.SetHome(cell, inHomeArea);
        var pawn = scene.SpawnPawn(cell);

        var corpse = StripScene.Kill(pawn);

        if (corpse != null)
        {
            Expect.IsFalse(scene.HasStrip(corpse));
        }
        StripScene.ExpectNoStripState();
    }

    [Test]
    public static void DesignatedPawnWithNothingToStripLeavesCorpseUndesignated()
    {
        using var scene = new StripScene(Find.CurrentMap, alsoOutsideHomeArea: true);
        var pawn = scene.SpawnPawn(scene.Cell(0));
        pawn.apparel.DestroyAll();
        pawn.inventory.DestroyAll();
        pawn.equipment.DestroyAllEquipment();
        scene.DesignateStrip(pawn);

        var corpse = StripScene.Kill(pawn);

        if (corpse != null)
        {
            Expect.IsFalse(corpse.AnythingToStrip(), "The corpse still had something to strip");
            Expect.IsFalse(scene.HasStrip(corpse));
        }
        StripScene.ExpectNoStripState();
    }

    [Test]
    public static void EachCorpseKeepsItsOwnPawnsDesignation()
    {
        using var scene = new StripScene(Find.CurrentMap, alsoOutsideHomeArea: true);
        var designated = scene.SpawnPawn(scene.Cell(0));
        var undesignated = scene.SpawnPawn(scene.Cell(1));
        scene.DesignateStrip(designated);

        var designatedCorpse = StripScene.Kill(designated);
        var undesignatedCorpse = StripScene.Kill(undesignated);

        if (designatedCorpse != null)
        {
            Expect.IsTrue(scene.HasStrip(designatedCorpse), "The designated pawn's corpse");
        }
        if (undesignatedCorpse != null)
        {
            Expect.IsFalse(scene.HasStrip(undesignatedCorpse), "The undesignated pawn's corpse");
        }
        StripScene.ExpectNoStripState();
    }
}
