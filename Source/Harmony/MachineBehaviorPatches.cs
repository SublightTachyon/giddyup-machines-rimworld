using GiddyUpMachines.Comps;
using RimWorld;
using Verse;
using Verse.AI;

namespace GiddyUpMachines.HarmonyPatches;

/// <summary>
/// Giddy-Up asks the animal training tracker whether Tameness is learned.
/// Machines answer yes internally without exposing animal training gameplay.
/// </summary>
public static class MachineBehaviorPatches
{
    public static bool TrainingHasLearnedPrefix(Pawn ___pawn, ref bool __result)
    {
        if (!___pawn.IsMachineMount())
            return true;

        __result = true;
        return false;
    }
    /// <summary>Never generate vanilla animal-training work for a machine.</summary>
    public static bool TrainJobOnThingPrefix(Thing t, ref Job? __result)
    {
        if (!t.IsMachineMount())
            return true;

        __result = null;
        return false;
    }
}
