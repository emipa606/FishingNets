using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace GTU_Processor;

public class JobDriver_FillProcessor : JobDriver
{
    private const TargetIndex ProcessorInd = TargetIndex.A;

    private const TargetIndex IngredientInd = TargetIndex.B;

    private ThingWithComps Processor => (ThingWithComps)job.GetTarget(ProcessorInd).Thing;

    private Thing Ingredient => job.GetTarget(IngredientInd).Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(Processor, job, 1, -1, null, errorOnFailed) &&
               pawn.Reserve(Ingredient, job, 1, -1, null, errorOnFailed);
    }

    [DebuggerHidden]
    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(ProcessorInd);
        this.FailOnBurningImmobile(ProcessorInd);
        AddEndCondition(() =>
            !Processor.GetComp<CompGTProcessor>().Full ? JobCondition.Ongoing : JobCondition.Succeeded);
        yield return Toils_General.DoAtomic(delegate
        {
            job.count = Processor.GetComp<CompGTProcessor>().IngredientRequired;
        });
        var reserveIngredient = Toils_Reserve.Reserve(IngredientInd);
        yield return reserveIngredient;
        yield return Toils_Goto.GotoThing(IngredientInd, PathEndMode.ClosestTouch)
            .FailOnDespawnedNullOrForbidden(IngredientInd).FailOnSomeonePhysicallyInteracting(IngredientInd);
        yield return Toils_Haul.StartCarryThing(IngredientInd, false, true)
            .FailOnDestroyedNullOrForbidden(IngredientInd);
        yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveIngredient, IngredientInd, TargetIndex.None,
            true);
        yield return Toils_Goto.GotoThing(ProcessorInd, PathEndMode.Touch);
        yield return Toils_General.Wait(200).FailOnDestroyedNullOrForbidden(IngredientInd)
            .FailOnDestroyedNullOrForbidden(ProcessorInd).FailOnCannotTouch(ProcessorInd, PathEndMode.Touch)
            .WithProgressBarToilDelay(ProcessorInd);
        yield return new Toil
        {
            initAction = delegate { Processor.GetComp<CompGTProcessor>().AddIngredient(Ingredient); },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
    }
}