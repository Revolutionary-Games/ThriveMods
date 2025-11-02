using Arch.Core;
using Arch.Core.Extensions;
using Components;
using Godot;

/// <summary>
///   Main class of the mod
/// </summary>
public partial class DamageNumbers : IMod
{
    private FloatingDamageNumbers damageNumbers;

    private IModInterface storedInterface;

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

        // Store the mod interface for use later
        storedInterface = modInterface;

        // Set up our GUI control
        damageNumbers = new FloatingDamageNumbers();

        // Subscribe to the events we are interested in
        modInterface.OnDamageReceived += OnDamageReceived;

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

        // Remember to unsubscribe from all the events we subscribed to in Initialize,
        // otherwise mod unloading won't work correctly
        storedInterface.OnDamageReceived -= OnDamageReceived;

        // And release our mod interface reference
        storedInterface = null;

        // Release our other resources we created
        damageNumbers.QueueFree();
        damageNumbers = null;

        // Success
        return true;
    }

    /// <summary>
    ///   Called once initial node setup has finished, and it is possible to add children to the root node
    /// </summary>
    /// <param name="currentScene">
    ///   The scene we want might want to attach to, could also get these from the mod interface
    /// </param>
    /// <remarks>
    ///   <para>
    ///     As this mod wants to be always active, we directly attach to the scene tree root to stay attached even when
    ///     game scenes are changed.
    ///   </para>
    /// </remarks>
    public void CanAttachNodes(Node currentScene)
    {
        GD.Print("DamageNumbers mod is attaching nodes");

        currentScene.GetTree().Root.AddChild(damageNumbers);
    }

    private void OnDamageReceived(Entity damageReceiver, float amount, string source, bool isPlayer)
    {
        if (damageReceiver != default && damageReceiver.IsAliveAndHas<WorldPosition>())
        {
            // Callback is multithreaded, so we need to queue the operation
            damageNumbers.QueueAddNumber(amount, damageReceiver.Get<WorldPosition>().Position);
        }
    }
}
