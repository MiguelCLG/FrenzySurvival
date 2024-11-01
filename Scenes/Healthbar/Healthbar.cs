using Godot;
using System;

public partial class Healthbar : ProgressBar
{
  [Export] public float Health { get; set; } = 100;
  [Export] public float MaxHealth { get; set; } = 100;
  public bool IsAlive { get; set; } = true;
  public override void _Ready()
  {
    Value = ConvertHealthToPercentage();

  }

  public void SetInitialValues(BaseCharacterResource resource)
  {
    Health = resource.HP;
    MaxHealth = resource.MaxHP;
    UpdateUI();
  }
  public void Heal(float value)
  {
    if (!IsAlive) return;
    Health += value;
    if (Health > MaxHealth)
      Health = MaxHealth;
    UpdateUI();
  }
  public void TakeDamage(float value)
  {
    if (!IsAlive) return;
    Health -= value;
    if (Health <= 0)
      Die();
    UpdateUI();
    //TODO: This is not updating the resource, send event to change to new stats
    //     AnimationPlayer.Play("hurt");
  }

  public void UpdateUI()
  {
    Value = ConvertHealthToPercentage();
  }

  public float ConvertHealthToPercentage()
  {
    var health = Health > 0 ? Health : 0;
    return health * 100 / MaxHealth;
  }

  public void Reset()
  {
    Health = MaxHealth;
    IsAlive = true;
  }

  public void Die()
  {
    IsAlive = false;
  }



}
