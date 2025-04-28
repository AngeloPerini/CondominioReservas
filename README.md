# Guia de Implantação - Sistema de Reservas do Condomínio

Este documento fornece instruções detalhadas para implantar o sistema de reservas do condomínio em um ambiente Windows usando Visual Studio Code.

## Requisitos de Sistema

### Ambiente de Desenvolvimento
- Windows 10 ou superior
- Visual Studio Code
- .NET SDK 9.0 ou superior
- Node.js 16.0 ou superior
- npm 8.0 ou superior
- PostgreSQL 16.0 ou superior

### Extensões Recomendadas para VS Code
- C# (ms-dotnettools.csharp)
- ESLint (dbaeumer.vscode-eslint)
- Prettier (esbenp.prettier-vscode)
- PostgreSQL (ckolkman.vscode-postgres)

## Passos para Implantação

### 1. Configuração do Banco de Dados PostgreSQL

1. Instale o PostgreSQL no Windows:
   - Baixe o instalador em https://www.postgresql.org/download/windows/
   - Execute o instalador e siga as instruções
   - Anote a senha do usuário 'postgres' durante a instalação

2. Crie o banco de dados:
   - Abra o pgAdmin (instalado com o PostgreSQL)
   - Conecte-se ao servidor local
   - Crie um novo banco de dados chamado 'CondominioReservas'

3. Execute o script SQL para criar as tabelas:
   - Abra o arquivo `Database/schema_atualizado.sql` no pgAdmin
   - Execute o script no banco de dados 'CondominioReservas'

### 2. Configuração do Backend (.NET C#)

1. Abra o projeto no Visual Studio Code:
   - Abra o VS Code
   - Selecione File > Open Folder
   - Navegue até a pasta `CondominioReservas/Backend`
   - Clique em 'Selecionar Pasta'

2. Configure a conexão com o banco de dados:
   - Abra o arquivo `CondominioReservas.API/appsettings.json`
   - Atualize a string de conexão com suas credenciais do PostgreSQL:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Database=CondominioReservas;Username=postgres;Password=SuaSenha"
   }
   ```

3. Restaure os pacotes NuGet:
   - Abra um terminal no VS Code (Terminal > New Terminal)
   - Navegue até a pasta do projeto API: `cd CondominioReservas.API`
   - Execute o comando: `dotnet restore`

4. Execute as migrações do banco de dados:
   - No terminal, execute: `dotnet ef database update`

5. Execute o backend:
   - No terminal, execute: `dotnet run`
   - O backend estará disponível em `https://localhost:5001` e `http://localhost:5000`

### 3. Configuração do Frontend (React)

1. Abra um novo terminal no VS Code:
   - Terminal > New Terminal
   - Navegue até a pasta do frontend: `cd ../Frontend/condominio-reservas-app`

2. Instale as dependências:
   - Execute o comando: `npm install`

3. Configure a URL da API:
   - Abra o arquivo `src/config/api.config.js`
   - Verifique se a URL do backend está correta:
   ```javascript
   DEV_API_URL: 'http://localhost:5000/api'
   ```

4. Execute o frontend:
   - No terminal, execute: `npm start`
   - O frontend estará disponível em `http://localhost:3000`

### 4. Configuração para Produção

#### Backend

1. Publique o backend:
   - No terminal, navegue até a pasta do projeto API
   - Execute o comando: `dotnet publish -c Release -o ./publish`
   - Os arquivos publicados estarão na pasta `publish`

2. Configure o IIS (opcional):
   - Instale o .NET Core Hosting Bundle
   - Crie um novo site no IIS apontando para a pasta `publish`
   - Configure o Application Pool para No Managed Code

#### Frontend

1. Crie a build de produção:
   - No terminal, navegue até a pasta do frontend
   - Execute o comando: `npm run build`
   - Os arquivos de build estarão na pasta `build`

2. Implante no servidor web:
   - Copie os arquivos da pasta `build` para o servidor web
   - Configure o servidor para servir a aplicação React

## Verificação da Implantação

1. Acesse o backend:
   - Navegue para `http://localhost:5000/swagger` para verificar a API
   - Todos os endpoints devem estar disponíveis e funcionando

2. Acesse o frontend:
   - Navegue para `http://localhost:3000`
   - Faça login com uma conta Google
   - Verifique se todas as funcionalidades estão operando corretamente

## Solução de Problemas

### Problemas de Conexão com o Banco de Dados
- Verifique se o PostgreSQL está em execução
- Confirme se as credenciais na string de conexão estão corretas
- Verifique se o firewall permite conexões na porta do PostgreSQL (geralmente 5432)

### Problemas de CORS
- Verifique se a política CORS no backend está configurada corretamente
- Confirme se a URL do frontend está incluída nas origens permitidas

### Problemas de Autenticação
- Verifique se as configurações de autenticação do Google estão corretas
- Confirme se o redirecionamento após o login está funcionando

## Suporte

Para suporte adicional, entre em contato com a equipe de desenvolvimento:
- Email: suporte@condominioreservas.com
- Telefone: (XX) XXXX-XXXX
