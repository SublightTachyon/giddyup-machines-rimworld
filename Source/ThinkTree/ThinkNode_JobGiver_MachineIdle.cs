using RimWorld;
using Verse;
using Verse.AI;

namespace GiddyUpMachines.ThinkTree;

public sealed class ThinkNode_JobGiver_MachineIdle : ThinkNode_JobGiver
{
    protected override Job TryGiveJob(Pawn pawn)
    {
        Job job = JobMaker.MakeJob(JobDefOf.Wait);
        job.expiryInterval = 600;
        job.checkOverrideOnExpire = true;
        return job;
    }
}
