using System.Collections.Generic;
using GiddyUpMachines.Comps;
using RimWorld;
using Verse;
using Verse.AI;

namespace GiddyUpMachines.Jobs;

/// <summary>
/// Supplies the machine-specific entry point into Giddy-Up's mounting job.
/// Giddy-Up normally builds this option only after an animal passes all of its
/// animal-centric eligibility checks. Machine mounts deliberately bypass those
/// checks, but the actual mounting, rendering, movement, combat, and save data
/// remain owned by Giddy-Up.
/// </summary>
public sealed class FloatMenuOptionProvider_MountMachine : FloatMenuOptionProvider
{
    protected override bool Drafted => true;
    protected override bool Undrafted => true;
    protected override bool Multiselect => false;
    protected override bool RequiresManipulation => true;

    public override IEnumerable<FloatMenuOption> GetOptionsFor(Pawn clickedPawn, FloatMenuContext context)
    {
        CompMachineMount? machineComp = clickedPawn.MachineMountComp();
        if (machineComp == null || machineComp.IsMounted)
            yield break;

        foreach (Pawn rider in context.ValidSelectedPawns)
        {
            if (!rider.IsColonistPlayerControlled || rider == clickedPawn)
                continue;

            if (!machineComp.HasFuel)
            {
                yield return Disabled("GM_CannotMountNoFuel".Translate(clickedPawn.LabelShortCap));
                yield break;
            }

            if (clickedPawn.Faction != rider.Faction)
            {
                yield return Disabled("GM_CannotMountWrongFaction".Translate());
                yield break;
            }

            if (clickedPawn.Dead || clickedPawn.Downed)
            {
                yield return Disabled("GM_CannotMountDisabled".Translate());
                yield break;
            }

            if (clickedPawn.IsForbidden(rider))
            {
                yield return Disabled("GM_CannotMountForbidden".Translate());
                yield break;
            }

            if (!rider.CanReach(clickedPawn, PathEndMode.Touch, Danger.Deadly))
            {
                yield return Disabled("GM_CannotMountUnreachable".Translate());
                yield break;
            }

            JobDef? mountJob = DefDatabase<JobDef>.GetNamedSilentFail("Mount");
            if (mountJob == null)
            {
                yield return Disabled("GM_CannotMountMissingGiddyUp".Translate());
                yield break;
            }

            yield return new FloatMenuOption(
                "GM_MountMachine".Translate(clickedPawn.LabelShortCap),
                () => StartGiddyUpMountJob(rider, clickedPawn, mountJob),
                MenuOptionPriority.High);
            yield break;
        }
    }

    private static FloatMenuOption Disabled(string label)
    {
        return new FloatMenuOption(label, null, MenuOptionPriority.High);
    }

    private static void StartGiddyUpMountJob(Pawn rider, Pawn machine, JobDef mountJob)
    {
        // Mirrors Giddy-Up's ordered GoMount path. Stopping the machine's
        // current job lets its JobDriver_Mount put the machine into Mounted.
        machine.jobs.StopAll();
        machine.jobs.EndCurrentJob(JobCondition.InterruptForced, startNewJob: false);
        machine.pather.StopDead();

        Job job = JobMaker.MakeJob(mountJob, machine);
        job.count = 1;
        job.playerForced = true;
        rider.jobs.TryTakeOrderedJob(job, JobTag.Misc);
    }
}
