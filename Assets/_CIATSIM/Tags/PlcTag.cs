
using System;
using UnityEngine;

// Tipos de variables que tendrá nuestro PLC
public enum TipoTagPLC
{
    Entrada,
    Salida,
    Memoria
}

// Representación de una variable del PLC
[Serializable]
public class PlcTag
{
    [SerializeField] private string nombre;
    [SerializeField] private string direccion;
    [SerializeField] private TipoTagPLC tipo;
    [SerializeField] private bool valor;

    // Propiedades de consulta
    public string Nombre => nombre;
    public string Direccion => direccion;
    public TipoTagPLC Tipo => tipo;
    public bool Valor => valor;

    // Constructor
    public PlcTag(
        string nombre,
        string direccion,
        TipoTagPLC tipo)
    {
        this.nombre = nombre;
        this.direccion = direccion;
        this.tipo = tipo;
        this.valor = false;
    }

    // Escribir un valor digital
    public void Escribir(bool nuevoValor)
    {
        valor = nuevoValor;
    }

    // Leer el valor digital
    public bool Leer()
    {
        return valor;
    }
}
