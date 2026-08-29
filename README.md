# Documentação da Arquitetura do Projeto

## Sistema de Rastreamento Logístico em Tempo Real

### 1. Introdução

## Navegação rápida

- [Visão geral da arquitetura](#3-visão-geral-da-arquitetura)
- [Componentes](#4-componentes-da-arquitetura)
- [Comunicação](#6-comunicação-entre-os-sistemas)
- [Funcionamento offline](#9-arquitetura-de-funcionamento-offline)
- [Banco de dados](#10-banco-de-dados)
- [Endpoints da API](#13-endpoints-principais-da-api)
- [Guia de execução](GUIA_EXECUCAO.md)
- [Apresentação dos diagramas](docs/arquitetura.html)

Os diagramas e fluxos visuais foram movidos para uma apresentação HTML independente. Ela funciona no navegador e não depende do suporte do preview do GitHub a SVG inline.

Este documento apresenta a arquitetura proposta para o sistema de rastreamento logístico desenvolvido para empresas de transporte e logística. O objetivo do projeto é permitir que gestores acompanhem, em tempo real, a localização dos caminhoneiros durante a execução de uma rota utilizando apenas o smartphone do motorista, sem a necessidade de instalar rastreadores físicos nos veículos.

A plataforma será composta por dois sistemas principais:

* Um **aplicativo mobile**, utilizado pelo caminhoneiro;
* Um **painel web**, utilizado pelos gestores da empresa.

Entre esses dois sistemas existirá um **Back-end**, responsável por receber, processar, armazenar e distribuir as informações de localização para os usuários conectados.

A arquitetura foi projetada considerando três necessidades importantes definidas no projeto: privacidade do motorista, economia de bateria do smartphone e funcionamento mesmo em regiões sem conexão com a internet. <FileCite ref_id="turn1file0"/>

---

# 2. Objetivo da Arquitetura

A arquitetura tem como finalidade organizar a comunicação entre todos os componentes do sistema de forma segura, escalável e simples de manter.

O fluxo principal do sistema consiste em capturar a localização GPS do motorista, enviar essas coordenadas ao servidor, armazená-las em um banco de dados e disponibilizá-las em tempo real para o painel web do gestor.

Além disso, a arquitetura também deve permitir que o aplicativo continue funcionando quando estiver sem internet. Nesse caso, as coordenadas serão armazenadas localmente no celular e enviadas automaticamente quando a conexão for restabelecida, utilizando a estratégia conhecida como **Store-and-Forward**. <FileCite ref_id="turn1file0"/>

---

# 3. Visão Geral da Arquitetura

A solução utiliza uma arquitetura centralizada baseada em um **Back-end Monolítico Modular**.

Isso significa que existirá apenas um servidor principal responsável por todas as regras de negócio do sistema, porém organizado internamente em módulos independentes, facilitando futuras manutenções e expansões.

A comunicação entre os componentes acontece através de dois protocolos principais:

* **REST API (HTTPS):** utilizado para operações como login, criação de rotas, envio de coordenadas e cadastro de entregas.
* **WebSocket:** utilizado para enviar atualizações de localização em tempo real ao painel web sem que o gestor precise atualizar a página manualmente.

<p><a href="docs/arquitetura.html#slide-1">Abrir slide da arquitetura (1)</a></p>

**Figura 1 — Visão geral da arquitetura proposta do sistema.**

O aplicativo do motorista é responsável por obter a localização através do GPS do smartphone. Essas informações são enviadas para o Back-end por meio de uma API REST utilizando conexão HTTPS. O Back-end processa os dados, salva as informações no banco PostgreSQL e envia atualizações instantâneas ao painel web através de WebSocket. O painel utiliza uma API de mapas, como Google Maps ou Mapbox, para representar visualmente a posição do caminhão.

---

# 4. Componentes da Arquitetura

## 4.1 Aplicativo Mobile

O aplicativo será utilizado exclusivamente pelo caminhoneiro durante sua jornada de trabalho.

Suas principais responsabilidades são:

* realizar autenticação do motorista;
* iniciar e finalizar o expediente;
* iniciar uma rota;
* capturar coordenadas GPS;
* registrar entregas;
* armazenar localmente coordenadas quando não houver internet;
* sincronizar automaticamente os dados quando a conexão retornar.

O aplicativo é a única parte do sistema que possui acesso direto ao GPS do smartphone. O servidor nunca acessa o GPS diretamente, apenas recebe as coordenadas enviadas pelo aplicativo. <FileCite ref_id="turn1file0"/>

### Informações coletadas

Cada ponto de localização deverá possuir informações semelhantes às definidas no projeto:

| Campo           | Descrição                         |
| --------------- | --------------------------------- |
| Latitude        | Posição geográfica norte/sul      |
| Longitude       | Posição geográfica leste/oeste    |
| Timestamp       | Data e hora da coleta pelo GPS    |
| Velocidade      | Velocidade informada pelo GPS     |
| Precisão        | Margem de precisão da localização |
| Status de envio | Indica se já foi sincronizado     |

Esses dados representam cada posição registrada durante a execução da rota. <FileCite ref_id="turn1file0"/>

---

## 4.2 Back-end

O Back-end representa o núcleo da aplicação.

Ele será responsável por toda a lógica do sistema, garantindo que os dados recebidos sejam válidos, armazenados corretamente e distribuídos aos usuários conectados.

As principais responsabilidades são:

* autenticação dos usuários;
* gerenciamento de motoristas;
* gerenciamento de rotas;
* gerenciamento de entregas;
* recebimento das coordenadas GPS;
* validação dos dados;
* armazenamento no banco de dados;
* sincronização de dados offline;
* comunicação em tempo real com o painel web.

O Back-end não será responsável por desenhar mapas ou capturar GPS. Sua função será centralizar e disponibilizar as informações para os demais sistemas.

---

# 5. Organização Interna do Back-end

Para facilitar a manutenção do projeto, o Back-end será dividido em módulos.

<p><a href="docs/arquitetura.html#slide-2">Abrir slide da arquitetura (2)</a></p>

**Figura 2 — Organização em camadas do Back-end.**

### API Controllers

Representam a porta de entrada da aplicação.

São responsáveis por receber as requisições enviadas pelo aplicativo e pelo painel web, encaminhando essas informações para os serviços responsáveis.

Exemplos de controladores:

* AuthController
* RotasController
* EntregasController
* LocalizacoesController

Esses controladores não devem conter regras complexas. Sua função principal é receber dados, chamar os serviços e retornar respostas.

### Services

A camada de serviços concentra toda a lógica de negócio da aplicação.

Exemplos:

* Serviço de autenticação;
* Serviço de rotas;
* Serviço de entregas;
* Serviço de localização;
* Serviço de sincronização;
* Serviço de comunicação em tempo real.

Essa organização evita que o código fique concentrado em um único local e facilita futuras alterações.

### Infrastructure

A camada de infraestrutura é responsável pela comunicação com recursos externos.

Ela realiza operações como:

* acesso ao PostgreSQL;
* persistência dos dados;
* consultas ao banco;
* integração com APIs externas;
* configuração do Entity Framework;
* comunicação com serviços geográficos quando necessário.

### Banco de Dados

O PostgreSQL será utilizado como banco de dados principal do sistema.

Ele armazenará permanentemente todas as informações importantes da aplicação.

---

# 6. Comunicação entre os Sistemas

A plataforma utilizará dois tipos principais de comunicação.

## 6.1 REST API

A API REST será utilizada para todas as operações convencionais do sistema.

Exemplos:

* login;
* cadastro de motorista;
* criação de rota;
* início de rota;
* finalização de rota;
* cadastro de entregas;
* atualização do status de entrega;
* envio de localização;
* sincronização de coordenadas offline.

Todas essas operações ocorrerão através de requisições HTTPS.

Exemplo do fluxo:

<p><a href="docs/arquitetura.html#slide-3">Abrir slide da arquitetura (3)</a></p>

**Figura 3 — Comunicação utilizando API REST.**

A API recebe a requisição, valida as informações e salva os dados antes de retornar uma resposta de sucesso ao aplicativo.

---

## 6.2 WebSocket

Enquanto a API REST trabalha com requisições tradicionais, o WebSocket será utilizado para comunicação contínua em tempo real.

Essa tecnologia permite que o servidor envie novas localizações automaticamente para o painel web imediatamente após recebê-las.

Dessa forma, o gestor não precisa atualizar a página constantemente.

Fluxo:

<p><a href="docs/arquitetura.html#slide-4">Abrir slide da arquitetura (4)</a></p>

**Figura 4 — Atualização da localização em tempo real utilizando WebSocket.**

A localização sempre passa primeiro pelo Back-end. Somente depois de validada e armazenada ela é distribuída aos usuários conectados ao painel.

---

# 7. Integração com WebSocket e SignalR

Como proposta tecnológica para o projeto, recomenda-se utilizar **SignalR** caso o Back-end seja desenvolvido em ASP.NET Core.

O SignalR é uma biblioteca que facilita a implementação de comunicação em tempo real utilizando WebSocket quando disponível.

Sua utilização permitirá que o painel receba eventos como:

* nova localização recebida;
* motorista iniciou rota;
* motorista finalizou rota;
* entrega atualizada;
* alteração de status da rota.

### Funcionamento dos grupos

Cada rota poderá possuir um grupo específico.

Exemplo:

<p><a href="docs/arquitetura.html#slide-5">Abrir slide da arquitetura (5)</a></p>

**Figura 5 — Distribuição de eventos por grupos de rota.**

Quando um gestor abrir determinada rota no painel, ele entra no grupo correspondente. Assim, apenas os usuários interessados naquela rota recebem as atualizações.

Essa abordagem reduz tráfego desnecessário e melhora a organização da comunicação em tempo real.

---

# 8. Integração com APIs de Geolocalização

O sistema trabalha com duas informações geográficas diferentes.

A primeira é a localização obtida pelo GPS do smartphone. Essa informação fornece apenas coordenadas geográficas.

Exemplo:

```json
{
  "latitude": -22.5231,
  "longitude": -48.5572
}
```

Essas coordenadas são suficientes para identificar a posição do motorista.

A segunda integração será realizada com uma **API de mapas**, responsável pela representação visual dessas coordenadas.

Como proposta de arquitetura, poderão ser utilizadas plataformas como:

* Google Maps;
* Mapbox.

Essas APIs poderão fornecer recursos como:

* exibição do mapa;
* posicionamento do marcador do caminhão;
* desenho visual da rota;
* conversão de coordenadas em endereço;
* cálculo de trajetos futuramente.

É importante destacar que a API de mapas não substitui o GPS. O GPS obtém a posição e a API de mapas apenas apresenta ou interpreta essas informações geográficas.

---

# 9. Arquitetura de Funcionamento Offline

Uma das principais exigências do projeto é permitir que o aplicativo continue registrando posições mesmo em áreas sem cobertura de internet. <FileCite ref_id="turn1file0"/>

Para isso será utilizada a estratégia **Store-and-Forward**.

O funcionamento ocorre da seguinte maneira:

<p><a href="docs/arquitetura.html#slide-6">Abrir slide da arquitetura (6)</a></p>

**Figura 6 — Funcionamento da sincronização offline.**

### Etapa 1 — Captura do GPS

O GPS registra normalmente a latitude, longitude, velocidade, precisão e horário da coleta.

Mesmo sem internet, o GPS continua funcionando porque a obtenção da localização depende dos satélites e não da rede móvel. <FileCite ref_id="turn1file0"/>

### Etapa 2 — Armazenamento local

Caso não exista conexão, o aplicativo salva cada coordenada em um banco local.

Esse banco poderá ser implementado utilizando SQLite, Room ou outra tecnologia equivalente dependendo da plataforma escolhida. <FileCite ref_id="turn1file0"/>

### Etapa 3 — Sincronização

Quando o smartphone detectar novamente uma conexão com a internet, o aplicativo agrupa as coordenadas pendentes e envia várias de uma única vez para a API.

Essa técnica reduz o consumo de internet e evita centenas de requisições individuais. <FileCite ref_id="turn1file0"/>

### Etapa 4 — Confirmação

Somente após receber uma confirmação do servidor os registros locais são considerados sincronizados e removidos do dispositivo.

Dessa forma evita-se perda de informações durante falhas de conexão.

---

# 10. Banco de Dados

O banco de dados principal será responsável por armazenar todas as informações permanentes do sistema.

A proposta inicial de modelagem é composta pelas seguintes entidades.

## Usuário

Representa qualquer pessoa autenticada no sistema.

| Campo      | Descrição           |
| ---------- | ------------------- |
| id         | Identificador       |
| nome       | Nome completo       |
| email      | E-mail              |
| senha_hash | Senha criptografada |
| tipo       | Gestor ou Motorista |

## Motorista

Armazena informações específicas do caminhoneiro.

| Campo      | Descrição           |
| ---------- | ------------------- |
| id         | Identificador       |
| usuario_id | Relação com usuário |
| telefone   | Contato             |

## Rota

Representa uma viagem realizada pelo motorista.

| Campo        | Descrição                      |
| ------------ | ------------------------------ |
| id           | Identificador                  |
| motorista_id | Motorista responsável          |
| status       | Planejada, Ativa ou Finalizada |
| data_inicio  | Horário de início              |
| data_fim     | Horário de encerramento        |

## Entrega

Representa cada encomenda vinculada a uma rota.

| Campo        | Descrição               |
| ------------ | ----------------------- |
| id           | Identificador           |
| rota_id      | Rota correspondente     |
| destinatario | Cliente                 |
| endereco     | Local da entrega        |
| status       | Pendente, Entregue etc. |

## Localização

Registra cada posição capturada durante a rota.

| Campo                 | Descrição                      |
| --------------------- | ------------------------------ |
| id                    | Identificador                  |
| rota_id               | Rota ativa                     |
| motorista_id          | Motorista                      |
| latitude              | Coordenada                     |
| longitude             | Coordenada                     |
| velocidade            | Informação GPS                 |
| precisao              | Precisão do sinal              |
| timestamp_gps         | Horário da coleta              |
| timestamp_recebimento | Horário recebido pelo servidor |

### Relacionamento entre entidades

<p><a href="docs/arquitetura.html#slide-7">Abrir slide da arquitetura (7)</a></p>

**Figura 7 — Relacionamento simplificado das principais entidades.**

Cada motorista poderá possuir diversas rotas ao longo do tempo. Cada rota poderá conter várias entregas e milhares de registros de localização.

---

# 11. Estados da Rota

Para garantir a privacidade do motorista, o rastreamento dependerá obrigatoriamente do estado da rota.

Os estados principais são:

<p><a href="docs/arquitetura.html#slide-8">Abrir slide da arquitetura (8)</a></p>

**Figura 8 — Estados possíveis de uma rota.**

### Planejada

A rota existe no sistema, porém ainda não foi iniciada.

Nesse momento nenhuma localização deve ser registrada.

### Ativa

Representa o período em que o motorista está realizando sua viagem.

Somente nesse estado o aplicativo poderá enviar coordenadas GPS e o servidor aceitará essas informações.

### Finalizada

Após o encerramento da rota o rastreamento deve ser interrompido.

Mesmo que o aplicativo tente enviar novas coordenadas, o Back-end deverá rejeitar essas informações, garantindo o cumprimento das regras de privacidade definidas no projeto. <FileCite ref_id="turn1file0"/>

---

# 12. Fluxo Completo de Localização

O processo completo de rastreamento ocorrerá da seguinte maneira.

<p><a href="docs/arquitetura.html#slide-9">Abrir slide da arquitetura (9)</a></p>

**Figura 9 — Fluxo completo do rastreamento desde o GPS até o painel web.**

1. O motorista inicia a rota.
2. O aplicativo começa a capturar coordenadas.
3. O sistema verifica se existe conexão.
4. Caso esteja offline, os dados são armazenados localmente.
5. Quando houver internet, as coordenadas são enviadas para a API.
6. O Back-end valida se a rota está ativa.
7. Os dados são armazenados no PostgreSQL.
8. Um evento é publicado pelo SignalR.
9. O painel web recebe a atualização.
10. O marcador do caminhão é atualizado no mapa.

---

# 13. Endpoints Principais da API

A API será organizada por recursos.

## Autenticação

| Método | Endpoint         | Função          |
| ------ | ---------------- | --------------- |
| POST   | /api/auth/login  | Realizar login  |
| POST   | /api/auth/logout | Encerrar sessão |

## Rotas

| Método | Endpoint                    | Função           |
| ------ | --------------------------- | ---------------- |
| POST   | /api/rotas                  | Criar rota       |
| GET    | /api/rotas                  | Listar rotas     |
| GET    | /api/rotas/{`id`}           | Consultar rota   |
| POST   | /api/rotas/{`id`}/iniciar   | Iniciar viagem   |
| POST   | /api/rotas/{`id`}/finalizar | Finalizar viagem |

## Entregas

| Método | Endpoint                    | Função             |
| ------ | --------------------------- | ------------------ |
| POST   | /api/entregas               | Criar entrega      |
| GET    | /api/rotas/{`id`}/entregas  | Consultar entregas |
| PATCH  | /api/entregas/{`id`}/status | Atualizar status   |

## Localizações

| Método | Endpoint                             | Função                    |
| ------ | ------------------------------------ | ------------------------- |
| POST   | /api/localizacoes                    | Enviar uma coordenada     |
| POST   | /api/localizacoes/batch              | Enviar várias coordenadas |
| GET    | /api/rotas/{`id`}/localizacoes       | Histórico GPS             |
| GET    | /api/rotas/{`id`}/ultima-localizacao | Última posição            |

---

# 14. Exemplo de Comunicação da API

### Envio de uma localização

```json
{
  "rotaId": 25,
  "latitude": -22.5231,
  "longitude": -48.5572,
  "velocidade": 72,
  "precisao": 8,
  "timestampGps": "2026-08-29T10:30:00"
}
```

Após validar a requisição, o servidor poderá responder:

```json
{
  "sucesso": true,
  "mensagem": "Localização registrada com sucesso."
}
```

---

# 15. Comunicação em Tempo Real

Quando uma nova localização for salva, o Back-end publicará um evento utilizando WebSocket.

Exemplo de mensagem enviada ao painel:

```json
{
  "evento": "novaLocalizacao",
  "rotaId": 25,
  "motoristaId": 8,
  "latitude": -22.5231,
  "longitude": -48.5572,
  "timestamp": "2026-08-29T10:30:00"
}
```

O painel receberá esse evento e moverá automaticamente o marcador correspondente no mapa.

Essa comunicação será independente das operações tradicionais da API REST.

---

# 16. Segurança e Privacidade

A privacidade é uma das principais restrições do projeto.

Por esse motivo, algumas regras devem ser obrigatórias no Back-end.

### Rota ativa

O servidor somente aceitará coordenadas quando a rota estiver ativa.

Caso a rota esteja finalizada, qualquer envio de localização deverá ser recusado.

### Autenticação

Todos os usuários deverão estar autenticados.

Como proposta tecnológica, poderá ser utilizado JWT para identificar cada usuário durante as requisições.

### Comunicação segura

Toda comunicação entre aplicativo, painel e servidor deverá utilizar HTTPS.

### Senhas

As senhas nunca deverão ser armazenadas em texto simples.

Apenas o hash criptográfico deverá ser salvo no banco de dados.

Essas medidas ajudam a proteger os dados pessoais dos motoristas e gestores, além de respeitar o princípio de privacidade definido no projeto. <FileCite ref_id="turn1file0"/>

---

# 17. Tecnologias Propostas

As tecnologias abaixo representam uma proposta de implementação compatível com a arquitetura apresentada.

| Camada          | Tecnologia sugerida              |
| --------------- | -------------------------------- |
| Back-end        | ASP.NET Core Web API             |
| Tempo real      | SignalR                          |
| Banco principal | PostgreSQL                       |
| ORM             | Entity Framework Core            |
| Aplicativo      | Android, Flutter ou React Native |
| Banco local     | SQLite                           |
| API de mapas    | Google Maps ou Mapbox            |
| Autenticação    | JWT                              |
| Comunicação     | HTTPS + WebSocket                |

Essas tecnologias poderão ser alteradas conforme a decisão da equipe, sem modificar o funcionamento geral da arquitetura.

---

# 18. Responsabilidade de Cada Componente

| Componente         | Responsabilidade                      |
| ------------------ | ------------------------------------- |
| Aplicativo Mobile  | Capturar GPS e registrar entregas     |
| SQLite Local       | Guardar dados quando estiver offline  |
| API REST           | Receber e validar informações         |
| Back-end           | Aplicar regras de negócio             |
| PostgreSQL         | Armazenar dados permanentemente       |
| SignalR/WebSocket  | Enviar atualizações em tempo real     |
| Painel Web         | Exibir rotas e localização            |
| Google Maps/Mapbox | Representar dados geográficos no mapa |

---

# 19. API Mockada

O protótipo funcional da API está em `GalloTracking.Api` e utiliza SQLite para persistência inicial. A estrutura usa Entity Framework Core, permitindo trocar o provider por PostgreSQL posteriormente sem alterar os controllers.

## Execução local

```bash
dotnet restore GalloTracking.sln
dotnet run --project GalloTracking.Api
```

Em ambiente de desenvolvimento, a documentação interativa fica disponível em `/swagger`. O banco `gallo.db` é criado automaticamente na primeira execução, junto com dados de demonstração.

Credenciais de desenvolvimento:

| Perfil | E-mail | Senha |
| ------ | ------ | ----- |
| Gestor | gestor@gallo.local | gallo123 |
| Motorista | motorista@gallo.local | gallo123 |

O fluxo recomendado é fazer login, iniciar a rota criada pelo seed, enviar uma localização e consultar o histórico. Localizações só são aceitas enquanto a rota estiver com status `Ativa`; o endpoint `/api/localizacoes/batch` simula a sincronização do modo offline.

O hub SignalR está disponível em `/hubs/localizacao`. Clientes autenticados podem chamar `EntrarNaRota(rotaId)` e recebem o evento `novaLocalizacao` quando uma coordenada da rota for salva.

Os testes de integração podem ser executados com:

```bash
dotnet test GalloTracking.sln
```

---

# 20. Conclusão

A arquitetura proposta organiza o sistema em três partes principais: aplicativo mobile, Back-end e painel web. O aplicativo é responsável pela captura das coordenadas GPS e pelo funcionamento offline. O Back-end centraliza toda a lógica do sistema, realiza autenticação, valida rotas, armazena informações e distribui atualizações em tempo real utilizando WebSocket. O painel web recebe essas atualizações e apresenta a localização dos motoristas em um mapa por meio de uma API geográfica.

A utilização de uma arquitetura monolítica modular permite desenvolver o MVP de maneira mais simples, mantendo uma estrutura organizada e preparada para futuras expansões. O modelo também atende às necessidades definidas no projeto, incluindo rastreamento somente durante rotas ativas, armazenamento offline com sincronização posterior e acompanhamento da localização em tempo real pelos gestores. <FileCite ref_id="turn1file0"/>
