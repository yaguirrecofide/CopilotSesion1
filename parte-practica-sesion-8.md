# Parte práctica · Sesión 8 — Proyecto integrador: una feature completa con Copilot

> **Duración:** 80 min de proyecto + 15 min de revisión cruzada · **Modalidad:** parejas (implementador / revisor, con rotación) · **Stack:** C# / .NET 10 · xUnit · NSubstitute · Git · VS Code
> **Objetivo:** entregar un pull request de la feature *Propuesta de renovación de pólizas* que otra pareja pueda aprobar con la rúbrica, aplicando el flujo completo: issue → plan → tests → implementación → review → documentación → PR.

| Fase | Qué se hace | Tiempo | Rol que lidera |
|---|---|---|---|
| 1 | Issue con criterios de aceptación | 10 min | Ambos |
| 2 | Plan con el agente, corregido y aprobado | 10 min | Implementador |
| 3 | Tests primero (regla + mocks) | 15 min | **Revisor escribe los tests** |
| — | **Rotación de roles** | — | — |
| 4 | Implementación con la red en verde | 20 min | Implementador (el que era revisor) |
| 5 | Review: @revisor, /review-financiero, review de cambios | 15 min | Revisor |
| 6 | ADR, README, commits y PR | 10 min | Ambos |
| 7 | Revisión cruzada con la rúbrica (PR de otra pareja) | 15 min | Ambos |

---

## Requisitos previos

- Repositorio con el **PR de personalización de la sesión 7 mergeado**: `.github/copilot-instructions.md`, `.github/instructions/tests.instructions.md`, prompts `/review-financiero`, `/issue-agente` (o `/tabla-casos`, `/adr`), agente `@revisor`, `.vscode/mcp.json`.
- Código de sesiones anteriores: `Poliza`, `PolizaService` (con `FiltrarVigentes` y `PolizasPorVencer`), `CalculadoraPrima` (con `PrimaAnualConRecargo`), `IClienteRepository`, `ClienteService`, `ReporteService`. `dotnet test` en verde.
- Si a tu pareja le falta algo, 5 minutos al inicio para igualar repos (clonar el del compañero vale).

> 💡 **Hoy no hay guía paso a paso del código.** Esta guía da el reto, el flujo, las herramientas por fase y la rúbrica. Las decisiones son suyas: eso es lo que se evalúa.

---

## El reto · Propuesta de renovación de pólizas

### Objetivo

Un `RenovacionService` que, para las pólizas que vencen en los próximos `N` días, genere una `PropuestaRenovacion` con la prima anual propuesta y permita exportar las propuestas a CSV.

### Reglas de negocio

1. **Prima propuesta** = `CalculadoraPrima.PrimaAnualConRecargo(poliza, siniestrosPrevios)`.
2. **Descuento cliente frecuente:** 3 % si el titular tiene **3 o más pólizas activas**. El conteo se obtiene vía `IClienteRepository` (añade el método que necesites a la interfaz).
3. **Siniestros previos:** se obtienen de un nuevo `ISiniestroRepository` (solo interfaz; en tests se mockea con NSubstitute; no hay implementación real).
4. Pólizas **inactivas** o **ya vencidas** no generan propuesta. `N <= 0` → `ArgumentOutOfRangeException`.
5. **CSV:** con cabecera, separador `;`, importes con 2 decimales, fechas `yyyy-MM-dd`, **sin datos personales** (solo número de póliza, producto, fecha de vencimiento, prima actual, prima propuesta).

### Ambigüedades a propósito (decidir y documentar en un ADR)

- ¿El 3 % de cliente frecuente se aplica **antes o después** del recargo por siniestralidad? ¿Es acumulable con el 5 % de SALUD?
- ¿Una póliza que vence **hoy** está "vencida" o "por vencer"? (Recuerda el ADR 0001 de la sesión 5.)
- ¿`PropuestaRenovacion` es un `record` inmutable o una clase? ¿Qué campos lleva?

### Entregables

Issue · plan aprobado (en el hilo del chat o en `docs/plan-renovacion.md`) · `RenovacionService` + `PropuestaRenovacion` + `ISiniestroRepository` · tests de regla y con mocks · ADR · README actualizado · commits con convención · PR con descripción.

