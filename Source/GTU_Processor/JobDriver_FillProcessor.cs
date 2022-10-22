using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace GTU_Processor;

public class JobDriver_FillProcessor : JobDriver
{
    private const TargetIndex ProcessorInd = TargetIndex.A;

    private const TargetIndex IngredientInd = TargetIndex.B;

    protected ThingWithComps Processor => (ThingWithComps)job.GetTarget(TargetIndex.A).Thing;

    protected Thing Ingredient => job.GetTarget(TargetIndex.B).Thing;

    public override bool TryMakePreToilReservations(bool errorOnFailed)
    {
        return pawn.Reserve(Processor, job, 1, -1, null, errorOnFailed) &&
               pawn.Reserve(Ingredient, job, 1, -1, null, errorOnFailed);
    }

    [DebuggerHidden]
    protected override IEnumerable<Toil> MakeNewToils()
    {
        this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
        this.FailOnBurningImmobile(TargetIndex.A);
        AddEndCondition(() =>
            !Processor.GetComp<CompGTProcessor>().Full ? JobCondition.Ongoing : JobCondition.Succeeded);
        yield return Toils_General.DoAtomic(delegate
        {
            job.count = Processor.GetComp<CompGTProcessor>().IngredientRequired;
        });
        var reserveIngredient = Toils_Reserve.Reserve(TargetIndex.B);
        yield return reserveIngredient;
        yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch)
            .FailOnDespawnedNullOrForbidden(TargetIndex.B).FailOnSomeonePhysicallyInteracting(TargetIndex.B);
        yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, true)
            .FailOnDestroyedNullOrForbidden(TargetIndex.B);
        yield return Toils_Haul.CheckForGetOpportunityDuplicate(reserveIngredient, TargetIndex.B, TargetIndex.None,
            true);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
        yield return Toils_General.Wait(200).FailOnDestroyedNullOrForbidden(TargetIndex.B)
            .FailOnDestroyedNullOrForbidden(TargetIndex.A).FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch)
            .WithProgressBarToilDelay(TargetIndex.A);
        yield return new Toil
        {
            initAction = delegate { Processor.GetComp<CompGTProcessor>().AddIngredient(Ingredient); },
            defaultCompleteMode = ToilCompleteMode.Instant
        };
    }
}