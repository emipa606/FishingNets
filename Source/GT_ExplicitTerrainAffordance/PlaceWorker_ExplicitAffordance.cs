using Verse;

namespace GT_ExplicitTerrainAffordance
{
    public class PlaceWorker_ExplicitAffordance : PlaceWorker
    {
        public override AcceptanceReport AllowsPlacing(BuildableDef checkingDef, IntVec3 loc, Rot4 rot, Map map,
            Thing thingToIgnore = null, Thing thing = null)
        {
            var comp = ((ThingDef) checkingDef).GetCompProperties<CompProperties_GTExplicitTerrainAffordance>();
            if (comp.requiredAffordances != null)
            {
                foreach (var terrainAffordanceDef in comp.requiredAffordances)
                {
                    if (map.terrainGrid.TerrainAt(loc).affordances.Contains(terrainAffordanceDef))
                    {
                        return true;
                    }
                }
            }

            if (comp.requiredTerrains == null)
            {
                return "TerrainCannotSupport".Translate();
            }

            foreach (var terrainDef in comp.requiredTerrains)
            {
                if (map.terrainGrid.TerrainAt(loc) == terrainDef)
                {
                    return true;
                }
            }

            return "TerrainCannotSupport".Translate();
        }
    }
}