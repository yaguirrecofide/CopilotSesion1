# Parte práctica · Sesión 4 — Generación de pruebas y refactorización asistida

> **Duración:** 70 minutos · **Modalidad:** individual o en parejas · **Stack:** C# / .NET 10 · xUnit · NSubstitute · VS Code
> **Objetivo:** generar pruebas que sirvan de oráculo (regla de negocio primero), practicar TDD con Copilot, aislar dependencias con mocks, y refactorizar código heredado con una red de pruebas en verde.

| Lab | Tema | Tiempo |
|---|---|---|
| 0 | Entorno listo + NSubstitute | 5 min |
| 1 | Tests por regla de negocio (tabla de casos → `/tests`) | 10 min |
| 2 | TDD con Copilot: el test es el prompt | 15 min |
| 3 | Interfaz + mock con NSubstitute | 10 min |
| 4 | Caracterizar y refactorizar `LiquidadorSiniestros` | 15 min |
| 5 | Catálogo de refactors sobre `PolizaService` | 10 min |
| 6 | Revisión responsable: el refactor que cambió el comportamiento | 5 min |

---

## Requisitos previos

- Sesión 3 completada: solución con `CopilotSesion1` y `CopilotSesion1.Tests` (xUnit), `dotnet test` en verde.
- Archivos del dominio: `Poliza.cs`, `PolizaService.cs`, `ValidadorRuc.cs` (con `FormatearRuc`), `CalculadoraPrima.cs`, `ClienteRepository.cs`, `LiquidadorSiniestros.cs`.
- Si te falta alguno, pídelo en modo **Agent** antes de empezar (2 min): *"crea X según esta descripción…"* con las reglas que aparecen en los labs anteriores.

> 💡 Patrón de toda la sesión: **Copilot escribe el andamiaje; tú pones el oráculo** (el valor esperado) y el criterio (qué vale la pena probar). Las respuestas no serán idénticas a esta guía.

### Flujo seguro de refactor (úsalo en los labs 4, 5 y 6)

1. **Red en verde:** `dotnet test` pasa (tests de regla o de caracterización).
2. **Un refactor acotado:** el prompt termina en *"sin cambiar comportamiento / firmas / excepciones"*.
3. **Lee el diff:** operadores (`<` vs `<=`), redondeos, excepciones, orden de evaluación.
4. **Tests en verde:** si algo se pone rojo, **Undo**; no "arregles" el test.
5. **Commit pequeño:** un refactor, un commit.

### Prompts de referencia

| Tarea | Prompt base |
|---|---|
| Tests por regla | `#file:X.cs /tests con xUnit, Theory + InlineData, exactamente estos casos: […], patrón AAA, nombres Metodo_Escenario_Resultado, en español` |
| TDD | `#file:XTests.cs implementa X en Y.cs para que pasen estos tests; no modifiques los tests; no añadas casos especiales por valor` |
| Interfaz | `extrae una interfaz IX con los métodos públicos de X; X la implementa; no cambies la lógica` |
| Caracterización | `#file:X.cs genera tests de caracterización: ejecuta M con N combinaciones representativas y usa como esperado el valor que devuelve HOY; marca con [Trait("Tipo","Caracterizacion")]` |
| Refactor | `extrae/renombra/reemplaza … ; sin cambiar comportamiento ni firmas públicas; mantén las mismas excepciones` |

---

## Lab 0 · Entorno listo + NSubstitute (5 min)

1. En `CopilotSesion1`, rama nueva:
   ```bash
   git checkout -b sesion-4
   ```
2. Añade NSubstitute al proyecto de pruebas (desde la carpeta padre):
   ```bash
   dotnet add CopilotSesion1.Tests package NSubstitute
   dotnet add CopilotSesion1.Tests package FluentAssertions
   ```
   > Si tu equipo usa **Moq** en lugar de NSubstitute, instala `Moq` y adapta los prompts del Lab 3 ("con Moq").
3. Comprueba que **todo está en verde** antes de empezar:
   ```bash
   dotnet build
   dotnet test
   ```
   Si tienes rojos de la sesión 3 (el test del 5 % de `CalculadoraPrima`), corrígelos ahora: la red tiene que estar verde.
