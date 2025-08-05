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

    private ThingDef thingToSpawn()
    {
        var tmpCatches = new List<Thing>();
        tmpCatches.Clear();
        if (!ModsConfig.OdysseyActive)
        {
            return Props.thingResult;
        }

        var waterBody = parent.Position.GetWaterBody(parent.Map);
        if (waterBody == null || waterBody.waterBodyType == WaterBodyType.None)
        {
            return Props.thingResult;
        }

        ThingDef result = null;
        if (!Rand.Chance(FishingUtility.PollutionToxfishChanceCurve.Evaluate(waterBody.PollutionPct)))
        {
            if (parent.Map.Biome.fishTypes.rareCatchesSetMaker != null &&
                (DebugSettings.alwaysRareCatches || parent.Map.waterBodyTracker.lastRareCatchTick == 0 ||
                 GenTicks.TicksGame - parent.Map.waterBodyTracker.lastRareCatchTick > 300000) && Rand.Chance(0.01f))
            {
                tmpCatches.AddRange(parent.Map.Biome.fishTypes.rareCatchesSetMaker.root.Generate());
                if (tmpCatches.Any())
                {
                    return tmpCatches.RandomElement().def;
                }
            }

            if ((!Rand.Chance(0.05f) || !waterBody.UncommonFish.TryRandomElement(out result)) &&
                !waterBody.CommonFishIncludingExtras.TryRandomElement(out result) && tmpCatches.Any())
            {
                return tmpCatches.RandomElement().def;
            }
        }
        else
        {
            return ThingDefOf.Fish_Toxfish;
        }

        return result ?? Props.thingResult;
    }

    private bool AttemptSpawn()
    {
        if (ticksRemaining > 0)
        {
            return false;
        }

        var thingToSpawn = this.thingToSpawn();

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
                    if (thing.def != thingToSpawn)
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

        var makeThing = ThingMaker.MakeThing(thingToSpawn);
        makeThing.stackCount = Props.resultCount;
        GenPlace.TryPlaceThing(makeThing, center, parent.Map, ThingPlaceMode.Direct, out var t);
        if (Props.spawnForbidden)
        {
            t.SetForbidden(true);
        }

        if (Props.showMessageIfOwned && parent.Faction == Faction.OfPlayer)
        {
            Messages.Message(
                "MessageCompSpawnerSpawnedItem".Translate(thingToSpawn.LabelCap).CapitalizeFirst(), makeThing,
                MessageTypeDefOf.PositiveEvent);
        }

        return true;
    }

    private bool TryFindSpawnCell(out IntVec3 result)
    {
        var thingToSpawn = this.thingToSpawn();
        foreach (var current in GenAdj.CellsAdjacent8Way(parent).InRandomOrder())
        {
            if (!current.Walkable(parent.Map))
            {
                continue;
            }

            var edifice = current.GetEdifice(parent.Map);
            if (edifice != null && thingToSpawn.IsEdifice())
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
                if (thing.def.category != ThingCategory.Item || thing.def == thingToSpawn &&
                    thing.stackCount <= thingToSpawn.stackLimit -
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

        var thing = ThingMaker.MakeThing(thingToSpawn());
        thing.stackCount = Props.resultCount;
        ResetTicksRemaining();
        return thing;
    }

    private void AddIngredient(int count)
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