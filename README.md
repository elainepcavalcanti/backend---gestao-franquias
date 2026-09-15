# Gestão de Franquias

API acadêmica para gerenciamento de uma rede de franquias.

O projeto permite controlar franqueadoras, unidades, usuários, produtos, fornecedores, estoque, vendas, royalties e chamados de suporte.

## Tecnologias utilizadas

- C#
- ASP.NET Core
- .NET 10
- Entity Framework Core
- MySQL 8
- Autenticação JWT
- Swagger/OpenAPI
- Git e GitHub

## Funcionalidades

- Cadastro e autenticação de usuários.
- Perfis de Administrador, Gestor e Operador.
- Cadastro e inativação de franqueadoras.
- CRUD de unidades franqueadas.
- Cadastro de responsáveis pelas unidades.
- CRUD de categorias, produtos e fornecedores.
- Controle de entrada e saída de estoque.
- Bloqueio de estoque negativo.
- Cadastro de vendas e cálculo automático do total.
- Atualização automática do estoque após uma venda.
- Configuração e cálculo de royalties.
- Abertura e encerramento de chamados.
- Relatórios de faturamento, royalties, estoque e vendas.
- Filtros e paginação nos principais endpoints.

## Estrutura principal

```text
Franquias.Api/
├── Controllers/
├── DTOs/
├── Entities/
├── Services/
├── FranquiasDbContext.cs
├── Startup.cs
└── Program.cs

database/
├── franquiasDB-schema.sql
└── dados-exemplo.sql

docs/
└── franqueadoras.http
```

## Requisitos

Para executar o projeto é necessário possuir:

- .NET SDK 10
- MySQL Server 8
- Git

## Configuração do banco

O arquivo abaixo cria o banco e suas tabelas:

```text
database/franquiasDB-schema.sql
```

O arquivo abaixo adiciona dados de demonstração:

```text
database/dados-exemplo.sql
```

Os scripts podem ser executados pelo MySQL Workbench ou pelo cliente de linha de comando do MySQL.

## Configuração segura da conexão

A senha do MySQL não deve ser enviada para o GitHub.

Dentro da pasta `Franquias.Api`, inicialize os User Secrets:

```powershell
dotnet user-secrets init --project .\Franquias.Api.csproj
```

Depois configure a conexão, substituindo `SUA_SENHA`:

```powershell
dotnet user-secrets set `
  "ConnectionStrings:DefaultConnection" `
  "Server=localhost;Port=3306;Database=franquiasDB;User=root;Password=SUA_SENHA;" `
  --project .\Franquias.Api.csproj
```

## Como executar

Na pasta principal do repositório, execute:

```powershell
dotnet build .\Franquias.Api\Franquias.Api.csproj
```

Depois:

```powershell
dotnet run --project .\Franquias.Api\Franquias.Api.csproj --launch-profile "Franquias.Api"
```

A documentação da API estará disponível em:

```text
https://localhost:7051/swagger/index.html
```

Para encerrar a API, pressione `Ctrl + C`.

## Autenticação

O login é realizado por:

```text
POST /api/Auth/login
```

O token recebido deve ser informado no botão `Authorize` do Swagger.

Os dados de exemplo incluem um usuário de demonstração:

```text
E-mail: admin.exemplo@franquias.local
Senha: Admin123!
```

Esse usuário deve ser utilizado somente em ambiente local de desenvolvimento.

## Principais endpoints

| Módulo | Endpoints |
|---|---|
| Autenticação | `/api/Auth` |
| Franqueadoras | `/api/franqueadoras` |
| Unidades | `/api/Unidades` |
| Responsáveis | `/api/Responsaveis` |
| Categorias | `/api/Categorias` |
| Produtos | `/api/Produtos` |
| Fornecedores | `/api/Fornecedores` |
| Estoque | `/api/Estoque` |
| Vendas | `/api/Vendas` |
| Royalties | `/api/Royalty` |
| Chamados | `/api/Chamados` |
| Relatórios | `/api/Relatorios` |

## Regras de negócio implementadas

- CNPJ de franqueadoras, unidades e fornecedores não pode ser duplicado.
- Unidades, usuários, produtos e franqueadoras podem ser inativados.
- Unidades inativas não podem realizar vendas.
- Produtos inativos não podem ser vendidos.
- O estoque não pode ficar negativo.
- A venda calcula automaticamente o valor total.
- A venda desconta automaticamente os produtos do estoque.
- O royalty é calculado sobre o faturamento da unidade.
- Chamados possuem prioridade, situação e data de encerramento.
- Endpoints protegidos exigem autenticação e perfil autorizado.

## Banco de dados

O projeto utiliza Entity Framework Core com MySQL.

As entidades são relacionadas por chaves estrangeiras, incluindo:

- Franqueadora e unidades.
- Unidade e responsáveis.
- Categoria, fornecedor e produtos.
- Unidade, produto e estoque.
- Venda e itens da venda.
- Unidade e cobranças de royalty.
- Unidade, usuário e chamados.

## Testes

Os endpoints foram testados manualmente pelo Swagger, incluindo:

- Login e autorização JWT.
- Cadastro e inativação de usuários.
- Cadastro e inativação de franqueadoras e unidades.
- Cadastro de responsáveis.
- Cadastro de produtos e fornecedores.
- Entrada e saída de estoque.
- Bloqueio de saída com saldo insuficiente.
- Cadastro de venda e atualização do estoque.
- Cálculo e pagamento de royalties.
- Abertura e encerramento de chamados.
- Consultas e relatórios.

## Repositório

O desenvolvimento das funcionalidades é realizado na branch:

```text
desenvolvimento
```

Após os testes, a versão final será integrada à branch `main`.
