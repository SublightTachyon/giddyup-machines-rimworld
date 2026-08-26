using Verse;

namespace GiddyUpMachines.Defs;

/// <summary>
/// Stable XML-facing marker and adapter metadata for machine mount ThingDefs.
/// Compatibility packs can add this extension and CompMachineMount to their
/// own Pawn race without depending on internal implementation details.
/// </summary>
public sealed class MachineMountExtension : DefModExtension
{
    public float fuelPerCell = 0.08f;
    public string fuelLabel = "Chemfuel";
}
