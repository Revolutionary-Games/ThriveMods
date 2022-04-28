using Godot;
using HarmonyLib;

[HarmonyPatch(typeof(CellEditorComponent))]
[HarmonyPatch(nameof(CellEditorComponent.OnEditorSpeciesSetup))]
internal class RandomPartCellEditorPatch
{
    /*static AccessTools.FieldRef<SomeGameClass, bool> isRunningRef =
        AccessTools.FieldRefAccess<SomeGameClass, bool>("isRunning");*/

    static void Postfix()
    {
        GD.Print("Cell editor setup should have happened");
    }
}
