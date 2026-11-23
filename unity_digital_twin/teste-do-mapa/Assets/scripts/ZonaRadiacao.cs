using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ZonaRadiacao : MonoBehaviour
{
    [Header("Nível de Radiação (µSv/h)")]
    public float radiacaoMaxima = 45f;
    public float radiacaoMinimaBorda = 2f;

    [Header("Alcance da Zona")]
    public float raioZona = 8f;

    void Reset()
    {
        Collider col = GetComponent<Collider>();
        col.isTrigger = true;

        if (col is SphereCollider sphere)
        {
            sphere.radius = raioZona;
        }
    }

    public float CalcularRadiacaoEm(Vector3 pontoRobo)
    {
        float distancia = Vector3.Distance(transform.position, pontoRobo);
        if (distancia >= raioZona) return 0f;

        float fator = 1f - Mathf.Clamp01(distancia / raioZona);
        return Mathf.Lerp(radiacaoMinimaBorda, radiacaoMaxima, fator);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.8f, 0f, 0.25f);
        Gizmos.DrawSphere(transform.position, raioZona);
    }
}