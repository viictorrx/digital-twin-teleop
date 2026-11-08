using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform alvo; // O objeto que a câmera vai seguir
    public Vector3 distancia = new Vector3(0, 8f, -6f); // Altura (Y) e distância para trás (Z)
    public float suavidade = 5f;

    void LateUpdate()
    {
        if (alvo != null)
        {
            // Calcula a posição desejada
            Vector3 posicaoDesejada = alvo.position + distancia;
            
            // Move a câmera suavemente (Lerp) para a nova posição
            transform.position = Vector3.Lerp(transform.position, posicaoDesejada, suavidade * Time.deltaTime);
            
            // Faz a câmera olhar sempre para o robô
            transform.LookAt(alvo);
        }
    }
}