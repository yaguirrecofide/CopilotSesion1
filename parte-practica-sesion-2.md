# Parte práctica · Sesión 2 — Copilot Chat: contexto, participantes y variables

> **Duración:** 70 minutos · **Modalidad:** individual o en parejas · **Stack:** C# / .NET 10 en Visual Studio Code
> **Objetivo:** usar las tres superficies de chat, elegir entre Ask y Agent, y dar contexto explícito a Copilot para obtener mejores respuestas, revisándolas con criterio.

| Lab | Tema | Tiempo |
|---|---|---|
| 0 | Entorno listo | 5 min |
| 1 | Tres superficies: Chat, Inline Chat y Quick Chat | 10 min |
| 2 | Ask vs Agent: la misma petición en los dos modos | 15 min |
| 3 | Participantes: `@workspace`, `@vscode`, `@terminal` | 10 min |
| 4 | Variables de contexto: `#file`, `#selection`, `#codebase` | 15 min |
| 5 | Explicar y documentar código heredado | 10 min |
| 6 | Revisión responsable: qué no pegar en el chat | 5 min |

---

## Requisitos previos

- Sesión 1 completada: proyecto `CopilotSesion1` con `Calculadora.cs`, `ValidadorRuc.cs`, `Textos.cs`, `Poliza.cs`, `PolizaService.cs` y `ClienteRepository.cs`.
- Extensión **GitHub Copilot Chat** instalada (viene con *GitHub Copilot* en versiones recientes de VS Code).
- VS Code actualizado. Esta guía usa los nombres actuales de la UI; si tu versión muestra algo distinto, busca el equivalente y avisa al instructor.

> 💡 Como en la sesión 1, las respuestas del chat **no serán idénticas** a las de esta guía. Lo que importa es observar cómo cambia la respuesta cuando cambia el contexto.

### Atajos de referencia

| Acción | Windows / Linux | macOS |
|---|---|---|
| Abrir vista de Chat | `Ctrl + Alt + I` | `Ctrl + Cmd + I` |
| Inline Chat (en el editor) | `Ctrl + I` | `Cmd + I` |
| Quick Chat (ventana flotante) | `Ctrl + Shift + Alt + L` | `Cmd + Shift + Option + L` |
| Cambiar de modo (Ask / Agent) | `Ctrl + .` | `Cmd + .` |
| Nueva conversación | `Ctrl + L` (con el chat enfocado) | `Cmd + L` |
| Adjuntar contexto | `Ctrl + /` o botón 📎 *Add Context* | `Cmd + /` |

### Cheat sheet de contexto

| Prefijo | Qué hace | Ejemplo |
|---|---|---|
| `@workspace` | Participante: busca en todo el proyecto | `@workspace ¿dónde se valida el RUC?` |
| `@vscode` | Participante: sabe de VS Code y su configuración | `@vscode ¿cómo activo format on save?` |
| `@terminal` | Participante: ayuda con comandos de terminal | `@terminal ¿cómo listo los SDK de .NET instalados?` |
| `#file` | Variable: adjunta un archivo concreto | `#file:Poliza.cs` |
| `#selection` | Variable: lo que tienes seleccionado en el editor | `explica #selection` |
| `#codebase` | Variable: búsqueda semántica en el código del proyecto | `#codebase ¿qué clases usan SqlConnection?` |
| `#terminalLastCommand` | Variable: el último comando y su salida | `¿por qué falló #terminalLastCommand?` |
| `/explain` `/fix` `/tests` `/doc` | Comandos slash (los veremos a fondo en la sesión 3) | `/explain` |

---

## Lab 0 · Entorno listo (5 min)

1. Abre la carpeta `CopilotSesion1` en VS Code.
2. Si usas Git, crea una rama para no mezclar con la sesión 1:
   ```bash
   git checkout -b sesion-2
   ```
   Si no usas Git, copia la carpeta como `CopilotSesion2` y ábrela.
3. Abre la vista de Chat con `Ctrl + Alt + I`. Debería aparecer en la barra lateral secundaria (derecha). Si no aparece, haz clic en el icono de Copilot de la barra de título → *Open Chat*.
4. Abajo, en el cuadro de texto del chat, localiza el **selector de modo** (dice *Ask* o *Agent*) y el **selector de modelo**. Déjalo en **Ask**.
5. Comprueba que compila:
   ```bash
   dotnet build
   ```

✅ **Checkpoint:** vista de Chat abierta en modo Ask, proyecto compila.

> ℹ️ Si ves un modo llamado **Edit**, es una versión anterior de VS Code (se retiró en febrero de 2026). Para esta sesión usa **Ask** y **Agent**.

---

## Lab 1 · Tres superficies (10 min)

**Objetivo:** reconocer dónde vive el chat y cuándo usar cada uno.

