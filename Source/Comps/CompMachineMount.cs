using GiddyUpMachines.Defs;
using RimWorld;
using Verse;
using Verse.AI;

namespace GiddyUpMachines.Comps;

public sealed class CompProperties_MachineMount : CompProperties
{
    public float fuelPerCell = 0.08f;

    public CompProperties_MachineMount()
    {
        compClass = typeof(CompMachineMount);
    }
}

public sealed class CompMachineMount : ThingComp
{
    private IntVec3 lastPosition = IntVec3.Invalid;
    private bool outOfFuelMessageSent;

    public CompProperties_MachineMount Props => (CompProperties_MachineMount)props;

    public Pawn Machine => (Pawn)parent;

    public CompRefuelable? Refuelable => parent.TryGetComp<CompRefuelable>();

    public bool HasFuel => Refuelable?.HasFuel == true;

    public bool IsMounted => Machine.CurJobDef?.defName == "Mounted";

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        lastPosition = Machine.Position;
    }

    public override void CompTick()
    {
        base.CompTick();

        if (!Machine.Spawned)
            return;

        if (!IsMounted)
        {
            lastPosition = Machine.Position;
            if (HasFuel)
                outOfFuelMessageSent = false;
            return;
        }

        if (!HasFuel)
        {
            StopMountedMachine();
            return;
        }

        if (!lastPosition.IsValid)
        {
            lastPosition = Machine.Position;
            return;
        }

        if (Machine.Position == lastPosition)
            return;

        // Giddy-Up moves the rider and mirrors the mount onto the rider's cell.
        // A position change is therefore one completed map-cell movement. Large
        // teleports only charge one cell so map transfers do not empty the tank.
        lastPosition = Machine.Position;
        Refuelable!.ConsumeFuel(Props.fuelPerCell);

        if (!Refuelable.HasFuel)
            StopMountedMachine();
    }

    public override string CompInspectStringExtra()
    {
        return "GM_FuelConsumption".Translate(Props.fuelPerCell.ToString("0.##"));
    }

    private void StopMountedMachine()
    {
        Pawn? rider = Machine.CurJob?.targetA.Pawn;
        rider?.pather?.StopDead();

        if (!outOfFuelMessageSent)
        {
            outOfFuelMessageSent = true;
            Messages.Message(
                "GM_OutOfFuelMessage".Translate(Machine.LabelShortCap),
                Machine,
                MessageTypeDefOf.NegativeEvent,
                historical: false);
        }

        // Ending Giddy-Up's Mounted job executes its own cleanup path, keeping
        // rider association and save state inside Giddy-Up instead of duplicating it.
        if (Machine.CurJobDef?.defName == "Mounted")
            Machine.jobs.EndCurrentJob(JobCondition.InterruptForced, startNewJob: true);
    }
}

public static class MachineMountUtility
{
    public static bool IsMachineMount(this Thing? thing)
    {
        return thing?.TryGetComp<CompMachineMount>() != null ||
               thing?.def?.GetModExtension<MachineMountExtension>() != null;
    }

    public static CompMachineMount? MachineMountComp(this Thing? thing)
    {
        return thing?.TryGetComp<CompMachineMount>();
    }
}
