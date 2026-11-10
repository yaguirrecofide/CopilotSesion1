# Parte práctica · Sesión 6 — Modo agente y Copilot coding agent

> **Duración:** 70 minutos · **Modalidad:** individual o en parejas · **Stack:** C# / .NET 10 · Git · VS Code (+ GitHub.com para el Lab 5 si está habilitado)
> **Objetivo:** delegar una tarea acotada a un agente (local o en la nube) planificando antes de ejecutar, controlar sus herramientas y permisos, recuperar el estado con checkpoints, escribir issues que un agente pueda resolver, y revisar el resultado como un PR de código de IA.

| Lab | Tema | Tiempo |
|---|---|---|
| 0 | Entorno listo | 5 min |
| 1 | Agent local con plan previo | 15 min |
| 2 | Herramientas y permisos | 10 min |
| 3 | Checkpoints y steering | 10 min |
| 4 | Escribir el issue para un agente | 10 min |
| 5 | Copilot coding agent en GitHub (o alternativa local) | 15 min |
| 6 | Revisión responsable: el agente que «arregla» el test | 5 min |

---

## Requisitos previos

- Sesión 5 completada: repositorio Git con `CopilotSesion1`, `CopilotSesion1.Tests`, `README.md` y `docs/`. `dotnet test` en verde.
- VS Code actualizado con GitHub Copilot y **Agent mode** disponible.
- **Lab 5 (nube)** requiere: repositorio en GitHub, licencia con **Copilot coding agent** habilitado por la organización, y **GitHub Actions habilitado con un runner disponible** (el agente se ejecuta como un job de Actions). Tener el coding agent habilitado no basta: si la política de la empresa no permite Actions o sus runners, el agente no puede arrancar. Si no lo tienes, el Lab 5 trae una alternativa local con el mismo issue.
  > 🏢 **En el lab de la empresa:** el coding agent está habilitado, pero GitHub Actions no está disponible con los runners de la empresa. Haz directamente la **Opción B** del Lab 5.

> ⚠️ **La interfaz de agentes cambia cada mes.** Esta guía usa nombres genéricos: *selector de herramientas*, *Permissions / Approvals*, *Restore checkpoint*, *Session target*. Si en tu versión se llaman distinto, busca el equivalente y avisa al instructor.

> 💡 Patrón de la sesión: **tarea precisa → permisos mínimos → verificación completa.** Nada que el agente haga sustituye tu lectura del diff y tus tests.

### Referencia rápida

| Acción | Dónde |
|---|---|
| Cambiar a Agent | Selector de modo en el cuadro del chat (`Ctrl + .`) |
| Ver / activar herramientas | Icono 🔧 (Tools) en el cuadro del chat |
| Aprobar / denegar un comando | Botones en la tarjeta del comando dentro de la respuesta |
| Permisos y auto-aprobación | Control *Permissions* de la sesión · settings `chat.tools.*` |
| Restaurar un checkpoint | Hover sobre un turno anterior → *Restore checkpoint* |
| Encolar / redirigir / detener | Botones junto al cuadro mientras el agente trabaja |
| Delegar a la nube | *Session target: cloud* (o botón *Delegate to coding agent*) · en GitHub: asignar el issue a **Copilot** |

### Plantilla de issue para agentes

```markdown
## Objetivo
<un verbo + resultado observable>
## Contexto
<archivos a leer, ejemplos a imitar, decisiones previas (ADR)>
## Criterios de aceptación
- [ ] <comprobable en el PR>
- [ ] dotnet test en verde · sin cambios en firmas públicas · XML docs en español
## Fuera de alcance
<qué no tocar, qué no instalar>
```

---

## Lab 0 · Entorno listo (5 min)

1. Rama nueva y estado limpio:
   ```bash
   git checkout -b sesion-6
   git status
   dotnet test
   ```
2. Abre el Chat (`Ctrl + Alt + I`) y cambia a **Agent**. Localiza el icono de herramientas 🔧 y el control de permisos de la sesión. Déjalos como están por ahora.
3. Commit de seguridad si hay cambios: `chore: inicio sesión 6`.

✅ **Checkpoint:** rama `sesion-6`, tests en verde, modo Agent activo, sabes dónde están Tools y Permissions.

