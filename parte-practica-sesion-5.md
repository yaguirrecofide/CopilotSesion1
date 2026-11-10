# Parte práctica · Sesión 5 — Code review, documentación y commits con Copilot

> **Duración:** 70 minutos (+ Lab 7 opcional) · **Modalidad:** individual o en parejas · **Stack:** C# / .NET 10 · Git · VS Code
> **Objetivo:** producir commits que expliquen el cambio, usar Copilot code review en VS Code con criterio, revisar código escrito por IA con una checklist, y generar documentación (XML docs, README, ADR, diagrama) que acompañe al código.

| Lab | Tema | Tiempo |
|---|---|---|
| 0 | Entorno con Git | 5 min |
| 1 | Commits con mensaje generado | 10 min |
| 2 | Code review de una selección | 10 min |
| 3 | Code review de cambios sin commitear | 10 min |
| 4 | Revisar un servicio escrito por Agent | 15 min |
| 5 | README, ADR y diagrama Mermaid | 15 min |
| 6 | Revisión responsable: el PR mal escrito | 5 min |
| 7 | Opcional: PR en GitHub con Copilot como reviewer | — |

---

## Requisitos previos

- Sesión 4 completada: `CopilotSesion1` y `CopilotSesion1.Tests` en verde, con `ClienteRepository`, `IClienteRepository`, `ClienteService`, `CalculadoraPrima`, `PolizaService` y `LiquidadorSiniestros` refactorizado.
- **Git** instalado y configurado (`git config user.name` / `user.email`). Si el proyecto no es un repositorio todavía: `git init && git add -A && git commit -m "chore: estado inicial sesión 5"`.
- VS Code actualizado con *GitHub Copilot*. Los nombres de los menús de review cambian entre versiones: si no ves *Review and Comment*, busca **Review** dentro del menú contextual de Copilot o en *Generate Code*.
- Lab 7 requiere cuenta de GitHub con repositorio propio y **Copilot code review habilitado** en el plan/organización. Si no lo tienes, sáltalo: el instructor lo demuestra.

> 💡 Patrón de la sesión: **primero Copilot, después tú.** Todo lo que Copilot genera (mensaje, hallazgo, documento) se lee y se decide; nada se acepta por defecto.

### Referencia rápida

| Acción | Dónde |
|---|---|
| Generar mensaje de commit | Source Control → icono ✨ de Copilot junto al cuadro de mensaje |
| Review de una selección | Selecciona → clic derecho → **Copilot → Review and Comment** (o *Generate Code → Review*) |
| Review de cambios sin commitear | Source Control → hover sobre **CHANGES** → **Copilot Code Review – Uncommitted Changes** |
| Ver hallazgos | Panel **Comments** (selección) · pestaña **Problems** (cambios) · inline en el editor |
| Aplicar / descartar un fix sugerido | Botones en el comentario inline |
| Documentación XML | Inline Chat → `/doc` |

**Convención de commits (Conventional Commits):** `tipo(scope): resumen ≤ 72 caracteres` · tipos: `feat` `fix` `refactor` `test` `docs` `chore` · cuerpo con el **por qué** · `Refs: TICKET`.

---

## Lab 0 · Entorno con Git (5 min)

1. Rama nueva y estado limpio:
   ```bash
   git checkout -b sesion-5
   git status
   dotnet test
   ```
   Si hay cambios sin commitear de la sesión 4, commitéalos ahora (`chore: cierre sesión 4`).
2. Comprueba que Source Control de VS Code muestra el repositorio y el icono ✨ de Copilot en el cuadro de mensaje.
3. Abre el Chat (`Ctrl + Alt + I`).

✅ **Checkpoint:** rama `sesion-5`, árbol limpio, tests en verde, icono de Copilot visible en Source Control.

---

## Lab 1 · Commits con mensaje generado (10 min)

**Objetivo:** generar el mensaje desde el diff, leerlo y corregirlo a la convención del equipo.

Harás **tres cambios pequeños y separados**; cada uno es un commit.

