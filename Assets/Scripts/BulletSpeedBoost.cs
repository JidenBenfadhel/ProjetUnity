using UnityEngine;

public class BulletSpeedBoost : Boost
{
    [SerializeField] private float speedRatio = 1.5f;

    public override void Apply(Collider player)
    {
        PlayerPowerUpManager manager = player.GetComponentInParent<PlayerPowerUpManager>();
        if (manager != null)
        {
            manager.ActivateBulletSpeedBoost(speedRatio, 5f);
        }
    }
}