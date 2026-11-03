# Parte práctica · Sesión 3 — Prompting efectivo y comandos slash

> **Duración:** 70 minutos · **Modalidad:** individual o en parejas · **Stack:** C# / .NET 10 en Visual Studio Code
> **Objetivo:** escribir prompts específicos y verificables (objetivo, contexto, restricciones, formato), usar ejemplos y roles, y aplicar los comandos `/explain`, `/doc`, `/tests` y `/fix` revisando cada resultado con criterio.

| Lab | Tema | Tiempo |
|---|---|---|
| 0 | Entorno listo + proyecto de pruebas xUnit | 5 min |
| 1 | De vago a efectivo: reescribir tres prompts | 10 min |
| 2 | Few-shot: enseñar con ejemplos | 10 min |
| 3 | Rol y formato de salida | 5 min |
| 4 | `/explain` y `/doc` | 10 min |
| 5 | `/tests` con xUnit (y la trampa del test que documenta un bug) | 15 min |
| 6 | `/fix`: causa o síntoma | 10 min |
| 7 | Revisión responsable: prompts que piden lo incorrecto | 5 min |

---

## Requisitos previos

- Sesiones 1 y 2 completadas: proyecto `CopilotSesion1` con `Calculadora.cs`, `ValidadorRuc.cs`, `Textos.cs`, `Poliza.cs`, `PolizaService.cs`, `ClienteRepository.cs`, `CalculadoraPrima.cs` y `LiquidadorSiniestros.cs`.
- .NET 10 SDK (`dotnet --version` → `10.x`) y VS Code con *GitHub Copilot* y *C# Dev Kit*.
- Si te falta `CalculadoraPrima.cs` (Lab 2 de la sesión 2), créala en modo **Agent** en 2 minutos: *"crea CalculadoraPrima con un método PrimaAnual(Poliza p) = prima mensual × 12 con 5 % de descuento si Producto es SALUD, sin distinguir mayúsculas; lanza ArgumentException si la prima es negativa"*.

> 💡 Las respuestas **no serán idénticas** a las de esta guía. Lo que importa es comparar qué cambia cuando cambia el prompt.

### Estructura de un buen prompt (úsala en todos los labs)

| Parte | Pregunta que responde | Ejemplo |
|---|---|---|
| **Objetivo** | ¿Qué quiero que haga? (un verbo) | *Genera un método…* |
| **Contexto** | ¿Qué debe mirar? | `#file:Poliza.cs`, `#selection` |
| **Restricciones** | ¿Con qué reglas? ¿Qué no tocar? | *C# 13, LINQ, sin cambiar la firma pública, N > 0* |
| **Formato** | ¿Cómo quiero la respuesta? | *Solo el método, en español, sin explicación* |

**Las 4 S:** Single (una tarea) · Specific (sin ambigüedad) · Short (conciso) · Surround (rodeado de contexto).

### Comandos slash de hoy

| Comando | Dónde | Para qué |
|---|---|---|
| `/explain` | Inline Chat o Chat con `#selection` | Explicar código |
| `/doc` | Inline Chat sobre un método o clase | Comentarios XML |
| `/tests` | Chat con `#selection` o `#file` | Generar pruebas unitarias |
| `/fix` | Inline Chat sobre el código con error | Proponer corrección |
| `/clear` · `/help` | Vista de Chat | Nueva conversación · ayuda |

> Escribe `/` en el chat para ver la lista de tu versión. Puedes añadir texto a un comando para afinarlo: `/tests con xUnit, usa Theory e InlineData`.

---

## Lab 0 · Entorno listo + proyecto de pruebas (5 min)

1. Abre la **carpeta padre** que contiene `CopilotSesion1` (vamos a tener dos proyectos hermanos). Si usas Git, dentro de `CopilotSesion1`:
   ```bash
   git checkout -b sesion-3
   ```
2. Desde la carpeta padre, crea el proyecto de pruebas y la referencia:
   ```bash
   dotnet new xunit -n CopilotSesion1.Tests
   dotnet add CopilotSesion1.Tests reference CopilotSesion1
   dotnet new sln -n CopilotSesion1
   dotnet sln add CopilotSesion1 CopilotSesion1.Tests
   ```
3. Abre la carpeta padre en VS Code (`code .`) y acepta cuando C# Dev Kit proponga cargar la solución.
4. Elimina el `UnitTest1.cs` de ejemplo y comprueba:
   ```bash
   dotnet build
   dotnet test
   ```
   Debe compilar y reportar `0` pruebas (todavía).
5. Abre el Chat (`Ctrl + Alt + I`) en modo **Ask**.

✅ **Checkpoint:** solución con dos proyectos, `dotnet test` corre sin errores, chat abierto.