1. **Cambio 1 (feat).** En `CalculadoraPrima`, agrega una sobrecarga `PrimaAnualConRecargo(Poliza p)` que llame a la existente con `siniestrosPrevios: 0`. Compila.
   - Stage solo ese archivo. Pulsa ✨. Lee el mensaje: ¿dice **qué** y **por qué**? ¿Inventó algo que no está en el diff?
   - Corrígelo a la convención, por ejemplo:
     ```
     feat(prima): sobrecarga PrimaAnualConRecargo sin siniestros previos

     Simplifica el uso desde servicios que aún no registran siniestralidad.
     Refs: COF-415
     ```
   - Commit.
2. **Cambio 2 (test).** Agrega un test para esa sobrecarga en `CalculadoraPrimaRecargoTests.cs`. `dotnet test`. Stage, ✨, corrige (`test(prima): …`), commit.
3. **Cambio 3 (trampa: dos intenciones).** Haz **a la vez** un rename de variable en `PolizaService` **y** un cambio en el mensaje de una excepción de `ValidadorRuc`. Stage ambos y pulsa ✨.
   - Observa: el mensaje generado tendrá un "y también" o un resumen vago. Eso es la señal.
   - **Divide el stage:** unstage uno, genera y commitea (`refactor(polizas): …`), luego el otro (`fix(ruc): …`).
4. Revisa el historial:
   ```bash
   git log --oneline -5
   ```

✅ **Checkpoint:** 4 commits con convención, cada uno con una sola intención y un cuerpo que explica el porqué.

🧠 **El diff ya dice el cómo.** El mensaje existe para el qué y el por qué; eso Copilot no lo sabe si no está en el código.

---

## Lab 2 · Code review de una selección (10 min)

**Objetivo:** pedir una revisión a Copilot sobre un método concreto y hacer triage de los hallazgos.

1. Abre `ClienteRepository.cs`. Si en la sesión 1 conservaste el método **inseguro** `BuscarNombrePorDocumento` (concatenación SQL), úsalo. Si no, pégalo de nuevo (es intencional):
   ```csharp
   public string? BuscarNombrePorDocumento(string documento)
   {
       using var conn = new SqlConnection(_connectionString);
       conn.Open();
       var cmd = new SqlCommand("SELECT Nombre FROM Clientes WHERE Documento = '" + documento + "'", conn);
       return cmd.ExecuteScalar()?.ToString();
   }
   ```
2. Selecciona el método → clic derecho → **Copilot → Review and Comment**.
3. Lee los hallazgos en el panel **Comments** / inline. Para cada uno, anota en `notas.md`: severidad, ¿es correcto?, ¿lo aplico, lo rechazo o lo reformulo?
   - Esperable: inyección SQL (alta), `SqlCommand` sin `using` (media), `documento` sin validar (media/baja).
4. **Aplica** el fix de la inyección si propone parámetros (`@documento`). Si el fix sugerido es incompleto (p. ej. parametriza pero deja el `SqlCommand` sin `using`), **recházalo** y corrige a mano o con Inline Chat: *"usa parámetros y `using` para el comando; no cambies la firma"*.
5. Vuelve a pedir **Review and Comment** sobre el método corregido. ¿Quedó algo? ¿Algún falso positivo?
6. `dotnet build`, stage, ✨, commit (`fix(clientes): parametriza consulta por documento`).

✅ **Checkpoint:** tabla de triage con al menos 3 hallazgos y una decisión argumentada por cada uno; el método quedó parametrizado.

🧠 **Los hallazgos son diagnósticos, no órdenes.** Un *Apply* ciego es un *Keep* ciego.

---

## Lab 3 · Code review de cambios sin commitear (10 min)

**Objetivo:** revisar exactamente lo que vas a commitear, con un bug plantado para ver si lo encuentra.

1. Haz **tres cambios** en `PolizaService.cs` sin commitear:
   - Uno correcto: extrae una constante `DiasAvisoVencimiento = 30` (si no lo hiciste en la sesión 4).
   - Un **bug plantado**: en `FiltrarVigentes`, cambia `p.FinVigencia >= fecha` por `p.FinVigencia > fecha` (o el operador inverso al que decidiste en la sesión 4).
   - Un smell: agrega un `catch (Exception) { return new List<Poliza>(); }` alrededor del filtro.