4. Abre el Chat (`Ctrl + Alt + I`) en modo **Ask**.

✅ **Checkpoint:** NSubstitute restaurado, `dotnet test` todo verde.

---

## Lab 1 · Tests por regla de negocio (10 min)

**Objetivo:** escribir la tabla de casos antes del prompt y dejar que Copilot genere solo el andamiaje.

### Parte A — `FormatearRuc`

1. **Sin abrir el chat**, escribe en `notas.md` la regla y la tabla de casos de `ValidadorRuc.FormatearRuc`:
   ```
   Regla: devuelve el RUC con guion antes del último dígito; recorta espacios; ArgumentException si no es válido.
   | Entrada          | Esperado          |
   | "20123456789"    | "2012345678-9"    |
   | "10987654321"    | "1098765432-1"    |
   | " 20123456789 "  | "2012345678-9"    |
   | "123"            | ArgumentException |
   | null             | ArgumentException |
   ```
2. Pide los tests con **exactamente** esos casos:
   ```
   #file:ValidadorRuc.cs /tests con xUnit en CopilotSesion1.Tests; Theory + InlineData con exactamente estos casos: [pega la tabla]; usa Assert.Throws<ArgumentException> para los inválidos; patrón Arrange-Act-Assert; nombres Metodo_Escenario_Resultado en español
   ```
3. Revisa cada `InlineData` contra tu tabla. Guarda como `ValidadorRucFormatoTests.cs` y ejecuta `dotnet test`.

### Parte B — `FiltrarVigentes`

4. Escribe la tabla para `PolizaService.FiltrarVigentes(polizas, fecha)`. Decide **tú** el caso ambiguo: ¿la fecha de fin de vigencia es inclusive? (Elige y anótalo; es una decisión de negocio.)
   ```
   | Póliza (inicio–fin, activa)       | Fecha      | ¿Incluida? |
   | 2026-01-01–2026-12-31, true       | 2026-06-15 | sí         |
   | 2026-01-01–2026-12-31, false      | 2026-06-15 | no         |
   | 2026-01-01–2026-06-15, true       | 2026-06-15 | sí (inclusive) ← tu decisión |
   | 2026-07-01–2026-12-31, true       | 2026-06-15 | no         |
   | lista vacía                        | 2026-06-15 | lista vacía |
   ```
5. Pide los tests con la tabla. Si el test del caso inclusive falla, **no cambies el test**: la regla la decidiste tú. Corrige el método (o déjalo rojo para el Lab 5).

✅ **Checkpoint:** dos archivos de tests nuevos cuyos valores esperados salen de **tus** tablas, no del código.

🧠 **La tabla de casos es el verdadero trabajo.** El código del test es lo que Copilot hace bien.

---

## Lab 2 · TDD con Copilot (15 min)

**Objetivo:** escribir el test primero y usarlo como prompt para la implementación.

**Regla nueva:** *recargo por siniestralidad* en `CalculadoraPrima`:
- `PrimaAnualConRecargo(Poliza p, int siniestrosPrevios)` = `PrimaAnual(p)` × factor.
- 0 siniestros → factor 1.00 · 1–2 siniestros → 1.10 · 3 o más → 1.25.
- `siniestrosPrevios < 0` → `ArgumentOutOfRangeException`.
- Redondeo a 2 decimales.

1. **Rojo.** Crea `CalculadoraPrimaRecargoTests.cs` en el proyecto de tests y escribe tú (o con completions, pero revisando) los tests a partir de la regla. Mínimo:
   ```csharp
   [Theory]
   [InlineData("VIDA", 100, 0, 1200.00)]
   [InlineData("VIDA", 100, 1, 1320.00)]
   [InlineData("VIDA", 100, 2, 1320.00)]
   [InlineData("VIDA", 100, 3, 1500.00)]
   [InlineData("SALUD", 100, 3, 1425.00)]   // 1140 × 1.25
   public void PrimaAnualConRecargo_SegunSiniestros_AplicaFactor(string producto, decimal mensual, int siniestros, decimal esperado)
   { /* Arrange: Poliza con esos datos · Act · Assert */ }

   [Fact]
   public void PrimaAnualConRecargo_SiniestrosNegativos_LanzaExcepcion() { /* ... */ }
   ```
   `dotnet test` → **falla al compilar**: el método no existe. Eso es el rojo.
