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
- Customers no tiene endpoints backend expuestos todavia.
- Sales no tiene endpoint HTTP todavia; el flujo de saga se valida por Outbox tecnico hasta que exista `POST /api/sales`.
