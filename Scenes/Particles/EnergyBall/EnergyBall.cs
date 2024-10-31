using Godot;
using System;
using System.Collections.Generic;

public partial class EnergyBall : Node2D
{
  [Export] PackedScene beamLineScene; // The beam line packed scene
  private List<Line2D> beamLines = new List<Line2D>(); // Store beam lines
  [Export] private int numBeamLines = 5; // Number of beam lines
  [Export] private float beamLength = 500f; // Length of the beams
  [Export] private float rotationSpeed = 1f; // Speed of rotation
  private float fadeSpeed = 100f;
  bool isDecreasing = false;

  public override void _Ready()
  {
    for (int i = 0; i < numBeamLines; i++)
    {
      // Instantiate the beam line and add it to the scene
      Line2D beamLine = beamLineScene.Instantiate<Line2D>();
      AddChild(beamLine);
      beamLines.Add(beamLine);

      // Define the start and end points of the beam
      beamLine.ClearPoints(); // Clear any existing points

      // Start point at the center of the EnergyBall (0, 0)
      beamLine.AddPoint(Vector2.Zero);

      // End point in a circular pattern around the center
      float angle = Mathf.Tau / numBeamLines * i; // Evenly spread angle
      Vector2 endPoint = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * beamLength;
      beamLine.AddPoint(endPoint);
      beamLine.Modulate = Color.Color8(255, 255, 255, 50);
    }
  }

  public override void _PhysicsProcess(double delta)
  {
    // Rotate the EnergyBall (and beams around it)
    Rotation += rotationSpeed * (float)delta;
    foreach (Line2D beamLine in beamLines)
    {
      var currentColor = beamLine.Modulate;
      var newAlpha = isDecreasing ? currentColor.A8 - (float)delta * fadeSpeed : currentColor.A8 + (float)delta * fadeSpeed;
      if (newAlpha <= 50)
      {
        newAlpha = 50;
        isDecreasing = false;
      }
      else if (newAlpha >= 255)
      {
        newAlpha = 255;
        isDecreasing = true;
      }
      beamLine.Modulate = Color.Color8(255, 255, 255, (byte)MathF.Round(newAlpha));
    }
  }
}