### Alcance

- **Mínimo (obligatorio):** reglas 1, 3 y 4 con tests.
- **Extensión (si alcanza el tiempo):** reglas 2 y 5. Se recorta alcance, nunca la fase 5 (review).

---

## Chuleta: qué usar en cada fase

| Fase | Herramienta de Copilot | Lo que sigue siendo tuyo |
|---|---|---|
| 1 Issue | `/issue-agente` · Ask: *"actúa como el agente que lo implementará; lista ambigüedades"* | Decisiones de negocio; criterios de aceptación comprobables |
| 2 Plan | Agent (o Plan agent): *"dame el plan: archivos, pasos, comandos, riesgos; no ejecutes"* | Leer, corregir, aprobar; fuera de alcance explícito |
| 3 Tests | `/tabla-casos` sobre las reglas · `/tests` con la tabla · NSubstitute para `ISiniestroRepository` e `IClienteRepository` | El valor esperado de cada caso; qué se mockea y qué no |
| 4 Implementar | Agent con *"sin modificar los tests; no añadas casos por valor"* · `#file` · completions | Keep/Undo por archivo; `dotnet test`; un commit por paso; caso extra fuera de la tabla |
| 5 Revisar | `@revisor` · `/review-financiero` · Source Control → *Copilot Code Review – Uncommitted Changes* | Triage: aceptar / rechazar / reformular; causa vs síntoma; checklist de código de IA |
| 6 Docs y PR | `/adr` · `#codebase` para README · ✨ commit · resumen de PR | El porqué del ADR; transparencia sobre qué generó Copilot; sin secretos ni datos personales |

> Si dudas: **Ask** para entender · **Inline** para cambiar lo que ves · **Agent** para multi-archivo con tests que verifiquen.

---

## Fase 1 · Issue (10 min) — ambos

1. Rama: `git checkout -b feature/renovacion`.
2. Crea `docs/issues/renovacion.md` con la plantilla (`/issue-agente` si lo tienes; si no, a mano):
   - **Objetivo** (un verbo, resultado observable).
   - **Contexto:** `PolizaService.PolizasPorVencer`, `CalculadoraPrima.PrimaAnualConRecargo`, `IClienteRepository`, ADR 0001.
   - **Criterios de aceptación:** mínimo 6, comprobables en el PR; uno por regla + tests en verde + "sin cambios en firmas públicas existentes".
   - **Fuera de alcance:** `ReporteService`, `Program.cs`, implementación real de `ISiniestroRepository`, paquetes nuevos.
3. **Decidid las ambigüedades ahora** y anotad la decisión en el issue (el ADR se redacta en la fase 6).
4. Segunda lectura por Copilot (Ask): *"actúa como el agente que implementará este issue; lista ambigüedades"*. Corrige lo razonable.
5. Commit: `docs(renovacion): issue de la feature`.

✅ **Criterio 1 de la rúbrica.**

---

## Fase 2 · Plan (10 min) — implementador

1. Agent, conversación nueva:
   ```
   #file:docs/issues/renovacion.md Antes de implementar, dame un plan: archivos a crear/modificar, pasos en orden, comandos a ejecutar y riesgos. No toques nada hasta que apruebe.
   ```
2. El revisor lee el plan en voz alta. Comprobad: ¿respeta el fuera de alcance? ¿Incluye tests y `dotnet test`? ¿Toca `IClienteRepository` solo para añadir el método de conteo? ¿Propone `record` para `PropuestaRenovacion`?
3. Corregid en el hilo. Cuando esté bien: *"Plan aprobado. **No ejecutes todavía**: primero escribiremos los tests."* Guardad el plan en `docs/plan-renovacion.md` (pega la respuesta) y commitead.

✅ **Criterio 2.**

---

## Fase 3 · Tests primero (15 min) — **el revisor escribe los tests**