2. **Verde.** Pide la implementación adjuntando los tests:
   ```
   #file:CalculadoraPrimaRecargoTests.cs #file:CalculadoraPrima.cs Implementa PrimaAnualConRecargo en CalculadoraPrima para que pasen estos tests. No modifiques los tests. No añadas casos especiales por valor (nada de if siniestros == 3). Usa una expresión switch para el factor.
   ```
   `dotnet test` → verde.
3. **Comprueba que resolvió la regla y no los casos.** Añade un test **nuevo** que no estaba en la tabla, por ejemplo `("VIDA", 100, 7, 1500.00)` y `("VIDA", 80, 2, 1056.00)`. Si falla, Copilot hardcodeó: vuelve al paso 2 con la restricción más explícita.
4. **Refactor.** Con la red en verde, pide en Inline Chat sobre el método:
   ```
   extrae el cálculo del factor a un método privado FactorPorSiniestralidad(int siniestros); sin cambiar comportamiento ni la firma pública
   ```
   `dotnet test` → sigue verde.

✅ **Checkpoint:** 7+ tests en verde, incluido uno que no estaba en la tabla original; el factor vive en un método con nombre.

🧠 **El test es el prompt:** entrada, salida y caso borde sin ambigüedad. *"Sin modificar los tests"* cierra el atajo.

---

## Lab 3 · Interfaz + mock con NSubstitute (10 min)

**Objetivo:** aislar una dependencia externa para probar lógica sin base de datos.

1. **Extrae la interfaz.** Abre `ClienteRepository.cs`, selecciona la clase y en Inline Chat:
   ```
   extrae una interfaz IClienteRepository con los métodos públicos; ClienteRepository la implementa; no cambies la lógica ni las firmas
   ```
   Acepta y `dotnet build`.
2. **Crea el servicio a probar.** En modo Ask:
   ```
   #file:IClienteRepository.cs Crea ClienteService con constructor que reciba IClienteRepository y un método string Saludar(string documento) que devuelva "Hola, {nombre}" si el repositorio encuentra el cliente, o "Cliente no encontrado" si devuelve null. Solo la clase.
   ```
   Insértalo como `ClienteService.cs`.
3. **Pide el test con mock:**
   ```
   #file:ClienteService.cs /tests con xUnit y NSubstitute en CopilotSesion1.Tests: caso cliente existe (el mock devuelve "Ana" → "Hola, Ana"), caso no existe (null → "Cliente no encontrado"); verifica con Received(1) que se llamó al repositorio con el documento correcto
   ```
4. Revisa: ¿`Substitute.For<IClienteRepository>()`? ¿`Returns` con el documento exacto? ¿`Received(1)`? Guarda como `ClienteServiceTests.cs` y `dotnet test`.
5. Pregunta de criterio (anota en `notas.md`): si `ClienteService` también usara `ValidadorRuc.EsValido`, ¿lo mockearías? (Pista: no cruza una frontera; es tu propia lógica.)

✅ **Checkpoint:** `ClienteServiceTests` en verde sin SQL Server, y una respuesta razonada sobre qué no mockear.

🧠 **Regla:** mockea lo que cruza una frontera (BD, HTTP, reloj); no mockees tu propia lógica.

---

## Lab 4 · Caracterizar y refactorizar `LiquidadorSiniestros` (15 min)

**Objetivo:** congelar el comportamiento actual de código heredado y refactorizarlo sin cambiarlo.

Usa la versión **original** con nombres crípticos (`Calc(decimal m, decimal d, decimal c, int t)`). Si ya la renombraste en la sesión 2, recupera la original desde la guía de la sesión 2 en un archivo `LiquidadorSiniestrosLegacy.cs`.

### Parte A — Tests de caracterización

1. Pide los tests que **congelan** lo que hace hoy:
   ```
   #file:LiquidadorSiniestros.cs genera tests de caracterización con xUnit: ejecuta Calc con 8 combinaciones representativas (deducible mayor y menor que el monto, coaseguro 0, 0.2 y 1, t = 0, 2, 3 y 10) y usa como valor esperado EXACTAMENTE lo que devuelve hoy el método. Marca cada test con [Trait("Tipo","Caracterizacion")]. No corrijas nada.
   ```
