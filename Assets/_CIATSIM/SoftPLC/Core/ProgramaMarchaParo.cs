namespace CIATSIM.SoftPLC
{
    /// <summary>
    /// Programa de usuario de la práctica: banda con marcha/paro.
    /// Es la misma lógica que tenía SoftPlcBasico, ahora ejecutada dentro del ciclo de scan.
    /// Más adelante este programa lo generará el IDE Ladder.
    /// </summary>
    public static class ProgramaMarchaParo
    {
        public const string Start = "%I0.0";       // BTN_START
        public const string Stop = "%I0.1";        // BTN_STOP
        public const string Marcha = "%M0.0";      // MEM_MARCHA (enclavamiento)
        public const string MotorBanda = "%Q0.0";  // MOTOR_BANDA
        public const string LamparaVerde = "%Q0.1"; // LAMP_VERDE

        /// <summary>Fase 2 del scan: solo lee y escribe la imagen de proceso.</summary>
        public static void Ejecutar(ImagenProceso io)
        {
            bool marcha = LogicaMarchaParo.Calcular(
                io.Leer(Start),
                io.Leer(Stop),
                io.Leer(Marcha));

            io.Escribir(Marcha, marcha);
            io.Escribir(MotorBanda, marcha);
            io.Escribir(LamparaVerde, marcha);
        }
    }
}
