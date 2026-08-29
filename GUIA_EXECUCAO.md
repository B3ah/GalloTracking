# Guia de Execução da API

Este documento descreve como executar e testar localmente a API mockada do Gallo Tracking.

## Pré-requisitos

- .NET SDK 9.0 ou superior
- Terminal PowerShell, Prompt de Comando ou equivalente

## Restaurar dependências

Na raiz do projeto, execute:

```bash
dotnet restore GalloTracking.sln
```

## Executar a API

```bash
dotnet run --project GalloTracking.Api
```

O terminal exibirá as URLs HTTP e HTTPS disponíveis.

O banco SQLite `gallo.db` será criado automaticamente na primeira execução, junto com os dados iniciais de demonstração.

## Acessar o Swagger

Abra no navegador:

```text
https://localhost:<porta>/swagger
```

Substitua `<porta>` pela porta exibida no terminal.

O Swagger permite executar os endpoints diretamente pelo navegador.

## Credenciais de desenvolvimento

| Perfil | E-mail | Senha |
| --- | --- | --- |
| Gestor | `gestor@gallo.local` | `gallo123` |
| Motorista | `motorista@gallo.local` | `gallo123` |

Essas credenciais existem apenas para o ambiente de desenvolvimento.

## Fluxo básico de teste

1. Execute `POST /api/auth/login` com uma das credenciais acima.
2. Copie o campo `token` retornado.
3. No Swagger, clique em **Authorize** e informe `Bearer <token>`.
4. Consulte `GET /api/rotas` e identifique o id da rota criada pelo seed.
5. Execute `POST /api/rotas/{id}/iniciar`.
6. Envie uma coordenada usando `POST /api/localizacoes`.
7. Consulte o histórico em `GET /api/rotas/{id}/localizacoes`.
8. Finalize a rota usando `POST /api/rotas/{id}/finalizar`.

Localizações só são aceitas enquanto a rota estiver com status `Ativa`.

## Sincronização offline

Para simular o envio de coordenadas armazenadas localmente no celular, utilize:

```text
POST /api/localizacoes/batch
```

O endpoint recebe uma lista de localizações. Todas as rotas informadas precisam estar ativas para que o lote seja persistido.

## Comunicação em tempo real

O hub SignalR está disponível em:

```text
/hubs/localizacao
```

Após a autenticação, o cliente pode entrar no grupo de uma rota chamando `EntrarNaRota(rotaId)`. Quando uma nova localização for salva, o evento `novaLocalizacao` será publicado para esse grupo.

## Executar os testes

```bash
dotnet test GalloTracking.sln
```

Os testes usam bancos SQLite temporários e validam login, ciclo da rota e bloqueio de localização fora de uma rota ativa.

## Estrutura da persistência

O banco inicial utiliza SQLite e é configurado em:

```text
GalloTracking.Api/appsettings.json
```

A conexão está isolada no Entity Framework Core. Para uma futura migração para PostgreSQL, será necessário trocar o provider e a connection string, mantendo as regras da API.
