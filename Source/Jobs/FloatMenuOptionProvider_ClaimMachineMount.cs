using System.Collections.Generic;
using GiddyUpMachines.Comps;
using RimWorld;
using Verse;
using Verse.AI;

namespace GiddyUpMachines.Jobs;

/// <summary>
/// Machine ownership is claimed rather than trained or tamed. This primarily
/// handles factionless debug spawns and motorcycles left behind by another
/// faction, while keeping constructed motorcycles player-owned from creation.
/// </summary>
public sealed class FloatMenuOptionProvider_ClaimMachineMount : FloatMenuOptionProvider
{
    protected override bool Drafted => true;
    protected override bool Undrafted => true;
    protected override bool Multiselect => false;
    protected override bool RequiresManipulation => true;

    public override IEnumerable<FloatMenuOption> GetOptionsFor(Pawn clickedPawn, FloatMenuContext context)
    {
        CompMachineMount? machineComp = clickedPawn.MachineMountComp();
        if (machineComp == null || clickedPawn.Faction == Faction.OfPlayer)
            yield break;

        foreach (Pawn worker in context.ValidSelectedPawns)
        {
            if (!worker.IsColonistPlayerControlled || worker == clickedPawn)
                continue;

            if (machineComp.IsMounted)
            {
                yield return Disabled("GM_CannotClaimMounted".Translate());
                yield break;
            }

            if (clickedPawn.Dead)
            {
                yield return Disabled("GM_CannotClaimDestroyed".Translate());
                yield break;
            }

            yield return new FloatMenuOption(
                "GM_ClaimMachine".Translate(clickedPawn.LabelShortCap),
                () => ClaimForPlayer(clickedPawn, worker),
                MenuOptionPriority.High);
            yield break;
        }
    }

    private static FloatMenuOption Disabled(string label)
    {
        return new FloatMenuOption(label, null, MenuOptionPriority.High);
    }

    private static void ClaimForPlayer(Pawn machine, Pawn worker)
    {
        if (machine.Faction == Faction.OfPlayer || machine.Dead)
            return;

        // Remove any behavior inherited from the old faction before handing
        // the machine to the colony's idle think tree.
        machine.jobs.StopAll();
        machine.pather.StopDead();
        machine.SetFaction(Faction.OfPlayer, worker);
        machine.SetForbidden(false, warnOnFail: false);

        Messages.Message(
            "GM_MachineClaimed".Translate(machine.LabelShortCap),
            machine,
            MessageTypeDefOf.PositiveEvent,
            historical: false);
    }
}
