using Godot;
using System;
using System.Linq;
using System.Collections.Generic;


public partial class Main : Node2D
{
  [Export] BaseCharacterResource[] mobsResourceReference;
  [Export] MobSpawnRules[] mobSpawnRules;
  [Export] Godot.Collections.Dictionary<string, AudioOptionsResource> mainSounds;

  private AudioManager audioManager;

  LevelUpUi levelUpUi;
  CharacterSelectionScreen characterSelectionScreen;
  Node2D playerReference;
  PackedScene mobScene;
  PackedScene mobBossScene;
  CanvasLayer UI;
  Node2D mobContainer;
  double spawnCooldown, time;
  int level = 1;
  double maxEnemies = 10;
  double currentEnemies = 0;
  double enemiesKilled = 0;
  public override void _Ready()
  {
    levelUpUi = GetNode<LevelUpUi>("%LevelUpUI");
    characterSelectionScreen = GetNode<CharacterSelectionScreen>("%CharacterSelectionScreen");
    mobScene = GD.Load<PackedScene>("res://Scenes/Mob/Mob.tscn");
    mobBossScene = GD.Load<PackedScene>("res://Scenes/Mob/MobBoss.tscn");
    mobContainer = GetNode<Node2D>("%MobContainer");
    playerReference = GetNode<Player>("Player");
    UI = GetNode<CanvasLayer>("%UI");
    spawnCooldown = time = 0;
    EventRegistry.RegisterEvent("OnMobDeath");
    EventSubscriber.SubscribeToEvent("OnMobDeath", OnMobDeath);
    EventRegistry.RegisterEvent("CharacterSelected");
    EventSubscriber.SubscribeToEvent("CharacterSelected", PlayerCharacterSelected);
    EventRegistry.RegisterEvent("OnPlayerDeath");
    EventSubscriber.SubscribeToEvent("OnPlayerDeath", OnPlayerDeath);

    GetTree().Paused = true;
    audioManager = GetNode<AudioManager>("/root/AudioManager");
    audioManager?.Play(mainSounds.GetValueOrDefault("music"), this);
    characterSelectionScreen.Visible = true;
  }

  public override void _Process(double delta)
  {
    if (!playerReference.GetNode<Healthbar>("Healthbar").IsAlive) return;
    spawnCooldown += delta;
    time += delta;
    //if (currentEnemies < maxEnemies && spawnCooldown > 1)
    //{
    //  CreateMob();
    //  spawnCooldown = 0;
    //}
    HandleSpawnRules();
    currentEnemies = mobContainer.GetChildren().Count;

    UI.GetNode<Label>("%EnemyCountLabel").Text = $"Enemies: {currentEnemies}";
    // Convert 'time' to minutes and seconds
    int minutes = (int)(time / 60);  // Divide by 60 to get minutes
    int seconds = (int)(time % 60);  // Modulus 60 to get remaining seconds

    // Format the time as "mm:ss"
    string formattedTime = $"{minutes:D2}:{seconds:D2}";
    UI.GetNode<Label>("%TimerLabel").Text = formattedTime;
    UI.GetNode<Label>("%EnemiesKilledLabel").Text = $"Kills: {enemiesKilled}";

    if (minutes > 10)
    {
      GD.Print("Jogo Terminado");
      GetNode<CanvasLayer>("%UI").GetNode<Control>("%GameOverScreen").Visible = true;

    }
  }

  private void HandleSpawnRules()
  {
    for (int i = 0; i < mobSpawnRules.Length; i++)
      foreach (var kvp in mobSpawnRules[i].GetUnitsToSpawn(time))
      {
        int numToSpawn = kvp.Value - CountSpawnedMobsWithResource(kvp.Key);
        if (numToSpawn > 0)
          for (int j = 0; j < numToSpawn; j++)
            CreateMobOfResource(kvp.Key);
      }
  }

