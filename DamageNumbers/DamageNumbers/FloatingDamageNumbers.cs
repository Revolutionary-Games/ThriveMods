using Godot;

/// <summary>
///   Handles positioning Labels as-if they were in a 3D scene
/// </summary>
/// <remarks>
///   <para>
///     Math is from this sample project: https://github.com/godotengine/godot-demo-projects/tree/master/3d/waypoints
///     Specifically from here: https://github.com/godotengine/godot-demo-projects/blob/master/3d/waypoints/waypoint.gd
///   </para>
/// </remarks>
public class FloatingDamageNumbers : Control
{
    public override void _Ready()
    {
        GD.Print("FloatingDamageNumbers class loaded");

        var label = new Label()
        {
            Text = "Test from a mod",
        };

        AddChild(label);
    }

    public override void _Process(float delta)
    {
    }
}