1. Tabla de casos por regla (en `notas.md` o con `/tabla-casos`): entradas, esperado, borde. Mínimo:
   - Regla 1: póliza VIDA 100/mes, 0 siniestros → propuesta 1200.00; con 3 siniestros → 1500.00.
   - Regla 3: el servicio **llama** a `ISiniestroRepository` con el número de póliza correcto (`Received(1)`).
   - Regla 4: inactiva → sin propuesta; vencida ayer → sin propuesta; vence hoy → según vuestro ADR; `N = 0` → excepción.
   - (Extensión) Regla 2: cliente con 3 pólizas activas → 3 % aplicado según vuestra decisión de orden.
2. Crea `CopilotSesion1.Tests/RenovacionServiceTests.cs` con `/tests` a partir de la tabla, **mocks** con NSubstitute para los dos repositorios, y nombres `Metodo_Escenario_Resultado`. Revisa cada `InlineData` contra la tabla: **el esperado lo decidís vosotros**.
3. `dotnet build` → **debe fallar** (no existe `RenovacionService`). Ese es el rojo. Commit: `test(renovacion): tests de regla y mocks (rojo)`.

✅ **Criterios 3 y 4.**

> 🔄 **Rotación:** quien escribió los tests pasa a ser revisor; el otro implementa contra esos tests.

---

## Fase 4 · Implementación (20 min) — implementador (nuevo)

1. Agent, en el mismo hilo del plan (o nuevo con `#file` del plan y de los tests):
   ```
   #file:docs/plan-renovacion.md #file:CopilotSesion1.Tests/RenovacionServiceTests.cs Ejecuta el plan aprobado para que pasen estos tests. No modifiques los tests. No añadas casos especiales por valor. Respeta el fuera de alcance del issue. Ejecuta dotnet test al final.
   ```
2. Aprobad cada comando leyéndolo. Al terminar, **Keep/Undo por archivo**: si tocó algo fuera de alcance, Undo ese archivo.
3. `dotnet test` → verde. **Caso extra fuera de la tabla** (p. ej. VIDA 80/mes con 2 siniestros → 1056.00): añádelo y vuelve a correr. Si falla, el agente hardcodeó: volver con la restricción más explícita.
4. Un commit por paso lógico (`feat(renovacion): ISiniestroRepository y PropuestaRenovacion`, `feat(renovacion): RenovacionService con recargo`, …). Mensajes con ✨ y corregidos.
5. Si alcanza el tiempo: reglas 2 y 5 con el mismo ciclo (tabla → test → implementación → caso extra).

✅ **Criterios 5, 6 y 9.**

---

## Fase 5 · Review (15 min) — revisor

1. **`@revisor`** (agente de solo lectura): *"Revisa RenovacionService.cs y RenovacionServiceTests.cs."* Tabla de hallazgos.
2. **`/review-financiero`** sobre `RenovacionService` (seguridad: ¿datos personales en el CSV?; correctitud: `decimal`, `DateOnly`, redondeos).
3. **Copilot Code Review – Uncommitted Changes** si queda algo sin commitear; si no, *Review and Comment* sobre la clase.
4. **Checklist de código de IA** (sesión 5): sobreingeniería, APIs inventadas, tests tautológicos, `catch (Exception)`, estilo ajeno.
5. **Triage** en `docs/review-renovacion.md`: por hallazgo, severidad y decisión (aplicar / rechazar con motivo / reformular). Los fixes los aplica el implementador; los tests siguen en verde después de cada uno.
6. Commit: `fix(renovacion): hallazgos de review` + `docs(renovacion): triage de review`.

✅ **Criterio 7.**

---

## Fase 6 · Docs y PR (10 min) — ambos

1. **ADR** `docs/adr/0003-descuento-cliente-frecuente.md` (o la ambigüedad que hayáis decidido) con `/adr` o a mano: contexto, decisión, **alternativas reales** que descartasteis, consecuencias. Lo que Copilot invente, se corrige.
2. **README:** sección de `RenovacionService` (qué hace, cómo probarlo). Prompt de sincronización: *"revisa si README y docs/arquitectura.md siguen siendo ciertos; lista lo que hay que actualizar"*.
3. **PR** (`feature/renovacion` → `main`), descripción con: qué cambió, por qué (enlace al issue y al ADR), cómo se probó (`dotnet test`, casos), **qué partes generó Copilot** y dónde mirar con más atención, sin secretos ni datos personales. Si tenéis GitHub con Copilot code review, pedid la review.
4. Commit final: `docs(renovacion): ADR y README`.

