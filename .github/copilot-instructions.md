# GitHub Copilot Instructions for Fishing Nets (Continued) Mod

## Mod Overview and Purpose

Fishing Nets (Continued) is a RimWorld mod that updates the original Fishing Nets mod by Sam. The mod adds the ability to catch fish from rivers, lakes, and oceans using various tiers of fishing nets. Additionally, it includes an option to build a fishing net on ice to reap the benefits of the ocean under the ice.

## Key Features and Systems

### Fishing Nets
- Different tiers of fishing nets that catch fish over time.
- Fishing nets occasionally break, with basic wooden fishing nets only lasting a couple of days.

### Ice Fishing
- An option to build a fishing net on ice to catch fish under the ice.

### Integration with Other Mods
- Compatible with various other mods such as Glitter Tech, Glitter Tech (no surgery), Tilled Soil, Furnace, and Dye Vat.

## Coding Patterns and Conventions

### C# Files
- **CompProperties_GTAnimation.cs**: Contains properties for animation components.
- **CompProperties_GTLifespan.cs**: Contains properties for lifespan components.
- **CompProperties_GTProcessor.cs**: Contains properties for processor components.
- **JobDefOf.cs**: Contains job definitions.
- **JobDriver_EmptyProcessor.cs**: Contains job driver for emptying processors.

### XML Files
- **Jobs_FishingNets.xml**: Contains job definitions related to fishing.
- **GTF_ResearchProjects.xml**: Contains research project definitions.
- **GTF_Buildings_Production.xml**: Contains building definitions related to production.
- **GTF_Items_Resource_Fish.xml**: Contains fish item definitions.
- **GTF_WorkGivers.xml**: Contains work giver definitions.

## XML Integration

### Defs
- **JobDef**: Defines jobs related to fishing.
- **ResearchProjectDef**: Defines research projects.
- **ThingDef**: Defines fish and building items.
- **WorkGiverDef**: Defines work givers.

## Harmony Patching

This mod uses Harmony patching to modify existing RimWorld game behavior. Harmony patches are used to add, remove, or modify code at runtime. For example, patches may be used to add new job definitions or modify the behavior of existing jobs.

### Example Harmony Patch
csharp
using HarmonyLib;
using RimWorld;
using Verse;

namespace FishingNets.HarmonyPatches
{
    [HarmonyPatch(typeof(JobGiver_Hunt))]
    [HarmonyPatch("TryGiveJob")]
    public static class Patch_JobGiver_Hunt_TryGiveJob
    {
        public static void Postfix(ref Job __result, Pawn ___pawn)
        {
            if (__result != null && __result.def == JobDefOf.Hunt)
            {
                __result.def = JobDefOf.Fish;
            }
        }
    }
}


## Suggestions for Copilot

1. **Consistent Naming**: Use consistent naming conventions for classes, methods, and variables to maintain code readability and maintainability.
2. **Modular Code**: Break down large methods into smaller, more manageable methods to improve code readability and maintainability.
3. **Documentation**: Add comments and documentation to explain the purpose and functionality of code.
4. **Testing**: Test code thoroughly to ensure it works as expected and does not introduce new bugs.
5. **Community Involvement**: Engage with the community through forums, Discord channels, and other communication platforms to gather feedback and improve the mod.

## Contributing

If you are interested in contributing to the Fishing Nets (Continued) mod, please follow these guidelines:

1. **Fork the Repository**: Fork the repository to your own GitHub account.
2. **Clone the Repository**: Clone the repository to your local machine.
3. **Create a Branch**: Create a new branch for your changes.
4. **Make Changes**: Make your changes to the code.
5. **Commit Changes**: Commit your changes with a descriptive commit message.
6. **Push Changes**: Push your changes to your forked repository.
7. **Create a Pull Request**: Create a pull request to merge your changes into the main repository.

## License

This mod is licensed under the MIT License. For more information, see the [LICENSE](LICENSE) file.

## Contact

If you have any questions or suggestions for the Fishing Nets (Continued) mod, please feel free to contact the author or the community through the provided channels.

## Project Solution Guidelines
- Relevant mod XML files are included as Solution Items under the solution folder named XML, these can be read and modified from within the solution.
- Use these in-solution XML files as the primary files for reference and modification.
- The `.github/copilot-instructions.md` file is included in the solution under the `.github` solution folder, so it should be read/modified from within the solution instead of using paths outside the solution. Update this file once only, as it and the parent-path solution reference point to the same file in this workspace.
- When making functional changes in this mod, ensure the documented features stay in sync with implementation; use the in-solution `.github` copy as the primary file.
- In the solution is also a project called Assembly-CSharp, containing a read-only version of the decompiled game source, for reference and debugging purposes.
- For any new documentation, update this copilot-instructions.md file rather than creating separate documentation files.


## Hard rules (must follow)
- Do NOT run commands that modify the repo (no git commit, git apply, dotnet format) unless explicitly asked.
- Prefer minimal reads: read only the smallest code region needed (around the suspicious lines).
- When mentioning SonarQube issues, automatically use the SonarQube MCP service to fetch and address issues instead of making inferred fixes without querying SonarQube first.
- When mentioning the rimworld log, automatically use the Rimworld MCP service to fetch the log.

