using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
    private const float NumberLifespan = 1.5f;

    private static readonly Vector3 NumberVelocity = new(0, 10.0f, -0.5f);
    private static readonly Vector3 VelocityRandomNess = new(0.05f, 5.0f, -1.0f);

    private readonly List<FloatingNumber> activeNumbers = new();
    private readonly Random random = new();

    private Camera camera;

    public override void _Ready()
    {
        GD.Print("FloatingDamageNumbers class loaded");

        // Fullscreen
        AnchorRight = 1;
        AnchorBottom = 1;

        // But don't block mouse input
        MouseFilter = MouseFilterEnum.Ignore;

        // For more easily seeing this Node in the Godot debugger we give us a name (needs to be unique in the parent
        // node we attach to)
        Name = nameof(FloatingDamageNumbers);
    }

    public override void _Process(float delta)
    {
        // When cameras are detached from the main scene they don't have Current set to false even if
        // they aren't active
        if (camera == null || (!camera.Current || !camera.IsInsideTree()))
        {
            TryGetCamera();

            // No camera, we can't do anything
            // TODO: would be good to still remove the existing numbers
            if (camera == null)
                return;

            GD.Print("DamageNumbers found a camera");
        }

        var cameraTransform = camera.GlobalTransform;
        var cameraTranslation = cameraTransform.origin;

        // If we wanted to constraint the labels to the screen area we would need this variable
        // var screenArea = GetViewport().GetVisibleRect();

        foreach (var number in activeNumbers)
        {
            number.LifespanRemaining -= delta;
            number.CurrentPosition += number.Velocity * delta;

            var numberTranslation = number.CurrentPosition;

            bool isBehindCamera = cameraTransform.basis.z.Dot(numberTranslation - cameraTranslation) > 0;

            // Fade if close to camera
            var distance = cameraTranslation.DistanceTo(numberTranslation);

            float alpha = Mathf.Clamp(RangeLerp(distance, 0, 2, 0, 1), 0, 1);

            // TODO: make it not start to fade immediately
            alpha = Mathf.Min(alpha, number.LifespanRemaining / number.TotalLifespan);

            var unprojectedPosition = camera.UnprojectPosition(numberTranslation);

            // In the example project there's some fancy logic for keeping stuff on-screen edges if it would otherwise
            // go off-screen
            if (isBehindCamera)
            {
                number.AssociatedLabel.Visible = false;
            }
            else
            {
                number.AssociatedLabel.RectPosition = unprojectedPosition;
                number.AssociatedLabel.Visible = true;
                number.AssociatedLabel.SelfModulate = new Color(1, 1, 1, alpha);
            }
        }

        var toRemove = activeNumbers.Where(n => n.LifespanRemaining < 0).ToList();

        foreach (var number in toRemove)
        {
            number.AssociatedLabel.Free();
            activeNumbers.Remove(number);
        }
    }

    public void AddNumber(float damage, Vector3 position)
    {
        var label = new Label()
        {
            Text = Math.Round(damage, 1).ToString(CultureInfo.CurrentCulture),
        };

        // TODO: make the text label center better on the position instead of having the left edge of the number there

        // TODO: change text colour based on the damage

        AddChild(label);

        activeNumbers.Add(new FloatingNumber
        {
            AssociatedLabel = label,
            CurrentPosition = position,
            LifespanRemaining = NumberLifespan,
            TotalLifespan = NumberLifespan,
            Velocity = NumberVelocity + VelocityRandomNess * (float)random.NextDouble(),
        });
    }

    /// <summary>
    ///   Seems to be missing from Godot C#, helpfully provided on Godot Q & A site:
    ///   https://godotengine.org/qa/91310/where-is-range_lerp-in-c%23 by AlexTheRegent
    ///   Modified to conform to naming style.
    /// </summary>
    private static float RangeLerp(float value, float iStart, float iStop, float oStart, float oStop)
    {
        return oStart + (oStop - oStart) * value / (iStop - iStart);
    }

    private void TryGetCamera()
    {
        camera = GetViewport().GetCamera();
    }

    private class FloatingNumber
    {
        public Vector3 CurrentPosition;
        public Vector3 Velocity;
        public float LifespanRemaining;
        public float TotalLifespan;

        public Label AssociatedLabel;
    }
}
