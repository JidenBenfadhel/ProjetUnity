using UnityEngine;

public class LifeBoost : Boost
{
    public override void Apply(Collider player)
    {
        PlayerPowerUpManager manager = player.GetComponentInParent<PlayerPowerUpManager>();
        if (manager != null)
        {
            manager.ActivateShieldBoost();
        }
    }
}