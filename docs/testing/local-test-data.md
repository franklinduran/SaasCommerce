# Data local recomendada

## Negocio principal

```txt
Business: Colmado La Bendicion
Sucursal: Principal
Usuario admin: Maria Perez
Email sugerido: admin+{{$timestamp}}@colmado.local
Password: Admin123!
RNC sugerido: 137001253
Telefono: 8095550101
```

## Seed Development existente

```txt
Email: admin@test.com
Password: Admin123!
BusinessId: 11111111-1111-1111-1111-111111111111
BranchId: 22222222-2222-2222-2222-222222222222
UserId: 44444444-4444-4444-4444-444444444444
```

## Productos sugeridos

```txt
Arroz selecto 5 lb
Aceite 16 oz
Salami
Leche evaporada
Pan sobao
Agua 16 oz
```

Inventario inicial:

```txt
Arroz: 20
Aceite: 15
Salami: 10
Leche: 30
Pan: 50
Agua: 100
```

## Cliente sugerido

```txt
Nombre: Maria Perez
Telefono: 8095550102
Uso: venta normal; venta fiada futura
```

## Notas de contratos actuales

- Products `pageSize` acepta `10`, `25` o `50`.
- Customers expone `POST/GET/PUT/DELETE /api/customers` y siempre filtra por `BusinessId` del token.
- Sales expone `POST /api/sales`, `GET /api/sales`, `GET /api/sales/{id}` y `POST /api/sales/{id}/cancel`.
- `POST /api/sales` crea la venta en `Received`, calcula precios desde Catalog y agrega `SaleCreatedEventV1` en Outbox.
- El flujo de saga se valida con API + Worker usando `requests/12-sales-saga-http-flow.http`.
