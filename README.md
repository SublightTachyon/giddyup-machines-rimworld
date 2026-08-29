# Giddy-Up Machines

An early RimWorld 1.6 compatibility framework that presents machine Pawns as
Giddy-Up 2 Continued mounts. The first vertical slice includes one craftable,
chemfuel-powered motorcycle.

## Current prototype

- Motorcycle is internally an animal-race `Pawn`, but has no food, rest,
  gender, pregnancy, products, taming UI, or training work.
- Giddy-Up handles mounting, dismounting, rider drawing, mounted movement,
  drafting, and rider combat.
- A machine-specific right-click **Mount Motorcycle** command bypasses
  Giddy-Up's animal-only eligibility filters, then hands the ride to
  Giddy-Up's normal `Mount` job and rider-state system.
- Factionless and foreign motorcycles can be transferred to the colony with
  the right-click **Claim Motorcycle** action; machines are claimed, never
  tamed or trained. Constructed motorcycles already belong to the colony.
- The rider remains the selected combatant and keeps their equipped weapon.
- `CompRefuelable` stores chemfuel; `CompMachineMount` consumes 0.08 fuel per
  map cell travelled.
- Empty motorcycles cannot be mounted. Running out of fuel stops the rider and
  safely ends Giddy-Up's mounted job.
- A manual right-click refuel job is included.
- The motorcycle uses directional RimWorld pawn art: a horizontal side profile
  for east/west movement plus dedicated front and rear views for south/north.
  RimWorld mirrors the east texture for west automatically.
- The motorcycle is assembled from the Architect menu after researching
  Machining.

This is a prototype. RunAndGun should naturally operate on the rider because
the rider remains a normal Pawn, but dedicated compatibility testing is still
needed.

## Requirements and load order

1. Harmony
2. Giddy-Up 2 - Continued
3. Giddy-Up Machines

The required Giddy-Up package ID is `MemeGoddess.GiddyUp`.

## Build

Install a .NET SDK, then run:

```bash
dotnet build Source/GiddyUpMachines.csproj -c Release
```

The assembly is written to `1.6/Assemblies/GiddyUpMachines.dll`.

## Local development install

The repository itself has the final RimWorld mod layout. Symlink it into the
game's `Mods` directory:

```bash
ln -s "$PWD" "$HOME/.local/share/Steam/steamapps/common/RimWorld/Mods/GiddyUpMachines"
```

Enable development mode to use **Debug actions -> Spawn pawn** as a fallback;
the PawnKindDef is `GM_Motorcycle`.

## First test pass

1. Enable Harmony, Giddy-Up 2 Continued, and Giddy-Up Machines.
2. Finish Machining research.
3. Choose **Architect -> Production -> Assemble motorcycle** and construct it.
4. For a motorcycle created with **Debug actions -> Spawn pawn**, select a
   colonist, right-click it, and choose **Claim Motorcycle** first.
5. Select a colonist, right-click the fueled motorcycle, and choose
   **Mount Motorcycle**.
6. Draft the rider and issue movement and attack orders.
7. Rotate or move in all four directions and confirm the side, front, and rear
   motorcycle sprites match the rider's facing.
8. Select a colonist and right-click the motorcycle to refuel it with chemfuel.
9. Let the tank empty and confirm the rider stops and dismounts.
10. Save and reload while mounted and while dismounted.

Errors and Harmony diagnostics are written to RimWorld's `Player.log`.
