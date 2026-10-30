# ArtistaConnect

O ArtistaConnect é uma API feita em C# para cadastrar e consultar músicas. A ideia do projeto é organizar os dados de músicas em um banco MySQL e disponibilizar algumas operações por uma API.

O projeto está dividido em partes menores. Assim, os modelos, as regras e o acesso ao banco não ficam todos no mesmo lugar.

## O que é usado

- C# e .NET 9
- ASP.NET Core Minimal API
- Entity Framework Core
- MySQL
- JWT para proteger as rotas de músicas

## Como o projeto está organizado

A solução está no arquivo `ArtistaConnect.sln` e contém estes projetos:

- **ArtistaConnect.Model**: guarda os modelos usados no projeto. `Artista` tem nome, biografia e gênero musical. `Musica` tem título, artista responsável e duração.
- **ArtistaConnect.Data**: cuida do acesso ao banco. `ArtistaContext` configura a conexão e o Entity Framework; `ArtistaDAO` faz as operações de inclusão, listagem, edição e remoção de músicas. A pasta `Migrations` registra as alterações da estrutura do banco.
- **ArtistaConnect.Service**: reúne serviços auxiliares. `MusicaService` e `ArtistaService` fazem verificações simples de dados, e `TokenService` cria tokens JWT.
- **ArtistaConnect.Business**: contém a classe `PaginaBusiness`, onde há uma validação inicial relacionada ao título da música.
- **ArtistaConnect.WebAPI**: é a entrada da aplicação. No `Program.cs` ficam a configuração do JWT e as rotas HTTP.

## Antes de começar

Você vai precisar de:

1. .NET SDK 9 instalado.
2. MySQL instalado e em execução.
3. Uma base de dados chamada `ArtistaConnect`.

### Configurar o banco

A conexão com o MySQL está configurada em `ArtistaConnect.Data/ArtistaContext.cs`. Antes de executar, confira os dados de servidor, porta, usuário e senha e ajuste-os para o seu MySQL local. Não coloque senhas reais em arquivos que serão enviados para o GitHub.

O projeto já tem uma migration inicial que cria a tabela `Musicas`. Para aplicá-la, abra um terminal na pasta que contém `ArtistaConnect.sln` e rode:

```powershell
dotnet ef database update --project .\ArtistaConnect.Data --startup-project .\ArtistaConnect.WebAPI
```

Se o comando `dotnet ef` não estiver disponível, instale a ferramenta do Entity Framework:

```powershell
dotnet tool install --global dotnet-ef
```

## Como executar

Na pasta da solução, restaure as dependências e inicie a API:

```powershell
dotnet restore
dotnet run --project .\ArtistaConnect.WebAPI
```

O endereço local aparece no terminal quando a aplicação inicia. As configurações de execução para desenvolvimento ficam em `ArtistaConnect.WebAPI/Properties/launchSettings.json`.

## Rotas disponíveis

### Gerar um token

Envie um `POST` para `/login/auth` com um artista no corpo:

```json
{
  "nome": "Nome do artista",
  "bio": "Uma breve descrição",
  "generoMusical": "Rock"
}
```

Essa rota gera um token JWT usando o nome enviado. Neste momento, ela não confere usuário ou senha; é uma implementação inicial para gerar o token.

### Rotas de músicas

As rotas abaixo exigem autenticação JWT. Envie o token recebido no cabeçalho `Authorization`, usando o esquema `Bearer` seguido do token.

| Método | Caminho | O que faz |
|---|---|---|
| `GET` | `/musicas/` | Lista as músicas cadastradas |
| `POST` | `/musicas/` | Cadastra uma música |
| `PUT` | `/musicas/` | Atualiza uma música |
| `DELETE` | `/musicas/` | Remove uma música |

Exemplo de corpo para cadastrar uma música:

```json
{
  "titulo": "Nome da música",
  "artistaResponsavel": "Nome do artista",
  "duracao": "3:45"
}
```

O título é a chave usada para identificar a música no banco. Para atualizar ou remover, envie no corpo a música com o título correspondente.

## Migrations

As migrations do Entity Framework ficam em `ArtistaConnect.Data/Migrations`. Quando o modelo do banco mudar, uma nova migration pode ser criada com:

```powershell
dotnet ef migrations add NomeDaMigration --project .\ArtistaConnect.Data --startup-project .\ArtistaConnect.WebAPI
```

Depois, aplique a migration ao banco:

```powershell
dotnet ef database update --project .\ArtistaConnect.Data --startup-project .\ArtistaConnect.WebAPI
```

## Observação

O arquivo `ArtistaConnect.WebAPI/ArtistaConnect.WebAPI.http` ainda tem um exemplo padrão de consulta para `/weatherforecast/`. As rotas implementadas atualmente estão descritas acima, no `Program.cs`.
