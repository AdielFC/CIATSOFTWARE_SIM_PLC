using System;
using System.Collections.Generic;

namespace CIATSIM.SoftPLC
{
    /// <summary>
    /// Imagen de proceso: la COPIA interna de las variables con la que trabaja el programa.
    ///
    /// Un PLC real no mira los botones a mitad del programa. Al inicio del ciclo
    /// copia las entradas a esta imagen, el programa lee y escribe SOLO aquí, y al
    /// final del ciclo la imagen de salidas se copia a los cables reales.
    /// Así, durante un ciclo, todo el programa ve los mismos valores.
    /// </summary>
    public class ImagenProceso
    {
        private readonly Dictionary<string, bool> valores =
            new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        /// <summary>Se dispara si el programa usa una dirección que no existe (ej. "%Q9.9").</summary>
        public event Action<string> DireccionDesconocida;

        public void Registrar(string direccion)
        {
            if (!valores.ContainsKey(direccion)) valores.Add(direccion, false);
        }

        public bool Contiene(string direccion) => valores.ContainsKey(direccion);

        /// <summary>Lo usa el programa para leer %I, %Q o %M.</summary>
        public bool Leer(string direccion)
        {
            if (valores.TryGetValue(direccion, out bool valor)) return valor;
            DireccionDesconocida?.Invoke(direccion);
            return false;
        }

        /// <summary>Lo usa el programa para escribir %Q o %M.</summary>
        public void Escribir(string direccion, bool valor)
        {
            if (valores.ContainsKey(direccion)) valores[direccion] = valor;
            else DireccionDesconocida?.Invoke(direccion);
        }

        /// <summary>Pone todo en falso (al pasar a STOP).</summary>
        public void Limpiar()
        {
            var claves = new List<string>(valores.Keys);
            foreach (string c in claves) valores[c] = false;
        }
    }
}
