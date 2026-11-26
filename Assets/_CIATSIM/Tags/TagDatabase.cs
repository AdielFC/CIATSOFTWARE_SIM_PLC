
using System;
using System.Collections.Generic;
using UnityEngine;

public class TagDatabase : MonoBehaviour
{
    [SerializeField]
    private List<PlcTag> tags = new List<PlcTag>();

    private Dictionary<string, PlcTag> indice;

    void Awake()
    {
        Inicializar();
    }

    private void Inicializar()
    {
        if (tags.Count == 0)
        {
            tags.Add(new PlcTag(
                "BTN_START", "%I0.0", TipoTagPLC.Entrada));

            tags.Add(new PlcTag(
                "BTN_STOP", "%I0.1", TipoTagPLC.Entrada));

            tags.Add(new PlcTag(
                "MOTOR_BANDA", "%Q0.0", TipoTagPLC.Salida));

            tags.Add(new PlcTag(
                "LAMP_VERDE", "%Q0.1", TipoTagPLC.Salida));

            tags.Add(new PlcTag(
                "MEM_MARCHA", "%M0.0", TipoTagPLC.Memoria));
        }

        indice = new Dictionary<string, PlcTag>(
            StringComparer.OrdinalIgnoreCase);

        foreach (PlcTag tag in tags)
        {
            if (string.IsNullOrWhiteSpace(tag.Direccion))
            {
                Debug.LogError("Tag sin direccion.");
                continue;
            }

            if (indice.ContainsKey(tag.Direccion))
            {
                Debug.LogError(
                    "Direccion duplicada: " + tag.Direccion);
                continue;
            }

            indice.Add(tag.Direccion, tag);
        }

        Debug.Log("CIATSIM: " + indice.Count +
                 " variables PLC registradas.");
    }

    public bool Escribir(string direccion, bool valor)
    {
        if (indice == null)
            Inicializar();

        if (indice.TryGetValue(direccion, out PlcTag tag))
        {
            tag.Escribir(valor);
            return true;
        }

        Debug.LogWarning("Tag no encontrado: " + direccion);
        return false;
    }

    public bool TryLeer(string direccion, out bool valor)
    {
        if (indice == null)
            Inicializar();

        if (indice.TryGetValue(direccion, out PlcTag tag))
        {
            valor = tag.Leer();
            return true;
        }

        valor = false;
        return false;
    }

    [ContextMenu("Probar sistema de Tags")]
    public void ProbarTags()
    {
        Escribir("%I0.0", true);

        if (TryLeer("%I0.0", out bool valor))
        {
            Debug.Log("CIATSIM: %I0.0 = " + valor);
        }
    }
    public IReadOnlyList<PlcTag> ObtenerTags()
    {
        return tags.AsReadOnly();
    }
}


