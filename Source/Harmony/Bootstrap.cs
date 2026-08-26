using System.Reflection;
using GiddyUpMachines.Comps;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace GiddyUpMachines.HarmonyPatches;

[StaticConstructorOnStartup]
public static class Bootstrap
{
    static Bootstrap()
    {
        Harmony harmony = new("Turone.GiddyUpMachines");

        PatchPrefix(
            harmony,
            AccessTools.Method(typeof(Pawn_TrainingTracker), nameof(Pawn_TrainingTracker.HasLearned)),
            nameof(MachineBehaviorPatches.TrainingHasLearnedPrefix));
        PatchPrefix(
            harmony,
            AccessTools.DeclaredMethod(typeof(WorkGiver_Train), nameof(WorkGiver_Train.JobOnThing)),
            nameof(MachineBehaviorPatches.TrainJobOnThingPrefix));

        // This avoids a compile-time dependency on GiddyUpCore.dll while still
        // targeting the exact job driver exposed by Giddy-Up 2 Continued.
        System.Type? mountDriverType = AccessTools.TypeByName("GiddyUp.Jobs.JobDriver_Mount");
        MethodInfo? reservationMethod = mountDriverType == null
            ? null
            : AccessTools.Method(mountDriverType, nameof(JobDriver.TryMakePreToilReservations));

        if (reservationMethod == null)
        {
            Log.Error("[Giddy-Up Machines] Could not find Giddy-Up's mount job driver. Check mod version and load order.");
            return;
        }

        PatchPrefix(harmony, reservationMethod, nameof(MountReservationPrefix));
    }

    public static bool MountReservationPrefix(JobDriver __instance, ref bool __result)
    {
        Pawn? machine = __instance.job?.targetA.Pawn;
        CompMachineMount? comp = machine?.MachineMountComp();
        if (comp == null || comp.HasFuel)
            return true;

        Messages.Message(
            "GM_CannotMountNoFuel".Translate(machine!.LabelShortCap),
            machine,
            MessageTypeDefOf.RejectInput,
            historical: false);
        __result = false;
        return false;
    }

    private static void PatchPrefix(Harmony harmony, MethodInfo? original, string prefixName)
    {
        if (original == null)
        {
            Log.Error($"[Giddy-Up Machines] Could not find method for patch prefix {prefixName}.");
            return;
        }

        System.Type patchType = prefixName == nameof(MountReservationPrefix)
            ? typeof(Bootstrap)
            : typeof(MachineBehaviorPatches);
        harmony.Patch(original, prefix: new HarmonyMethod(patchType, prefixName));
    }
}
