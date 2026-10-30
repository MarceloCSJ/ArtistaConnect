# ArtistaConnect

Solução .NET 9 com API Minimal, Entity Framework Core e MySQL, acompanhada do
front-end Angular em `../Front`.

## Configuração da API

A API requer configuração local para a connection string do MySQL e a chave JWT:

- `ConnectionStrings:ArtistaConnect`
- `JWTSetting:SecretKey` (mínimo 32 bytes)

No PowerShell, na raiz `ArtistaConnect`, grave os valores em User Secrets
(substitua o placeholder pela senha local do MySQL; não a envie nem a salve
no repositório):

```powershell
dotnet user-secrets set "ConnectionStrings:ArtistaConnect" "Server=localhost;Port=3306;Database=ArtistaConnect;Uid=root;Pwd=<SENHA_DO_MYSQL>;" --project .\ArtistaConnect.WebAPI
$jwtSecret = [Convert]::ToBase64String([Security.Cryptography.RandomNumberGenerator]::GetBytes(48))
dotnet user-secrets set "JWTSetting:SecretKey" $jwtSecret --project .\ArtistaConnect.WebAPI
dotnet watch --project .\ArtistaConnect.WebAPI --launch-profile http
```

O User Secrets fica fora do repositório e permanece disponível em execuções
futuras neste computador. O perfil `http` inicia a API em `http://localhost:5006`.
Na primeira inicialização, o Entity Framework aplica as migrations; o MySQL
precisa estar ativo e a connection string deve usar uma conta válida.

Se preferir usar variáveis de ambiente em vez de User Secrets, defina
`ConnectionStrings__ArtistaConnect` e `JWTSetting__SecretKey` no mesmo terminal
antes de executar o comando `dotnet watch`.

A migration `AddSecureUsersAndCatalogs` acrescenta tabelas para usuários,
artistas e gêneros musicais; não remove a tabela `Musicas`.

## Projetos

- `ArtistaConnect.Model`: entidades e contratos.
- `ArtistaConnect.Data`: contexto MySQL e migrations.
- `ArtistaConnect.Service`: geração de JWT.
- `ArtistaConnect.WebAPI`: endpoints HTTP.
- `../Front`: aplicação Angular.