using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Alvo")]
    public Transform alvo; // Arraste o robo-base para aqui

    [Header("Ajustes de Posição")]
    public Vector3 offset = new Vector3(0f, 3.5f, -6f);
    public float suavidade = 8f;

    void LateUpdate()
    {
        if (alvo == null) return;

        // Calcula a posição desejada atrás e acima do robô
        Vector3 posicaoDesejada = alvo.position + alvo.TransformDirection(offset);
        
        // Move a câmara suavemente até lá
        transform.position = Vector3.Lerp(transform.position, posicaoDesejada, suavidade * Time.deltaTime);

        // Faz a câmara olhar sempre para o robô (um pouco acima da base)
        transform.LookAt(alvo.position + Vector3.up * 1.2f);
    }
}