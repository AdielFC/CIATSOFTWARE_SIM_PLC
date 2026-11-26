# SoftPLC: ciclo de scan y marcha/paro con enclavamiento

## 1. Qué cambió en una frase

`SoftPlcBasico` ya tenía bien la lógica de marcha/paro, pero la ejecutaba **directo sobre los tags**
en cada `Update`. Ahora corre un **ciclo de scan** como un PLC real, cada 10 ms.

```
 ┌──────────────────────────────────────────────────────────────────┐
 │ 1. LEER ENTRADAS    TagDatabase %I  →  imagen de proceso         │
 │ 2. EJECUTAR LÓGICA  el programa lee/escribe SOLO la imagen       │
 │ 3. ESCRIBIR SALIDAS imagen %Q y %M  →  TagDatabase               │
 └───────────────────── se repite cada 10 ms ───────────────────────┘
```

**¿Por qué importa?** Durante un ciclo, todo el programa ve **los mismos valores** de entrada,
aunque el usuario suelte el botón a la mitad. Además las salidas cambian todas juntas al final.
Cuando el programa Ladder tenga muchas líneas, esto evita resultados raros. Por eso así trabajan
los PLC reales (Siemens, Allen-Bradley, etc.).

## 2. Marcha/paro con enclavamiento

```
     START %I0.0     STOP %I0.1            MARCHA %M0.0
  |----] [-----+--------]/[---------------------( )----|
  |            |
  |  MARCHA    |
  |----] [-----+     ← contacto de enclavamiento
     %M0.0

  |----] [--------------------------------------( )----|   MOTOR_BANDA %Q0.0
     %M0.0
  |----] [--------------------------------------( )----|   LAMP_VERDE  %Q0.1
     %M0.0
```

`MARCHA = (START OR MARCHA) AND NOT STOP`

| START | STOP | MARCHA antes | MARCHA después | Qué pasa |
|:-----:|:----:|:------------:|:--------------:|----------|
| 0 | 0 | 0 | 0 | Reposo |
| 1 | 0 | 0 | **1** | Arranca la banda |
| 0 | 0 | 1 | **1** | Suelto START y sigue: **enclavamiento** |
| 0 | 1 | 1 | 0 | Paro |
| 1 | 1 | x | 0 | Los dos a la vez: **gana STOP** (seguridad) |

## 3. Archivos

| Archivo | Cambio |
|---------|--------|
| `SoftPLC/SoftPlcBasico.cs` | **Modificado.** Mismo componente y mismo campo `tagDatabase`, así que las escenas P01/P02 siguen funcionando sin reconectar nada. Ahora arma el ciclo de scan y lo ejecuta con tiempo fijo. Nuevos campos en el Inspector: `Modo Inicial`, `Tiempo Scan Ms`, y diagnóstico (`Modo Actual`, `Ciclos Ejecutados`). Clic derecho en el componente da **PLC en RUN / PLC en STOP**. |
| `Tags/TagDatabase.cs` | **2 líneas.** Ahora implementa `ICampoPLC`, porque ya tenía exactamente `TryLeer` y `Escribir`. |
| `SoftPLC/Core/ICampoPLC.cs` | Nuevo. "Los cables" del PLC: dónde se leen las entradas y se escriben las salidas. |
| `SoftPLC/Core/ImagenProceso.cs` | Nuevo. La copia interna de %I/%Q/%M con la que trabaja el programa. |
| `SoftPLC/Core/CicloScan.cs` | Nuevo. Las 3 fases del scan y el selector RUN/STOP. |
| `SoftPLC/Core/LogicaMarchaParo.cs` | Nuevo. La fórmula del enclavamiento (una línea). |
| `SoftPLC/Core/ProgramaMarchaParo.cs` | Nuevo. El programa de la práctica: %I0.0, %I0.1, %M0.0 → %Q0.0, %Q0.1. |
| `SoftPLC/Core/CIATSIM.SoftPLC.Core.asmdef` | Nuevo. Hace que el núcleo sea C# puro (sin Unity) para poder probarlo. |
| `Tests/EditMode/CicloScanTests.cs` | Nuevo. Pruebas automáticas. |

**No cambian:** `BotonPLC3D`, `LamparaPLC`, `PanelTagsUI`, `PlcTag` ni las escenas.

### Regla de oro

- **Botones** → escriben sus `%I` en TagDatabase (como antes).
- **Programa** → solo lee/escribe la `ImagenProceso`. Nunca toca objetos 3D ni TagDatabase.
- **Lámparas/motor** → leen sus `%Q` de TagDatabase (como antes).

Cuando exista el IDE Ladder, solo se cambia `ciclo.Programa = ...` en `SoftPlcBasico` por el intérprete
del diagrama. El resto no se toca.

## 4. Cómo probarlo

1. Abre `P01_PanelIndustrial` o `P02_AdministradorTags` y dale **Play**.
2. Clic en START: la lámpara se enciende. Suelta y sigue encendida.
3. Clic en STOP: se apaga.
4. En P02, abre el panel de Tags y verás `%M0.0`, `%Q0.0` y `%Q0.1` cambiar juntos.
5. Selecciona `SistemaCIATSIM`: en `SoftPlcBasico` verás subir `Ciclos Ejecutados` (unos 100 por segundo).
6. Prueba el selector: clic derecho en el componente y luego **PLC en STOP**. Todo se apaga y los botones ya no hacen nada.
   **PLC en RUN** lo reactiva, y hay que volver a pulsar START.

**Pruebas automáticas:** *Window > General > Test Runner > EditMode > Run All*.

## 5. Notas

- Un clic más corto que un ciclo (10 ms) podría perderse, igual que en un PLC real. Con el mouse no pasa en la práctica.
- En la industria el STOP se cablea **normalmente cerrado** (NC) por seguridad. Se puede agregar después.
- `LamparaPLC` en la escena apunta a `%Q0.1` (LAMP_VERDE). `%Q0.0` (MOTOR_BANDA) quedará listo para la banda 3D de Magi.
