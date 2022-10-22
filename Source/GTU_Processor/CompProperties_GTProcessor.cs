using Verse;

namespace GTU_Processor;

public class CompProperties_GTProcessor : CompProperties
{
    public IntRange durationIntervalRange = new IntRange(100, 100);

    public int ingredientCount = -1;

    public bool requiresPower;

    public int resultCount = 1;

    public bool showMessageIfOwned;

    public bool spawnForbidden;

    public int spawnMaxAdjacent = -1;

    public bool spawnOnFloor;
    public ThingDef thingIngredient;

    public ThingDef thingResult;

    public string workVerb = "Processing";

    public string workVerbPast = "Processed";

    public bool writeRequiredIngredients;

    public bool writeTimeLeftToProcess;

    public CompProperties_GTProcessor()
    {
        compClass = typeof(CompGTProcessor);
    }
}