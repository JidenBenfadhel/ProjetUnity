using UnityEngine;

public class LifeBoost : Boost
{
    [SerializeField] private int healAmount = 1;

    public override void Apply(Collider player)
    {
        PlayerHealth hp = player.GetComponentInParent<PlayerHealth>();

        if (hp != null)
        {
            hp.health += 1;
        }
    }
}