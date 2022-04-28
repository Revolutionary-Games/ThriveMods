using Godot;
using HarmonyLib;

public class RandomPartChallenge : IMod
{
    private const string OurHarmonyId = "com.revolutionarygamesstudio.thrive.mod.randomPartChallenge";

    private Harmony? harmony;

    public bool Initialize(IModInterface modInterface, ModInfo currentModInfo)
    {
        GD.Print("Patching with Harmony for RandomPartChallenge");
        harmony = new Harmony(OurHarmonyId);
        harmony.PatchAll();

        return true;
    }

    public bool Unload()
    {
        if (harmony != null)
        {
            GD.Print("Un-patching changes of RandomPartChallenge");
            harmony.UnpatchAll(OurHarmonyId);
        }

        return true;
    }

    public void CanAttachNodes(Node currentScene)
    {
    }
}
