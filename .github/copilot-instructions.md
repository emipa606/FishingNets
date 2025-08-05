# GitHub Copilot Instructions for Fishing Nets (Continued) Mod

## Mod Overview and Purpose
**Fishing Nets (Continued)** is an enhanced version of an original mod by Sam. The mod introduces the ability to catch fish in RimWorld from various bodies of water including rivers, lakes, and oceans, using different tiers of fishing nets. As an innovative approach to resource gathering, this mod adds depth to the gameplay by integrating fishing into the colony's daily activities.

## Key Features and Systems
- **Fishing Nets**: Various types and tiers of nets that perform differently in terms of yield and durability. Basic wooden nets break down faster, encouraging players to progress to more advanced netting.
- **Ice Fishing**: Unique gameplay mechanics allowing players to place nets on ice and benefit from the ocean resources beneath.
- **Resource Management**: The fishing nets system includes lifespans, necessitating periodic replacement, thus requiring strategic resource and time management.

## Coding Patterns and Conventions
- **Namespace and Class Structure**: The mod follows a logical class structure using internal and public classes for component definition and processing logic (e.g., `CompGTAnimation`, `CompProperties_GTAnimation`). 
- **Code Consistency**: Utilizes meaningful class and method names that align with the mod's functionality, aiding readability and maintenance.
- **Access Modifiers**: Ensures classes and methods have appropriate access levels, using `internal` where components are not meant to be publicly exposed.

## XML Integration
- Integrate XML definitions for custom items and buildings, like fishing nets. XML files define attributes and behavior specifics which are used by the C# code.
- Modifies XML for jobs and work givers (e.g., `JobDefOf`, `WorkGiver_FillProcessor`) to introduce new job definitions related to the mod's features.

## Harmony Patching
- Employ Harmony for safe method patching to enhance base game methods where needed without altering the original game code base.
- Use Harmony for injecting custom logic related to fishing nets mechanics, ensuring compatibility with other mods and the base game updates.

## Suggestions for Copilot
- Utilize Copilot to quickly generate boilerplate code for new components, leveraging templates for ThingComps and CompProperties.
- Use Copilot to assist in creating custom job drivers and work givers, ensuring consistent interaction with your fishing nets.
- Suggest XML structure templates that align with RimWorld's modding framework when defining new items or altering existing ones.
- Generate test cases using Harmony Patch to ensure comprehensive testing of each new addition in isolated scenarios.
- Leverage Copilot's ability to refactor commonly occurring patterns for efficiency in the net-related component implementations.

This guide aims to help mod developers understand the core structures and methodologies in place, ensuring consistency and mod compatibility throughout development.
