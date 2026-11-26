
using UnityEngine;

public class SoftPlcBasico : MonoBehaviour
{
    [SerializeField]
    private TagDatabase tagDatabase;

    private void Awake()
    {
        if (tagDatabase == null)
            tagDatabase = GetComponent<TagDatabase>();

        if (tagDatabase == null)
            Debug.LogError("CIATSIM: Falta TagDatabase.");
    }

    private void Update()
    {
        if (tagDatabase == null)
            return;
       
        // Leer las entradas del PLC
        if (!tagDatabase.TryLeer("%I0.0", out bool start))
            return;

        if (!tagDatabase.TryLeer("%I0.1", out bool stop))
            return;

        // Leer la memoria interna de marcha
        if (!tagDatabase.TryLeer("%M0.0", out bool marcha))
            return;

        // Enclavamiento con prioridad de paro
        bool nuevaMarcha = !stop && (start || marcha);

        // Actualizar memoria del PLC
        tagDatabase.Escribir("%M0.0", nuevaMarcha);

        // Actualizar motor de banda
        tagDatabase.Escribir("%Q0.0", nuevaMarcha);

        // Actualizar lampara verde
        tagDatabase.Escribir("%Q0.1", nuevaMarcha);
    }
}
