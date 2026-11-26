
using UnityEngine;

public class LamparaPLC : MonoBehaviour
{
    [Header("Configuracion del PLC")]
    [SerializeField] private TagDatabase tagDatabase;
    [SerializeField] private string direccionSalida = "%Q0.1";

    [Header("Apariencia de la lampara")]
    [SerializeField] private Renderer lenteVisual;

    [SerializeField]
    private Color colorApagado =
        new Color32(40, 90, 55, 255);

    [SerializeField]
    private Color colorEncendido =
        new Color32(57, 255, 102, 255);

    private MaterialPropertyBlock propiedades;
    private bool? estadoAnterior;

    private static readonly int ColorId =
        Shader.PropertyToID("_BaseColor");

    private void Awake()
    {
        propiedades = new MaterialPropertyBlock();

        if (tagDatabase == null)
        {
            tagDatabase = GetComponentInParent<TagDatabase>();
        }

        if (tagDatabase == null)
        {
            Debug.LogWarning(
                "CIATSIM: Falta asignar TagDatabase a la lampara.");
        }

        if (lenteVisual == null)
        {
            Debug.LogWarning(
                "CIATSIM: Falta asignar el lente visual.");
        }
    }

    private void Update()
    {
        if (tagDatabase == null || lenteVisual == null)
            return;

        if (!tagDatabase.TryLeer(
            direccionSalida, out bool encendida))
            return;

        if (estadoAnterior.HasValue &&
            estadoAnterior.Value == encendida)
            return;

        ActualizarApariencia(encendida);
        estadoAnterior = encendida;
    }

    private void ActualizarApariencia(bool encendida)
    {
        Color nuevoColor = encendida
            ? colorEncendido
            : colorApagado;

        lenteVisual.GetPropertyBlock(propiedades);

        propiedades.SetColor(ColorId, nuevoColor);

        lenteVisual.SetPropertyBlock(propiedades);
    }
}
