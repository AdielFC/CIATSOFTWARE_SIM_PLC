namespace CIATSIM.SoftPLC
{
    /// <summary>
    /// Marcha/paro con enclavamiento (auto-retención), con paro dominante.
    ///
    /// En Ladder:
    ///
    ///      START %I0.0      STOP %I0.1           MARCHA %M0.0
    ///   |----] [------+--------]/[--------------------( )----|
    ///   |             |
    ///   |  MARCHA     |
    ///   |----] [------+   (contacto de enclavamiento)
    ///      %M0.0
    ///
    /// En texto:  MARCHA = (START OR MARCHA) AND NOT STOP
    ///
    /// - START es un pulsador: vale 1 solo mientras se mantiene presionado.
    /// - Al soltarlo, la rama de abajo (el propio MARCHA) "sostiene" la bobina: eso es el enclavamiento.
    /// - STOP abre el contacto ]/[ y se pierde el enclavamiento.
    /// - START y STOP a la vez: gana STOP (seguridad).
    /// </summary>
    public static class LogicaMarchaParo
    {
        public static bool Calcular(bool start, bool stop, bool marchaAnterior)
        {
            return (start || marchaAnterior) && !stop;
        }
    }
}