1. **Vista de Chat.** Con el chat enfocado, escribe:
   ```
   ¿Qué diferencia hay entre record y class en C#? Responde en 3 líneas.
   ```
   Es una pregunta general: no necesita contexto de tu proyecto.
2. **Inline Chat.** Abre `Textos.cs`, coloca el cursor dentro de `Invertir` y pulsa `Ctrl + I`. Escribe:
   ```
   haz que devuelva string.Empty si texto es null
   ```
   Observa que la propuesta aparece **como diff dentro del editor**. Lee el cambio y pulsa **Accept** (o `Ctrl + Enter`). Si no te convence, **Discard**.
3. **Quick Chat.** Desde cualquier archivo pulsa `Ctrl + Shift + Alt + L` y escribe:
   ```
   ¿cómo se declara un DateOnly con la fecha de hoy?
   ```
   Es una consulta rápida que no quieres que ocupe la conversación principal. Cierra la ventana con `Esc`.
4. Anota en `notas.md` una línea por superficie: *cuándo la usarías*.

✅ **Checkpoint:** usaste las tres superficies y `Invertir` ahora controla `null`.

🧠 **Regla práctica:** pregunta general → Chat; cambio en el código que tienes delante → Inline Chat; duda de 10 segundos → Quick Chat.

---

## Lab 2 · Ask vs Agent (15 min)

**Objetivo:** entender qué puede y qué no puede hacer cada modo, y revisar antes de aceptar.

### Parte A — Ask

1. En la vista de Chat, modo **Ask**, escribe:
   ```
   Crea una clase CalculadoraPrima con un método que calcule la prima anual de una póliza: prima mensual × 12, con 5% de descuento si el producto es "SALUD". Usa el record Poliza del proyecto.
   ```
2. Observa: Copilot responde con un **bloque de código**, pero **no crea ningún archivo**. Fíjate si usó las propiedades reales (`PrimaMensual`, `Producto`) o inventó nombres. Probablemente inventó, porque no le diste `Poliza.cs` como contexto.
3. Pasa el ratón sobre el bloque de código: verás botones para **Insert at cursor**, **Insert into new file** y **Copy**. No los uses todavía.

### Parte B — Agent

4. Cambia a modo **Agent** (`Ctrl + .` o el selector de abajo). Pulsa `Ctrl + L` para iniciar una conversación nueva y escribe **exactamente la misma petición**.
5. Observa la diferencia:
   - Copilot **lee** `Poliza.cs` por su cuenta (verás la lista de archivos consultados).
   - **Crea** `CalculadoraPrima.cs` en el proyecto.
   - Es probable que proponga ejecutar `dotnet build`. Aparecerá un botón de **confirmación**: léelo antes de aprobar.
6. Antes de dar por bueno el resultado, abre `CalculadoraPrima.cs` y revisa como reviewer:
   - ¿El descuento se aplica sobre la prima anual o sobre la mensual? ¿Da lo mismo?
   - ¿Qué pasa si `PrimaMensual` es negativa? ¿Y si `Producto` viene en minúsculas?
   - ¿Agregó algo que no pediste (interfaces, pruebas, comentarios de más)?
7. En la vista de Chat aparecen los cambios pendientes con **Keep** / **Undo** por archivo. Si algo no te gusta, usa **Undo** y pide la corrección en el mismo hilo:
   ```
   compara el producto sin distinguir mayúsculas y lanza ArgumentException si la prima es negativa
   ```
8. Cuando estés conforme, **Keep** y compila:
   ```bash
   dotnet build
   ```

✅ **Checkpoint:** `CalculadoraPrima.cs` existe, compila, y anotaste al menos una cosa que corregiste o rechazaste.

🧠 **Reflexiona:** Agent hizo más trabajo, pero también tomó más decisiones por ti. ¿En qué tareas de tu proyecto real usarías Ask y en cuáles Agent?

---

## Lab 3 · Participantes (10 min)

**Objetivo:** dirigir la pregunta a la fuente de conocimiento correcta.

Vuelve a modo **Ask** y nueva conversación (`Ctrl + L`).

1. **Sin participante:**
   ```
   ¿Dónde se valida que un RUC tenga 11 dígitos?
   ```
   Fíjate en la respuesta: con un archivo cualquiera abierto, suele ser genérica o inventada.
2. **Con `@workspace`:**
   ```
   @workspace ¿Dónde se valida que un RUC tenga 11 dígitos?
   ```
   Ahora debería citar `ValidadorRuc.cs` y la línea concreta. Haz clic en la referencia: te lleva al archivo.
3. **`@workspace` para orientarte en un proyecto nuevo:**
   ```
   @workspace describe qué hace este proyecto y lista sus clases con una línea cada una
   ```
   Esta es la primera pregunta que harías al heredar un repositorio.
