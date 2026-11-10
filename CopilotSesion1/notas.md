Regla: devuelve el RUC con guion antes del último dígito; recorta espacios; ArgumentException si no es válido.
| Entrada          | Esperado          |
| "20123456789"    | "2012345678-9"    |
| "10987654321"    | "1098765432-1"    |
| " 20123456789 "  | "2012345678-9"    |
| "123"            | ArgumentException |
| null             | ArgumentException |

---

## Code Review Senior: ReporteService.cs
**Contexto:** Entidad Financiera - Análisis de Seguridad, Correctitud, Recursos, Diseño y Cobertura de Pruebas

| Línea | Hallazgo | Categoría | Severidad | Fix Recomendado |
|-------|----------|-----------|-----------|-----------------|
| 10-12 | Constructor sin parámetros introduce acoplamiento débil. Si PolizaService fue removido, constructor está correcto. Revisar si hubo intención de reutilizar. | Diseño | MEDIA | Documentar decisión: ReporteService es stateless. Si requiere estado futuro, considerar factory pattern. |
| 24 | `polizas == null` retorna `[]` sin validación de negocio. Oculta errores de callers que pasan null inadecuadamente. | Correctitud | MEDIA | Cambiar a: `ArgumentNullException.ThrowIfNull(polizas)`. Retornar `[]` solo si lista vacía pero no nula. |
| 32-50 | Flujo de filtrado secuencial crea lista intermedia dos veces. Ineficiente en grandes volúmenes. | Recursos | BAJA | Combinar filtros: `.Where(p => (!inicioVigencia.HasValue \|\| p.InicioVigencia >= inicioVigencia.Value) && ...)`. |
| 48-50 | Agrupación por `Producto` no valida valores vacíos/nulos. Si Poliza.Producto = null, causará NullReferenceException. | Correctitud | ALTA | Validar en Poliza record o aquí: `.Where(p => !string.IsNullOrEmpty(p.Producto))` antes de GroupBy. |
| 63-70 | Validación de ruta solo verifica no-nulo/no-espacios. NO valida path traversal (`../../../etc/passwd`) o caracteres inválidos en Windows. | Seguridad | ALTA | Agregar: `try { Path.GetFullPath(ruta); } catch { throw new ArgumentException("Ruta inválida"); }` para detener traversal. |
| 63-70 | StreamWriter en ExportarCsv no tiene `Flush()` explícito antes de cerrar. `using` lo hace implícito pero bajo estrés puede perder datos. | Recursos | BAJA | Agregar `writer.Flush()` antes de bloque finally o dejar `using` (aceptable). |
| 74 | Encabezados hardcoded en CSV. Si Poliza agrega propiedades, CSV se desincroniza silenciosamente. | Correctitud | MEDIA | Usar reflection o generar dinámicamente: `typeof(Poliza).GetProperties()`. |
| 75-85 | Formato de Prima (`{poliza.PrimaMensualSoles}`) sin especificador de precisión decimal. Puede exportar "150.5" o "150.50" inconsistentemente según CultureInfo. | Correctitud | MEDIA | Especificar: `{poliza.PrimaMensualSoles:F2}` para garantizar 2 decimales. |
| 89-93 | Catches específicos IOException/UnauthorizedAccessException, pero otros errores (OutOfMemoryException, StackOverflowException) se propagan sin contexto. | Correctitud | BAJA | Aceptable: no capturar OOM/SOE. Agregar logs si existe framework de logging. |
| 98-116 | `File.WriteAllText()` es thread-safe, pero si mismo `ruta` se escribe desde múltiples threads, última escritura gana (race condition). | Seguridad | BAJA | No verificable sin contexto de concurrencia. Documentar: "No es seguro para acceso concurrente al mismo archivo." |
| 110 | Encoder = `JavaScriptEncoder.UnsafeRelaxedJsonEscaping` puede permitir caracteres de control no escapados. | Seguridad | MEDIA | Si datos contienen PII, usar `JsonSerializerOptions.Default` o `JavaScriptEncoder.Default`. Revisar si datos bancarios merecen maximal encoding. |
| 111 | JSON serializa TODO de Poliza (Numero, PrimaMensual, etc.). Si Numero es confidencial/PII, se exporta sin encriptación. | Seguridad | ALTA | Añadir máscara en Poliza (usar JsonIgnore para campos sensibles) O crear DTO ExportablePoliza sin PII. Implementar auditoría de acceso a exports. |
| 125-132 | `EscaparCsv()` escapa "=" al inicio con prefijo "'", pero no documenta que esto cambia la semántica del dato (de fórmula a string). | Correctitud | MEDIA | Agregar comentario: "Prefijo ' previene inyección Excel; interpretado como string en lectura." O usar QUOTE ALL values. |
| 122 | Escapado solo verifica `StartsWith("=", "+", "-", "@")` pero no `StartsWith("|")` (Lotus 1-2-3 legacy). | Correctitud | BAJA | No verificable sin contexto de cliente. Si target es Excel moderno, suficiente. Documentar. |
| N/A (Pruebas) | NO HAY TEST para escapado CSV con caracteres especiales (ejemplo: valor="=SUM(A:A)" debe retornar "'=SUM(A:A)"). | Pruebas | ALTA | Agregar test: `EscaparCsv_ConFórmula_DebePrefijearConComilla()`. |
| N/A (Pruebas) | NO HAY TEST para valores decimales con muchas posiciones (ejemplo: 123.456789). Línea 85 `{poliza.PrimaMensualSoles}` sin formato. | Pruebas | MEDIA | Agregar test CSV con prima = 150.5555m, verificar export. |
| N/A (Pruebas) | NO HAY TEST para JSON con Poliza.Numero = null. Línea 111 puede fallar o serializar null silenciosamente. | Pruebas | MEDIA | Agregar test: `ExportarJson_ConPolizasNulo_DebeSerializarComoNull()`. |
| N/A (Pruebas) | NO HAY TEST para ruta inválida con path traversal (`../../sensitive.csv`). | Pruebas | ALTA | Agregar test: `ExportarCsv_ConRutaTraversal_DebeLanzarArgumentException()`. |
| N/A (Pruebas) | NO HAY TEST para ResumenPorProducto con Poliza.Producto = null o string.Empty. | Pruebas | ALTA | Agregar test: `ResumenPorProducto_ConProductoNulo_NoDebeCausarExcepción()`. |
| N/A (Pruebas) | NO HAY TEST para CSV con valores que contienen comillas y comas: "Producto, \"XYZ\"". | Pruebas | MEDIA | Agregar test: `EscaparCsv_ConComillaYComa_DebeEscaparCorrectamente()`. |
| N/A (Pruebas) | NO HAY TEST para rango de fechas con finVigencia < inicioVigencia (lógica invertida). | Pruebas | MEDIA | Agregar test: `ResumenPorProducto_ConRangoInvertido_DebeRetornarVacio()`. |
| N/A (Pruebas) | NO HAY TEST para decimal.MaxValue en suma primas (overflow?). Aunque C# decimal maneja range, sumar 9 billones x 1000 pólizas causa OverflowException. | Pruebas | BAJA | No verificable sin especificación. Agregar comentario si suma está capped. |

---

### Resumen Ejecutivo:
- **CRÍTICOS (5):** Validación de Producto null (48), Path traversal (63), PII en JSON (111), Tests CSV special chars (N/A), Tests path traversal (N/A)
- **MEDIOS (7):** Validación null genérica (24), Headers hardcoded (74), Decimales sin formato (85), Encoder relajado (110), Escapado no documentado (125), Tests decimales (N/A), Tests Producto null (N/A)
- **BAJOS (4):** Filtrado ineficiente (32), Flush implícito (63), OOM handling (89), Caracteres legacy (122)
- **No verificables:** Concurrencia (98), Especificación de rango overflow (N/A)

**Recomendación:** Priorizar (1) Tests de seguridad path/CSV, (2) Máscarar PII en export, (3) Validar Producto/Numero no-null.
