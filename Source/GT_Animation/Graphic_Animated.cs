using UnityEngine;
using Verse;

namespace GT_Animation;

public class Graphic_Animated : Graphic_Collection
{
    private int currentFrame;

    private bool initFromComps;

    private bool randomized;

    private int ticksPerFrame = 15;

    private int ticksPrev;

    public override Material MatSingle => subGraphics[0].MatSingle;

    public override Graphic GetColoredVersion(Shader newShader, Color newColor, Color newColorTwo)
    {
        if (newColorTwo != Color.white)
        {
            Log.Error("Cannot use Graphic_Animated.GetColoredVersion with a non-white colorTwo.");
        }

        return GraphicDatabase.Get<Graphic_Animated>(path, newShader, drawSize, newColor, Color.white, data);
    }

    public override Material MatAt(Rot4 rot, Thing thing = null)
    {
        return thing == null ? MatSingle : MatSingleFor(thing);
    }

    public override Material MatSingleFor(Thing thing)
    {
        return thing == null ? MatSingle : SubGraphicFor(thing).MatSingle;
    }

    public Graphic SubGraphicFor(Thing thing)
    {
        return thing == null ? subGraphics[0] : subGraphics[thing.thingIDNumber % subGraphics.Length];
    }

    public override void DrawWorker(Vector3 loc, Rot4 rot, ThingDef thingDef, Thing thing, float extraRotation)
    {
        if (thingDef == null)
        {
            Log.Error("Graphic_Animated with null thingDef");
            return;
        }

        if (subGraphics == null)
        {
            Log.Error("Graphic_Animated has no subgraphics");
            return;
        }

        Graphic graphic;
        if (thing == null)
        {
            graphic = SubGraphicFor(null);
        }
        else
        {
            if (thingDef.HasComp(typeof(CompGTAnimation)) && !initFromComps)
            {
                var comp = thing.TryGetComp<CompGTAnimation>();
                ticksPerFrame = comp.Props.frameSpeed;
                randomized = comp.Props.randomized;
                initFromComps = true;
            }

            var ticksCurrent = Find.TickManager.TicksGame;
            if (ticksCurrent >= ticksPrev + ticksPerFrame)
            {
                ticksPrev = ticksCurrent;
                if (randomized)
                {
                    currentFrame = Rand.Range(0, subGraphics.Length - 1);
                }
                else
                {
                    currentFrame++;
                }
            }

            if (currentFrame >= subGraphics.Length)
            {
                currentFrame = 0;
            }

            graphic = subGraphics[currentFrame];
        }

        graphic.DrawWorker(loc, rot, thingDef, thing, extraRotation);
    }

    public override string ToString()
    {
        return $"Animated(subgraphic[0]={subGraphics[0]}, count={subGraphics.Length})";
    }
}