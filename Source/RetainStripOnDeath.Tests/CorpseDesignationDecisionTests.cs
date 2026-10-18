using DevTools.Testing;

namespace RetainStripOnDeath.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class CorpseDesignationDecisionTests
{
    [TestCase(true, true, true, true, true, ExpectedResult = true)]
    [TestCase(true, true, true, true, false, ExpectedResult = false)]
    [TestCase(true, true, true, false, true, ExpectedResult = true)]
    [TestCase(true, true, true, false, false, ExpectedResult = false)]
    [TestCase(true, true, false, true, true, ExpectedResult = true)]
    [TestCase(true, true, false, true, false, ExpectedResult = false)]
    [TestCase(true, true, false, false, true, ExpectedResult = true)]
    [TestCase(true, true, false, false, false, ExpectedResult = false)]
    [TestCase(true, false, true, true, true, ExpectedResult = true)]
    [TestCase(true, false, true, true, false, ExpectedResult = false)]
    [TestCase(true, false, true, false, true, ExpectedResult = false)]
    [TestCase(true, false, true, false, false, ExpectedResult = false)]
    // Out of bounds skips the home area check
    [TestCase(true, false, false, true, true, ExpectedResult = true)]
    [TestCase(true, false, false, true, false, ExpectedResult = false)]
    [TestCase(true, false, false, false, true, ExpectedResult = true)]
    [TestCase(true, false, false, false, false, ExpectedResult = false)]
    [TestCase(false, true, true, true, true, ExpectedResult = false)]
    [TestCase(false, false, true, true, true, ExpectedResult = false)]
    public static bool DecidesFromSettingHomeAreaAndStrippability(
        bool corpseSpawned,
        bool alsoOutsideHomeArea,
        bool inBounds,
        bool inHomeArea,
        bool anythingToStrip
    ) =>
        Rimworld_Pawn_Kill.ShouldDesignateCorpse(
            corpseSpawned,
            alsoOutsideHomeArea,
            inBounds,
            inHomeArea,
            () => anythingToStrip
        );

    [TestCase(false, true, true, true)]
    [TestCase(true, false, true, false)]
    public static void DoesNotCheckStrippabilityWhenAlreadyDecided(
        bool corpseSpawned,
        bool alsoOutsideHomeArea,
        bool inBounds,
        bool inHomeArea
    )
    {
        var calls = 0;

        var result = Rimworld_Pawn_Kill.ShouldDesignateCorpse(
            corpseSpawned,
            alsoOutsideHomeArea,
            inBounds,
            inHomeArea,
            () =>
            {
                calls++;
                return true;
            }
        );

        Expect.IsFalse(result);
        Expect.AreEqual(0, calls);
    }
}
