using Godot;
using System;

public partial class Menu : Control
{
  public void OnStartPressed() { GetTree().ChangeSceneToFile("res://Scenes/Main.tscn"); }
  public void OnOptionsPressed() { GD.Print("NOT IMPLEMENTED"); }
  public void OnQuitPressed() { GetTree().Quit(); }
}
