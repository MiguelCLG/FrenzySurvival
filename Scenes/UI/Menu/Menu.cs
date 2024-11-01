using Godot;
using System;

public partial class Menu : Control
{
  [Export] Control StartButton;
  [Export] Control OptionsButton;
  [Export] Control QuitButton;
  [Export] Control Title;

  public override void _Ready()
  {
    /* Tween tween;
    tween = CreateTween();
    tween.TweenProperty(Title, "position", new Vector2(450, 30), .2f);
    tween.TweenProperty(StartButton, "position", new Vector2(500, 100), .2f);
    tween.TweenProperty(OptionsButton, "position", new Vector2(500, 180), .2f);
    tween.TweenProperty(QuitButton, "position", new Vector2(500, 260), .2f); */
  }
  public void OnStartPressed() { GetTree().ChangeSceneToFile("res://Scenes/Main.tscn"); }
  public void OnOptionsPressed() { GD.Print("NOT IMPLEMENTED"); }
  public void OnQuitPressed() { GetTree().Quit(); }

  public void OnHover(string property)
  {
    Tween tween;
    tween = CreateTween();
    tween.SetParallel(true);
    switch (property)
    {
      case "Start":
        tween.TweenProperty(StartButton, "scale", new Vector2(1.2f, 1f), .2f);
        tween.TweenProperty(OptionsButton, "position", new Vector2(OptionsButton.Position.X, StartButton.Position.Y + 80), .2f);
        tween.TweenProperty(QuitButton, "position", new Vector2(QuitButton.Position.X, StartButton.Position.Y + 160), .1f);
        break;
      case "Options":
        tween.TweenProperty(OptionsButton, "scale", new Vector2(1.2f, 1f), .2f);
        tween.TweenProperty(QuitButton, "position", new Vector2(QuitButton.Position.X, StartButton.Position.Y + 160), .1f);
        break;
      case "Quit":
        tween.TweenProperty(QuitButton, "scale", new Vector2(1.2f, 1f), .1f);
        break;
    }
  }
  public void OnHoverExit(string property)
  {
    Tween tween;
    tween = CreateTween();
    tween.SetParallel(true);

    switch (property)
    {
      case "Start":
        tween.TweenProperty(StartButton, "scale", new Vector2(1f, 1f), .2f);
        tween.TweenProperty(OptionsButton, "position", new Vector2(500, 180), .2f);
        tween.TweenProperty(QuitButton, "position", new Vector2(500, 260), .2f);
        break;
      case "Options":
        tween.TweenProperty(OptionsButton, "scale", new Vector2(1f, 1f), .2f);
        tween.TweenProperty(QuitButton, "position", new Vector2(500, 260), .2f);
        break;
      case "Quit":
        tween.TweenProperty(QuitButton, "scale", new Vector2(1f, 1f), .2f);
        break;
    }
  }
}
