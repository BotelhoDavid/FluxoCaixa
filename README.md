# 💰 FluxoCaixa API

API robusta para controle de fluxo de caixa, desenvolvida com as melhores práticas de engenharia de software, incluindo **Domain-Driven Design (DDD)**, **TDD**, e **SOLID**.

![C#](https://img.shields.io/badge/C%23-239120?style=for-the-badge&logo=c-sharp&logoColor=white)
![.NET](https://img.shields.io/badge/.NET-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![SQL Server](https://img.shields.io/badge/SQL_Server-CC2927?style=for-the-badge&logo=microsoft-sql-server&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-2496ED?style=for-the-badge&logo=docker&logoColor=white)
![Swagger](https://img.shields.io/badge/Swagger-85EA2D?style=for-the-badge&logo=swagger&logoColor=black)

---

## 🚀 Tecnologias e Práticas

- **.NET 10**: Plataforma de desenvolvimento de alta performance.
- **Entity Framework Core**: ORM para acesso a dados.
- **JWT (JSON Web Token)**: Autenticação segura via Bearer Token.
- **Serilog**: Log estruturado com escrita em Console e Arquivo.
- **Scalar**: Documentação de API interativa e moderna.
- **ClosedXML**: Geração de relatórios em Excel.
- **Mapster**: Mapeamento objeto-objeto de alta performance.
- **Architecture**:
  - **DDD**: Domínio rico, separação de camadas (Application, Domain, Infra).
  - **SOLID**: Princípios de design orientado a objetos.
  - **Clean Code**: Código legível e manutenível.
- **Tests**: Testes unitários com xUnit, Moq e FluentAssertions.

---

## 🔐 Autenticação

A API utiliza autenticação via **JWT Bearer Token**. Para acessar os endpoints protegidos (Lançamentos), você deve primeiro realizar o login e incluir o token no header da requisição.

**Header:**
`Authorization: Bearer <seu_token_aqui>`

---

## 📡 Endpoints

### 1. Autenticação

#### 🆕 Registrar Usuário
Cria uma nova conta de usuário.

- **URL**: `/api/Autenticacao/registrar`
- **Método**: `POST`
- **Body**:
```json
{
  "nome": "João Silva",
  "email": "joao@exemplo.com",
  "password": "senhaForte123"
}
```

#### 🔑 Login
Autentica um usuário e retorna o Token de acesso.

- **URL**: `/api/Autenticacao/login`
- **Método**: `POST`
- **Body**:
```json
{
  "email": "joao@exemplo.com",
  "password": "senhaForte123"
}
```
- **Resposta Sucesso (200)**:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6...",
  "expiresIn": 3600,
  "tokenType": "Bearer"
}
```

---

### 2. Lançamentos Financeiros

> ⚠️ **Requer Autenticação**: Todos os endpoints abaixo exigem o Header `Authorization`.

#### 📝 Registrar Lançamento
Adiciona um novo lançamento (crédito ou débito) no fluxo de caixa.

- **URL**: `/api/lancamento`
- **Método**: `POST`
- **Body**:
```json
{
  "descricao": "Venda de Serviços",
  "valor": 1500.00,
  "dataLancamento": "2024-01-29T10:00:00",
  "tipo": 1  // 1 = Crédito, 2 = Débito
}
```

#### 📋 Listar Lançamentos
Retorna todos os lançamentos cadastrados.

- **URL**: `/api/lancamento`
- **Método**: `GET`
- **Resposta**:
```json
[
  {
    "descricao": "Venda de Serviços",
    "valor": 1500.00,
    "dataLancamento": "2024-01-29T10:00:00",
    "tipo": 1
  }
]
```

#### 📊 Gerar Relatório Diário (Excel)
Gera e baixa um arquivo Excel (.xlsx) com os lançamentos de um dia específico, incluindo o cálculo do saldo final (Crédito - Débito).

- **URL**: `/api/lancamento/relatorio`
- **Método**: `GET`
- **Parâmetros de Query**:
    - `data` (Obrigatório): Data do relatório (formato ISO 8601 ou `YYYY-MM-DD`).
- **Exemplo de Chamada**:
  `/api/lancamento/relatorio?data=2024-01-30`
- **Resposta**: Arquivo binário `relatorio_fluxocaixa_{data}.xlsx` contendo:
    - Listagem detalhada de lançamentos.
    - Totalizadores de Créditos e Débitos.
    - Valor Final (Saldo) com destaque visual.

---

## 🛠️ Como Executar

### Pré-requisitos
- .NET SDK (versão 10 ou superior)
- SQL Server

### Passos
1. Clone o repositório.
2. Configure a string de conexão no `Program.cs` ou variáveis de ambiente.
3. Execute a aplicação:
   ```bash
   dotnet run --project src/FluxoCaixa.API
   ```
4. Acesse a documentação (Scalar) em: `http://localhost:5000/scalar/v1`

---

## 🧪 Como Rodar os Testes

Execute o comando na raiz do projeto:

```bash
dotnet test
```

Os testes cobrem:
- Regras de negócio da entidade `Usuario` e `SenhaHash`.
- Serviços de Aplicação (`Autenticacao` e `Lancamento`), incluindo a geração de relatórios.
