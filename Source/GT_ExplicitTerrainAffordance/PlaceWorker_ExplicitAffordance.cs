using Verse;

namespace GT_ExplicitTerrainAffordance;

public class PlaceWorker_ExplicitAffordance : PlaceWorker
{
    public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map,
        Thing thingToIgnore = null, Thing thing = null)
    {
        var comp = ((ThingDef)checkingDef).GetCompProperties<CompProperties_GTExplicitTerrainAffordance>();
        if (comp.requiredAffordances != null && comp.requiredAffordances.Any(terrrainAffordance =>
                map.terrainGrid.TerrainAt(loc).affordances.Contains(terrrainAffordance)))
        {
            return true;
        }

        if (comp.requiredTerrains == null)
        {
            return "TerrainCannotSupport".Translate();
        }

        if (comp.requiredTerrains.Any(terrain => map.terrainGrid.TerrainAt(loc) == terrain))
        {
            return true;
        }

        return "TerrainCannotSupport".Translate();
    }
}