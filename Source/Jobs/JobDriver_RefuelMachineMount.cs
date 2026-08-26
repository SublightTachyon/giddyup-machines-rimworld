using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace GiddyUpMachines.Jobs;

public sealed class JobDriver_RefuelMachineMount : JobDriver
{
    private Pawn Machine => (Pawn)job.GetTarget(TargetIndex.A).Thing;
    private Thing Fuel => job.GetTarget(TargetIndex.B).Thing;
    private CompRefuelable Refuelable => Machine.GetComp<CompRefuelable>();

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(Machine, job, 1, -1, null, errorOnFailed) &&
               pawn.Reserve(Fuel, job, 1, -1, null, errorOnFailed);
    }

    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDestroyedNullOrForbidden(TargetIndex.A);
        this.FailOnDestroyedNullOrForbidden(TargetIndex.B);
        this.FailOn(() => Machine.CurJobDef?.defName == "Mounted");
        AddEndCondition(() => Refuelable.GetFuelCountToFullyRefuel() > 0
            ? JobCondition.Ongoing
            : JobCondition.Succeeded);

        yield return Toils_General.DoAtomic(() =>
        {
            job.count = Refuelable.GetFuelCountToFullyRefuel();
        });
        yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch)
            .FailOnSomeonePhysicallyInteracting(TargetIndex.B);
        yield return Toils_Haul.StartCarryThing(
            TargetIndex.B,
            putRemainderInQueue: false,
            subtractNumTakenFromJobCount: true);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
        yield return Toils_General.Wait(120)
            .WithProgressBarToilDelay(TargetIndex.A)
            .FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);

        Toil refuel = new()
        {
            defaultCompleteMode = ToilCompleteMode.Instant,
            initAction = () =>
            {
                Thing? carriedFuel = pawn.carryTracker.CarriedThing;
                if (carriedFuel == null)
                    return;

                int amount = System.Math.Min(
                    carriedFuel.stackCount,
                    Refuelable.GetFuelCountToFullyRefuel());
                if (amount <= 0)
                    return;

                Refuelable.Refuel(amount);
                carriedFuel.SplitOff(amount).Destroy(DestroyMode.Vanish);
            }
        };
        yield return refuel;
    }
}
