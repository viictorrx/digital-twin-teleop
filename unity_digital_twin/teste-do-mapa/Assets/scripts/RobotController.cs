using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class RobotController : MonoBehaviour
{
    public enum ModoOperacao
    {
        Autonomo,
        EmergenciaManual
    }

    [Header("Modo de Operação")]
    public ModoOperacao modoAtual = ModoOperacao.Autonomo;
    public KeyCode teclaAlternarEmergencia = KeyCode.E;

    [Header("Configurações de Movimento")]
    public float velocidadeMovimento = 2f;
    public float velocidadeRotacao = 50f;

    [Header("Sensores (Leque Rente ao Chão)")]
    public float distanciaSensor = 2.5f;
    public float anguloLeque = 25f;
    public float alturaOrigemSensor = 0.12f;
    public LayerMask layerObstaculos;

    [Header("Patrulha Autônoma e Desvio")]
    public float tempoGiroDesvio = 0.85f;
    public float anguloBuscaLateral = 60f;
    public float tempoChecagemTravamento = 1.5f;
    public float deslocamentoMinimoTravamento = 0.15f;

    private Rigidbody rb;
    private float cronometroDesvio;
    private bool estaDesviando;
    private int direcaoGiroAtual = 1;

    private Vector3 ultimaPosicaoCheck;
    private float cronometroTravamento;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ultimaPosicaoCheck = rb.position;
    }

    void Update()
    {
        if (Input.GetKeyDown(teclaAlternarEmergencia))
        {
            modoAtual = (modoAtual == ModoOperacao.Autonomo)
                ? ModoOperacao.EmergenciaManual
                : ModoOperacao.Autonomo;

            estaDesviando = false;
            cronometroTravamento = 0f;
            ultimaPosicaoCheck = rb.position;
            Debug.Log($"[Guardião] Modo alterado para: {modoAtual}");
        }

        if (modoAtual == ModoOperacao.Autonomo)
        {
            AvaliarSensoresEmLeque();
        }
    }

    void FixedUpdate()
    {
        if (modoAtual == ModoOperacao.Autonomo)
        {
            VerificarTravamento();
            ExecutarPatrulhaAutonoma();
        }
        else
        {
            ExecutarControleManual();
        }
    }

    private void AvaliarSensoresEmLeque()
    {
        Vector3 origem = transform.position + Vector3.up * alturaOrigemSensor;

        Vector3 dirCentro = transform.forward;
        Vector3 dirEsquerda = Quaternion.Euler(0f, -anguloLeque, 0f) * transform.forward;
        Vector3 dirDireita = Quaternion.Euler(0f, anguloLeque, 0f) * transform.forward;

        bool hitCentro = ChecarRaio(origem, dirCentro, distanciaSensor);
        bool hitEsquerda = ChecarRaio(origem, dirEsquerda, distanciaSensor * 0.85f);
        bool hitDireita = ChecarRaio(origem, dirDireita, distanciaSensor * 0.85f);

        if ((hitCentro || hitEsquerda || hitDireita) && !estaDesviando)
        {
            IniciarDesvio(origem);
        }
    }

    private bool ChecarRaio(Vector3 origem, Vector3 direcao, float distancia)
    {
        bool atingiu = Physics.Raycast(origem, direcao, out _, distancia, layerObstaculos);
        Debug.DrawRay(origem, direcao * distancia, atingiu ? Color.red : Color.green);
        return atingiu;
    }

    private void IniciarDesvio(Vector3 origem)
    {
        estaDesviando = true;
        cronometroDesvio = tempoGiroDesvio;
        direcaoGiroAtual = EscolherMelhorLadoGiro(origem);
    }

    private int EscolherMelhorLadoGiro(Vector3 origem)
    {
        Vector3 dirEsq = Quaternion.Euler(0f, -anguloBuscaLateral, 0f) * transform.forward;
        Vector3 dirDir = Quaternion.Euler(0f, anguloBuscaLateral, 0f) * transform.forward;

        float distEsquerda = MedirEspacoLivre(origem, dirEsq);
        float distDireita = MedirEspacoLivre(origem, dirDir);

        // -1 gira para a esquerda, 1 gira para a direita
        return (distEsquerda >= distDireita) ? -1 : 1;
    }

    private float MedirEspacoLivre(Vector3 origem, Vector3 direcao)
    {
        float alcanceTeste = distanciaSensor * 2f;
        if (Physics.Raycast(origem, direcao, out RaycastHit hit, alcanceTeste, layerObstaculos))
        {
            return hit.distance;
        }
        return alcanceTeste;
    }

    private void VerificarTravamento()
    {
        if (estaDesviando)
        {
            cronometroTravamento = 0f;
            ultimaPosicaoCheck = rb.position;
            return;
        }

        cronometroTravamento += Time.fixedDeltaTime;
        if (cronometroTravamento >= tempoChecagemTravamento)
        {
            float distanciaPercorrida = Vector3.Distance(
                new Vector3(rb.position.x, 0f, rb.position.z),
                new Vector3(ultimaPosicaoCheck.x, 0f, ultimaPosicaoCheck.z)
            );

            if (distanciaPercorrida < deslocamentoMinimoTravamento)
            {
                Vector3 origem = transform.position + Vector3.up * alturaOrigemSensor;
                IniciarDesvio(origem);
                cronometroDesvio = tempoGiroDesvio * 1.4f;
            }

            ultimaPosicaoCheck = rb.position;
            cronometroTravamento = 0f;
        }
    }

    private void ExecutarPatrulhaAutonoma()
    {
        if (estaDesviando)
        {
            float anguloGiro = direcaoGiroAtual * velocidadeRotacao * Time.fixedDeltaTime;
            Quaternion novaRotacao = rb.rotation * Quaternion.Euler(0f, anguloGiro, 0f);
            rb.MoveRotation(novaRotacao);

            cronometroDesvio -= Time.fixedDeltaTime;
            if (cronometroDesvio <= 0f)
            {
                estaDesviando = false;
                ultimaPosicaoCheck = rb.position;
                cronometroTravamento = 0f;
            }
            return;
        }

        Vector3 deslocamento = transform.forward * velocidadeMovimento * Time.fixedDeltaTime;
        rb.MovePosition(rb.position + deslocamento);
    }

    private void ExecutarControleManual()
    {
        float vertical = Input.GetAxisRaw("Vertical");     // W / S
        float horizontal = Input.GetAxisRaw("Horizontal"); // A / D

        if (Mathf.Abs(horizontal) > 0.01f)
        {
            float anguloGiro = horizontal * velocidadeRotacao * Time.fixedDeltaTime;
            Quaternion novaRotacao = rb.rotation * Quaternion.Euler(0f, anguloGiro, 0f);
            rb.MoveRotation(novaRotacao);
        }

        if (Mathf.Abs(vertical) > 0.01f)
        {
            Vector3 deslocamento = transform.forward * vertical * velocidadeMovimento * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + deslocamento);
        }
    }
}