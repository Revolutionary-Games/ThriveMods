using Arch.Core;
using Arch.Core.Extensions;
using Components;
using Godot;
using HarmonyLib;

namespace CellAutopilot
{
    /// <summary>
    ///   Makes player microbe use AI movement
    /// </summary>
    [HarmonyPatch(typeof(MicrobeStage))]
    [HarmonyPatch("SpawnPlayer")]
    internal class MicrobeStagePatch
    {
        private static void Postfix(MicrobeStage __instance)
        {
            Entity? player = __instance.Player;

            if (player == null! || player == default(Entity) || !player.Value.IsAlive())
            {
                GD.PrintErr("Couldn't get player after spawn");
                return;
            }

            if (player.Value.Has<MicrobeAI>())
            {
                // Already present, can't try to add the component again
                return;
            }

            GD.Print("Adding AI component to player");
            player.Value.Add(new MicrobeAI
            {
                FocusedPrey = Entity.Null,
            });
        }
    }
}