4. **`@vscode`:**
   ```
   @vscode ¿cómo hago que VS Code formatee el código al guardar?
   ```
   Debería darte el setting exacto (`editor.formatOnSave`) y, en muchos casos, un botón para abrir la configuración.
5. **`@terminal`:**
   ```
   @terminal ¿cómo veo los SDK de .NET instalados?
   ```
   Debería proponer `dotnet --list-sdks` con un botón para insertarlo en la terminal.

✅ **Checkpoint:** comparaste la pregunta 1 con y sin `@workspace`, y usaste los tres participantes.

> ℹ️ En modo **Agent** no hace falta `@workspace`: el agente ya busca en el proyecto por su cuenta. Los participantes son sobre todo una herramienta de **Ask**.

---

## Lab 4 · Variables de contexto (15 min)

**Objetivo:** adjuntar exactamente lo que Copilot necesita, ni más ni menos.

### Parte A — `#file`

1. Cierra todas las pestañas del editor. En modo Ask, nueva conversación, escribe:
   ```
   Escribe un método que devuelva cuántos días de vigencia le quedan a una póliza a partir de hoy.
   ```
   Como en el Lab 2, es probable que invente nombres de propiedades.
2. Repite adjuntando el archivo. Escribe `#file:` y elige `Poliza.cs` del desplegable (o arrastra el archivo al chat, o usa 📎 *Add Context*):
   ```
   #file:Poliza.cs Escribe un método que devuelva cuántos días de vigencia le quedan a una póliza a partir de hoy.
   ```
   Ahora debería usar `FinVigencia` y `DateOnly.FromDateTime(DateTime.Today)`.
3. Compara: ¿qué pasa si la póliza ya venció? ¿Devuelve negativo, cero o lanza excepción? Elige lo que prefieras y pídeselo.

### Parte B — `#selection`

4. Abre `PolizaService.cs`, **selecciona** el cuerpo del método `FiltrarVigentes` y escribe en el chat:
   ```
   #selection ¿Este método considera inclusive la fecha de fin de vigencia? Responde sí/no y explica.
   ```
   `#selection` adjunta solo lo seleccionado: más preciso y más barato en tokens que todo el archivo.

### Parte C — `#codebase`

5. Sin seleccionar nada, escribe:
   ```
   #codebase ¿Qué clases abren una conexión a SQL Server?
   ```
   `#codebase` hace búsqueda semántica en todo el proyecto. Debería encontrar `ClienteRepository`.
6. Ahora combina variables:
   ```
   #file:Poliza.cs #file:PolizaService.cs Propón un método en PolizaService que devuelva las pólizas que vencen en los próximos 30 días, usando el mismo estilo del método existente.
   ```
   Fíjate si respeta el estilo (LINQ, nombres en español) del método que ya existe.

### Parte D — `#terminalLastCommand`

7. En la terminal, provoca un error a propósito:
   ```bash
   dotnet run --project NoExiste.csproj
   ```
8. En el chat:
   ```
   ¿Por qué falló #terminalLastCommand?
   ```

✅ **Checkpoint:** usaste `#file`, `#selection`, `#codebase` y `#terminalLastCommand`, y explicaste con tus palabras cuándo usar cada uno.

🧠 **Clave:** `@workspace` deja que Copilot **busque**; `#file` y `#selection` le dicen **exactamente qué mirar**. Cuando sabes dónde está el problema, sé explícito.

---

## Lab 5 · Explicar y documentar código heredado (10 min)

**Objetivo:** usar el chat para entender código que no escribiste.

1. Crea `LiquidadorSiniestros.cs` con este código "heredado" (pégalo tal cual, con sus nombres crípticos):
   ```csharp
   public static class LiquidadorSiniestros
   {
       public static decimal Calc(decimal m, decimal d, decimal c, int t)
       {
           var r = m - d;
           if (r < 0) r = 0;
           r = r * (1 - c);
           if (t > 2) r = r * 0.9m;
           return Math.Round(r, 2);
       }
   }
   ```
2. Selecciona el método y pregunta en Inline Chat (`Ctrl + I`):
   ```
   /explain
   ```
   Lee la explicación. ¿Dedujo qué son `m`, `d`, `c` y `t`? (Pista: monto, deducible, coaseguro, tramo o número de siniestro.)
3. Ahora dale contexto y pide documentación. En la vista de Chat:
   ```
   #selection Este método liquida un siniestro: m = monto reclamado, d = deducible, c = porcentaje de coaseguro (0 a 1), t = número de siniestros previos del asegurado. Renombra los parámetros con nombres descriptivos y agrega documentación XML (/// <summary>) en español.
   ```