  public void CreateMob()
  {
    var mob = mobScene.Instantiate<Mob>();
    mobContainer.AddChild(mob);
    mob.mobResource = mobsResourceReference[level - 1];
    mob.healthbar.SetInitialValues(mob.mobResource);
    mob.AnimationPlayer.SpriteFrames = mob.mobResource.AnimatedFrames;

    Vector2 randomPositionPositive = playerReference.GlobalPosition + new Vector2(Random.Shared.Next(100, 300), Random.Shared.Next(100, 300));
    Vector2 randomPositionNegative = playerReference.GlobalPosition + new Vector2(Random.Shared.Next(-300, -100), Random.Shared.Next(-300, -100));
    mob.GlobalPosition = Random.Shared.NextDouble() > 0.5 ? randomPositionPositive : randomPositionNegative;

    // mob.GlobalPosition = playerReference.GlobalPosition - new Vector2(Random.Shared.Next(-100, 100), Random.Shared.Next(-100, 100));
  }
  public void CreateMobOfResource(BaseCharacterResource charResourse)
  {
    Vector2 randomPositionPositive = playerReference.GlobalPosition + new Vector2(Random.Shared.Next(100, 300), Random.Shared.Next(100, 300));
    Vector2 randomPositionNegative = playerReference.GlobalPosition + new Vector2(Random.Shared.Next(-300, -100), Random.Shared.Next(-300, -100));
    var position = Random.Shared.NextDouble() > 0.5 ? randomPositionPositive : randomPositionNegative;
    GD.Print(charResourse.ResourceName);
    if (charResourse.ResourcePath.Contains("Boss"))
    {
      var mob = mobBossScene.Instantiate<MobBoss>();
      mob.mobResource = charResourse;
      mobContainer.AddChild(mob);
      mob.healthbar.SetInitialValues(mob.mobResource);
      mob.GlobalPosition = position;
    }
    else
    {
      var mob = mobScene.Instantiate<Mob>();
      mob.mobResource = charResourse;
      mob.AnimationPlayer.SpriteFrames = mob.mobResource.AnimatedFrames;
      mobContainer.AddChild(mob);
      mob.healthbar.SetInitialValues(mob.mobResource);
      mob.GlobalPosition = position;
    }




    // mob.GlobalPosition = playerReference.GlobalPosition - new Vector2(Random.Shared.Next(-100, 100), Random.Shared.Next(-100, 100));
  }
  private int CountSpawnedMobsWithResource(BaseCharacterResource targetResource)
  {
    int count = 0;

    foreach (var child in mobContainer.GetChildren())
    {
      if (child is Mob mob && mob.mobResource == targetResource)
      {
        count++;
      }
    }

    return count;
  }
  public async void OnPlayerDeath(object sender, object[] args)
  {
    await ToSignal(GetTree().CreateTimer(5f), "timeout");

    GetNode<CanvasLayer>("%UI").GetNode<Control>("%GameOverScreen").Visible = true;

    GD.Print("Jogo Terminado");
    PauseGame();
  }

  public void PauseGame()
  {
    audioManager.PauseAllSounds();
    GetTree().Paused = true;
  }

  public void PlayerCharacterSelected(object sender, object[] args)
  {
    (playerReference as Player).playerResource = (args[0] as CharacterSelectionResource).CharacterResource;
    (playerReference as Player).PrepareCharacter();
    levelUpUi.SetPlayerResource((args[0] as CharacterSelectionResource).CharacterResource);
    levelUpUi.Visible = true;
  }
  public async void OnMobDeath(object sender, object[] args)
  {
    if (args[0] is Mob mob)
    {
      await ToSignal(GetTree().CreateTimer(2f), "timeout");
      if (IsInstanceValid(mob) && !mob.IsQueuedForDeletion())
      {
        enemiesKilled++;
        //if (Random.Shared.NextDouble() > 0.5)
        //  mob.DropKi();
        mob.HandleLootDrop();
        mob.GetParent().RemoveChild(mob);
        mob.QueueFree();
      }
    }
  }

  public void Restart()
  {
    GetTree().Paused = false;
    foreach (Timer timer in GetTree().GetNodesInGroup("Timers"))
    {
      timer.Stop();
      timer.GetParent().RemoveChild(timer);
      timer.QueueFree();
    }
    GetTree().CallDeferred("reload_current_scene");
  }
  public void Options()
  {
    var audioOptions = GetNode<CanvasLayer>("%UI").GetNode<Control>("%AudioOptionsScreen");
    if (audioOptions is AudioOptionsScreen sfxOptions)
    {
      sfxOptions.Visible = true;
    }
  }
  public void Exit()
  {
    GetTree().Paused = false;
    audioManager.UnpauseAllSounds();
    GetTree().ChangeSceneToFile("res://Scenes/UI/Menu/Menu.tscn");
  }

  public override void _UnhandledKeyInput(InputEvent @event)
  {
    if (Input.IsActionPressed("ui_cancel"))
    {
      HandlePause();
    }
  }

  public override void _Input(InputEvent @event)
  {
    if (Input.IsActionPressed("ui_cancel") && @event is InputEventJoypadButton)
    {
      HandlePause();
    }
  }

  public void HandlePause()
  {
    GetTree().Paused = !GetTree().Paused;
    UI.GetNode<Control>("%PauseScreen").Visible = GetTree().Paused;
    if (GetTree().Paused)
      audioManager.PauseAllSoundsFromBus("Sound Effects");
    else
      audioManager.UnpauseAllSounds();  // TODO: Unpause all from bus.

  }

  public override void _ExitTree()
  {
    audioManager?.StopSound(this);
    EventSubscriber.UnsubscribeFromEvent("OnMobDeath", OnMobDeath);
    EventSubscriber.UnsubscribeFromEvent("OnPlayerDeath", OnPlayerDeath);
    EventSubscriber.UnsubscribeFromEvent("CharacterSelected", PlayerCharacterSelected);
    EventRegistry.UnregisterEvent("OnMobDeath");
    EventRegistry.UnregisterEvent("OnPlayerDeath");
    EventRegistry.UnregisterEvent("ActionFinished");
    EventRegistry.UnregisterEvent("TakeDamage");
    EventRegistry.UnregisterEvent("OnComboFinished");
    EventRegistry.UnregisterEvent("OnComboFinished");
    EventRegistry.UnregisterEvent("CharacterSelected");
    //audioManager?.StopSound(this);
  }
}
