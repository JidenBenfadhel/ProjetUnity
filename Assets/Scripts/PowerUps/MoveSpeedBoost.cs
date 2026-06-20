using UnityEngine;

public class MoveSpeedBoost : Boost
{
    [SerializeField] private float speedRatio = 1.3f;

    public override void Apply(Collider player)
    {
        PlayerPowerUpManager manager = player.GetComponentInParent<PlayerPowerUpManager>();
        if (manager != null)
        {
            manager.ActivateMoveSpeedBoost(speedRatio, 5f); 
        }
    }
}