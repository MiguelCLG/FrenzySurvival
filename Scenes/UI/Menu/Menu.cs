using Godot;
using System;
using System.Collections.Generic;

public partial class Menu : Control
{
  [Export] Control StartButton;
  [Export] Control OptionsButton;
  [Export] Control QuitButton;
  [Export] Control Title;

  [Export] Godot.Collections.Dictionary<string, AudioOptionsResource> menuSounds;

  private AudioManager audioManager;

  public override void _Ready()
  {
    StartButton = GetNode<Control>("%StartGameButton");
    OptionsButton = GetNode<Control>("%OptionsButton");
    QuitButton = GetNode<Control>("%QuitGameButton");

    /* Tween tween;
    tween = CreateTween();
    tween.TweenProperty(Title, "position", new Vector2(450, 30), .2f);
    tween.TweenProperty(StartButton, "position", new Vector2(500, 100), .2f);
    tween.TweenProperty(OptionsButton, "position", new Vector2(500, 180), .2f);
    tween.TweenProperty(QuitButton, "position", new Vector2(500, 260), .2f); */
    audioManager = GetNode<AudioManager>("/root/AudioManager");
    audioManager?.Play(menuSounds.GetValueOrDefault("music"), this);
    StartButton.GrabFocus();
  }

  private void OnInputEvent(InputEvent @event)
  {
    
    Node focusOwner = GetViewport().GuiGetFocusOwner();
    if(focusOwner is null || focusOwner is not Button) StartButton.GrabFocus();

    if (@event is InputEventKey eventKey && eventKey.Pressed && eventKey.IsAction("ui_accept"))
    {
      // This gets the node that has focus, and if its a button it will press it.
      if (focusOwner is Button button)
      {
        button.EmitSignal("pressed");
      }
    }
  }

  public override void _ExitTree()
  {
      // Stop any sound associated with this node
      audioManager?.StopSound(this);
  }
  public void OnStartPressed() 
  {     
    audioManager?.Play(menuSounds.GetValueOrDefault("HoverClick"), this);
    GetTree().ChangeSceneToFile("res://Scenes/Main.tscn"); 
  }
  public void OnOptionsPressed() 
  { 
    audioManager?.Play(menuSounds.GetValueOrDefault("HoverClick"), this);
    var audioOptions = GetNode<Control>("%AudioOptionsScreen");
    if(audioOptions is AudioOptionsScreen sfxOptions)
    {
      sfxOptions.Visible = true;
    }
  }
  public void OnQuitPressed() { GetTree().Quit(); }
  
  public void OnHover(string property)
  {
    Tween tween;
    tween = CreateTween();
    tween.SetParallel(true);
    audioManager?.Play(menuSounds.GetValueOrDefault("HoverClick"), this);

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
        tween.TweenProperty(OptionsButton, "position", new Vector2(OptionsButton.Position.X, StartButton.Position.Y + 80), .2f);
        tween.TweenProperty(QuitButton, "position", new Vector2(QuitButton.Position.X, StartButton.Position.Y + 160), .1f);
        break;
      case "Options":
        tween.TweenProperty(OptionsButton, "scale", new Vector2(1f, 1f), .2f);
        tween.TweenProperty(QuitButton, "position", new Vector2(QuitButton.Position.X, StartButton.Position.Y + 160), .1f);
        break;
      case "Quit":
        tween.TweenProperty(QuitButton, "scale", new Vector2(1f, 1f), .2f);
        break;
    }
  }

}