✅ **Criterios 8 y 10.**

---

## Fase 7 · Revisión cruzada (15 min) — ambos

Recibís el PR de otra pareja. Rellenad la rúbrica **sin hablar con los autores**; si necesitáis preguntar algo, es un hallazgo del criterio 10 (la descripción no lo explicaba).

### Rúbrica

| # | Criterio | Cumple | Parcial | No cumple | Evidencia / comentario |
|---|---|:---:|:---:|:---:|---|
| 1 | Issue con objetivo, contexto, criterios de aceptación y fuera de alcance | | | | |
| 2 | Plan del agente revisado y aprobado antes de ejecutar (visible) | | | | |
| 3 | Tests de regla con valor esperado justificable; al menos un borde por regla | | | | |
| 4 | Mocks de `ISiniestroRepository` (y `IClienteRepository` si aplica) con `Received` | | | | |
| 5 | **Eliminatorio** · `dotnet test` en verde; ningún test modificado para pasar | | | | |
| 6 | Código respeta `copilot-instructions.md` (decimal, DateOnly, guard clauses, español, XML docs) | | | | |
| 7 | Review hecha (`@revisor` / `/review-financiero` / code review) con triage visible | | | | |
| 8 | ADR de una decisión ambigua con alternativas y consecuencias reales | | | | |
| 9 | Commits en Conventional Commits, una intención por commit | | | | |
| 10 | **Eliminatorio** · Descripción del PR: qué, por qué, cómo se probó, qué generó Copilot; sin secretos ni datos personales | | | | |

**Trampas conocidas** (si aparece una, es "no cumple" en el criterio correspondiente): el agente hardcodeó los casos (3/5) · "haz que pasen los tests" (5) · mockear `CalculadoraPrima` (4) · decisión escondida en un comentario en vez de ADR (8) · archivos fuera de alcance tocados (2) · datos personales en el CSV (10).

**Veredicto:** ☐ Aprobado ☐ Aprobado con cambios menores ☐ Cambios necesarios. Entregad la rúbrica al instructor y a los autores.

---

## Cierre del curso

- Encuesta final (5 min).
- Lo que se llevan: un repositorio listo para equipo y un flujo de issue a PR con verificación en cada paso.
- **GH-300:** los módulos de Microsoft Learn enlazados en cada guía cubren los dominios del examen; practicad con el *assessment* de cada módulo.
- **Lo que sigue:** un issue con criterios de aceptación ya es una spec pequeña. El curso de *Spec-Driven Development con OpenSpec* empieza donde este termina.
- **Mentoría en sprints:** disponible para acompañar la adopción en los proyectos reales.

---

## Troubleshooting

| Síntoma | Qué hacer |
|---|---|
| Repos desiguales en la pareja | Clonad el más completo; 5 min máximo |
| El agente ejecuta sin esperar el plan | Stop → Restore checkpoint → repetir con "no toques nada hasta que apruebe" |
| Tests en rojo tras implementar y "no sé por qué" | `#terminalLastCommand` + `#selection` → "explica por qué falla; no corrijas"; decidir quién está mal (código o test) |
| El 3 % da resultados distintos entre parejas | Normal: depende de la decisión del ADR. Lo que se evalúa es que esté documentada y testeada |
| Sin tiempo para la fase 5 | Recortad reglas 2 y 5, nunca la review. Un PR sin review no se aprueba |
| `@revisor` no aparece | Usad `/review-financiero` en modo Ask; mismo contenido |

---

## Para profundizar

- Microsoft Learn · [GitHub Copilot Fundamentals (ruta GH-300)](https://learn.microsoft.com/en-us/training/paths/copilot/)
- GitHub Docs · [Best practices for using GitHub Copilot](https://docs.github.com/en/copilot/using-github-copilot/best-practices-for-using-github-copilot)
- OpenSpec · [Spec-Driven Development](https://github.com/Fission-AI/OpenSpec)

**Gracias por estas ocho sesiones.**
