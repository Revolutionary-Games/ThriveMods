using System;
using Godot;
using HarmonyLib;

namespace CellAutopilot;

/// <summary>
///   Harmony patcher boilerplate for this mod
/// </summary>
public class CellAutopilot : IMod
{
    private const string OurHarmonyId = $"com.revolutionarygamesstudio.thrive.mod.{nameof(CellAutopilot)}";

    private Harmony? harmony;

    public bool Initialize(IModInterface modInterface, ModInfo currentModInfo)
    {
        GD.Print($"Patching with Harmony for {GetType().Name}");

        try
        {
            harmony = new Harmony(OurHarmonyId);
            harmony.PatchAll();
        }
        catch (Exception e)
        {
            throw new HarmonyLoadException(e);
        }

        return true;
    }

    public bool Unload()
    {
        if (harmony != null)
        {
            GD.Print($"Un-patching changes of {GetType().Name}");

            try
            {
                harmony.UnpatchAll(OurHarmonyId);
            }
            catch (Exception e)
            {
                throw new HarmonyLoadException(e);
            }
        }

        return true;
    }

    public void CanAttachNodes(Node currentScene)
    {
    }
}
