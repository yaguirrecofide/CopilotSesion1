# Parte práctica · Sesión 1 — GitHub Copilot: instalación y code completions

> **Duración:** 70 minutos · **Modalidad:** individual o en parejas · **Stack:** C# / .NET 10 en Visual Studio Code
> **Objetivo:** salir con Copilot instalado, funcionando y dominando las sugerencias en línea (completions), revisándolas con criterio de IA responsable.

| Lab | Tema | Tiempo |
|---|---|---|
| 0 | Entorno listo | 10 min |
| 1 | Primeras sugerencias: aceptar, rechazar, aceptar por palabra | 10 min |
| 2 | Comentarios a código | 10 min |
| 3 | Múltiples sugerencias y panel de alternativas | 10 min |
| 4 | El poder del contexto | 10 min |
| 5 | Next Edit Suggestions (NES) | 5 min |
| 6 | Configuración por lenguaje | 5 min |
| 7 | Revisión responsable de una sugerencia | 10 min |

---

## Requisitos previos

- Cuenta de GitHub con acceso a Copilot (licencia de la organización **Business/Enterprise**, o plan **Free/Pro** personal).
- **Visual Studio Code** actualizado a la última versión estable.
- **.NET 10 SDK** instalado: `dotnet --version` debe mostrar `10.x`. Descarga: <https://dotnet.microsoft.com/download/dotnet/10.0>
- Acceso a internet. En red corporativa: proxy y certificado configurados en VS Code (ver *Troubleshooting*).

> 💡 Copilot es probabilístico: tus sugerencias **no serán idénticas** a las de esta guía ni a las de tu compañero. Eso es normal. Lo importante es el procedimiento y la revisión.

> ℹ️ Todo el curso se hace en **Visual Studio Code**. Copilot también existe en Visual Studio, Rider u otros IDEs, pero los labs, atajos y capturas de esta guía son de VS Code; usa VS Code durante las sesiones aunque en tu proyecto trabajes con otro IDE.

### Atajos de referencia

