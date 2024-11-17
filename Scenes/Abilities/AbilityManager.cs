using Godot;
using Godot.Collections;

public partial class AbilityManager : Node2D
{
  public AnimatedSprite2D AnimationPlayer;
  public Array<Ability> abilityArray = new Array<Ability>();
  public bool IsActive = true;
  public int actionindex = 0;
  private Ability currentAbility = null;  // Track the currently active ability
  private int facingDirection = 1;
  private int ki = 0;

  public override void _Ready()
  {
    AnimationPlayer = GetParent().GetNode<AnimatedSprite2D>("Portrait");
    foreach (Ability child in GetChildren())
    {
      child.AnimationPlayer = AnimationPlayer;
      abilityArray.Add(child);
    }
    EventRegistry.RegisterEvent("ActionFinished");
    EventSubscriber.SubscribeToEvent("ActionFinished", DoNextActionAsync);
    EventRegistry.RegisterEvent("ActionCanceled");
    EventSubscriber.SubscribeToEvent("ActionCanceled", ActionCanceled);
  }

  public void AddAbility(Ability ability)
  {
    ability.AnimationPlayer = AnimationPlayer;
    abilityArray.Add(ability);
    AddChild(ability);
  }

  public void SetTargetGroup(string targetGroup)
  {
    foreach (Ability ability in abilityArray)
    {
      ability.SetTargetGroup(targetGroup);
    }
  }

  public void SetKI(int newKi)
  {
    ki = newKi;
  }

  public int GetKI()
  {
    return ki;
  }

  public void AddDamage(int newDamage)
  {
    foreach (Ability ability in abilityArray)
    {
      ability.abilityResource.Damage += newDamage;
    }
  }

  public void SetFacingDirection(int direction)
  {
    facingDirection = direction;

    foreach (Ability ability in abilityArray)
    {
      ability.SetFacingDirection(direction);
    }
  }

  public async void ActionCanceled(object sender, object[] args)
  {
    if (!IsInstanceValid(AnimationPlayer) || AnimationPlayer.IsQueuedForDeletion()) return;
    if (AnimationPlayer.Animation != "death")
    {
      if (AnimationPlayer.Animation == "hurt")
      {
        await ToSignal(AnimationPlayer, "animation_finished");
      }
      if (IsActive)
      {
        await ToSignal(GetTree().CreateTimer(3f, true, true), "timeout");
        DoNextActionAsync();
      }
    }
  }
  public void CancelCurrentAbility()
  {
    // Check if there's an ability currently executing and cancel it
    if (currentAbility != null) // Assuming IsExecuting() exists to track active ability status
    {
      currentAbility.Cancel();
      currentAbility = null; // Clear current ability after canceling
    }
  }

  public void DoNextActionAsync()
  {
    if (AnimationPlayer.Animation == "death" || AnimationPlayer.Animation == "hurt" || !IsActive) { GD.Print("Animation Playing: " + AnimationPlayer.Animation); return; }


    if (actionindex > abilityArray.Count - 1)
    {
      actionindex = 0;
      EventRegistry.GetEventPublisher("OnComboFinished").RaiseEvent(new object[] { this });
    }
    else
    {
      Ability nextAbility = abilityArray[actionindex];
      if (nextAbility.abilityResource.kiRequired <= ki) // Ensure enough Ki to perform the action
      {
        nextAbility.SetFacingDirection(facingDirection);
        currentAbility = nextAbility;  // Track the current executing ability
        nextAbility.Action();
        nextAbility.SpendKi(-nextAbility.abilityResource.kiRequired);
      }
      else // Not enough Ki, skip to the next action
      {
        actionindex++;
        EventRegistry.GetEventPublisher("ActionFinished").RaiseEvent(new object[] { this });
        return;
      }
      actionindex++;
    }
  }

  public void DoNextActionAsync(object sender, object[] args)
  {
    if (AnimationPlayer.Animation == "death" || !IsActive) return;

    if (args[0] is Node2D node)
    {
      var isInChildren = GetChildren().Contains(node);
      var isThisAbilityManager = node == this;
      if (!isThisAbilityManager && !isInChildren)
        return;
    }

    if (actionindex > abilityArray.Count - 1)
    {
      actionindex = 0;
      EventRegistry.GetEventPublisher("OnComboFinished").RaiseEvent(new object[] { this });
    }
    else
    {
      Ability nextAbility = abilityArray[actionindex];
      if (nextAbility.abilityResource.kiRequired <= ki) // Ensure enough Ki to perform the action
      {
        nextAbility.SetFacingDirection(facingDirection);
        currentAbility = nextAbility;  // Track the current executing ability
        nextAbility.Action();
        nextAbility.SpendKi(-nextAbility.abilityResource.kiRequired);
      }
      else // Not enough Ki, skip to the next action
      {
        actionindex++;
        EventRegistry.GetEventPublisher("ActionFinished").RaiseEvent(new object[] { this });
        return;
      }
      actionindex++;
    }
  }

  public AbilityResource GetNextAbility()
  {
    var i = actionindex < abilityArray.Count ? actionindex : 0;
    return abilityArray[i].abilityResource;
  }

  public void Deactivate()
  {
    IsActive = false;
    CancelCurrentAbility();
  }
  public void UnsubscribeFromEvents()
  {
    EventSubscriber.UnsubscribeFromEvent("ActionFinished", DoNextActionAsync);
  }

  public override void _ExitTree()
  {
    UnsubscribeFromEvents();
  }
}