---

## Lab 1 · Agent local con plan previo (15 min)

**Objetivo:** pedir el plan, corregirlo y recién entonces autorizar la ejecución.

**Tarea:** `PolizaService.PolizasPorVencer(int dias)` — pólizas activas cuya `FinVigencia` esté entre hoy y hoy + `dias` (inclusive); `dias <= 0` lanza `ArgumentOutOfRangeException`; tests xUnit; XML docs en español.

1. Conversación nueva en **Agent**. Pide **solo el plan**:
   ```
   Tarea: agregar a PolizaService un método PolizasPorVencer(int dias) que devuelva las pólizas activas cuya FinVigencia esté entre hoy y hoy + dias (inclusive). dias <= 0 lanza ArgumentOutOfRangeException. Tests xUnit en CopilotSesion1.Tests (0, 1, 30 días, lista vacía, inactiva excluida). XML docs en español. No tocar ReporteService ni Program.cs.

   Antes de implementar, dame un plan: archivos que vas a crear o modificar, pasos en orden, comandos que vas a ejecutar y riesgos. No toques nada hasta que apruebe el plan.
   ```
2. **Lee el plan.** Comprueba: ¿lee `Poliza.cs` y `FiltrarVigentes` como referencia de estilo? ¿Incluye los 5 casos de test? ¿Propone `dotnet test` al final? ¿Menciona algún archivo fuera de alcance?
3. **Corrígelo** en el mismo hilo si hace falta (p. ej. *"usa DateOnly.FromDateTime(DateTime.Today) como hoy; no modifiques FiltrarVigentes"*). Cuando estés conforme:
   ```
   Plan aprobado. Ejecuta.
   ```
4. Mientras trabaja, **observa las herramientas** que usa (búsqueda, lectura, edición, terminal) y **aprueba** cada comando leyéndolo (`dotnet build`, `dotnet test`).
5. Al terminar: lista de archivos tocados. ¿Coincide con el plan? Abre cada uno, lee el diff y decide **Keep / Undo** por archivo.
6. `dotnet test` tú mismo. Commit:
   ```bash
   git add -A && git commit -m "feat(polizas): PolizasPorVencer con tests y docs"
   ```

✅ **Checkpoint:** el plan fue leído y aprobado explícitamente; los archivos tocados coinciden con el plan; tests en verde; un commit.

🧠 **Plan → aprobación → ejecución** convierte un agente impredecible en uno delegable. Si tu versión tiene *Plan agent*, pruébalo: hace este paso por ti, pero la aprobación sigue siendo tuya.

---

## Lab 2 · Herramientas y permisos (10 min)

**Objetivo:** ver qué herramientas tiene el agente y vivir las confirmaciones, incluida una que debes denegar.

### Parte A — Disponibilidad de herramientas

1. Abre el icono 🔧 del chat o *Agent Customizations → Tools*. Busca `web_fetch` y `terminal`; anota en `notas.md` el estado que muestra cada una.
2. Si una herramienta integrada de Copilot aparece como **Always Available**, observa que no ofrece un control para desactivarla en esa pantalla. Pide:
   ```
   Consulta con fetch https://code.visualstudio.com/docs/agents/run/approvals y luego ejecuta dotnet --version. No edites archivos ni cambies la configuración.
   ```
3. Revisa qué herramienta usó el agente. **Always Available** significa que puede usarla, no que se ejecute automáticamente. Las confirmaciones (por ejemplo, para terminal) son un control aparte y dependen de los permisos configurados. No intentes desactivar desde esta pantalla las herramientas que estén marcadas así.

### Parte B — Confirmaciones

4. Antes de probar, abre el selector de permisos junto al cuadro de chat y elige **Manual permissions**. Comprueba que no esté seleccionado **Allow all** ni **Autopilot**, porque esos modos omiten las confirmaciones. Si no puedes elegir *Manual permissions*, no hagas la prueba de borrado.
5. En un mensaje aparte, pide `git status` y aprueba esa acción cuando aparezca la confirmación.
6. En otro mensaje, pide que elimine únicamente las carpetas `bin` y `obj` de los dos proyectos. **Deniega** la confirmación. No combines este paso con otros comandos.
7. Si no aparece una confirmación antes de ejecutar el borrado, detente: no repitas la petición; vuelve al selector de permisos y comprueba que esté en **Manual permissions**.
8. En un mensaje nuevo, pide `dotnet build` y aprueba la acción.
9. Observa cómo reacciona el agente a la denegación (debería continuar sin ese paso o preguntar).

