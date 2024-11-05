using System.Linq;
using System.Threading.Tasks;
using Godot;
using Godot.Collections;

public partial class AbilityManager : Node2D
{
  public AnimatedSprite2D AnimationPlayer;
  public Array<Ability> abilityArray = new Array<Ability>();

  public int actionindex = 0;
  int facingDirection = 1;
  int ki = 0;

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
  }

  public void AddAbility(Ability ability)
  {
    ability.AnimationPlayer = AnimationPlayer;
    abilityArray.Add(ability);
    AddChild(ability);
  }

  public void SetKI(int newKi)
  {
    ki = newKi;
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

  public void DoNextActionAsync()
  {
    if (actionindex > abilityArray.Count - 1)
    {
      actionindex = 0;

      EventRegistry.GetEventPublisher("OnComboFinished").RaiseEvent(new object[] { });
    }
    else
    {
      if (abilityArray[actionindex].abilityResource.kiRequired <= ki) // it is not a super
      {
        abilityArray[actionindex].SetFacingDirection(facingDirection);
        abilityArray[actionindex].Action();
        abilityArray[actionindex].SpendKi(-abilityArray[actionindex].abilityResource.kiRequired);
      }
      else
      {
        actionindex++;
        EventRegistry.GetEventPublisher("ActionFinished").RaiseEvent(new object[] { });
        return;
      }
      actionindex++;
    }
  }

  public void DoNextActionAsync(object sender, object[] args)
  {

    if (actionindex > abilityArray.Count - 1)
    {
      actionindex = 0;
      EventRegistry.GetEventPublisher("OnComboFinished").RaiseEvent(new object[] { });

    }
    else
    {
      if (abilityArray[actionindex].abilityResource.kiRequired <= ki) // it is not a super
      {
        abilityArray[actionindex].SetFacingDirection(facingDirection);
        abilityArray[actionindex].Action();
        abilityArray[actionindex].SpendKi(-abilityArray[actionindex].abilityResource.kiRequired);
      }
      else // it is a super but does not have enough ki
      {
        actionindex++;
        EventRegistry.GetEventPublisher("ActionFinished").RaiseEvent(new object[] { });
        return;
      }
      actionindex++;
    }
  }

  public override void _ExitTree()
  {
    EventSubscriber.UnsubscribeFromEvent("ActionFinished", DoNextActionAsync);
  }
}