2. **No ejecutes los tests todavía.** En Source Control, hover sobre **CHANGES** → **Copilot Code Review – Uncommitted Changes**.
3. Revisa la pestaña **Problems** y los comentarios inline:
   - ¿Detectó el `catch` que traga excepciones? (Muy probable.)
   - ¿Detectó el cambio de operador? (Poco probable: no conoce la regla de negocio. Si lo marcó, ¿con qué argumento?)
   - ¿Comentó algo sobre la constante? (Debería ser neutro o positivo.)
4. Ahora `dotnet test`: el test del caso inclusive del Lab 1 de la sesión 4 debe ponerse **rojo**. Eso es lo que Copilot **no** podía saber y el test sí.
5. Corrige el operador, elimina el `catch`, `dotnet test` verde, stage, ✨, commit.
6. En `notas.md`: ¿qué encontró Copilot, qué encontró el test, qué habría encontrado solo un reviewer humano?

✅ **Checkpoint:** tres columnas en tus notas (Copilot / tests / humano) con al menos un ítem en cada una.

🧠 **Copilot revisa lo evidente; los tests revisan la regla; el humano revisa la intención.** Las tres capas se complementan.

---

## Lab 4 · Revisar un servicio escrito por Agent (15 min)

**Objetivo:** revisar código generado por IA con la checklist de señales típicas.

### Parte A — Generar

1. Modo **Agent**, conversación nueva:
   ```
   Crea ReporteService en CopilotSesion1 con: (1) ResumenPorProducto(List<Poliza>) que devuelva, por producto, cantidad de pólizas activas y suma de primas mensuales; (2) ExportarCsv(List<Poliza>, string ruta) y ExportarJson(List<Poliza>, string ruta); (3) filtro opcional por rango de fechas de inicio de vigencia. Usa los records y servicios existentes. Incluye tests en CopilotSesion1.Tests. Ejecuta dotnet test al final.
   ```
2. Deja que termine. **No hagas Keep todavía.** Lee la lista de archivos tocados.

### Parte B — Revisar con la checklist

3. Abre `ReporteService.cs` y revisa como reviewer buscando **cada** señal; anota línea y decisión en `notas.md`:

   | Señal | ¿Está? | Línea / decisión |
   |---|---|---|
   | Sobreingeniería (interfaces, opciones, genéricos no pedidos) | | |
   | Código muerto / comentarios obvios | | |
   | APIs o paquetes inventados (verifica con IntelliSense y `dotnet build`) | | |
   | Tests tautológicos (asserts que repiten la implementación; mira los esperados) | | |
   | Manejo de errores genérico (`catch (Exception)`, `return null` por defecto) | | |
   | Estilo ajeno al proyecto (inglés, otro patrón, otra librería) | | |
   | Seguridad: rutas de archivo sin validar, datos personales en el CSV | | |

4. Pide una **segunda opinión** con la checklist de la teoría (modo Ask, conversación nueva):
   ```
   Actúa como revisor senior de .NET en una entidad financiera. #file:ReporteService.cs Revisa contra: seguridad (rutas, datos personales), correctitud (null, decimal, DateOnly), recursos (using), diseño (responsabilidad única, duplicación con PolizaService), pruebas (qué regla no tiene test). Responde en tabla Línea | Hallazgo | Severidad | Fix. Marca como "No verificable" lo que dependa de reglas de negocio. No reescribas el archivo.
   ```
5. Compara tu tabla con la de Copilot: ¿qué encontraste tú que Copilot no, y viceversa?
6. Decide: **Keep** con correcciones (pídelas en el mismo hilo de Agent, acotadas) o **Undo** y vuelve a pedir con un prompt más estricto (*"sin interfaces, sin opciones de configuración, CSV con cabecera y separador ';', sin incluir el documento del cliente"*).
7. `dotnet test` verde, commit (`feat(reportes): resumen por producto y exportación CSV/JSON`).

✅ **Checkpoint:** tabla de señales completa, comparación con la review de Copilot, y un `ReporteService` que puedes explicar línea por línea.

