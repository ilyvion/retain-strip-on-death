using DevTools.Testing;

namespace RetainStripOnDeath.Tests;

// A live-map scene for killing pawns and checking their corpses' Strip designations. Dispose puts
// back everything a test touched, since all Playing fixtures share one map.
internal sealed class StripScene : IDisposable
{
    private readonly Map map;
    private readonly bool alsoOutsideHomeAreaBefore;
    private readonly HashSet<Thing> thingsBefore;
    private readonly Dictionary<IntVec3, bool> homeBefore = [];
    private readonly List<Pawn> pawns = [];

    public StripScene(Map map, bool alsoOutsideHomeArea)
    {
        this.map = map;
        alsoOutsideHomeAreaBefore = Settings.AlsoOutsideHomeArea;
        Settings.AlsoOutsideHomeArea = alsoOutsideHomeArea;
        thingsBefore = [.. map.listerThings.AllThings];

        Expect.IsEmpty(
            Rimworld_Pawn_Kill.ShouldStripCorpseOfPawn,
            "An earlier test leaked a strip entry"
        );
        Expect.IsEmpty(Rimworld_Pawn_Kill.PawnCorpse, "An earlier test leaked a corpse entry");
    }

    // Cells near the map center, a few cells apart per index so each corpse gets its own cell.
    public IntVec3 Cell(int index) => map.Center + new IntVec3(index * 3, 0, 0);

    // Factionless, so killing it sends no letter and changes no goodwill. Wears a parka so its
    // corpse always has something to strip.
    private Pawn MakePawn()
    {
        var pawn = PawnGenerator.GeneratePawn(
            new PawnGenerationRequest(PawnKindDefOf.Villager, faction: null)
        );
        pawn.apparel.Wear(
            (Apparel)
                ThingMaker.MakeThing(
                    ThingDefOf.Apparel_Parka,
                    GenStuff.DefaultStuffFor(ThingDefOf.Apparel_Parka)
                )
        );
        pawns.Add(pawn);
        return pawn;
    }

    public Pawn SpawnPawn(IntVec3 cell)
    {
        var pawn = MakePawn();
        _ = GenSpawn.Spawn(pawn, cell, map);
        return pawn;
    }

    public void DesignateStrip(Pawn pawn) =>
        map.designationManager.AddDesignation(new Designation(pawn, DesignationDefOf.Strip));

    public void SetHome(IntVec3 cell, bool value)
    {
        var home = map.areaManager.Home;
        _ = homeBefore.TryAdd(cell, home[cell]);
        home[cell] = value;
    }

    // Kills the pawn and returns its corpse, checking that the corpse is where the pawn died so
    // the home-area state the test set up applies to it.
    public static Corpse? Kill(Pawn pawn)
    {
        var cell = pawn.Position;
        pawn.Kill(null);

        Expect.IsTrue(pawn.Dead);
        var corpse = pawn.Corpse;
        Expect.IsNotNull(corpse, "Killing the pawn made no corpse");
        if (corpse == null)
        {
            return null;
        }
        Expect.IsTrue(corpse.Spawned, "The corpse wasn't placed on the map");
        Expect.AreEqual(cell, corpse.Position, "The corpse wasn't placed where the pawn died");
        return corpse;
    }

    public bool HasStrip(Corpse corpse) =>
        map.designationManager.DesignationOn(corpse, DesignationDefOf.Strip) != null;

    public static void ExpectNoStripState()
    {
        Expect.IsEmpty(Rimworld_Pawn_Kill.ShouldStripCorpseOfPawn, "A strip entry was left behind");
        Expect.IsEmpty(Rimworld_Pawn_Kill.PawnCorpse, "A corpse entry was left behind");
    }

    public void Dispose()
    {
        foreach (var pawn in pawns)
        {
            if (pawn.Corpse is { Destroyed: false } corpse)
            {
                corpse.Destroy();
            }
            else if (!pawn.Destroyed)
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

        var home = map.areaManager.Home;
        foreach (var cellHome in homeBefore)
        {
            home[cellHome.Key] = cellHome.Value;
        }

        Settings.AlsoOutsideHomeArea = alsoOutsideHomeAreaBefore;
        Rimworld_Pawn_Kill.ShouldStripCorpseOfPawn.Clear();
        Rimworld_Pawn_Kill.PawnCorpse.Clear();
    }
}
