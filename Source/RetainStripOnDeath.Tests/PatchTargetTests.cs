using DevTools.Testing;

namespace RetainStripOnDeath.Tests;

[TestFixture(TestType.MainMenu)]
internal sealed class PatchTargetTests
{
    [Test]
    public static void KillHasTheModsPrefixAndPostfix()
    {
        var patches = Harmony.GetPatchInfo(AccessTools.Method(typeof(Pawn), nameof(Pawn.Kill)));

        Expect.IsNotNull(patches, "Pawn.Kill isn't patched");
        if (patches == null)
        {
            return;
        }
        Expect.Any(patches.Prefixes, p => p.owner == Constants.Id, "The mod's prefix");
        Expect.Any(patches.Postfixes, p => p.owner == Constants.Id, "The mod's postfix");
    }

    // Kill calls the (Building_Grave, Building_Bed) overload, which forwards to this one.
    [Test]
    public static void MakeCorpseHasTheModsPostfix()
    {
        var patches = Harmony.GetPatchInfo(
            AccessTools.Method(
                typeof(Pawn),
                nameof(Pawn.MakeCorpse),
                [typeof(Building_Grave), typeof(bool), typeof(float)]
            )
        );

        Expect.IsNotNull(patches, "Pawn.MakeCorpse isn't patched");
        if (patches == null)
        {
            return;
        }
        Expect.Any(patches.Postfixes, p => p.owner == Constants.Id, "The mod's postfix");
    }
}