🧠 **Sesgo de automatización:** el código de IA se ve pulido y por eso se revisa menos. Revísalo más.

---

## Lab 5 · README, ADR y diagrama Mermaid (15 min)

**Objetivo:** generar documentación útil y corregir lo que Copilot no puede saber.

### Parte A — README

1. Modo Ask:
   ```
   #codebase Genera un README.md para esta solución: propósito (ejercicios del curso GitHub Copilot sobre un dominio de pólizas), requisitos (.NET 10 SDK, VS Code + C# Dev Kit), cómo compilar y ejecutar los tests, estructura de carpetas y clases principales con una línea cada una. Máximo 80 líneas, en español.
   ```
2. Revisa: ¿los comandos son los correctos (`dotnet test` desde la carpeta de la solución)? ¿Inventó clases que no existen o describió mal alguna? Corrige y guarda.

### Parte B — ADR (registro de decisión)

3. Crea `docs/adr/0001-fin-vigencia-inclusive.md` pidiendo:
   ```
   Redacta un ADR con secciones Contexto, Decisión, Alternativas consideradas, Consecuencias, para esta decisión: "En PolizaService.FiltrarVigentes la fecha de fin de vigencia es inclusive (una póliza que vence hoy sigue vigente hoy)". En español, máximo 40 líneas.
   ```
4. **Lee las Alternativas y las Consecuencias:** Copilot las inventó porque no estuvo en la conversación donde lo decidieron. Reemplázalas por las reales (p. ej. *"el negocio cobra la prima del día completo"*, *"alinea con el comportamiento del core de pólizas"*). Esa parte es tuya.
5. Segundo ADR (opcional): `0002-consultas-parametrizadas.md` para la decisión de la sesión 1 (SQL con parámetros).

### Parte C — Diagrama

6. Pide el diagrama:
   ```
   #file:PolizaService.cs #file:CalculadoraPrima.cs #file:ReporteService.cs #file:ClienteService.cs #file:IClienteRepository.cs Genera un diagrama Mermaid classDiagram con estas clases, sus métodos públicos y las dependencias entre ellas. Sin métodos privados. Solo el bloque mermaid.
   ```
