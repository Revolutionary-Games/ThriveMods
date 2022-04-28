using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;
using HarmonyLib;

namespace CellAutopilot
{
    [HarmonyPatch(typeof(PlayerMicrobeInput))]
    internal static class PlayerMicrobeInputPatch
    {
        static readonly AccessTools.FieldRef<PlayerMicrobeInput, bool> AutoMoveRef =
            AccessTools.FieldRefAccess<PlayerMicrobeInput, bool>("autoMove");

        public static void ResetAutoMove(PlayerMicrobeInput instance)
        {
            AutoMoveRef(instance) = false;
        }

        private static bool HasKeyRunAttribute(MethodInfo methodInfo)
        {
            if (methodInfo.GetCustomAttributes<RunOnKeyDownAttribute>().Any())
                return true;

            if (methodInfo.GetCustomAttributes<RunOnAxisAttribute>().Any())
                return true;

            if (methodInfo.GetCustomAttributes<RunOnKeyAttribute>().Any())
                return true;

            return false;
        }

        private static IEnumerable<MethodBase> TargetMethods()
        {
            return typeof(PlayerMicrobeInput).GetMethods(BindingFlags.Public | BindingFlags.Instance)
                .Where(method =>
                    HasKeyRunAttribute(method) &&
                    !method.Name.StartsWith(nameof(PlayerMicrobeInput.ToggleAutoMove)) &&
                    !method.Name.StartsWith(nameof(PlayerMicrobeInput.ShowSignalingCommandsMenu)) &&
                    !method.Name.StartsWith(nameof(PlayerMicrobeInput.CloseSignalingCommandsMenu)));
        }

        private static bool Prefix()
        {
            // Skip all the input methods we hooked into
            return false;
        }
    }

    [HarmonyPatch(typeof(PlayerMicrobeInput))]
    [HarmonyPatch(nameof(PlayerMicrobeInput.ToggleAutoMove))]
    internal class PlayerMicrobeInputAutoMovePatch
    {
        private static void PostFix(PlayerMicrobeInput __instance)
        {
            GD.Print("auto move was pressed (suppressed the press)");
            PlayerMicrobeInputPatch.ResetAutoMove(__instance);
        }
    }
}
