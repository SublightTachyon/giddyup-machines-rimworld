using System.Collections.Generic;
using GiddyUpMachines.Comps;
using GiddyUpMachines.Defs;
using RimWorld;
using Verse;
using Verse.AI;

namespace GiddyUpMachines.Jobs;

public sealed class FloatMenuOptionProvider_RefuelMachineMount : FloatMenuOptionProvider
{
    protected override bool Drafted => true;
    protected override bool Undrafted => true;
    protected override bool Multiselect => false;
    protected override bool RequiresManipulation => true;

    public override IEnumerable<FloatMenuOption> GetOptionsFor(Pawn clickedPawn, FloatMenuContext context)
    {
        CompMachineMount? machineComp = clickedPawn.MachineMountComp();
        if (machineComp?.Refuelable == null)
            yield break;

        foreach (Pawn worker in context.ValidSelectedPawns)
        {
            if (!worker.IsColonistPlayerControlled || worker == clickedPawn)
                continue;

            if (machineComp.IsMounted)
            {
                yield return new FloatMenuOption("GM_CannotRefuelMounted".Translate(), null);
                yield break;
            }

            if (machineComp.Refuelable.GetFuelCountToFullyRefuel() <= 0)
            {
                yield return new FloatMenuOption("GM_AlreadyFull".Translate(), null);
                yield break;
            }

            Thing? fuel = FindFuel(worker);
            if (fuel == null)
            {
                yield return new FloatMenuOption("GM_NoChemfuel".Translate(), null);
                yield break;
            }

            yield return new FloatMenuOption(
                "GM_RefuelMachine".Translate(clickedPawn.LabelShortCap),
                () =>
                {
                    Job job = JobMaker.MakeJob(GMDefOf.GM_RefuelMachineMount, clickedPawn, fuel);
                    job.count = machineComp.Refuelable.GetFuelCountToFullyRefuel();
                    job.playerForced = true;
                    worker.jobs.TryTakeOrderedJob(job, JobTag.Misc);
                });
            yield break;
        }
    }

    private static Thing? FindFuel(Pawn worker)
    {
        return GenClosest.ClosestThingReachable(
            worker.Position,
            worker.Map,
            ThingRequest.ForDef(ThingDefOf.Chemfuel),
            PathEndMode.ClosestTouch,
            TraverseParms.For(worker),
            9999f,
            thing => !thing.IsForbidden(worker) && worker.CanReserve(thing));
    }
}