| Acción | Windows / Linux | macOS |
|---|---|---|
| Aceptar sugerencia | `Tab` | `Tab` |
| Rechazar sugerencia | `Esc` | `Esc` |
| Aceptar siguiente palabra | `Ctrl + →` | `Cmd + →` |
| Siguiente / anterior sugerencia | `Alt + ]` / `Alt + [` | `Option + ]` / `Option + [` |
| Disparar sugerencia manualmente | `Alt + \` | `Option + \` |
| Panel con varias sugerencias | `Ctrl + Enter` | `Ctrl + Enter` |
| Inline chat | `Ctrl + I` | `Cmd + I` |

---

## Lab 0 · Entorno listo (10 min)

1. **Activa tu licencia.** Si tu organización te asignó Copilot, entra en <https://github.com/settings/copilot> y acepta la invitación si aparece. Si no tienes licencia, activa **Copilot Free** desde esa misma página.
2. **Instala las extensiones.** En VS Code abre Extensions (`Ctrl+Shift+X`) e instala:
   - **GitHub Copilot** (si aparecen *GitHub Copilot* y *GitHub Copilot Chat* por separado, instala ambas).
   - **C# Dev Kit** (instala también la extensión *C#* como dependencia).
3. **Inicia sesión.** Haz clic en el icono de Copilot de la barra de estado (abajo a la derecha) o en *Accounts* → **Sign in with GitHub** y autoriza en el navegador.
4. **Crea el proyecto de la parte práctica:**
   ```bash
   dotnet new console -n CopilotSesion1
   cd CopilotSesion1
   code .
   ```
   Al abrir, acepta cuando C# Dev Kit pregunte si quieres cargar la solución / restaurar.
5. **Verifica.** Abre `Program.cs`, borra su contenido y escribe `Console.WriteLine(` — deberías ver texto gris sugerido (ghost text). Acepta con `Tab` y ejecuta:
   ```bash
   dotnet run
   ```

✅ **Checkpoint:** icono de Copilot activo en la barra de estado, ghost text en `Program.cs` y `dotnet run` imprime algo.

❌ ¿No aparece nada? Ve a la sección [Troubleshooting](#troubleshooting) antes de continuar.

> Durante la parte práctica cada lab agrega un archivo `.cs` al proyecto y lo llamas desde `Program.cs`. Puedes dejar en `Program.cs` solo las llamadas del lab en curso.

---

## Lab 1 · Primeras sugerencias (10 min)

**Objetivo:** practicar aceptar, rechazar y aceptar parcialmente.

1. Crea `Calculadora.cs` con este contenido:
   ```csharp
   public static class Calculadora
   {
       public static int Sumar(int a, int b) => a + b;

       public static int Restar(
   ```
2. Espera la sugerencia tras `Restar(` y pulsa **`Tab`** para aceptarla completa.
3. Debajo escribe `public static int Multiplicar(` y esta vez pulsa **`Esc`** para rechazarla. Vuelve a dispararla con **`Alt + \`**.
4. Escribe `public static decimal Dividir(` y acepta **palabra por palabra** con **`Ctrl + →`** hasta la firma. Termina el cuerpo tú mismo y decide qué pasa si `b == 0`.
5. En `Program.cs` imprime los cuatro resultados y ejecuta:
   ```csharp
   Console.WriteLine(Calculadora.Sumar(7, 3));
   Console.WriteLine(Calculadora.Restar(7, 3));
   Console.WriteLine(Calculadora.Multiplicar(7, 3));
   Console.WriteLine(Calculadora.Dividir(7, 0));
   ```
   ```bash
   dotnet run
   ```

✅ **Checkpoint:** el programa compila y ejecuta. Usaste `Tab`, `Esc`, `Alt + \` y `Ctrl + →`.

🧠 **Reflexiona:** ¿la sugerencia de `Dividir` controló la división entre cero (`DivideByZeroException`)? Si no, ¿lo habrías detectado sin leerla?

---

## Lab 2 · Comentarios a código (10 min)

**Objetivo:** usar comentarios en lenguaje natural para generar implementaciones.

1. Crea `ValidadorRuc.cs`:
   ```csharp
   public static class ValidadorRuc
   {
       // Valida un RUC peruano:
       // - no debe ser null ni vacío
       // - debe tener exactamente 11 dígitos numéricos
       // - debe empezar con "10" (persona natural) o "20" (persona jurídica)
       // Devuelve true si cumple todas las reglas.
       public static bool EsValido(string? ruc)
       {
   ```
2. Acepta la sugerencia con `Tab`. Si solo sugiere una línea, sigue pulsando `Enter` y aceptando.
3. Debajo, escribe solo el comentario y deja que Copilot proponga la firma:
   ```csharp
       // Devuelve "PERSONA NATURAL", "PERSONA JURIDICA" o "DESCONOCIDO" según el prefijo del RUC
   ```
4. En `Program.cs` agrega este comentario y deja que Copilot genere los casos:
   ```csharp
   // Probar ValidadorRuc.EsValido con: null, "123", "20123456789", "10987654321", "30123456789", "2012345678A"
   ```
5. Ejecuta `dotnet run` y verifica a mano que cada resultado sea correcto.

✅ **Checkpoint:** los seis casos se imprimen con el resultado esperado (`False, False, True, True, False, False`).

🧠 **Prueba:** cambia el comentario a uno vago (`// valida ruc`) en un método nuevo. Compara la calidad de la sugerencia con la anterior.

---

## Lab 3 · Múltiples sugerencias (10 min)

**Objetivo:** explorar alternativas en lugar de quedarte con la primera.

1. Crea `Textos.cs`:
   ```csharp
   public static class Textos
   {
       // Invierte una cadena de texto
       public static string Invertir(string texto)
       {
   ```
2. Cuando aparezca el ghost text, **no lo aceptes**. Pulsa **`Alt + ]`** varias veces para ver las alternativas y **`Alt + [`** para volver. También puedes pasar el ratón sobre el ghost text y usar las flechas de la barra flotante.
3. Ahora pulsa **`Ctrl + Enter`** para abrir el **panel de sugerencias** con varias propuestas a la vez.
4. Elige la que te parezca más legible y acéptala desde el panel.
5. Repite con:
   ```csharp
       // Cuenta cuántas vocales tiene un texto, sin distinguir mayúsculas ni tildes
       public static int ContarVocales(string texto)
       {
   ```
6. Compara al menos dos alternativas: ¿alguna maneja `á, é, í, ó, ú`? ¿Alguna falla con `null`? ¿Alguna usa LINQ y otra un bucle?

✅ **Checkpoint:** viste al menos 3 alternativas para un mismo método y elegiste una con un criterio explícito (legibilidad, rendimiento, casos borde).

---

## Lab 4 · El poder del contexto (10 min)

**Objetivo:** comprobar que los archivos abiertos influyen en las sugerencias.

1. Crea `Poliza.cs`:
   ```csharp
   public record Poliza(
       string Numero,
       string Producto,          // "SALUD", "VIDA", "VEHICULAR"
       decimal PrimaMensual,
       DateOnly InicioVigencia,
       DateOnly FinVigencia,
       bool Activa);
   ```
2. **Cierra** `Poliza.cs` (y cualquier otra pestaña). Crea `PolizaService.cs` y escribe:
   ```csharp
   public class PolizaService
   {
       // Devuelve las pólizas activas y vigentes en la fecha indicada
       public List<Poliza> FiltrarVigentes(List<Poliza> polizas, DateOnly fecha)
       {
   ```
   Observa la sugerencia y **anótala**, sin aceptarla todavía. ¿Usa las propiedades reales del record o inventa nombres (`IsActive`, `StartDate`…)?
3. Pulsa `Esc`, **abre `Poliza.cs` en una pestaña junto a** `PolizaService.cs` y vuelve a disparar la sugerencia con `Alt + \`.
4. Compara: con el record abierto, Copilot debería usar `Activa`, `InicioVigencia` y `FinVigencia` correctamente.
5. Acepta la mejor versión y añade un segundo método solo con comentario:
   ```csharp
       // Suma la prima mensual de las pólizas activas agrupadas por producto
   ```
   (Es probable que proponga LINQ con `GroupBy` y `ToDictionary`.)

✅ **Checkpoint:** explicaste con tus palabras por qué cambió la sugerencia al abrir el archivo.

🧠 **Clave:** nombres descriptivos + archivos relacionados abiertos + comentarios claros = mejores sugerencias.

---

## Lab 5 · Next Edit Suggestions (5 min)

**Objetivo:** ver cómo Copilot predice la *siguiente edición*, no solo la siguiente línea.

1. Verifica que NES esté activo: `Ctrl + ,` → busca `nextEditSuggestions` → marca **GitHub › Copilot › Next Edit Suggestions: Enabled**.
2. En `Poliza.cs`, renombra la propiedad `PrimaMensual` a `PrimaMensualSoles` **editando el texto a mano** (no uses *Rename Symbol*, queremos ver a Copilot trabajar).
3. Vuelve a `PolizaService.cs` y coloca el cursor cerca de donde se usaba `PrimaMensual`. Copilot debería mostrar una flecha o resaltado en el margen proponiendo la edición. Pulsa **`Tab`** para saltar a ella y otra vez **`Tab`** para aplicarla.
4. Compila con `dotnet build` para confirmar que no quedó ninguna referencia rota.

✅ **Checkpoint:** aplicaste al menos una edición sugerida fuera de la posición del cursor.

> Si tu versión de VS Code no muestra NES, sáltalo y continúa: es una función que GitHub sigue evolucionando.

---

## Lab 6 · Configuración por lenguaje (5 min)

**Objetivo:** controlar dónde sugiere Copilot.

1. Crea `notas.md` y escribe un par de líneas: verás sugerencias en Markdown.
2. Haz clic en el **icono de Copilot** en la barra de estado y desactiva las completions **solo para Markdown**.
3. Abre la configuración JSON (`Ctrl + Shift + P` → *Preferences: Open User Settings (JSON)*) y comprueba que se agregó algo como:
   ```json
   "github.copilot.enable": {
     "*": true,
     "markdown": false,
     "plaintext": false
   }
   ```
4. Vuelve a `notas.md` y confirma que ya no hay ghost text; en los `.cs` sigue funcionando.
5. (Opcional) Vuelve a activarlo si lo prefieres.

✅ **Checkpoint:** sabes activar y desactivar Copilot por lenguaje desde la UI y desde `settings.json`.

> 🔒 **Para saber:** en planes Business/Enterprise el administrador puede definir **content exclusions** (archivos o repos que Copilot no usa como contexto, p. ej. `appsettings.*.json` con cadenas de conexión) y la política de **sugerencias que coinciden con código público**. Esas opciones no se cambian desde tu VS Code.

---

## Lab 7 · Revisión responsable (10 min)

**Objetivo:** aplicar los principios de IA responsable: *tú eres responsable del código que aceptas.*

1. Agrega el paquete de SQL Server al proyecto (solo para compilar; no hace falta una base de datos real):
   ```bash
   dotnet add package Microsoft.Data.SqlClient
   ```
2. Crea `ClienteRepository.cs`:
   ```csharp
   using Microsoft.Data.SqlClient;

   public class ClienteRepository
   {
       private readonly string _connectionString;

       public ClienteRepository(string connectionString) => _connectionString = connectionString;

       // Busca un cliente por su número de documento recibido desde la pantalla de búsqueda
       public string? BuscarNombrePorDocumento(string documento)
       {
           using var conn = new SqlConnection(_connectionString);
           conn.Open();
           var sql = "SELECT Nombre FROM Clientes WHERE Documento = '" +
   ```
3. Observa la sugerencia. Es muy probable que termine la **concatenación** de `documento` dentro del SQL, porque el código ya empezó así. Acéptala.
4. **Revisa como lo haría un code reviewer** y responde en un comentario en el archivo:
   - ¿Qué pasa si `documento` vale `' OR '1'='1`? (inyección SQL)
   - ¿Se liberan los recursos (`SqlCommand`, `SqlDataReader`) con `using`?
   - ¿Hay credenciales o datos sensibles en el código?
5. **Corrige** el método. Borra el cuerpo y guía a Copilot con un mejor contexto:
   ```csharp
       // Busca un cliente por documento usando SqlCommand con parámetros (@documento) para evitar inyección SQL
       public string? BuscarNombrePorDocumentoSeguro(string documento)
       {
   ```
6. Compila con `dotnet build`. Compara ambas versiones y comenta con tu pareja qué principio de IA responsable aplicaste (pista: **Fiabilidad y seguridad**, **Privacidad y seguridad**, **Responsabilidad**).

✅ **Checkpoint:** tu versión final usa `cmd.Parameters.AddWithValue("@documento", documento)` (o `Add` con tipo explícito) y explicas por qué la primera sugerencia era un riesgo.

🧠 **Mensaje central:** Copilot imita el contexto que le das. Si el código alrededor tiene malas prácticas, las reproducirá. La revisión humana no es opcional.

---

## Checklist final

- [ ] Copilot instalado, con sesión iniciada y licencia activa.
- [ ] Uso con soltura `Tab`, `Esc`, `Ctrl + →`, `Alt + ]` / `Alt + [`, `Alt + \` y `Ctrl + Enter`.
- [ ] Genero código a partir de comentarios claros y específicos.
- [ ] Entiendo cómo los archivos abiertos y los nombres influyen en las sugerencias.
- [ ] Sé activar y desactivar Copilot por lenguaje.
- [ ] Reviso cada sugerencia antes de aceptarla (seguridad, casos borde, recursos).

---

## Troubleshooting

| Síntoma | Causa probable | Qué hacer |
|---|---|---|
| Icono de Copilot tachado o con alerta | Sesión no iniciada o sin licencia | Cierra sesión y vuelve a entrar; revisa <https://github.com/settings/copilot> |
| No hay ghost text en un lenguaje | Deshabilitado para ese lenguaje | Icono de la barra de estado → habilitar; revisa `github.copilot.enable` |
| Errores de conexión o certificado | Proxy o SSL corporativo | Configura `http.proxy` y `http.proxyStrictSSL` según la política de TI; prueba fuera de VPN |
| Un archivo concreto nunca sugiere | Content exclusion de la organización | Consulta al administrador; es el comportamiento esperado |
| Sugerencias pobres o irrelevantes | Poco contexto | Abre archivos relacionados, mejora nombres y comentarios |
| `dotnet` no se reconoce o versión < 10 | SDK no instalado o PATH | Instala .NET 10 SDK y reinicia VS Code; `dotnet --list-sdks` |
| IntelliSense de C# no carga | C# Dev Kit sin solución | `Ctrl+Shift+P` → *.NET: Open Solution* o reabre la carpeta del proyecto |
| Otro problema | — | *View → Output* → canal **GitHub Copilot**; comando *GitHub Copilot: Collect Diagnostics* |

---

## Para profundizar

- Microsoft Learn · [Responsible AI with GitHub Copilot](https://learn.microsoft.com/en-us/training/modules/responsible-ai-with-github-copilot/)
- Microsoft Learn · [Introduction to GitHub Copilot](https://learn.microsoft.com/en-us/training/modules/introduction-to-github-copilot/)
- GitHub Docs · [GitHub Copilot](https://docs.github.com/en/copilot)
- Planes y precios · <https://github.com/features/copilot/plans>

**Próxima sesión:** Copilot Chat y prompt engineering.
