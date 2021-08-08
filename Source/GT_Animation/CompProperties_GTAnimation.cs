using Verse;

namespace GT_Animation
{
    internal class CompProperties_GTAnimation : CompProperties
    {
        public int frameSpeed = 15;
        public bool randomized = false;

        public CompProperties_GTAnimation()
        {
            compClass = typeof(CompGTAnimation);
        }
    }
}