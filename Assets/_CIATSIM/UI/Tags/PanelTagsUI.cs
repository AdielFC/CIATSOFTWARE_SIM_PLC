
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public class PanelTagsUI : MonoBehaviour
{
    [Header("Conexion con el PLC")]
    [SerializeField] private TagDatabase tagDatabase;

    private readonly List<PlcTag> tags = new List<PlcTag>();
    private readonly List<Label> valores = new List<Label>();

    private float siguienteActualizacion;

    private void Start()
    {
        if (tagDatabase == null)
        {
            Debug.LogError(
                "CIATSIM: Asigna TagDatabase al PanelTagsUI.");
            return;
        }

        UIDocument documento = GetComponent<UIDocument>();
        VisualElement root = documento.rootVisualElement;

        // Limpiar la interfaz
        root.Clear();
        tags.Clear();
        valores.Clear();

        // Configuracion general
        root.pickingMode = PickingMode.Ignore;
        root.style.flexDirection = FlexDirection.Row;
        root.style.justifyContent = Justify.FlexEnd;
        root.style.paddingTop = 10;
        root.style.paddingRight = 10;

        // Contenedor principal compacto
        VisualElement columna = new VisualElement();

        columna.style.width = 320;
        columna.style.maxWidth = Length.Percent(100);
        columna.style.flexShrink = 0;
        columna.pickingMode = PickingMode.Ignore;

        root.Add(columna);

        // Boton para mostrar u ocultar Tags
        Button botonMostrar = new Button();

        botonMostrar.text = "Mostrar / Ocultar Tags";
        botonMostrar.style.width = 190;
        botonMostrar.style.height = 30;
        botonMostrar.style.alignSelf = Align.FlexEnd;
        botonMostrar.style.marginBottom = 5;
        botonMostrar.style.fontSize = 11;

        columna.Add(botonMostrar);

        // Panel de administracion
        VisualElement panel = new VisualElement();

        panel.style.backgroundColor =
            new Color(0.08f, 0.11f, 0.16f, 0.96f);

        panel.style.paddingTop = 10;
        panel.style.paddingBottom = 10;
        panel.style.paddingLeft = 10;
        panel.style.paddingRight = 10;

        panel.style.borderTopLeftRadius = 8;
        panel.style.borderTopRightRadius = 8;
        panel.style.borderBottomLeftRadius = 8;
        panel.style.borderBottomRightRadius = 8;

        panel.style.flexShrink = 0;

        columna.Add(panel);

        // Administrador inicialmente cerrado
        bool abierto = false;
        panel.style.display = DisplayStyle.None;

        botonMostrar.clicked += () =>
        {
            abierto = !abierto;

            panel.style.display = abierto
                ? DisplayStyle.Flex
                : DisplayStyle.None;
        };

        // Titulo principal
        Label titulo = new Label("CIATSOFTWARE | TAGS");

        titulo.style.fontSize = 16;
        titulo.style.color = Color.white;

        titulo.style.unityFontStyleAndWeight =
            FontStyle.Bold;

        titulo.style.marginBottom = 5;

        panel.Add(titulo);

        // Descripcion
        Label descripcion = new Label(
            "Variables del PLC virtual - Solo lectura");

        descripcion.style.fontSize = 10;

        descripcion.style.color =
            new Color(0.65f, 0.72f, 0.80f);

        descripcion.style.marginBottom = 8;

        panel.Add(descripcion);

        // Encabezado de la tabla
        VisualElement encabezado = CrearFila();

        encabezado.style.height = 26;
        encabezado.style.marginBottom = 4;

        encabezado.style.backgroundColor =
            new Color(0.16f, 0.21f, 0.29f);

        // Columnas ajustadas al ancho de 320 px
        CrearCelda(encabezado, "NOMBRE", 105, true);
        CrearCelda(encabezado, "DIRECCION", 70, true);
        CrearCelda(encabezado, "TIPO", 62, true);
        CrearCelda(encabezado, "VALOR", 44, true);

        panel.Add(encabezado);

        // Contenedor de variables
        // Sin ScrollView para las cinco variables iniciales
        VisualElement tabla = new VisualElement();

        tabla.style.flexDirection = FlexDirection.Column;
        tabla.style.flexShrink = 0;

        panel.Add(tabla);

        int indiceFila = 0;

        // Cargar variables del PLC virtual
        foreach (PlcTag tag in tagDatabase.ObtenerTags())
        {
            tags.Add(tag);

            VisualElement fila = CrearFila();

            // Altura reducida para ahorrar espacio
            fila.style.height = 28;

            // Colores alternados entre filas
            if (indiceFila % 2 == 0)
            {
                fila.style.backgroundColor =
                    new Color(0.12f, 0.16f, 0.22f);
            }
            else
            {
                fila.style.backgroundColor =
                    new Color(0.09f, 0.13f, 0.19f);
            }

            // Informacion de la variable
            CrearCelda(fila, tag.Nombre, 105);
            CrearCelda(fila, tag.Direccion, 70);
            CrearCelda(fila, tag.Tipo.ToString(), 62);

            // Estado de la variable
            Label estado = CrearCelda(
                fila, "FALSE", 44);

            valores.Add(estado);

            tabla.Add(fila);

            indiceFila++;
        }

        // Total de variables
        Label totalTags = new Label(
            "Total de variables: " + tags.Count);

        totalTags.style.fontSize = 10;

        totalTags.style.color =
            new Color(0.60f, 0.68f, 0.78f);

        totalTags.style.marginTop = 7;

        panel.Add(totalTags);

        // Actualizar valores al iniciar
        ActualizarValores();

        Debug.Log(
            "CIATSIM: Administrador de Tags iniciado. " +
            tags.Count + " variables cargadas.");
    }

    // Crear una fila horizontal
    private VisualElement CrearFila()
    {
        VisualElement fila = new VisualElement();

        fila.style.flexDirection = FlexDirection.Row;
        fila.style.alignItems = Align.Center;
        fila.style.flexShrink = 0;

        fila.style.paddingLeft = 4;
        fila.style.paddingRight = 4;

        return fila;
    }

    // Crear las celdas de la tabla
    private Label CrearCelda(
        VisualElement fila,
        string contenido,
        float ancho,
        bool esEncabezado = false)
    {
        Label celda = new Label(contenido);

        celda.style.width = ancho;
        celda.style.flexShrink = 0;

        // Tipografia compacta
        celda.style.fontSize =
            esEncabezado ? 9 : 10;

        celda.style.color = esEncabezado
            ? new Color(0.73f, 0.81f, 0.91f)
            : Color.white;

        celda.style.unityTextAlign =
            TextAnchor.MiddleLeft;

        if (esEncabezado)
        {
            celda.style.unityFontStyleAndWeight =
                FontStyle.Bold;
        }

        fila.Add(celda);

        return celda;
    }

    // Actualizar los valores periodicamente
    private void Update()
    {
        if (Time.unscaledTime < siguienteActualizacion)
            return;

        siguienteActualizacion =
            Time.unscaledTime + 0.2f;

        ActualizarValores();
    }

    // Consultar los valores reales del PLC
    private void ActualizarValores()
    {
        for (int i = 0; i < tags.Count; i++)
        {
            bool activo = tags[i].Leer();

            valores[i].text =
                activo ? "TRUE" : "FALSE";

            valores[i].style.color = activo
                ? new Color(0.25f, 0.95f, 0.45f)
                : new Color(0.75f, 0.78f, 0.82f);
        }
    }
}
