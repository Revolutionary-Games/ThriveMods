using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using Godot;
using HarmonyLib;

namespace CellAutopilot
{
    /// <summary>
    ///   Sets up the player microbe so that it is used by AI
    /// </summary>
    [HarmonyPatch(typeof(Microbe))]
    [HarmonyPatch(nameof(Microbe.ProcessSync))]
    internal class MicrobeProcessPatch
    {
        private static readonly AccessTools.FieldRef<Microbe, MicrobeAI?> MicrobeAIRef =
            AccessTools.FieldRefAccess<Microbe, MicrobeAI?>("ai");

        private static void Postfix(Microbe __instance)
        {
            if (__instance.GetGroups().Cast<string>().All(g => g != Constants.AI_GROUP))
            {
                GD.Print("Adding player to AI group and creating AI instance for the cell");
                __instance.AddToGroup(Constants.AI_GROUP);

                MicrobeAIRef(__instance) = new MicrobeAI(__instance);
            }
        }
    }

    /// <summary>
    ///   This patch removes the check not allowing AI to run on the player microbe
    /// </summary>
    [HarmonyPatch(typeof(Microbe))]
    [HarmonyPatch(nameof(Microbe.AIThink))]
    internal class MicrobeAIThinkPatch
    {
        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var isPlayer = typeof(Microbe).GetProperty(nameof(Microbe.IsPlayerMicrobe))?.GetMethod;

            if (isPlayer == null)
                throw new Exception("Didn't find getter for player microbe check");

            bool foundCheck = false;
            bool foundBranch = false;

            foreach (var instruction in instructions)
            {
                if (instruction.Calls(isPlayer))
                {
                    foundCheck = true;
                }

                if (foundCheck && !foundBranch && instruction.Branches(out var label) && label.HasValue)
                {
                    // Replace the value we have on stack right now with always false to not hit the exception with
                    // AI not allowed for the player cell
                    yield return new CodeInstruction(OpCodes.Pop);
                    yield return new CodeInstruction(OpCodes.Ldc_I4_0);

                    foundBranch = true;
                }

                yield return instruction;
            }

            if (!foundBranch)
                GD.PrintErr("Cannot find player no AI allowed check in ", nameof(Microbe.AIThink));
        }
    }
}