2. **Verifica los valores a mano o ejecutando:** Copilot puede calcular mal el "valor actual". Para cada caso, ejecuta `Calc` en un `Program.cs` temporal (o confía en `dotnet test`: si un test de caracterización falla recién creado, el esperado está mal calculado, no el código). Ajusta hasta que los 8 pasen.
3. Comenta en el archivo: `// Estos tests congelan el comportamiento actual, bugs incluidos. No son la regla de negocio.`

### Parte B — Refactor con la red en verde

4. Refactor 1 (Inline Chat sobre `Calc`):
   ```
   renombra los parámetros: m → montoReclamado, d → deducible, c → porcentajeCoaseguro, t → siniestrosPrevios; renombra r → montoLiquidado; sin cambiar comportamiento
   ```
   `dotnet test` → verde.
5. Refactor 2:
   ```
   reemplaza los números mágicos 2 y 0.9m por constantes privadas con nombre de dominio (umbral de siniestros y factor de descuento); sin cambiar valores ni comportamiento
   ```
   `dotnet test` → verde.
6. Refactor 3:
   ```
   extrae el cálculo del descuento por siniestralidad a un método privado AplicarDescuentoPorSiniestralidad; mantén la firma pública de Calc; sin cambiar comportamiento
   ```
   `dotnet test` → verde.
7. **Ahora sí, la regla de negocio.** Con el código legible, escribe en `notas.md` qué hace realmente (`montoLiquidado = max(0, monto − deducible) × (1 − coaseguro)`, 10 % menos si hay más de 2 siniestros previos) y pregúntale a Copilot:
   ```
   #selection ¿Qué pasa si porcentajeCoaseguro es mayor que 1 o negativo? ¿Y si deducible es negativo? Lista los casos no controlados; no cambies el código.
   ```
   Esos casos van a un test de **regla de negocio** (sin el Trait de caracterización), y la corrección es un cambio funcional, no un refactor: se hace aparte, con su propio commit.

✅ **Checkpoint:** 8 tests de caracterización en verde antes y después de tres refactors; `Calc` legible; lista de casos no controlados para corregir en un cambio separado.

🧠 **Caracterizar ≠ validar:** aquí el esperado SÍ es el valor actual, porque el objetivo es congelar, no juzgar. Por eso se marcan con un Trait.

---

## Lab 5 · Catálogo de refactors sobre `PolizaService` (10 min)

**Objetivo:** aplicar tres refactors del catálogo con prompts acotados.

Red: los tests de `FiltrarVigentes` del Lab 1 (y los que tengas) en verde.

1. Pide el diagnóstico **sin cambios**:
   ```
   #file:PolizaService.cs lista los code smells con línea y refactor sugerido (método largo, números mágicos, condicionales anidados, duplicación, nombres); no cambies nada todavía
   ```
2. **Guard clauses** (Inline Chat sobre `FiltrarVigentes`):
   ```
   convierte las validaciones de argumentos en guard clauses con retorno temprano / ArgumentNullException; mantén las mismas excepciones y mensajes; sin cambiar el resultado
   ```
   `dotnet test`.
3. **Constantes con nombre** (si tienes el método "vencen en los próximos 30 días" de la sesión 2):
   ```
   reemplaza el literal 30 por una constante DiasAvisoVencimiento; sin cambiar valores ni comportamiento
   ```
   `dotnet test`.
4. **Bucle ↔ LINQ** sobre el método que prefieras:
   ```
   reescribe este método con LINQ (o con foreach si ya es LINQ) manteniendo el orden de los resultados y el mismo comportamiento con lista vacía y null
   ```
   `dotnet test`. Compara legibilidad: **quédate con la versión más clara**, no con la más corta.
5. Un commit por refactor:
   ```bash
   git add -A && git commit -m "refactor(PolizaService): guard clauses en FiltrarVigentes"
   ```

✅ **Checkpoint:** tres refactors, tres commits, tests en verde en cada paso.

