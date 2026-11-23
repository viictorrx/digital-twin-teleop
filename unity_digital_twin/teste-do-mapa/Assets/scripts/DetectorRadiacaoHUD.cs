using UnityEngine;

public class DetectorRadiacaoHUD : MonoBehaviour
{
    [Header("Referências")]
    public Transform alvoRobo;
    public RobotController robotController;

    [Header("Valores Base (µSv/h)")]
    public float radiacaoFundo = 0.18f;
    public float limiteAlerta = 10f;
    public float limiteCritico = 35f;

    private float radiacaoAtual;
    private float oscilacao;

    void Update()
    {
        if (alvoRobo == null) return;

        float leituraZonas = 0f;
        ZonaRadiacao[] zonas = FindObjectsByType<ZonaRadiacao>(FindObjectsSortMode.None);

        foreach (ZonaRadiacao zona in zonas)
        {
            float valor = zona.CalcularRadiacaoEm(alvoRobo.position);
            if (valor > leituraZonas)
            {
                leituraZonas = valor;
            }
        }

        oscilacao = Random.Range(-0.08f, 0.08f) * Mathf.Max(1f, leituraZonas * 0.1f);
        float alvoRadiacao = Mathf.Max(radiacaoFundo, leituraZonas) + oscilacao;
        radiacaoAtual = Mathf.Lerp(radiacaoAtual, alvoRadiacao, Time.deltaTime * 6f);
    }

    void OnGUI()
    {
        float largura = 270f;
        float altura = 115f;
        float margem = 16f;
        Rect painel = new Rect(Screen.width - largura - margem, margem, largura, altura);

        string estado = "SEGURO";
        Color corTexto = Color.green;

        if (radiacaoAtual >= limiteCritico)
        {
            estado = "PERIGO CRÍTICO (USAR WASD)";
            corTexto = new Color(1f, 0.25f, 0.25f);
        }
        else if (radiacaoAtual >= limiteAlerta)
        {
            estado = "ALERTA DE RADIAÇÃO";
            corTexto = new Color(1f, 0.8f, 0.1f);
        }

        GUI.Box(painel, "");

        GUILayout.BeginArea(new Rect(painel.x + 10, painel.y + 8, painel.width - 20, painel.height - 16));

        GUIStyle estiloTitulo = new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 13
        };
        GUILayout.Label("GUARDIÃO // DOSÍMETRO", estiloTitulo);

        GUIStyle estiloValor = new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 16
        };
        estiloValor.normal.textColor = corTexto;
        GUILayout.Label($"{radiacaoAtual:0.00} µSv/h", estiloValor);

        GUILayout.Label($"Status: {estado}");

        string modoTexto = (robotController != null)
            ? robotController.modoAtual.ToString()
            : "Autonomo";
        GUILayout.Label($"Modo: {modoTexto} [Tecla E]");

        GUILayout.EndArea();
    }
}