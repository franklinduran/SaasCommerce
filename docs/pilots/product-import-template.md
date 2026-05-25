# Guía de Importación de Productos

**Endpoint:** `POST /api/products/import`  
**Permiso requerido:** `products.import`

---

## Formato del Archivo CSV

### Columnas requeridas

| Columna | Tipo | Requerido | Descripción |
|---------|------|-----------|-------------|
| Name | Texto | ✅ | Nombre del producto (máx. 200 chars) |
| Sku | Texto | ✅ | Código único del producto (mayúsculas recomendadas) |
| CategoryName | Texto | ❌ | Nombre de categoría (se crea si no existe) |
| SalePrice | Decimal | ✅ | Precio de venta (ej: 175.00) |
| CostPrice | Decimal | ✅ | Costo del producto (ej: 140.00) |
| StockQuantity | Decimal | ❌ | Cantidad inicial en inventario |

### Ejemplo

```csv
Name,Sku,CategoryName,SalePrice,CostPrice,StockQuantity
Arroz El Gallo 5lbs,ARR-GALL-5LB,Víveres,175.00,140.00,100
Cerveza Presidente 12oz,CRV-PRES-12,Bebidas,65.00,47.00,120
Leche Parmalat 1L,LEC-PARM-1L,Lácteos,95.00,75.00,60
Ron Barceló 750ml,RON-BARC-750,Licores,1250.00,950.00,20
Detergente Ace 500g,DET-ACE-500,Limpieza,85.00,62.00,50
```

---

## Reglas de Negocio

### Validación por fila
- `Name` es obligatorio — fila omitida si vacío
- `Sku` es obligatorio — fila omitida si vacío
- `SalePrice` y `CostPrice` deben ser >= 0
- `StockQuantity` si se proporciona debe ser >= 0

### Manejo de duplicados
- Si el `Sku` ya existe en el catálogo, **la fila se omite** (no es error fatal)
- La importación continúa con el resto de filas
- Se retorna un conteo de omitidos en la respuesta

### Categorías
- Si `CategoryName` no existe, **se crea automáticamente**
- Si está vacío, el producto queda sin categoría

### Inventario
- Si `StockQuantity > 0`, se crea un movimiento de tipo `InitialStock`
- El stock se asigna a la sucursal principal del usuario importador

---

## Límites

| Límite | Valor |
|--------|-------|
| Máximo de filas | 500 por archivo |
| Tamaño máximo del archivo | No hay límite explícito |
| Encoding soportado | UTF-8 |
| Separador | Coma (`,`) |
| Campos con comas | Usar comillas dobles: `"Arroz, Premium"` |

---

## Respuesta del API

### Importación exitosa (201 Created)

```json
{
  "isSuccess": true,
  "data": {
    "importedCount": 5,
    "skippedCount": 1,
    "totalRows": 6,
    "errors": [
      {
        "rowNumber": 3,
        "name": "Producto Sin Nombre",
        "messages": ["Name is required."]
      }
    ]
  }
}
```

### Archivo vacío (400 Bad Request)

```json
{
  "isSuccess": false,
  "error": {
    "code": "IMPORT_EMPTY_FILE",
    "message": "El archivo CSV está vacío o no contiene datos."
  }
}
```

---

## Errores Comunes

| Error | Causa | Solución |
|-------|-------|---------|
| `IMPORT_EMPTY_FILE` | Archivo sin contenido | Verificar que el CSV no esté vacío |
| `IMPORT_USER_CONTEXT_REQUIRED` | Token inválido o expirado | Re-autenticarse |
| Fila omitida: "SKU already exists" | SKU duplicado | Usar un SKU diferente o actualizar el producto existente |
| Fila omitida: "Name is required" | Campo Name vacío | Completar el nombre del producto |
| Fila omitida: "SalePrice must be >= 0" | Precio negativo | Corregir el precio |

---

## Ejemplo con Python para Automatización

```python
import requests

def import_products(csv_path: str, access_token: str, base_url: str):
    with open(csv_path, 'rb') as f:
        response = requests.post(
            f"{base_url}/api/products/import",
            headers={"Authorization": f"Bearer {access_token}"},
            files={"file": ("products.csv", f, "text/csv")},
        )
    return response.json()
```
