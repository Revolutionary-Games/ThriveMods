using System;
using System.Linq;
using Godot;
using HarmonyLib;

[HarmonyPatch(typeof(CellEditorComponent))]
[HarmonyPatch(nameof(CellEditorComponent.OnEditorSpeciesSetup))]
internal class RandomPartCellEditorPatch
{
    private const int MaxDistance = 1000;

    private static readonly AccessTools.FieldRef<CellEditorComponent, OrganelleLayout<OrganelleTemplate>>
        EditedOrganellesRef =
            AccessTools.FieldRefAccess<CellEditorComponent, OrganelleLayout<OrganelleTemplate>>(
                "editedMicrobeOrganelles");

    private static void Postfix(CellEditorComponent __instance)
    {
        var random = new Random();

        var nucleus = SimulationParameters.Instance.GetOrganelleType("nucleus");

        var organelles = EditedOrganellesRef(__instance);

        // If there are no organelles specified, this is probably the multicellular editor and the cell editor is
        // not initialized yet
        if (organelles.Count < 1)
        {
            GD.Print("No organelles to edit, won't add a random part just yet");
            return;
        }

        GD.Print("Adding random part in the cell editor");

        // Let's follow at least some placing rules
        bool hasNucleus = organelles.Any(o => o.Definition == nucleus);

        var rotationsToTry = Enumerable.Range(0, 6).OrderBy(_ => random.Next()).ToList();

        // Select a random valid organelle type to add
        foreach (var organelleDefinition in
                 SimulationParameters.Instance.GetAllOrganelles().OrderBy(_ => random.Next()))
        {
            if (organelleDefinition.Unimplemented ||
                organelleDefinition.EditorButtonGroup == OrganelleDefinition.OrganelleGroup.Hidden)
                continue;

            if (organelleDefinition == nucleus && hasNucleus)
                continue;

            if (organelleDefinition.RequiresNucleus && !hasNucleus)
                continue;

            if (organelleDefinition.Unique && organelles.Any(o => o.Definition == organelleDefinition))
                continue;

            for (int radius = 2; radius < MaxDistance; ++radius)
            {
                // Valid candidate for adding, try to find a position
                for (int q = 1; q < radius; ++q)
                {
                    // Every other coordinate will be a negative position to check
                    int actualQ = (q % 2 == 0 ? -1 : 1) * q / 2;

                    for (int r = 1; r < radius; ++r)
                    {
                        int actualR = (r % 2 == 0 ? -1 : 1) * r / 2;

                        foreach (var rotation in rotationsToTry)
                        {
                            var template = new OrganelleTemplate(organelleDefinition, new Hex(actualQ, actualR),
                                rotation);

                            if (!organelles.CanPlaceAndIsTouching(template))
                                continue;

                            if (AddOrganelle(__instance, template))
                            {
                                GD.Print("Hope you like your new random ", organelleDefinition.InternalName);
                                return;
                            }
                        }
                    }
                }
            }
        }

        GD.Print("Could not find a place for a random organelle");
    }

    [HarmonyReversePatch]
    [HarmonyPatch(typeof(CellEditorComponent), "AddOrganelle", typeof(OrganelleTemplate))]
    private static bool AddOrganelle(object instance, OrganelleTemplate organelle)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
///   Hooks into and disables a bunch of the editor methods, along with the following classes,
///   we don't want the player to have access to place additional parts or undo the random part
/// </summary>
[HarmonyPatch(typeof(CellEditorComponent))]
internal class RandomPartCellEditorDisableThingsPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("IsValidPlacement")]
    private static bool Prefix1(ref bool __result)
    {
        __result = false;
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(CellEditorComponent.ShowOrganelleOptions))]
    private static bool Prefix2()
    {
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch("TryRemoveHexAt")]
    private static bool Prefix3()
    {
        return false;
    }
}

[HarmonyPatch(typeof(EditorBase<CellEditorAction, MicrobeStage>))]
internal class RandomPartEditorBasePatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(MicrobeEditor.Undo))]
    private static bool Prefix1()
    {
        return false;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(MicrobeEditor.Redo))]
    private static bool Prefix2()
    {
        return false;
    }
}

[HarmonyPatch(typeof(HexEditorComponentBase<ICellEditorData, CellEditorAction, OrganelleTemplate>))]
internal class RandomPartHexEditorComponentBasePatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(CellEditorComponent.StartHexMove))]
    private static bool Prefix1()
    {
        return false;
    }
}