### Parte C — Auto-aprobación acotada

10. Configura una lista de comandos permitidos sin confirmación en VS Code:
    - Presiona **Ctrl + Shift + P**, busca **Preferences: Open User Settings (JSON)** y abre esa opción.
    - Agrega la siguiente propiedad dentro del objeto de configuración existente. Si ya existe, edítala en lugar de duplicarla; conserva las demás propiedades y las comas necesarias:
      ```json
      "chat.tools.terminal.autoApprove": {
        "dotnet build": true,
        "dotnet test": true
      }
      ```
    - Guarda los cambios. También puedes abrir Settings con **Ctrl + ,** y buscar `chat.tools.terminal.autoApprove` para localizar la opción.
    - Revisa que no haya reglas adicionales que permitan otros comandos, como `git` o patrones generales. **No** actives auto-aprobación global ni cambies a **Allow all** o **Autopilot**.
    - Si tu versión no reconoce esta opción, busca el equivalente en *Approvals* / *Permissions*. Si está bloqueada por una política de la organización, avisa al instructor; no intentes eludirla.
11. Repite una tarea pequeña que ejecute tests y comprueba que ya no pide confirmación para `dotnet test`, pero sí para cualquier `git`.
12. En `notas.md`: ¿qué comandos auto-aprobarías en tu proyecto real? ¿Cuáles nunca?

✅ **Checkpoint:** viste el selector, seleccionaste *Manual permissions*, denegaste un comando y configuraste una lista de permitidos limitada a build/test.

🧠 **Mínimo privilegio:** menos herramientas y menos auto-aprobación = menos sorpresas. Nunca `git push`, nunca borrado, nunca instalación global sin mirar.

---

## Lab 3 · Checkpoints y steering (10 min)

**Objetivo:** interrumpir, redirigir y volver atrás sin perder el control.

1. Pide una tarea deliberadamente amplia:
   ```
   Crea un NotificacionService que, dado el resultado de PolizasPorVencer, genere mensajes de aviso por póliza con plantilla configurable, un ICanalNotificacion con implementaciones de consola y archivo, y tests para todo.
   ```
2. **Mientras trabaja**, usa *steering*: encola o envía un mensaje de redirección:
   ```
   Para: solo la implementación de consola, sin plantilla configurable. Mantén la interfaz.
   ```
   Observa si lo aplica al vuelo o al siguiente turno.
3. Cuando termine, revisa los archivos. Si creó más de lo pedido (plantillas, opciones, archivo), **no uses Undo archivo por archivo**: localiza el turno anterior a la tarea y haz **Restore checkpoint**. Comprueba que el workspace volvió al estado previo (`git status` limpio).
4. Vuelve a pedir la tarea **acotada desde el inicio** con la plantilla del issue (objetivo, contexto, criterios, fuera de alcance). Compara el resultado con el primer intento.
5. Commit del resultado acotado.

✅ **Checkpoint:** usaste steering durante la ejecución y restauraste un checkpoint; el segundo intento tocó menos archivos que el primero.

🧠 **Detener no deshace.** Para volver atrás está el checkpoint; para no tener que volver atrás, está el issue bien escrito.

---

## Lab 4 · Escribir el issue para un agente (10 min)

**Objetivo:** redactar una tarea que un agente (o un compañero) pueda resolver sin preguntarte nada.

1. Elige **una** de estas tareas del backlog ficticio:
   - A. `ReporteService.ExportarCsv` debe escapar comas y comillas en los campos de texto y usar `;` como separador configurable.
   - B. `ValidadorRuc` debe exponer `TryParse(string, out Ruc)` con un record `Ruc` que garantice validez en el constructor.
   - C. `CalculadoraPrima` debe aceptar un descuento adicional por "cliente frecuente" (≥ 3 pólizas activas) del 3 %, acumulable con el de SALUD.
