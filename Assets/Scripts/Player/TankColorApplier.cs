using UnityEngine;

public class TankColorApplier : MonoBehaviour
{
    [Header("Matériau de Référence")]
    [Tooltip("Glisse ici un matériau vierge (ex: un matériau URP Lit blanc). Le script s'en servira de base pour peindre le tank.")]
    public Material tankBaseMaterial;

    void Start()
    {
        if (GameManager.Instance != null)
        {
            Color targetColor = GameManager.Instance.playerSelectedColor;
            ApplyNewMaterialWithColor(targetColor);
        }
    }

    private void ApplyNewMaterialWithColor(Color color)
    {
        if (tankBaseMaterial == null)
        {
            Debug.LogError("[TankColorApplier] Tu as oublié de glisser un matériau dans la case 'Tank Base Material' !");
            return;
        }

        MeshRenderer[] renderers = GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer rend in renderers)
        {
            if (rend == null) continue;

            string objName = rend.gameObject.name.ToLower();

            if (objName.Contains("track") || objName.Contains("wheel"))
            {
                continue;
            }

            Material newCustomMaterial = new Material(tankBaseMaterial);
            
            // On lui injecte la couleur sélectionnée
            if (newCustomMaterial.HasProperty("_BaseColor")) newCustomMaterial.SetColor("_BaseColor", color);
            if (newCustomMaterial.HasProperty("_Color")) newCustomMaterial.SetColor("_Color", color);

            // On remplace le matériau d'origine par notre nouveau matériau personnalisé
            rend.material = newCustomMaterial;
        }
    }
}