> ℹ️ Si `dotnet add reference` falla, revisa que el nombre de la carpeta coincida con el `.csproj` (`CopilotSesion1/CopilotSesion1.csproj`). Si prefieres evitar la solución, basta con la referencia; `dotnet test CopilotSesion1.Tests` funciona igual.

---

## Lab 1 · De vago a efectivo (10 min)

**Objetivo:** convertir prompts vagos en especificaciones pequeñas.

Para cada prompt: **envíalo tal cual** en una conversación nueva (`Ctrl + L`), anota qué decidió Copilot por ti, y luego **reescríbelo** con objetivo + contexto + restricciones + formato y vuelve a enviarlo.

1. Prompt vago A:
   ```
   haz una función para formatear documentos
   ```
   Reescritura sugerida (compárala con la tuya):
   ```
   #file:ValidadorRuc.cs Agrega a ValidadorRuc un método FormatearRuc(string ruc) que devuelva el RUC con un guion antes del último dígito ("20123456789" → "2012345678-9"). Si no pasa EsValido, lanza ArgumentException. Solo el método, sin explicación, comentarios en español.
   ```
2. Prompt vago B:
   ```
   mejora PolizaService
   ```
   Pista: *mejorar* no es un objetivo. Decide **una** cosa (legibilidad, validación de argumentos, un método nuevo) y pídela con restricciones: *"sin cambiar las firmas públicas"*.
3. Prompt vago C:
   ```
   explica esto
   ```
   Pista: ¿qué es *esto*? Usa `#selection` sobre `LiquidadorSiniestros.Calc` y pide el nivel: *"para un desarrollador junior, en 5 líneas, en español"*.
4. En `notas.md` escribe, para cada prompt, qué parte faltaba (objetivo, contexto, restricciones o formato).

✅ **Checkpoint:** tres prompts reescritos y una nota con la parte que faltaba en cada uno.

🧠 **Prueba del otro desarrollador:** si un compañero no podría implementar tu prompt sin preguntarte nada, Copilot tampoco.

---

## Lab 2 · Few-shot: enseñar con ejemplos (10 min)

**Objetivo:** comprobar que los ejemplos fijan reglas mejor que las descripciones.

1. **Zero-shot.** Conversación nueva, modo Ask:
   ```
   #file:Poliza.cs Escribe un método Resumen(Poliza p) que devuelva una línea de texto legible con los datos de la póliza.
   ```
   Observa el formato que inventó.
2. **One-shot.** Añade un ejemplo de salida:
   ```
   #file:Poliza.cs Escribe un método Resumen(Poliza p) que devuelva exactamente este formato:
   POL-0001 · SALUD · S/ 120.00 · vigente hasta 31/12/2026
   Usa "vencida el" en lugar de "vigente hasta" si FinVigencia es anterior a hoy.
   ```
3. **Few-shot.** Ahora una regla con casos borde, sin describirla con palabras:
   ```
   #file:ValidadorRuc.cs Implementa FormatearRuc según estos ejemplos:
   "20123456789" → "2012345678-9"
   "10987654321" → "1098765432-1"
   " 20123456789 " → "2012345678-9"
   "123" → ArgumentException
   null → ArgumentException
   ```
   Fíjate si dedujo el `Trim()` solo a partir del tercer ejemplo.
4. Inserta `FormatearRuc` en `ValidadorRuc.cs` y `Resumen` en `PolizaService.cs` (o donde prefieras). Compila.

✅ **Checkpoint:** `FormatearRuc` maneja espacios y nulos, y puedes explicar qué cambió entre zero-shot y one-shot.

🧠 **Clave:** una entrada y una salida valen más que un párrafo. Un test unitario es el mejor ejemplo posible (lo usamos en el Lab 5).

---

## Lab 3 · Rol y formato de salida (5 min)

**Objetivo:** orientar las prioridades de Copilot y obtener una respuesta revisable.

1. Conversación nueva, modo Ask:
   ```
   Actúa como revisor senior de .NET en una entidad financiera.
   #file:ClienteRepository.cs
   Revisa el archivo buscando: inyección SQL, recursos sin liberar, manejo de null y nombres poco claros.
   Responde en español con una tabla: Línea | Problema | Severidad | Fix propuesto.
   No reescribas el archivo.
   ```
2. Repite **sin** la primera línea (sin rol) y compara: ¿cambió el orden de prioridades o la severidad?
3. Pide un formato distinto para el mismo análisis:
   ```
   Convierte la tabla anterior en una lista de tareas en Markdown, ordenada por severidad, máximo 5 ítems.
   ```

✅ **Checkpoint:** tienes la tabla de revisión y notaste al menos una diferencia entre con rol y sin rol.

🧠 **El rol no cambia el modelo;** cambia qué prioriza. El formato decide si la respuesta es revisable fila por fila o un bloque que nadie lee.