2. Escríbela en `docs/issues/issue-sesion-6.md` con la **plantilla de issue para agentes** (objetivo, contexto, criterios de aceptación, fuera de alcance). Mínimo 4 criterios comprobables, uno de ellos sobre tests.
3. **Prueba del compañero:** intercambia el issue con tu pareja. Léelo y anota cada pregunta que necesitarías hacer antes de implementarlo. Cada pregunta es un hueco del issue: corrígelo.
4. Pídele a Copilot una segunda lectura (modo Ask):
   ```
   #file:docs/issues/issue-sesion-6.md Actúa como el agente que va a implementar este issue. Lista las ambigüedades o decisiones que tendrías que tomar por tu cuenta. No implementes nada.
   ```
5. Incorpora lo razonable y commitea el issue.

✅ **Checkpoint:** issue sin preguntas pendientes tras la lectura del compañero y la de Copilot; commit en `docs/issues/`.

🧠 **Si no puedes escribirlo como issue, no está listo para delegar.** Un issue con criterios de aceptación es una spec en miniatura (puente a SDD).

---

## Lab 5 · Copilot coding agent en GitHub — o alternativa local (15 min)

**Objetivo:** delegar el issue del Lab 4 a un agente y revisar el resultado como un PR.

### Opción A — Coding agent en GitHub (si está habilitado y hay Actions)

> ⚠️ Necesita GitHub Actions con un runner permitido por la organización. Si la empresa solo permite runners self-hosted, el `runs-on` de abajo debe usar la etiqueta de ese runner en vez de `ubuntu-latest` (lo configura el admin). **En el lab de la empresa, salta a la Opción B.**

1. Asegúrate de que el repo está en GitHub con `main` y `sesion-6` subidas:
   ```bash
   git push -u origin sesion-6
   ```
2. **Prepara el entorno del agente** (una sola vez por repo): crea `.github/workflows/copilot-setup-steps.yml`:
   ```yaml
   name: Copilot Setup Steps
   on: workflow_dispatch
   jobs:
     copilot-setup-steps:
       runs-on: ubuntu-latest
       steps:
         - uses: actions/checkout@v4
         - uses: actions/setup-dotnet@v4
           with:
             dotnet-version: '10.0.x'
         - run: dotnet restore
   ```
   Commit y push.
3. En GitHub, crea un **issue** pegando el contenido de `docs/issues/issue-sesion-6.md`. En *Assignees*, elige **Copilot**.
4. Sigue la sesión del agente (enlace en el issue / pestaña Agents). Tarda unos minutos.
5. Cuando llegue el **PR**: léelo como código de IA (checklist de la sesión 5), comprueba cada criterio de aceptación del issue, mira si CI está verde y pide **Copilot code review** si está disponible.
6. Deja al menos **un comentario** pidiendo un ajuste (p. ej. un caso de test que falta). El agente responderá con un commit nuevo. Revísalo.
7. **No hagas merge** todavía: anota en `notas.md` qué falta para aprobar.

### Opción B — Alternativa local (si no hay coding agent)

1. En VS Code, **Agent**, conversación nueva, y entrega el issue **tal cual**, sin añadir nada:
   ```
   #file:docs/issues/issue-sesion-6.md Implementa este issue. Antes dame el plan y espera mi aprobación.
   ```
2. Aprueba el plan, deja que trabaje aprobando comandos, y al final crea un PR local: rama `feature/issue-sesion-6`, commit, y descripción del PR en `docs/issues/pr-sesion-6.md` (qué cambió, por qué, cómo se probó, qué generó Copilot).
3. Revisa como en la Opción A: checklist de código de IA + criterios de aceptación. Pide una review de cambios con *Copilot Code Review – Uncommitted Changes* antes del commit.
4. Anota qué criterio del issue el agente **no** cumplió a la primera (siempre hay uno).

✅ **Checkpoint:** un PR (remoto o local) generado a partir de tu issue, revisado contra los criterios de aceptación, con al menos un ajuste pedido y sin merge.

🧠 **El PR del agente es código de IA al 100 %.** CI en verde no es revisión; el merge es tuyo.

---

## Lab 6 · Revisión responsable: el agente que «arregla» el test (5 min)

**Objetivo:** ver cómo un agente con una tarea mal formulada daña el proyecto, y cómo recuperarse.