4. Revisa la propuesta e insértala. Compila.
5. Pregunta algo que el código **no** dice:
   ```
   #selection ¿Qué pasa si c es mayor que 1? ¿Y si es negativo? Propón validaciones.
   ```

✅ **Checkpoint:** `LiquidadorSiniestros` tiene nombres descriptivos y documentación XML, y detectaste al menos un caso borde no controlado.

🧠 **Reflexiona:** la explicación del paso 2 fue plausible pero, sin contexto, Copilot **adivinó** el significado de las variables. Una explicación convincente no es una explicación correcta.

---

## Lab 6 · Revisión responsable: qué no pegar en el chat (5 min)

**Objetivo:** aplicar **Privacidad y seguridad** en el uso del chat.

1. Lee este prompt y **no lo envíes**:
   ```
   #file:appsettings.json Mi conexión es "Server=prod-sql01;User Id=sa;Password=P@ss2026!" y este JSON de un cliente real {"documento":"45678912","nombre":"Juan Pérez","salario":8500}. ¿Por qué falla la consulta?
   ```
2. Con tu pareja, identifica qué **no** debería estar ahí y por qué:
   - Credenciales de producción.
   - Datos personales de un cliente real.
   - Un archivo de configuración con secretos adjuntado con `#file`.
3. Reescribe el prompt de forma segura en `notas.md`. Ejemplo de dirección: describe el error, pega el **mensaje de excepción** (sin cadena de conexión) y usa datos **ficticios**.
4. Recuerda lo visto en la sesión 1: en planes Business/Enterprise el administrador puede configurar **content exclusions** para que archivos como `appsettings.*.json` nunca se envíen como contexto. Pregúntate: ¿tu organización los tiene configurados? ¿Para qué archivos de tu proyecto los pedirías?

✅ **Checkpoint:** reescribiste el prompt sin secretos ni datos personales y nombraste dos archivos de tu proyecto real que deberían excluirse.

🧠 **Mensaje central:** el chat es cómodo y rápido; eso es exactamente lo que lo hace peligroso para secretos y datos de clientes. Describe el problema, no pegues el secreto.

---

## Checklist final

- [ ] Uso la vista de Chat, Inline Chat y Quick Chat, y sé cuándo conviene cada una.
- [ ] Distingo Ask (responde) de Agent (edita y ejecuta) y reviso el diff antes de **Keep**.
- [ ] Uso `@workspace`, `@vscode` y `@terminal` para dirigir la pregunta.
- [ ] Uso `#file`, `#selection`, `#codebase` y `#terminalLastCommand` para dar contexto exacto.
- [ ] Hago que Copilot me explique y documente código heredado, y verifico que la explicación sea correcta.
- [ ] No pego credenciales ni datos personales en el chat.

---

## Troubleshooting

| Síntoma | Causa probable | Qué hacer |
|---|---|---|
| No aparece la vista de Chat | Extensión *GitHub Copilot Chat* no instalada o deshabilitada | Extensions → busca *GitHub Copilot Chat* → instala/habilita; reinicia VS Code |
| No hay selector Ask / Agent | Versión antigua de VS Code o de la extensión | Actualiza VS Code (`Help → Check for Updates`) y la extensión |
| `@workspace` responde "no encuentro" | Proyecto aún no indexado | Espera a que termine el indexado (barra de estado); o usa `#codebase` |
| `#file:` no muestra el desplegable | Escribiste espacio después de `#file` | Escribe `#file:` sin espacio y elige del listado, o usa 📎 *Add Context* |
| Agent no ejecuta comandos | Confirmación pendiente o terminal bloqueada | Revisa el botón *Continue/Allow* en el chat; comprueba la terminal |
| Agent cambió más archivos de los esperados | Petición demasiado amplia | **Undo** los que sobren; vuelve a pedir acotando: "solo en X.cs" |
| Respuesta en inglés | Idioma por defecto del modelo | Escribe "responde en español" o configura `github.copilot.chat.localeOverride` |
| Otro problema | — | *View → Output* → canal **GitHub Copilot Chat** |

---

## Para profundizar

- VS Code Docs · [Use chat in VS Code](https://code.visualstudio.com/docs/copilot/chat/copilot-chat)
- VS Code Docs · [Chat modes](https://code.visualstudio.com/docs/copilot/chat/chat-modes)
- GitHub Docs · [Copilot Chat cheat sheet](https://docs.github.com/en/copilot/using-github-copilot/copilot-chat/github-copilot-chat-cheat-sheet?tool=vscode)
- Microsoft Learn · [Responsible AI with GitHub Copilot](https://learn.microsoft.com/en-us/training/modules/responsible-ai-with-github-copilot/)

**Próxima sesión:** prompt engineering y comandos slash (`/explain`, `/fix`, `/tests`, `/doc`).