---

## Lab 4 · `/explain` y `/doc` (10 min)

**Objetivo:** usar los comandos empaquetados y afinarlos con texto.

1. Abre `LiquidadorSiniestros.cs` (ya con nombres descriptivos si hiciste el Lab 5 de la sesión 2; si no, usa el de nombres crípticos: mejor aún). Selecciona el método y en **Inline Chat** (`Ctrl + I`):
   ```
   /explain
   ```
2. Ahora afina el comando:
   ```
   /explain para un desarrollador junior que no conoce seguros; define deducible y coaseguro; máximo 6 líneas; en español
   ```
   Compara las dos explicaciones.
3. Selecciona `ValidadorRuc.EsValido` y en Inline Chat:
   ```
   /doc
   ```
   Lee el `<summary>` generado. ¿Describe **las reglas** (11 dígitos, prefijo 10 o 20) o solo dice "valida un RUC"? Si es vago, rechaza y pide:
   ```
   /doc en español; en el summary enumera las tres reglas de validación; documenta el parámetro y el valor de retorno
   ```
4. Repite `/doc` sobre `CalculadoraPrima.PrimaAnual` y revisa que la documentación diga **cómo** se aplica el descuento.
5. Compila.

✅ **Checkpoint:** `EsValido` y `PrimaAnual` tienen documentación XML precisa, y viste que `/explain` cambia con el texto que le añades.

---

## Lab 5 · `/tests` con xUnit (15 min)

**Objetivo:** generar pruebas, revisarlas como oráculo de negocio y detectar un test que documenta un bug.

### Parte A — ValidadorRuc

1. Abre `ValidadorRuc.cs`, selecciona la clase completa y en la vista de Chat (modo Ask o Agent):
   ```
   #selection /tests con xUnit en el proyecto CopilotSesion1.Tests; usa [Theory] e [InlineData]; cubre: RUC válido con 10 y con 20, 10 dígitos, 12 dígitos, letras, espacios alrededor, null y cadena vacía; nombres de test en español
   ```
2. Crea `ValidadorRucTests.cs` en `CopilotSesion1.Tests` con el resultado (en Agent lo creará solo; revisa el diff antes de **Keep**).
3. **Revisa cada `InlineData`** antes de ejecutar: ¿el valor esperado es el que dicta la regla de negocio o el que el código devuelve hoy? Añade a mano un caso que falte.
4. Ejecuta:
   ```bash
   dotnet test
   ```

### Parte B — La trampa: CalculadoraPrima

5. Introduce un **bug a propósito** en `CalculadoraPrima.PrimaAnual`: cambia el descuento de `0.95m` a `0.90m` (10 % en lugar de 5 %). Guarda.
6. Selecciona el método y pide:
   ```
   #selection /tests con xUnit; cubre producto SALUD, VIDA, "salud" en minúsculas y prima negativa
   ```
7. Mira el test para SALUD: ¿qué valor esperado puso Copilot? Lo más probable es que **afirme el 10 %**, porque dedujo la regla del código, no del negocio. El test pasaría y el bug quedaría "documentado".
8. Corrige el `InlineData` esperado al 5 % (regla de negocio), ejecuta `dotnet test` y confirma que **falla**. Ese fallo es correcto: delata el bug.
9. Deja el bug para el Lab 6.

✅ **Checkpoint:** `ValidadorRucTests` pasa; `CalculadoraPrimaTests` tiene al menos un test en rojo que refleja la regla real.

🧠 **El oráculo eres tú:** `/tests` escribe rápido lo evidente; decidir qué es correcto sigue siendo trabajo humano.

---

## Lab 6 · `/fix`: causa o síntoma (10 min)

**Objetivo:** corregir con Copilot sin perder el control del diff.

### Parte A — Fix de lógica guiado por el test

1. Ejecuta `dotnet test` y copia el mensaje del test en rojo (`Assert.Equal() Failure: Expected … Actual …`).
2. Abre `CalculadoraPrima.cs`, selecciona `PrimaAnual` y en Inline Chat:
   ```
   /fix el test CalculadoraPrimaTests falla: [pega el mensaje]. Corrige la lógica para que el descuento de SALUD sea 5 %. No modifiques los tests ni la firma del método.
   ```
3. Lee el diff: ¿tocó solo la constante? ¿Añadió algo que no pediste? Acepta solo si puedes explicarlo. `dotnet test` → verde.

### Parte B — Síntoma vs causa

4. Introduce este bug en `PolizaService.FiltrarVigentes`: haz que reciba `List<Poliza>? polizas` y elimina cualquier comprobación de null si la había. En `Program.cs` llama al método con `null` y ejecuta `dotnet run`: `NullReferenceException`.
5. Selecciona el método y en Inline Chat:
   ```
   /fix
   ```
   Es muy probable que proponga devolver una lista vacía o envolver en `if (polizas != null)`.
