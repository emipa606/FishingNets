using System;
using RimWorld;
using Verse;
using Verse.AI;

namespace GTU_Processor;

public class WorkGiver_FillProcessor : WorkGiver_Scanner
{
    public override ThingRequest PotentialWorkThingRequest =>
        ThingRequest.ForGroup(ThingRequestGroup.BuildingArtificial);

    public override PathEndMode PathEndMode => PathEndMode.Touch;

    public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        var processor = t as ThingWithComps;
        var comp = processor?.GetComp<CompGTProcessor>();
        if (comp == null || comp.Full)
        {
            return false;
        }

        if (t.IsForbidden(pawn))
        {
            return false;
        }

        LocalTargetInfo target = t;
        if (!pawn.CanReserve(target, 1, -1, null, forced))
        {
            return false;
        }

        if (pawn.Map.designationManager.DesignationOn(t, DesignationDefOf.Deconstruct) != null)
        {
            return false;
        }

        if (FindIngredient(pawn, processor) != null)
        {
            return !t.IsBurning();
        }

        JobFailReason.Is("Could not find ingredient");
        return false;
    }

    public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
    {
        var processor = t as ThingWithComps;
        var t2 = FindIngredient(pawn, processor);
        return new Job(JobDefOf.FillGTProcessor, t, t2);
    }

    private Thing FindIngredient(Pawn pawn, ThingWithComps processor)
    {
        _ = processor.GetComp<CompGTProcessor>();

        var position = pawn.Position;
        var map = pawn.Map;
        var thingReq = ThingRequest.ForDef(ThingDefOf.WoodLog); //comp.Props.thingIngredient);
        var peMode = PathEndMode.ClosestTouch;
        var traverseParms = TraverseParms.For(pawn);
        var validator = (Predicate<Thing>)Predicate;
        return GenClosest.ClosestThingReachable(position, map, thingReq, peMode, traverseParms, 9999f, validator);

        bool Predicate(Thing x)
        {
            return !x.IsForbidden(pawn) && pawn.CanReserve(x);
        }
    }
}