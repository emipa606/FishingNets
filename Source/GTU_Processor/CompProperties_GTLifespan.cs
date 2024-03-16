using Verse;

namespace GTU_Processor;

public class CompProperties_GTLifespan : CompProperties
{
    public readonly string endVerb = "Expires";

    public readonly string expiredMessage = null;
    public bool killAtEnd;

    public IntRange lifetimeRange = new IntRange(100, 100);

    public bool showMessageIfOwned;

    public bool writeTimeLeft;

    public CompProperties_GTLifespan()
    {
        compClass = typeof(CompGTLifespan);
    }
}