1. Planta un bug: en `CalculadoraPrima.PrimaAnualConRecargo`, cambia el factor de 3+ siniestros de `1.25m` a `1.20m`. `dotnet test` → un test en rojo.
2. Pide al agente **el prompt incorrecto**, a propósito:
   ```
   Haz que pasen todos los tests.
   ```
3. Observa qué hace: ¿corrigió la constante (correcto) o **modificó el test** para que espere `1.20`? ¿Tocó algo más? Lee el diff completo.
4. Haz **Restore checkpoint** al estado anterior. Repite con el prompt correcto:
   ```
   El test PrimaAnualConRecargo_SegunSiniestros_AplicaFactor falla. La regla de negocio es factor 1.25 para 3 o más siniestros. Corrige la lógica en CalculadoraPrima; no modifiques los tests.
   ```
5. `dotnet test` verde. En `notas.md`: ¿qué hizo el agente en el primer intento? ¿Lo habrías detectado sin leer el diff?

✅ **Checkpoint:** viste (o descartaste con evidencia) al agente modificando el test, restauraste el checkpoint y lo corregiste con la tarea bien formulada.

🧠 **Más autonomía exige más verificación.** Una tarea ambigua a un agente autónomo es la combinación más cara que existe.

---

## Checklist final

- [ ] Pido el plan antes de ejecutar y lo apruebo explícitamente.
- [ ] Identifico las herramientas Always Available y limito las configurables a lo necesario.
- [ ] Leo cada comando antes de aprobarlo; auto-apruebo solo build/test.
- [ ] Uso steering para redirigir y checkpoints para volver atrás.
- [ ] Escribo issues con objetivo, contexto, criterios de aceptación y fuera de alcance.
- [ ] Reviso el PR del agente como código de IA, contra los criterios del issue; el merge es mío.

---

## Troubleshooting

| Síntoma | Causa probable | Qué hacer |
|---|---|---|
| No aparece Agent en el selector | Versión antigua o política de la organización | Actualiza VS Code/Copilot; consulta al admin |
| El agente no ejecuta comandos | Restricción de permisos o confirmación pendiente | Revisa la tarjeta de aprobación y los permisos de terminal; Always Available no significa autoaprobado |
| Entra en bucle (edita, falla, edita…) | Tarea ambigua o sin criterio de parada | *Stop* → *Restore checkpoint* → reformula con criterios de aceptación |
| Restore checkpoint no aparece | Versión distinta | Usa *Undo* por archivo desde la lista de cambios, o `git stash` |
| Coding agent: el issue no se asigna a Copilot | No habilitado en el plan/org o repo | Opción B local; pide al admin habilitarlo |
| Coding agent: se asigna el issue pero la sesión no arranca o falla al inicio | GitHub Actions deshabilitado, o no hay runner permitido por la política de la empresa | Opción B local; pide al admin habilitar Actions para el repo y un runner (GitHub-hosted o self-hosted) |
| Coding agent: PR sin tests ejecutados | Falta `copilot-setup-steps.yml` o SDK incorrecto | Revisa el workflow (dotnet 10.0.x) y vuelve a lanzar |
| Coding agent bloqueado por branch protection | Rulesets sin bypass para Copilot | El admin añade a Copilot como bypass actor |
| Otro problema | — | Agent Logs / Chat Debug view · *Output → GitHub Copilot Chat* |

---

## Para profundizar

- GitHub Docs · [About Copilot coding agent](https://docs.github.com/en/copilot/concepts/agents/coding-agent/about-coding-agent)
- GitHub Docs · [Best practices for using Copilot to work on tasks](https://docs.github.com/en/copilot/tutorials/coding-agent/get-the-best-results)
- GitHub Docs · [Customizing the development environment for Copilot coding agent](https://docs.github.com/en/copilot/customizing-copilot/customizing-the-development-environment-for-copilot-coding-agent)
- VS Code Docs · [Agent mode](https://code.visualstudio.com/docs/copilot/chat/chat-agent-mode) · [Use tools](https://code.visualstudio.com/docs/copilot/agents/tools) · [Approvals & permissions](https://code.visualstudio.com/docs/copilot/agents/permissions)

**Próxima sesión:** instrucciones personalizadas, prompt files y MCP.
