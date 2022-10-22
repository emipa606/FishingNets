using System.Collections.Generic;
using System.Diagnostics;
using RimWorld;
using UnityEngine;
using Verse;

namespace GTU_Processor;

public class CompGTProcessor : ThingComp
{
    private const string ActiveOffGraphicSuffix = "_ActiveOff";

    private const string ActiveOnGraphicSuffix = "_ActiveOn";

    private int ingredientStored;

    private int tickInterval;
    private int ticksRemaining;

    public CompProperties_GTProcessor Props => (CompProperties_GTProcessor)props;

    private bool PowerOn
    {
        get
        {
            var comp = parent.GetComp<CompPowerTrader>();
            return comp is { PowerOn: true };
        }
    }

    private bool Empty => ingredientStored <= 0;

    public int IngredientRequired => Props.ingredientCount - ingredientStored;

    public bool Full => IngredientRequired <= 0;

    public bool Completed => Full && ticksRemaining <= 0;

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref ticksRemaining, "ticksRemaining");
        Scribe_Values.Look(ref ingredientStored, "ingredientStored");
        Scribe_Values.Look(ref tickInterval, "tickInterval");
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        if (!respawningAfterLoad)
        {
            ResetTicksRemaining();
        }
    }

    public override void CompTickRare()
    {
        TickInterval(250);
    }

    private void TickInterval(int interval)
    {
        if (Props.requiresPower && !PowerOn)
        {
            return;
        }

        if (!Full)
        {
            return;
        }

        if (ticksRemaining > 0)
        {
            ticksRemaining -= interval;
        }

        if (Props.spawnOnFloor && AttemptSpawn())
        {
            ResetTicksRemaining();
        }
    }

    private bool AttemptSpawn()
    {
        if (ticksRemaining > 0)
        {
            return false;
        }

        if (Props.spawnMaxAdjacent >= 0)
        {
            var num = 0;
            for (var i = 0; i < 9; i++)
            {
                var c = parent.Position + GenAdj.AdjacentCellsAndInside[i];
                if (!c.InBounds(parent.Map))
                {
                    continue;
                }

                var thingList = c.GetThingList(parent.Map);
                foreach (var thing in thingList)
                {
                    if (thing.def != Props.thingResult)
                    {
                        continue;
                    }

                    num += thing.stackCount;
                    if (num >= Props.spawnMaxAdjacent)
                    {
                        return false;
                    }
                }
            }
        }

        if (!TryFindSpawnCell(out var center))
        {
            return false;
        }

        {
            var thing = ThingMaker.MakeThing(Props.thingResult);
            thing.stackCount = Props.resultCount;
            GenPlace.TryPlaceThing(thing, center, parent.Map, ThingPlaceMode.Direct, out var t);
            if (Props.spawnForbidden)
            {
                t.SetForbidden(true);
            }

            if (Props.showMessageIfOwned && parent.Faction == Faction.OfPlayer)
            {
                Messages.Message(
                    "MessageCompSpawnerSpawnedItem".Translate(Props.thingResult.LabelCap).CapitalizeFirst(), thing,
                    MessageTypeDefOf.PositiveEvent);
            }

            return true;
        }
    }

    private bool TryFindSpawnCell(out IntVec3 result)
    {
        foreach (var current in GenAdj.CellsAdjacent8Way(parent).InRandomOrder())
        {
            if (!current.Walkable(parent.Map))
            {
                continue;
            }

            var edifice = current.GetEdifice(parent.Map);
            if (edifice != null && Props.thingResult.IsEdifice())
            {
                continue;
            }

            if (edifice is Building_Door { FreePassage: false })
            {
                continue;
            }

            if (parent.def.passability != Traversability.Impassable &&
                !GenSight.LineOfSight(parent.Position, current, parent.Map))
            {
                continue;
            }

            var isRightThing = false;
            var thingList = current.GetThingList(parent.Map);
            foreach (var thing in thingList)
            {
                if (thing.def.category != ThingCategory.Item || thing.def == Props.thingResult &&
                    thing.stackCount <= Props.thingResult.stackLimit -
                    Props.resultCount)
                {
                    continue;
                }

                isRightThing = true;
                break;
            }

            if (isRightThing)
            {
                continue;
            }

            result = current;
            return true;
        }

        result = IntVec3.Invalid;
        return false;
    }

    public Thing TakeOutResult()
    {
        if (!Completed)
        {
            Log.Warning("Tried to remove processor result before it completed.");
            return null;
        }

        var thing = ThingMaker.MakeThing(Props.thingResult);
        thing.stackCount = Props.resultCount;
        ResetTicksRemaining();
        return thing;
    }

    public void AddIngredient(int count)
    {
        if (Full)
        {
            Log.Warning("Tried to add ingredients to CompProperties_GTProcessor when it is full.");
        }

        var num = Mathf.Min(count, IngredientRequired);
        if (num <= 0)
        {
            return;
        }

        ingredientStored += num;
    }

    public void AddIngredient(Thing ingredient)
    {
        if (ingredient.def != Props.thingIngredient)
        {
            return;
        }

        var num = Mathf.Min(ingredient.stackCount, IngredientRequired);
        if (num <= 0)
        {
            return;
        }

        AddIngredient(num);
        ingredient.SplitOff(num).Destroy();
    }

    private void ResetTicksRemaining()
    {
        ingredientStored = 0;
        ticksRemaining = Props.durationIntervalRange.RandomInRange;
        tickInterval = ticksRemaining;
    }

    public override string CompInspectStringExtra()
    {
        string str = null;
        switch (Full)
        {
            case false when Props.writeRequiredIngredients:
                str = $"{ingredientStored}/{Props.ingredientCount} {Props.thingIngredient.label}";
                break;
            case true when !Completed && Props.writeTimeLeftToProcess:
            {
                var progInt = 0f;
                var workVerb = $" {Props.workVerb} ";
                if (tickInterval != 0)
                {
                    progInt = 1f - ((float)ticksRemaining / tickInterval);
                }

                str = progInt.ToStringPercent() + workVerb + ticksRemaining.ToStringTicksToPeriod();
                break;
            }
            default:
            {
                if (Completed && !Props.spawnOnFloor && Props.writeTimeLeftToProcess)
                {
                    var workVerbPast = $" {Props.workVerbPast}";
                    str = 1f.ToStringPercent() + workVerbPast;
                }

                break;
            }
        }

        return str;
    }

    [DebuggerHidden]
    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        if (!Prefs.DevMode)
        {
            yield break;
        }

        yield return new Command_Action
        {
            defaultLabel = "DEBUG: Progress 1 hour",
            action = delegate { ticksRemaining -= 2500; }
        };
        yield return new Command_Action
        {
            defaultLabel = "DEBUG: Progress 1 day",
            action = delegate { ticksRemaining -= 60000; }
        };
        yield return new Command_Action
        {
            defaultLabel = "DEBUG: Complete processing",
            action = delegate { ticksRemaining = 0; }
        };
    }
}