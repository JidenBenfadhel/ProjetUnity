using UnityEngine;

public class BulletSpeedBoost : Boost
{
    [SerializeField] private float speedRatio = 1.5f;

    public override void Apply(Collider player)
    {
        PlayerController controller = player.GetComponentInParent<PlayerController>();

        if (controller != null)
        {
            controller.projectileSpeed *= speedRatio;
        }
    }
}