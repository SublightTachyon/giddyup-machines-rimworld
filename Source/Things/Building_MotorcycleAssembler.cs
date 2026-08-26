using GiddyUpMachines.Defs;
using RimWorld;
using Verse;

namespace GiddyUpMachines.Things;

/// <summary>
/// Construction blueprints need to finish as Buildings. This one-tick shell
/// converts the completed assembly into the actual motorcycle Pawn.
/// </summary>
public sealed class Building_MotorcycleAssembler : Building
{
    private int convertOnTick = -1;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        if (!respawningAfterLoad)
            convertOnTick = Find.TickManager.TicksGame + 1;
    }

    protected override void Tick()
    {
        base.Tick();
        if (!Spawned || convertOnTick < 0 || Find.TickManager.TicksGame < convertOnTick)
            return;

        Map map = Map;
        IntVec3 position = Position;
        Rot4 rotation = Rotation;
        convertOnTick = -1;

        Pawn motorcycle = PawnGenerator.GeneratePawn(GMDefOf.GM_Motorcycle, Faction.OfPlayer);
        motorcycle.Rotation = rotation;

        Destroy(DestroyMode.Vanish);
        GenSpawn.Spawn(motorcycle, position, map, rotation);
        Messages.Message(
            "GM_AssemblyComplete".Translate(motorcycle.LabelShortCap),
            motorcycle,
            MessageTypeDefOf.PositiveEvent,
            historical: false);
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref convertOnTick, "convertOnTick", -1);
    }
}
