# Guia de Execução da API

Este documento descreve como executar e testar localmente a API mockada do Gallo Tracking.

## Pré-requisitos

- .NET SDK 9.0 ou superior
- Terminal PowerShell, Prompt de Comando ou equivalente

## Restaurar dependências

Na raiz do projeto:

```bash
dotnet restore GalloTracking.sln
```

## Executar a API

```bash
dotnet run --project GalloTracking.Api
```

O terminal exibirá as URLs HTTP e HTTPS disponíveis. O banco SQLite `gallo.db` será criado automaticamente na primeira execução, junto com os dados de demonstração.

## Acessar o Swagger

Abra no navegador:

```text
https://localhost:<porta>/swagger
```

Substitua `<porta>` pela porta exibida no terminal.

## Credenciais de desenvolvimento

| Perfil | E-mail | Senha |
| --- | --- | --- |
| Gestor | `gestor@gallo.local` | `gallo123` |
| Motorista | `motorista@gallo.local` | `gallo123` |

Essas credenciais existem apenas para desenvolvimento.

## Fluxo básico de teste

1. Execute `POST /api/auth/login`.
2. Informe uma das credenciais acima.
3. Copie o campo `token` retornado.
4. No Swagger, clique em **Authorize** e informe `Bearer <token>`.
5. Consulte `GET /api/rotas` e copie o id da rota criada pelo seed.
6. Execute `POST /api/rotas/{id}/iniciar`.
7. Envie uma coordenada usando `POST /api/localizacoes`.
8. Consulte `GET /api/rotas/{id}/localizacoes`.
9. Finalize a rota com `POST /api/rotas/{id}/finalizar`.

Localizações só são aceitas enquanto a rota estiver com status `Ativa`.

## Sincronização offline

Use `POST /api/localizacoes/batch` para simular o envio de coordenadas armazenadas no celular quando a conexão retornar.

Todas as rotas informadas precisam estar ativas para que o lote seja persistido.

## Comunicação em tempo real

O hub SignalR está disponível em `/hubs/localizacao`.

Clientes autenticados podem chamar `EntrarNaRota(rotaId)` e recebem o evento `novaLocalizacao` quando uma coordenada da rota for salva.

## Executar os testes

```bash
dotnet test GalloTracking.sln
```

Os testes usam bancos SQLite temporários e validam login, ciclo da rota e bloqueio de localização fora de uma rota ativa.

## Executar testes com cobertura

```bash
dotnet test GalloTracking.sln --settings coverage.runsettings --collect:"XPlat Code Coverage"
```

Os relatórios `coverage.cobertura.xml` e `coverage.opencover.xml` são gerados dentro de `TestResults`. A configuração exclui o projeto de testes do cálculo.

## Persistência

O banco inicial utiliza SQLite e é configurado em `GalloTracking.Api/appsettings.json`.

A conexão está isolada no Entity Framework Core. Para uma futura migração para PostgreSQL, será necessário trocar o provider e a connection string.