6. Pregúntate: ¿es eso lo que el negocio quiere, o `null` aquí es un error del llamador que debería explotar con `ArgumentNullException`? Decide tú y vuelve a pedir:
   ```
   /fix lanza ArgumentNullException con el nombre del parámetro si polizas es null; no devuelvas lista vacía
   ```
7. `dotnet build` y, si quieres, un test para el caso `null`.

✅ **Checkpoint:** el test de SALUD está en verde sin haber tocado el test, y elegiste conscientemente entre tapar el síntoma y corregir la causa.

🧠 **Nunca aceptes un `/fix`** que no puedas explicar por qué funciona.

---

## Lab 7 · Revisión responsable: prompts que piden lo incorrecto (5 min)

**Objetivo:** reconocer prompts que, bien ejecutados, producen un mal resultado.

1. Con tu pareja, lee estos tres prompts y **no los envíes**:
   ```
   haz que pase el test CalculadoraPrimaTests
   ```
   ```
   quita las validaciones de ValidadorRuc para que compile más rápido el proyecto
   ```
   ```
   genera tests para todo el proyecto hasta llegar al 100 % de cobertura
   ```
2. Para cada uno, escribe en `notas.md`: qué haría Copilot probablemente (modificar el test o hardcodear el valor · eliminar reglas de negocio · afirmar el comportamiento actual, bugs incluidos) y **cómo lo reescribirías** para que el resultado sea verificable.
3. Opcional: envía el primero en una rama aparte y comprueba qué tocó: ¿el código o el test?

✅ **Checkpoint:** tres prompts reescritos con objetivo verificable y restricciones explícitas ("sin modificar el test", "explica antes de eliminar", "tests por regla de negocio").

🧠 **Copilot hace lo que le pides.** Si pides un atajo, obtienes un atajo.

---

## Checklist final

- [ ] Escribo prompts con objetivo, contexto, restricciones y formato (y aplico las 4 S).
- [ ] Uso ejemplos (one-shot / few-shot) para fijar formato y reglas con casos borde.
- [ ] Asigno un rol y un formato de salida cuando quiero una respuesta revisable.
- [ ] Uso `/explain` y `/doc` y los afino añadiendo texto al comando.
- [ ] Uso `/tests` y reviso cada assert contra la regla de negocio, no contra el código actual.
- [ ] Uso `/fix` leyendo el diff y distinguiendo causa de síntoma.
- [ ] Detecto prompts que piden atajos y los reescribo para que sean verificables.

---

## Troubleshooting

| Síntoma | Causa probable | Qué hacer |
|---|---|---|
| `dotnet test` no encuentra pruebas | Falta la referencia o el archivo no está en el proyecto de tests | `dotnet add CopilotSesion1.Tests reference CopilotSesion1`; revisa que el `.cs` esté en la carpeta `CopilotSesion1.Tests` |
| `/tests` dice que no hay proyecto de pruebas | Copilot no detecta xUnit | Indícalo en el prompt: "en el proyecto CopilotSesion1.Tests con xUnit"; o acepta su propuesta de crearlo |
| El proyecto principal compila los tests y falla | Carpeta `Tests` anidada dentro de `CopilotSesion1` | Mueve `CopilotSesion1.Tests` como carpeta hermana (Lab 0, paso 2) |
| `/doc` genera comentarios en inglés | Idioma por defecto | Añade "en español" al comando o configura `github.copilot.chat.localeOverride` |
| `/fix` cambia más de lo pedido | Prompt sin límites | Discard; repite con "no modifiques X" y "solo en este método" |
| El comando no aparece al escribir `/` | Versión de VS Code o extensión | Actualiza; mientras tanto escribe la petición en lenguaje natural |
| Otro problema | — | *View → Output* → canal **GitHub Copilot Chat** |

---

## Para profundizar

- Microsoft Learn · [Introduction to prompt engineering with GitHub Copilot](https://learn.microsoft.com/en-us/training/modules/introduction-prompt-engineering-with-github-copilot/)
- GitHub Docs · [Prompt engineering for Copilot Chat](https://docs.github.com/en/copilot/concepts/prompting/prompt-engineering)
- GitHub Docs · [Copilot Chat cheat sheet](https://docs.github.com/en/copilot/using-github-copilot/copilot-chat/github-copilot-chat-cheat-sheet?tool=vscode)
- VS Code Docs · [Copilot features reference (slash commands)](https://code.visualstudio.com/docs/copilot/reference/copilot-vscode-features)
- xUnit · [Getting started with .NET](https://xunit.net/docs/getting-started/v3/getting-started)

**Próxima sesión:** generación de pruebas y refactorización asistida.
