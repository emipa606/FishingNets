using Verse;

namespace GTU_Processor;

public class CompProperties_GTProcessor : CompProperties
{
    public readonly int ingredientCount = -1;

    public readonly int resultCount = 1;

    public readonly int spawnMaxAdjacent = -1;

    public readonly string workVerb = "Processing";

    public readonly string workVerbPast = "Processed";
    public IntRange durationIntervalRange = new IntRange(100, 100);

    public bool requiresPower;

    public bool showMessageIfOwned;

    public bool spawnForbidden;

    public bool spawnOnFloor;
    public ThingDef thingIngredient;

    public ThingDef thingResult;

    public bool writeRequiredIngredients;

    public bool writeTimeLeftToProcess;

    public CompProperties_GTProcessor()
    {
        compClass = typeof(CompGTProcessor);
    }
}