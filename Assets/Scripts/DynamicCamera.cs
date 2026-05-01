using UnityEngine;

public class DynamicCamera : MonoBehaviour
{
    [Header("Cibles")]
    public Transform player;

    [Header("Mouvement")]
    public float smoothTime = 0.3f;
    public Vector3 offset; 

    [Header("Limites de l'arène (Bords)")]
    public Vector2 minBounds; // Les coordonnées minimales (ex: X = -15, Z = -15)
    public Vector2 maxBounds; // Les coordonnées maximales (ex: X = 15, Z = 15)

    private Vector3 velocity = Vector3.zero;

    private void Start()
    {
        // Si l'offset n'est pas réglé dans l'Inspector, on prend la position initiale de la caméra
        if (offset == Vector3.zero)
        {
            offset = transform.position;
        }
    }

    private void LateUpdate()
    {
        if (player == null) return;

        Vector3 centerPoint = GetCenterPoint();

        Vector3 desiredPosition = centerPoint + offset;

        // Clamper (bloquer) les coordonnées pour ne pas dépasser les bords de l'arène
        desiredPosition.x = Mathf.Clamp(desiredPosition.x, minBounds.x, maxBounds.x);
        desiredPosition.z = Mathf.Clamp(desiredPosition.z, minBounds.y, maxBounds.y);

        // Déplacement ultra fluide vers la nouvelle position
        transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothTime);
    }

    private Vector3 GetCenterPoint()
    {
        // On récupère tous les ennemis actuellement dans l'arène grâce à leur Tag
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");

        // S'il n'y a plus d'ennemis, on centre uniquement sur le joueur
        if (enemies.Length == 0)
        {
            return player.position;
        }

        // Sinon, on additionne les positions du joueur ET de tous les ennemis
        Vector3 totalPositions = player.position;
        int targetCount = 1;

        foreach (GameObject enemy in enemies)
        {
            totalPositions += enemy.transform.position;
            targetCount++;
        }

        // On divise par le nombre total de cibles pour trouver le point central exact
        return totalPositions / targetCount;
    }
}