namespace CIATSIM.SoftPLC
{
    /// <summary>
    /// "El campo": los cables reales del PLC (botones, lámparas, motores).
    ///
    /// En la escena lo implementa <c>TagDatabase</c>: los botones 3D escriben ahí
    /// sus entradas (%I) y las lámparas leen ahí sus salidas (%Q).
    /// El ciclo de scan solo habla con el campo a través de esta interfaz,
    /// por eso se puede probar sin Unity usando un campo falso.
    /// </summary>
    public interface ICampoPLC
    {
        bool TryLeer(string direccion, out bool valor);
        bool Escribir(string direccion, bool valor);
    }
}
