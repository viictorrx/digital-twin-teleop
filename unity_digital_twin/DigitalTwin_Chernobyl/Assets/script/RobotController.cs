using UnityEngine;

public class RobotController : MonoBehaviour
{
    public float linearSpeed = 0.8f;
    public float angularSpeed = 90.0f;
    public float distanciaFrente = 1.5f;   // Alcance do sensor frontal (vermelho)
    public float distanciaLateral = 0.8f;  // Alcance mais curto para não bater nas casas ao lado
    public float anguloLateral = 30.0f;    // Ângulo mais fechado (30 graus) para passar em ruas estreitas
    public float alturaSensor = 0.5f;

    private Rigidbody rb;
    private bool girandoParado = false;
    private float direcaoGiroFixa = 1f;

void Start()
{
    rb = GetComponent<Rigidbody>();
    // Trava o capotamento (X e Z) e impede que o robô saia do chão (Position Y)
    rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezePositionY;
}

    void FixedUpdate()
    {
        Vector3 origemSensor = transform.position + new Vector3(0, alturaSensor, 0);

        Vector3 frente = transform.forward;
        Vector3 esquerda = Quaternion.Euler(0, -anguloLateral, 0) * frente;
        Vector3 direita = Quaternion.Euler(0, anguloLateral, 0) * frente;

        bool obstaculoFrente = Physics.Raycast(origemSensor, frente, out RaycastHit hitFrente, distanciaFrente);
        bool obstaculoEsquerda = Physics.Raycast(origemSensor, esquerda, out RaycastHit hitEsquerda, distanciaLateral);
        bool obstaculoDireita = Physics.Raycast(origemSensor, direita, out RaycastHit hitDireita, distanciaLateral);

        Debug.DrawRay(origemSensor, frente * distanciaFrente, Color.red);
        Debug.DrawRay(origemSensor, esquerda * distanciaLateral, Color.yellow);
        Debug.DrawRay(origemSensor, direita * distanciaLateral, Color.yellow);

        // CASO 1: Obstáculo bem na frente -> Para e gira no próprio eixo até liberar a frente
        if (obstaculoFrente)
        {
            rb.linearVelocity = Vector3.zero;

            // Define o lado do giro apenas no início da manobra para nunca ficar tremendo
            if (!girandoParado)
            {
                girandoParado = true;
                float espacoEsquerda = obstaculoEsquerda ? hitEsquerda.distance : distanciaLateral;
                float espacoDireita = obstaculoDireita ? hitDireita.distance : distanciaLateral;

                // Gira para o lado que tiver mais espaço livre
                direcaoGiroFixa = (espacoEsquerda > espacoDireita) ? -1f : 1f;
            }

            Quaternion curva = Quaternion.Euler(0f, angularSpeed * direcaoGiroFixa * Time.fixedDeltaTime, 0f);
            rb.MoveRotation(rb.rotation * curva);
        }
        // CASO 2: Frente livre -> Continua andando pelo corredor e só corrige a direção suavemente
        else
        {
            girandoParado = false;

            float desvioSuave = 0f;
            if (obstaculoEsquerda && !obstaculoDireita)
            {
                desvioSuave = 0.5f; // Chegou perto da parede esquerda, puxa suave para a direita
            }
            else if (obstaculoDireita && !obstaculoEsquerda)
            {
                desvioSuave = -0.5f; // Chegou perto da parede direita, puxa suave para a esquerda
            }
            else if (obstaculoEsquerda && obstaculoDireita)
            {
                // Se as duas laterais tocarem num beco estreito, centraliza pela parede mais distante
                desvioSuave = (hitEsquerda.distance < hitDireita.distance) ? 0.3f : -0.3f;
            }

            if (desvioSuave != 0f)
            {
                Quaternion ajuste = Quaternion.Euler(0f, angularSpeed * desvioSuave * Time.fixedDeltaTime, 0f);
                rb.MoveRotation(rb.rotation * ajuste);
            }

            Vector3 movimento = transform.forward * linearSpeed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + movimento);
        }
    }
}