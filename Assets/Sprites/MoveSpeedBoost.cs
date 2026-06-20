using UnityEngine;

public class MoveSpeedBoost : Boost
{
    [SerializeField] private float speedRatio = 1.3f;

    public override void Apply(Collider player)
    {
        PlayerController controller = player.GetComponentInParent<PlayerController>();

        if (controller != null)
            controller.moveSpeed *= speedRatio;
    }
}