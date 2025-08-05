using System.Collections.Generic;
using System.Diagnostics;
using RimWorld;
using Verse;
using Verse.AI;

namespace GTU_Processor;

public class JobDriver_EmptyProcessor : JobDriver
{
    private const TargetIndex ProcessorInd = TargetIndex.A;

    private const TargetIndex ResultInd = TargetIndex.B;

    private const TargetIndex StorageCellInd = TargetIndex.C;

    private ThingWithComps Processor => (ThingWithComps)job.GetTarget(ProcessorInd).Thing;

    protected Thing Result => job.GetTarget(ResultInd).Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(Processor, job, 1, -1, null, errorOnFailed);
    }

    [DebuggerHidden]
    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(ProcessorInd);
        this.FailOnBurningImmobile(ProcessorInd);
        yield return Toils_Goto.GotoThing(ProcessorInd, PathEndMode.Touch);
        yield return Toils_General.Wait(200).FailOnDestroyedNullOrForbidden(ProcessorInd)
            .FailOnCannotTouch(ProcessorInd, PathEndMode.Touch)
            .FailOn(() => !Processor.GetComp<CompGTProcessor>().Completed).WithProgressBarToilDelay(ProcessorInd);
        yield return new Toil
        {
            initAction = delegate
            {
                var thing = Processor.GetComp<CompGTProcessor>().TakeOutResult();
                GenPlace.TryPlaceThing(thing, pawn.Position, Map, ThingPlaceMode.Near);
                var currentPriority = StoreUtility.CurrentStoragePriorityOf(thing);
                if (StoreUtility.TryFindBestBetterStoreCellFor(thing, pawn, Map, currentPriority, pawn.Faction,
                        out var c))
                {
                    job.SetTarget(StorageCellInd, c);
                    job.SetTarget(ResultInd, thing);
                    job.count = thing.stackCount;
                }
                else
                {
                    EndJobWith(JobCondition.Incompletable);
                }
            },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
        yield return Toils_Reserve.Reserve(ResultInd);
        yield return Toils_Reserve.Reserve(StorageCellInd);
        yield return Toils_Goto.GotoThing(ResultInd, PathEndMode.ClosestTouch);
        yield return Toils_Haul.StartCarryThing(ResultInd);
        var carryToCell = Toils_Haul.CarryHauledThingToCell(StorageCellInd);
        yield return carryToCell;
        yield return Toils_Haul.PlaceHauledThingInCell(StorageCellInd, carryToCell, true);
    }
}