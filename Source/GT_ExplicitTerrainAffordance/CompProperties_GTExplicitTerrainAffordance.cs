using System.Collections.Generic;
using Verse;

namespace GT_ExplicitTerrainAffordance;

internal class CompProperties_GTExplicitTerrainAffordance : CompProperties
{
    public readonly List<TerrainAffordanceDef> requiredAffordances = null;
    public readonly List<TerrainDef> requiredTerrains = null;

    public CompProperties_GTExplicitTerrainAffordance()
    {
        compClass = typeof(CompGTExplicitTerrainAffordance);
    }
}