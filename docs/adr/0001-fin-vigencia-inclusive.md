# ADR 0001: Fin de Vigencia Inclusivo en PolizaService.FiltrarVigentes

## Contexto

En sistemas de seguros, es común que las pólizas tengan una fecha de fin de vigencia. Surgió la necesidad de definir si una póliza que vence "hoy" debe considerarse como vigente durante ese mismo día. Esto impacta consultas de cobertura activa, generación de reportes y análisis de riesgos.

La implementación actual de `PolizaService.FiltrarVigentes()` usa la condición:
`csharp
poliza.FinVigencia >= fecha
`

## Decisión

**La fecha de fin de vigencia es INCLUSIVA:** una póliza que vence hoy sigue siendo vigente durante todo ese día.

Esto alinea con la práctica del negocio de seguros, donde la cobertura permanece activa hasta el último momento del día de vencimiento.

## Alternativas Consideradas

1. **Fin de vigencia exclusivo** (`FinVigencia > fecha`): póliza vence al inicio del día. Genera complejidad operativa y reclamaciones rechazadas injustamente.
2. **Timestamp con hora** (`DateTime` en lugar de `DateOnly`): añade complejidad innecesaria sin beneficio en este dominio.

## Consecuencias

- ✅ Comportamiento intuitivo: póliza con vencimiento 2025-12-31 cubre todo ese día.
- ✅ Menos rechazos de siniestros por tecnicismo de fecha.
- ⚠️ Requiere documentar explícitamente este comportamiento en tests.
- ⚠️ Si en futuro se necesita exactitud de hora, habrá refactorización.
