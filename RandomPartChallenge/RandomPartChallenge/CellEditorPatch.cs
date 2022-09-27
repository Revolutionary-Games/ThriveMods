using System;
using System.Linq;
using Godot;
using HarmonyLib;

[HarmonyPatch(typeof(CellEditorComponent))]
[HarmonyPatch(nameof(CellEditorComponent.OnEditorSpeciesSetup))]
internal class RandomPartCellEditorPatch
{
    private const int MaxDistance = 1000;

    /// <summary>
    ///   We need to know when we need to prevent further organelle placing as our random organelle add now depends
    ///   on the organelle placing check passing
    /// </summary>
    internal static bool PerformingAutomaticAdd;

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

        PerformingAutomaticAdd = true;

        // Select a random valid organelle type to add
        // TODO: for easier mode could make the nucleus much less likely or require the resulting stationary ATP
        // balance to be positive
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

                            var placed = CreatePlaceActionIfPossible(__instance, template);

                            if (placed != null && EnqueueAction(__instance, new CombinedEditorAction(placed)))
                            {
                                GD.Print("Hope you like your new random ", organelleDefinition.InternalName);
                                PerformingAutomaticAdd = false;
                                return;
                            }
                        }
                    }
                }
            }
        }

        GD.Print("Could not find a place for a random organelle");
        PerformingAutomaticAdd = false;
    }

    // We need to match the nullability of the actual method that is reverse patched
    // ReSharper disable once ReturnTypeCanBeNotNullable
    [HarmonyReversePatch]
    [HarmonyPatch(typeof(CellEditorComponent), "CreatePlaceActionIfPossible", typeof(OrganelleTemplate))]
    private static CombinedEditorAction? CreatePlaceActionIfPossible(object instance, OrganelleTemplate organelle)
    {
        throw new NotImplementedException();
    }

    [HarmonyReversePatch]
    [HarmonyPatch(
        typeof(HexEditorComponentBase<ICellEditorData, CombinedEditorAction, EditorAction, OrganelleTemplate>),
        "EnqueueAction", typeof(CombinedEditorAction))]
    private static bool EnqueueAction(object instance, CombinedEditorAction action)
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
        if (RandomPartCellEditorPatch.PerformingAutomaticAdd)
        {
            GD.Print("Allowing normal placement logic to check position");
            return true;
        }

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
    [HarmonyPatch("TryCreateRemoveHexAtAction")]
    private static bool Prefix3(ref EditorAction? __result)
    {
        __result = null;
        return false;
    }
}

[HarmonyPatch(typeof(EditorBase<EditorAction, MicrobeStage>))]
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

[HarmonyPatch(typeof(HexEditorComponentBase<ICellEditorData, CombinedEditorAction, EditorAction, OrganelleTemplate>))]
internal class RandomPartHexEditorComponentBasePatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(CellEditorComponent.StartHexMove))]
    private static bool Prefix1()
    {
        return false;
    }
}
