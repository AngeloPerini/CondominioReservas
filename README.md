# Configuração do Ambiente de Desenvolvimento Windows com VS Code

Este documento descreve os passos necessários para configurar o ambiente de desenvolvimento para o sistema de reservas do condomínio no Windows usando Visual Studio 2022.

## Requisitos de Sistema

- Windows 10 ou superior
- Visual Studio Code
- .NET SDK 9.0 ou superior
- Node.js 16.x ou superior
- PostgreSQL 17 ou superior
- Git

## Instalação das Ferramentas

### 1. Visual Studio 2022

1. Baixe o Visual Studio Code do site oficial: https://code.visualstudio.com/
2. Execute o instalador e siga as instruções na tela
3. Recomendamos marcar todas as opções durante a instalação, incluindo "Adicionar ao PATH"

### 2. .NET SDK

1. Baixe o .NET SDK 9.0 ou superior: [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/pt-br/download)
2. Execute o instalador e siga as instruções na tela
3. Verifique a instalação abrindo um Prompt de Comando e digitando:
   ```
   dotnet --version
   ```

### 3. Node.js

1. Baixe o Node.js (versão LTS recomendada): [https://nodejs.org/](https://nodejs.org/pt)
2. Execute o instalador e siga as instruções na tela
3. Verifique a instalação abrindo um Prompt de Comando e digitando:
   ```
   node --version
   npm --version
   ```

### 4. PostgreSQL

1. Baixe o PostgreSQL: https://www.postgresql.org/download/windows/
2. Execute o instalador e siga as instruções na tela
3. Anote a senha do usuário 'postgres' durante a instalação
4. Mantenha a porta padrão (5432)
5. Após a instalação, você pode usar o pgAdmin (incluído) para gerenciar o banco de dados

### 5. Git

1. Baixe o Git: https://git-scm.com/download/win
2. Execute o instalador e siga as instruções na tela
3. Recomendamos usar as opções padrão durante a instalação
4. Verifique a instalação abrindo um Prompt de Comando e digitando:
   ```
   git --version
   ```

## Extensões Recomendadas para Visual Studio

Abra o VS Code e instale as seguintes extensões:

1. **Microsoft.AspNetCore.Authentication.Core** - Tipos comuns do ASP.NET Core usados ​​pelos vários componentes de middleware de autenticação.
2. **Microsoft.AspNetCore.Http** - Implementações de recursos HTTP padrão do ASP.NET Core.
3. **Prettier** - Formatação de código
4. **PostgreSQL** - Suporte para PostgreSQL
5. **Thunder Client** - Cliente REST para testar APIs
6. **GitLens** - Funcionalidades Git avançadas
7. **React Developer Tools** - Ferramentas para desenvolvimento React

## Configuração do Projeto

### 1. Clonar o Repositório (ou criar um novo)

```bash
git clone [URL_DO_REPOSITORIO] CondominioReservas
cd CondominioReservas
```

Ou crie uma nova estrutura de pastas:

```bash
mkdir CondominioReservas
cd CondominioReservas
mkdir Backend Frontend Database Docs
```

### 2. Configurar o Banco de Dados

1. Abra o pgAdmin
2. Crie um novo banco de dados chamado "CondominioReservas"
3. Execute o script SQL do arquivo `Database/schema_atualizado.sql`

### 3. Configurar o Backend (.NET)

1. Abra o VS Code na pasta do projeto
2. Abra um terminal no VS Code (Terminal > New Terminal)
3. Navegue até a pasta Backend:
   ```bash
   cd Backend
   ```
4. Crie um novo projeto Web API:
   ```bash
   dotnet new webapi -n CondominioReservas.API
   cd CondominioReservas.API
   ```
5. Adicione os pacotes necessários:
   ```bash
   dotnet add package Microsoft.EntityFrameworkCore
   dotnet add package Microsoft.EntityFrameworkCore.Tools
   dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
   dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
   ```

### 4. Configurar o Frontend (React)

1. Abra um novo terminal no VS Code
2. Navegue até a pasta Frontend:
   ```bash
   cd Frontend
   ```
3. Crie um novo projeto React:
   ```bash
   npx create-react-app condominio-reservas-app --template typescript
   cd condominio-reservas-app
   ```
4. Instale os pacotes necessários:
   ```bash
   npm install react-router-dom axios tailwindcss @headlessui/react
   ```
5. Inicialize o Tailwind CSS:
   ```bash
   npx tailwindcss init
   ```

## Executando o Projeto

### Backend

1. No terminal do VS Code, navegue até a pasta do backend:
   ```bash
   cd Backend/CondominioReservas.API
   ```
2. Execute o projeto:
   ```bash
   dotnet run
   ```
3. A API estará disponível em https://localhost:5001 ou http://localhost:5000

### Frontend

1. Em outro terminal do VS Code, navegue até a pasta do frontend:
   ```bash
   cd Frontend/condominio-reservas-app
   ```
2. Execute o projeto:
   ```bash
   npm start
   ```
3. O frontend estará disponível em http://localhost:3000

## Dicas para Desenvolvimento no VS Code

1. Use o terminal integrado para executar comandos
2. Utilize o depurador integrado do VS Code para depurar o código C#
3. Para o frontend, use a extensão React Developer Tools para depurar componentes
4. Use o Thunder Client para testar as APIs do backend
5. Configure o Auto Save (File > Auto Save) para salvar automaticamente os arquivos

## Solução de Problemas Comuns

1. **Erro de porta em uso**: Altere a porta no arquivo `launchSettings.json` do backend
2. **Problemas de CORS**: Verifique a configuração CORS no backend
3. **Erro de conexão com o banco de dados**: Verifique a string de conexão e se o PostgreSQL está em execução
4. **Problemas com Node.js**: Verifique se a versão do Node.js é compatível com o projeto React
