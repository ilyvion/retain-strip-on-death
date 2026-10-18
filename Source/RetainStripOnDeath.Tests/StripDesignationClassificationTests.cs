using DevTools.Testing;

namespace RetainStripOnDeath.Tests;

// Covers the 0.1.1 fix for pawns dying without a current map, which used to throw while reading
// the pawn's designation.
[TestFixture(TestType.Playing)]
internal sealed class StripDesignationClassificationTests
{
    [Test]
    public static void NoMapGivesNull()
    {
        var calls = 0;

        var result = Rimworld_Pawn_Kill.ClassifyStripDesignation(
            null,
            _ =>
            {
                calls++;
                return true;
            }
        );

        Expect.IsFalse(result.HasValue);
        Expect.AreEqual(0, calls);
    }

    [Test]
    public static void MapWithoutDesignationManagerGivesNull()
    {
        using var map = new Map();
        var calls = 0;

        var result = Rimworld_Pawn_Kill.ClassifyStripDesignation(
            map,
            _ =>
            {
                calls++;
                return true;
            }
        );

        Expect.IsFalse(result.HasValue);
        Expect.AreEqual(0, calls);
    }

    [TestCase(true)]
    [TestCase(false)]
    public static void MapWithDesignationManagerGivesWhetherThereIsAStrip(bool hasStrip)
    {
        var map = Find.CurrentMap;
        DesignationManager? asked = null;

        var result = Rimworld_Pawn_Kill.ClassifyStripDesignation(
            map,
            designationManager =>
            {
                asked = designationManager;
                return hasStrip;
            }
        );

        Expect.AreEqual<bool?>(hasStrip, result);
        Expect.ReferencesAreEqual(map.designationManager, asked);
    }
}
