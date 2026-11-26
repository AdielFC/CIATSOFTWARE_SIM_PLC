using CIATSIM.SoftPLC;
using UnityEngine;

/// <summary>
/// PLC virtual de la escena (va en SistemaCIATSIM, junto a TagDatabase).
///
/// Antes: la lógica se ejecutaba directo sobre los tags en cada Update.
/// Ahora: cada "tiempoScanMs" se ejecuta un ciclo de scan completo:
///   1. Lee las entradas %I de TagDatabase a la imagen de proceso.
///   2. Ejecuta el programa (marcha/paro) solo sobre la imagen.
///   3. Escribe las salidas %Q y marcas %M de la imagen a TagDatabase.
/// BotonPLC3D, LamparaPLC y el panel de Tags no cambian: siguen usando TagDatabase.
/// </summary>
public class SoftPlcBasico : MonoBehaviour
{
    [SerializeField]
    private TagDatabase tagDatabase;

    [Header("Ciclo de scan")]
    [Tooltip("Modo del PLC al dar Play.")]
    [SerializeField] private ModoPLC modoInicial = ModoPLC.RUN;

    [Tooltip("Tiempo de ciclo en milisegundos. Un PLC pequeño real ronda 1 a 20 ms.")]
    [Range(1, 500)]
    [SerializeField] private int tiempoScanMs = 10;

    [Header("Diagnóstico (solo lectura en Play)")]
    [SerializeField] private ModoPLC modoActual;
    [SerializeField] private long ciclosEjecutados;

    // Máximo de ciclos por frame, para no congelar Unity si un frame tarda mucho.
    private const int MaxCiclosPorFrame = 10;

    private CicloScan ciclo;
    private float acumulado;

    public ModoPLC Modo => ciclo != null ? ciclo.Modo : ModoPLC.STOP;

    private void Awake()
    {
        if (tagDatabase == null)
            tagDatabase = GetComponent<TagDatabase>();

        if (tagDatabase == null)
            Debug.LogError("CIATSIM: Falta TagDatabase.");
    }

    private void Start()
    {
        if (tagDatabase == null)
            return;

        // Se construye en Start para que TagDatabase ya haya cargado sus tags en Awake.
        ciclo = new CicloScan(tagDatabase);

        foreach (PlcTag tag in tagDatabase.ObtenerTags())
        {
            switch (tag.Tipo)
            {
                case TipoTagPLC.Entrada: ciclo.RegistrarEntrada(tag.Direccion); break;
                case TipoTagPLC.Salida: ciclo.RegistrarSalida(tag.Direccion); break;
                case TipoTagPLC.Memoria: ciclo.RegistrarMemoria(tag.Direccion); break;
            }
        }

        ciclo.Imagen.DireccionDesconocida += dir =>
            Debug.LogWarning("CIATSIM: el programa usa un tag que no existe: " + dir);

        ciclo.Programa = ProgramaMarchaParo.Ejecutar;
        ciclo.CambiarModo(modoInicial);
    }

    private void Update()
    {
        if (ciclo == null)
            return;

        // Acumulamos tiempo real y ejecutamos los ciclos que "quepan":
        // así el scan es de tiempo fijo aunque cambien los FPS.
        acumulado += Time.deltaTime;
        float periodo = tiempoScanMs / 1000f;

        int ciclos = 0;
        while (acumulado >= periodo && ciclos < MaxCiclosPorFrame)
        {
            ciclo.EjecutarCiclo();
            acumulado -= periodo;
            ciclos++;
        }

        if (ciclos == MaxCiclosPorFrame)
            acumulado = 0f;

        modoActual = ciclo.Modo;
        ciclosEjecutados = ciclo.CiclosEjecutados;
    }

    [ContextMenu("PLC en RUN")]
    public void PonerEnRun()
    {
        ciclo?.CambiarModo(ModoPLC.RUN);
    }

    [ContextMenu("PLC en STOP")]
    public void PonerEnStop()
    {
        ciclo?.CambiarModo(ModoPLC.STOP);
    }
}
