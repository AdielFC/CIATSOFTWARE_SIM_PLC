using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace CIATSIM.SoftPLC.Tests
{
    /// <summary>
    /// Pruebas del ciclo de scan y de marcha/paro.
    /// En Unity: Window > General > Test Runner > EditMode > Run All.
    /// </summary>
    public class CicloScanTests
    {
        /// <summary>Campo falso: hace de TagDatabase sin necesitar una escena.</summary>
        private class CampoFalso : ICampoPLC
        {
            public readonly Dictionary<string, bool> Tags =
                new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase)
                {
                    { "%I0.0", false }, { "%I0.1", false },
                    { "%Q0.0", false }, { "%Q0.1", false },
                    { "%M0.0", false }
                };

            public int Escrituras;

            public bool TryLeer(string direccion, out bool valor) => Tags.TryGetValue(direccion, out valor);

            public bool Escribir(string direccion, bool valor)
            {
                if (!Tags.ContainsKey(direccion)) return false;
                Tags[direccion] = valor;
                Escrituras++;
                return true;
            }
        }

        private CampoFalso campo;
        private CicloScan plc;

        [SetUp]
        public void Preparar()
        {
            campo = new CampoFalso();
            plc = new CicloScan(campo);
            plc.RegistrarEntrada("%I0.0");
            plc.RegistrarEntrada("%I0.1");
            plc.RegistrarSalida("%Q0.0");
            plc.RegistrarSalida("%Q0.1");
            plc.RegistrarMemoria("%M0.0");
            plc.Programa = ProgramaMarchaParo.Ejecutar;
            plc.CambiarModo(ModoPLC.RUN);
        }

        private void Pulsar(string dir, bool valor) => campo.Tags[dir] = valor;
        private bool Motor => campo.Tags["%Q0.0"];
        private bool Lampara => campo.Tags["%Q0.1"];

        // ---------------- Tabla de verdad ----------------

        [TestCase(false, false, false, ExpectedResult = false, TestName = "Reposo: sigue apagado")]
        [TestCase(true, false, false, ExpectedResult = true, TestName = "START enciende")]
        [TestCase(false, false, true, ExpectedResult = true, TestName = "Enclavamiento: sigue encendido")]
        [TestCase(false, true, true, ExpectedResult = false, TestName = "STOP apaga")]
        [TestCase(true, true, false, ExpectedResult = false, TestName = "START y STOP a la vez: gana STOP")]
        public bool TablaDeVerdad(bool start, bool stop, bool marchaAnterior)
        {
            return LogicaMarchaParo.Calcular(start, stop, marchaAnterior);
        }

        // ---------------- Secuencia completa ----------------

        [Test]
        public void Secuencia_Start_Soltar_Stop()
        {
            plc.EjecutarCiclo();
            Assert.IsFalse(Motor, "Al inicio la banda está detenida");

            Pulsar("%I0.0", true);
            plc.EjecutarCiclo();
            Assert.IsTrue(Motor, "START arranca la banda");
            Assert.IsTrue(Lampara, "La lámpara verde acompaña al motor");
            Assert.IsTrue(campo.Tags["%M0.0"], "La marca de marcha queda en 1");

            Pulsar("%I0.0", false);
            plc.EjecutarCiclo();
            plc.EjecutarCiclo();
            Assert.IsTrue(Motor, "El enclavamiento la mantiene encendida");

            Pulsar("%I0.1", true);
            plc.EjecutarCiclo();
            Assert.IsFalse(Motor, "STOP detiene la banda");
            Assert.IsFalse(Lampara);

            Pulsar("%I0.1", false);
            plc.EjecutarCiclo();
            Assert.IsFalse(Motor, "Sin START no vuelve a arrancar");
        }

        [Test]
        public void Las_salidas_solo_cambian_al_terminar_un_ciclo()
        {
            Pulsar("%I0.0", true);
            Assert.IsFalse(Motor, "Sin ciclo de scan el PLC aún no vio el botón");

            plc.EjecutarCiclo();
            Assert.IsTrue(Motor);
        }

        [Test]
        public void El_programa_trabaja_sobre_la_imagen_no_sobre_el_campo()
        {
            // Si alguien fuerza una salida en el campo, el siguiente scan la corrige:
            // manda lo que calculó el programa.
            campo.Tags["%Q0.0"] = true;
            plc.EjecutarCiclo();
            Assert.IsFalse(Motor);
        }

        [Test]
        public void En_STOP_se_apagan_las_salidas_y_no_corre_el_scan()
        {
            Pulsar("%I0.0", true);
            plc.EjecutarCiclo();
            Assert.IsTrue(Motor);

            plc.CambiarModo(ModoPLC.STOP);
            Assert.IsFalse(Motor, "Al pasar a STOP se apagan las salidas");

            long ciclos = plc.CiclosEjecutados;
            plc.EjecutarCiclo();
            Assert.AreEqual(ciclos, plc.CiclosEjecutados, "En STOP no corre el scan");
            Assert.IsFalse(Motor);
        }

        [Test]
        public void Al_volver_a_RUN_hay_que_pulsar_START_de_nuevo()
        {
            Pulsar("%I0.0", true);
            plc.EjecutarCiclo();
            Pulsar("%I0.0", false);
            plc.EjecutarCiclo();
            Assert.IsTrue(Motor);

            plc.CambiarModo(ModoPLC.STOP);
            plc.CambiarModo(ModoPLC.RUN);
            plc.EjecutarCiclo();
            Assert.IsFalse(Motor);
        }

        [Test]
        public void Avisa_si_el_programa_usa_un_tag_que_no_existe()
        {
            string avisado = null;
            plc.Imagen.DireccionDesconocida += d => avisado = d;
            plc.Programa = io => io.Escribir("%Q9.9", true);

            plc.EjecutarCiclo();
            Assert.AreEqual("%Q9.9", avisado);
        }
    }
}
