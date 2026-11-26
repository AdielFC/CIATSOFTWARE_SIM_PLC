
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider))]
public class BotonPLC3D : MonoBehaviour
{
    [Header("Configuracion del PLC")]
    [SerializeField] private TagDatabase tagDatabase;
    [SerializeField] private string direccionEntrada = "%I0.0";

    [Header("Apariencia del boton")]
    [SerializeField] private Renderer visualPulsador;

    [SerializeField] private Color colorReposo = Color.green;

    [SerializeField]
    private Color colorPresionado =
        new Color(0.2f, 1f, 0.3f);

    private MaterialPropertyBlock propiedades;
    private bool presionado = false;

    private static readonly int ColorId =
        Shader.PropertyToID("_BaseColor");

    private void Awake()
    {
        propiedades = new MaterialPropertyBlock();

        if (visualPulsador == null)
            visualPulsador = GetComponent<Renderer>();
    }

    private void Start()
    {
        ActualizarColor(false);
    }

    private void Update()
    {
        if (tagDatabase == null || Mouse.current == null)
            return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            Camera camara = Camera.main;

            if (camara == null)
                return;

            Ray rayo = camara.ScreenPointToRay(
                Mouse.current.position.ReadValue());

            if (Physics.Raycast(rayo, out RaycastHit impacto, 100f))
            {
                if (impacto.transform == transform ||
                    impacto.transform.IsChildOf(transform))
                {
                    presionado = tagDatabase.Escribir(
                        direccionEntrada, true);

                    if (presionado)
                        ActualizarColor(true);
                }
            }
        }

        if (presionado &&
            Mouse.current.leftButton.wasReleasedThisFrame)
        {
            Soltar();
        }
    }

    private void Soltar()
    {
        if (tagDatabase != null)
            tagDatabase.Escribir(direccionEntrada, false);

        presionado = false;
        ActualizarColor(false);
    }

    private void OnDisable()
    {
        if (Application.isPlaying && presionado)
            Soltar();
    }

    private void ActualizarColor(bool activo)
    {
        if (visualPulsador == null)
            return;

        Color color = activo
            ? colorPresionado
            : colorReposo;

        visualPulsador.GetPropertyBlock(propiedades);
        propiedades.SetColor(ColorId, color);
        visualPulsador.SetPropertyBlock(propiedades);
    }
}
