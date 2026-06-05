using UnityEngine;

public class WaterScroll : MonoBehaviour
{
    [Header("Vitesse du courant")]
    public float scrollSpeedX = 0f;
    public float scrollSpeedY = 0.15f;

    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    void Update()
    {
        float offsetX = Time.time * scrollSpeedX;
        float offsetY = Time.time * scrollSpeedY;
        if (rend != null && rend.material != null)
        {
            rend.material.mainTextureOffset = new Vector2(offsetX, offsetY);
        }
    }
}