using Godot;

/// <summary>
///   Main class of the mod
/// </summary>
public class DamageNumbers : IMod
{
    private FloatingDamageNumbers damageNumbers;

    /// <summary>
    ///   Called when the mod is being loaded
    /// </summary>
    /// <param name="modInterface">An object with many useful mod operations we can use</param>
    /// <param name="currentModInfo">
    ///   This is the info of our own mod JSON file, in case we want to look at something there
    /// </param>
    /// <returns>True on success, false on failure</returns>
    public bool Initialize(IModInterface modInterface, ModInfo currentModInfo)
    {
        GD.Print("DamageNumbers mod is initializing");

        damageNumbers = new FloatingDamageNumbers();

        // Success
        return true;
    }

    /// <summary>
    ///   Called when this mod needs to be unloaded
    /// </summary>
    /// <returns>True on success</returns>
    public bool Unload()
    {
        GD.Print("DamageNumbers mod is unloading");

        // Success
        return true;
    }

    /// <summary>
    ///   Called once initial node setup has finished and it is possible to add children to the root node
    /// </summary>
    /// <param name="currentScene">The scene we want to attach to, could also get these from the mod interface</param>
    public void CanAttachNodes(Node currentScene)
    {
        GD.Print("DamageNumbers mod is attaching nodes");

        currentScene.GetParent().AddChild(damageNumbers);
    }
}
