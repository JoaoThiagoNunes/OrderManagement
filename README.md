# E-comerce API

API para gerenciamento de pedidos de e-commerce. Desenvolvida com .NET 8, Entity Framework Core e SQL Server.

---

## Requisitos

- .NET 8 SDK
- SQL Server Express
- Visual Studio 2022

## Como rodar

```bash
git clone https://github.com/JoaoThiagoNunes/OrderManagement.git
cd OrderManagement/OrderManagement.Api
```
Configure a connection string no `appsettings.json`:
```json
"DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=OrderManagement;Trusted_Connection=True;TrustServerCertificate=True;"
```

```bash
dotnet ef database update
dotnet run
```
---

## Endpoints

| Método | Rota | Descrição |
|--------|------|-----------|
| GET | `/api/orders` | Lista todos os pedidos |
| GET | `/api/orders/{id}` | Busca pedido por ID |
| POST | `/api/orders` | Cria pedido |
| PUT | `/api/orders/{id}` | Altera pedido |
| DELETE | `/api/orders/{id}` | Cancela pedido |
| PATCH | `/api/orders/{id}/status` | Atualiza status |

## Transições de status

`Iniciado` → `Processado` → `Enviado`  
`Iniciado` ou `Processado` → `Cancelado`

Pedidos só podem ser alterados enquanto estiverem com status `Iniciado`.
