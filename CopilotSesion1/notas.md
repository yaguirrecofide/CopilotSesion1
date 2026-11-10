Regla: devuelve el RUC con guion antes del último dígito; recorta espacios; ArgumentException si no es válido.
| Entrada          | Esperado          |
| "20123456789"    | "2012345678-9"    |
| "10987654321"    | "1098765432-1"    |
| " 20123456789 "  | "2012345678-9"    |
| "123"            | ArgumentException |
| null             | ArgumentException |
