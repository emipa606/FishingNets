using System.Collections.Generic;
using Verse;

namespace GT_ExplicitTerrainAffordance;

internal class CompProperties_GTExplicitTerrainAffordance : CompProperties
{
    public List<TerrainAffordanceDef> requiredAffordances = null;
    public List<TerrainDef> requiredTerrains = null;

    public CompProperties_GTExplicitTerrainAffordance()
    {
        compClass = typeof(CompGTExplicitTerrainAffordance);
    }
}