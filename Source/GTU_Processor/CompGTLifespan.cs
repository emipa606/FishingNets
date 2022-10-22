using RimWorld;
using Verse;

namespace GTU_Processor;

public class CompGTLifespan : ThingComp
{
    private int tickInterval;
    private int ticksRemaining;

    public CompProperties_GTLifespan Props => (CompProperties_GTLifespan)props;

    public override void PostExposeData()
    {
        base.PostExposeData();
        Scribe_Values.Look(ref ticksRemaining, "ticksRemaining");
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
        if (ticksRemaining > 0)
        {
            ticksRemaining -= interval;
        }
        else
        {
            if (Props.killAtEnd)
            {
                TryToKill();
            }
            else
            {
                TryToEnd();
            }

            ResetTicksRemaining();
        }
    }

    private void TryToEnd()
    {
        ShowEndMessage();
        parent.Destroy();
    }

    private void TryToKill()
    {
        ShowEndMessage();
        parent.Kill();
    }

    private void ShowEndMessage()
    {
        if (!Props.showMessageIfOwned)
        {
            return;
        }

        if (Props.expiredMessage == null)
        {
            Messages.Message(parent.Label + " has expired.".CapitalizeFirst(), parent,
                MessageTypeDefOf.NeutralEvent, false);
        }
        else
        {
            Messages.Message(Props.expiredMessage, parent, MessageTypeDefOf.NeutralEvent, false);
        }
    }

    private void ResetTicksRemaining()
    {
        ticksRemaining = Props.lifetimeRange.RandomInRange;
        tickInterval = ticksRemaining;
    }

    public override string CompInspectStringExtra()
    {
        string str;
        if (!Props.writeTimeLeft)
        {
            return null;
        }

        if (ticksRemaining > 0)
        {
            str = $"{Props.endVerb} in {ticksRemaining.ToStringTicksToPeriod()}";
        }
        else
        {
            str = "Expired";
        }

        return str;
    }
}