---

## Lab 6 · Revisión responsable: el refactor que cambió el comportamiento (5 min)

**Objetivo:** detectar un cambio de comportamiento disfrazado de refactor.

1. Sobre `LiquidadorSiniestros` (ya refactorizado, tests de caracterización en verde), pide en Inline Chat **a propósito** un prompt vago:
   ```
   simplifica y mejora este método
   ```
2. **Antes de aceptar**, lee el diff buscando: ¿`siniestrosPrevios > UmbralSiniestros` se convirtió en `>=`? ¿`Math.Round` cambió de sitio o de modo (`MidpointRounding`)? ¿Se eliminó el `if (r < 0) r = 0`? ¿Cambió el orden entre deducible y coaseguro?
3. Acepta y ejecuta `dotnet test`. Si algún test de caracterización se puso **rojo**, el "refactor" cambió comportamiento. **Undo**. No pidas `/fix` sobre el test.
4. Reescribe el prompt para que sea un refactor de verdad:
   ```
   mejora la legibilidad de este método sin cambiar comportamiento: nombres, guard clauses y extracción de métodos; no toques operadores de comparación, redondeos ni el orden de los cálculos
   ```
5. En `notas.md`: ¿qué cambió en el primer intento? ¿Lo habrías visto sin los tests de caracterización?

✅ **Checkpoint:** identificaste (o descartaste con evidencia) un cambio de comportamiento, y el método quedó refactorizado con la red en verde.

🧠 **Un test rojo tras un refactor es información.** La herramienta hace el cambio; la evidencia de que es correcto la produces tú.

---

## Checklist final

- [ ] Escribo la regla y la tabla de casos **antes** de pedir tests; el valor esperado sale de la regla.
- [ ] Hago TDD con Copilot: test primero, "sin modificar los tests", y añado un caso nuevo para detectar hardcodeo.
- [ ] Extraigo interfaces y uso mocks solo para lo que cruza una frontera.
- [ ] Caracterizo código heredado antes de tocarlo y distingo esos tests de los de regla de negocio.
- [ ] Refactorizo con el flujo seguro: verde antes, un cambio acotado, diff leído, verde después, commit pequeño.
- [ ] Nunca genero los tests después del refactor que los justifica, ni arreglo un test para que un refactor pase.

---

## Troubleshooting

| Síntoma | Causa probable | Qué hacer |
|---|---|---|
| `Substitute` no se reconoce | Paquete no restaurado o falta `using NSubstitute;` | `dotnet restore`; añade el `using` |
| Test de caracterización recién creado falla | Copilot calculó mal el valor "actual" | Ejecuta `Calc` con esos valores y corrige el esperado; el código no se toca |
| Tras un refactor varios tests en rojo | Cambió comportamiento o una firma | **Undo**; repite con "sin cambiar firmas/operadores/redondeos" |
| Copilot hardcodea los casos en TDD | Prompt sin restricción | Añade "no añadas casos especiales por valor; usa una expresión switch/fórmula" y un test nuevo |
| Agent toca `Program.cs` u otros archivos | Petición amplia | Undo esos archivos; "no toques Program.cs" en el prompt |
| `dotnet test` no ve los tests nuevos | Archivo fuera del proyecto de tests | Mueve el `.cs` a `CopilotSesion1.Tests` |
| Otro problema | — | *View → Output* → canal **GitHub Copilot Chat** |

---

## Para profundizar

- GitHub Docs · [Writing tests with GitHub Copilot](https://docs.github.com/en/copilot/tutorials/copilot-chat-cookbook/testing-code)
- GitHub Docs · [Refactoring code with GitHub Copilot](https://docs.github.com/en/copilot/tutorials/copilot-chat-cookbook/refactoring-code)
- xUnit · [Theory e InlineData](https://xunit.net/docs/getting-started/v3/getting-started)
- NSubstitute · [Getting started](https://nsubstitute.github.io/help/getting-started/)
- coverlet · [Cobertura en .NET](https://github.com/coverlet-coverage/coverlet)
- Michael Feathers · *Working Effectively with Legacy Code* (tests de caracterización)

**Próxima sesión:** code review, documentación y commits con Copilot.
