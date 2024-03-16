using Verse;

namespace GT_Animation;

internal class CompProperties_GTAnimation : CompProperties
{
    public readonly int frameSpeed = 15;
    public readonly bool randomized = false;

    public CompProperties_GTAnimation()
    {
        compClass = typeof(CompGTAnimation);
    }
}