7. Pégalo en `docs/arquitectura.md` dentro de un bloque ```mermaid. Si tienes la extensión *Markdown Preview Mermaid Support*, ábrelo con `Ctrl + Shift + V`; si no, GitHub lo renderiza al subirlo.
8. Comprueba que las dependencias dibujadas coinciden con el código (¿`ReporteService` usa `PolizaService` o no?).

### Parte D — Sincronización

9. Prompt de cierre:
   ```
   Revisa si README.md y docs/arquitectura.md siguen siendo ciertos tras los cambios sin commitear; lista lo que hay que actualizar. No edites los archivos.
   ```
10. Commit: `docs: README, ADR 0001 y diagrama de arquitectura`.

✅ **Checkpoint:** README sin comandos ni clases inventadas, ADR con alternativas y consecuencias reales, diagrama que coincide con el código.

🧠 **Lo que Copilot no sabe es el porqué.** Eso es exactamente lo que vale la pena documentar, y lo escribes tú.

---

## Lab 6 · Revisión responsable: el PR mal escrito (5 min)

**Objetivo:** reconocer qué no debe ir en un PR o commit y cuándo una review de IA no basta.

1. Lee esta descripción de PR y **no la uses**:
   ```
   ## Cambios
   Arreglé lo de los reportes. Copilot lo revisó y no encontró nada, así que está listo para merge.
   Para probar usar la conexión Server=prod-sql01;User Id=sa;Password=P@ss2026! y el cliente 45678912 (Juan Pérez).
   ```
2. Identifica al menos **cuatro** problemas (pistas: credenciales, datos personales, "Copilot lo revisó" como aprobación, sin qué ni por qué, sin referencia a tests).
3. Reescribe la descripción en `notas.md`: qué cambió, por qué, cómo se probó (`dotnet test`, casos), qué partes generó Copilot y merecen más atención del reviewer, y datos **ficticios** si hace falta un ejemplo.
4. Pregunta para tu equipo: ¿la definición de "listo" incluye review humano y documentación actualizada? Anota qué añadirías.

✅ **Checkpoint:** descripción de PR reescrita sin secretos, con transparencia sobre el uso de IA y con evidencia de pruebas.

🧠 **Review de IA ≠ aprobación.** Quien hace merge responde por el código; la herramienta no firma.

---

## Lab 7 · Opcional: PR en GitHub con Copilot como reviewer

Solo si tienes repositorio propio en GitHub y Copilot code review habilitado.

1. Sube el proyecto y la rama:
   ```bash
   git remote add origin https://github.com/<tu-usuario>/CopilotSesion1.git
   git push -u origin sesion-5
   ```
2. Crea el PR desde `sesion-5` hacia `main`. En la descripción, usa el botón de Copilot para generar el **resumen** y complétalo con el porqué (Lab 6).
3. En **Reviewers**, pide la review a **Copilot** (o desde CLI: `gh pr edit --add-reviewer @copilot`).
4. Cuando llegue (≈ 1 min): lee cada comentario con su severidad, aplica los cambios sugeridos que correspondan, responde o resuelve el resto. Observa que la review es un **Comment**, no una aprobación.
5. Haz un push con una corrección y comprueba si hay re-review (depende de la configuración del repositorio).

✅ **Checkpoint:** PR con resumen generado + porqué humano, review de Copilot triageada, aprobación humana pendiente.

---

## Checklist final

- [ ] Genero el mensaje de commit desde el diff y lo corrijo a qué + por qué con la convención del equipo; un commit, una intención.
- [ ] Uso Copilot code review sobre una selección y sobre los cambios sin commitear, y triageo cada hallazgo.
- [ ] Reviso código escrito por IA con la checklist de señales (sobreingeniería, APIs inventadas, tests tautológicos, errores genéricos…).
- [ ] Genero README, XML docs, ADR y diagramas con Copilot, y escribo yo el porqué.
- [ ] Verifico que la documentación siga siendo cierta en cada PR.
- [ ] No pongo secretos ni datos personales en commits ni PR; declaro qué partes generó Copilot; sé que su review no es aprobación.

---

## Troubleshooting

| Síntoma | Causa probable | Qué hacer |
|---|---|---|
| No aparece el icono ✨ en Source Control | Extensión desactualizada o repositorio no detectado | Actualiza Copilot; `git init` si falta; recarga VS Code |
| No existe *Review and Comment* en el menú | Nombre distinto según versión | Busca **Review** en el submenú de Copilot o en *Generate Code*; o usa el chat con `#selection` y la checklist |
| El review de cambios no muestra nada | No hay cambios sin commitear o aún no terminó | Comprueba CHANGES en Source Control; espera el indicador de progreso |
| Mensaje de commit en inglés | Idioma por defecto | Escribe "en español" tras generarlo y vuelve a pulsar ✨, o configura instrucciones de commit (sesión 7) |
| Agent genera un `ReporteService` minúsculo en el Lab 4 | Prompt corto | Añade requisitos: CSV con cabecera, JSON indentado, filtro por fechas, tests |
| Mermaid no se renderiza en VS Code | Falta extensión de preview | Instala *Markdown Preview Mermaid Support* o revísalo en GitHub |
| Lab 7: Copilot no aparece como reviewer | Code review no habilitado en el plan/org | Sáltalo; pide al administrador habilitarlo |
| Otro problema | — | *View → Output* → **GitHub Copilot Chat** |

---

## Para profundizar

- GitHub Docs · [Using GitHub Copilot code review](https://docs.github.com/en/copilot/how-tos/use-copilot-agents/request-a-code-review/use-code-review)
- VS Code Docs · [Source control with Copilot (commit messages)](https://code.visualstudio.com/docs/copilot/overview)
- Conventional Commits · <https://www.conventionalcommits.org/>
- ADR · <https://adr.github.io/>
- Mermaid · [classDiagram](https://mermaid.js.org/syntax/classDiagram.html)

**Próxima sesión:** modo agente y Copilot coding agent en GitHub.
