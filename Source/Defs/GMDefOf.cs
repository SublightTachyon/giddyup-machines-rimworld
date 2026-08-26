using RimWorld;
using Verse;

namespace GiddyUpMachines.Defs;

[DefOf]
public static class GMDefOf
{
    public static PawnKindDef GM_Motorcycle = null!;
    public static JobDef GM_RefuelMachineMount = null!;

    static GMDefOf()
    {
        DefOfHelper.EnsureInitializedInCtor(typeof(GMDefOf));
    }
}
