using System;
using System.Collections.Generic;

namespace CIATSIM.SoftPLC
{
    /// <summary>Selector RUN/STOP, como en un PLC real.</summary>
    public enum ModoPLC
    {
        STOP = 0,
        RUN = 1
    }

    /// <summary>
    /// El corazón del PLC virtual: el ciclo de scan.
    ///
    ///   1. LEER ENTRADAS    campo %I  -> imagen
    ///   2. EJECUTAR LÓGICA  el programa lee/escribe solo la imagen
    ///   3. ESCRIBIR SALIDAS imagen %Q y %M -> campo
    ///
    /// C# puro (sin Unity) para poder probarlo con pruebas unitarias.
    /// SoftPlcBasico lo usa en la escena con TagDatabase como campo.
    /// </summary>
    public class CicloScan
    {
        private readonly ICampoPLC campo;
        private readonly List<string> entradas = new List<string>();
        private readonly List<string> salidas = new List<string>();
        private readonly List<string> memorias = new List<string>();

        public ImagenProceso Imagen { get; } = new ImagenProceso();

        /// <summary>El programa de usuario (hoy marcha/paro; después, el intérprete Ladder).</summary>
        public Action<ImagenProceso> Programa { get; set; }

        public ModoPLC Modo { get; private set; } = ModoPLC.STOP;

        public long CiclosEjecutados { get; private set; }

        public CicloScan(ICampoPLC campo)
        {
            this.campo = campo ?? throw new ArgumentNullException(nameof(campo));
        }

        public void RegistrarEntrada(string direccion) => Registrar(entradas, direccion);
        public void RegistrarSalida(string direccion) => Registrar(salidas, direccion);
        public void RegistrarMemoria(string direccion) => Registrar(memorias, direccion);

        private void Registrar(List<string> lista, string direccion)
        {
            if (string.IsNullOrWhiteSpace(direccion) || Imagen.Contiene(direccion)) return;
            lista.Add(direccion);
            Imagen.Registrar(direccion);
        }

        /// <summary>
        /// Cambia entre RUN y STOP. En cualquier cambio la imagen vuelve a cero y
        /// las salidas se apagan: así el enclavamiento no "revive" solo al volver a RUN.
        /// </summary>
        public void CambiarModo(ModoPLC nuevoModo)
        {
            if (Modo == nuevoModo) return;
            Modo = nuevoModo;
            Imagen.Limpiar();
            EscribirSalidas();
        }

        /// <summary>Ejecuta UN ciclo completo. En STOP no hace nada.</summary>
        public void EjecutarCiclo()
        {
            if (Modo != ModoPLC.RUN) return;

            LeerEntradas();     // Fase 1
            EjecutarLogica();   // Fase 2
            EscribirSalidas();  // Fase 3

            CiclosEjecutados++;
        }

        // ---------------- Fase 1 ----------------
        private void LeerEntradas()
        {
            foreach (string dir in entradas)
            {
                campo.TryLeer(dir, out bool valor);
                Imagen.Escribir(dir, valor);
            }
        }

        // ---------------- Fase 2 ----------------
        private void EjecutarLogica()
        {
            Programa?.Invoke(Imagen);
        }

        // ---------------- Fase 3 ----------------
        private void EscribirSalidas()
        {
            foreach (string dir in salidas) campo.Escribir(dir, Imagen.Leer(dir));

            // Las marcas %M son internas, pero las publicamos para verlas en el panel de Tags.
            foreach (string dir in memorias) campo.Escribir(dir, Imagen.Leer(dir));
        }
    }
}
