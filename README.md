# Documentação da Arquitetura do Projeto

## Sistema de Rastreamento Logístico em Tempo Real

### 1. Introdução

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

<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 720 500" role="img" aria-label="Diagrama da arquitetura do sistema mostrando o aplicativo do motorista à esquerda conectado por HTTPS ao back-end central. O painel web à direita também se conecta ao back-end por HTTPS e recebe atualizações por WebSocket. O banco PostgreSQL fica abaixo do back-end. O aplicativo possui armazenamento local SQLite para funcionamento offline. Uma API de mapas fica conectada ao painel web para exibição geográfica.">
  <rect width="720" height="500" rx="18" fill="#FFFFFF" stroke="#D9D9D9"/>
  <rect x="36" y="150" width="150" height="170" rx="12" fill="#E8F0FE" stroke="#7AA7FF"/>
  <text x="111" y="175" font-family="Arial" font-size="16" font-weight="bold" text-anchor="middle" dominant-baseline="middle" fill="#1F3A5F">
    App Motorista
  </text>
  <rect x="51" y="190" width="120" height="24" rx="6" fill="#FFFFFF" stroke="#AAB7C4"/>
  <text x="111" y="202" font-family="Arial" font-size="11" text-anchor="middle" dominant-baseline="middle" fill="#334155">
    GPS Smartphone
  </text>
  <rect x="51" y="224" width="120" height="24" rx="6" fill="#FFFFFF" stroke="#AAB7C4"/>
  <text x="111" y="236" font-family="Arial" font-size="11" text-anchor="middle" dominant-baseline="middle" fill="#334155">
    Rota Ativa
  </text>
  <rect x="51" y="258" width="120" height="46" rx="6" fill="#D1FAE5" stroke="#34D399"/>
  <text x="111" y="270" font-family="Arial" font-size="11" font-weight="bold" text-anchor="middle" dominant-baseline="middle" fill="#065F46">
    SQLite Local
  </text>
  <text x="111" y="286" font-family="Arial" font-size="9" text-anchor="middle" dominant-baseline="middle" fill="#065F46">
    Coordenadas
  </text>
  <text x="111" y="298" font-family="Arial" font-size="9" text-anchor="middle" dominant-baseline="middle" fill="#065F46">
    Offline
  </text>
  <rect x="270" y="110" width="180" height="250" rx="14" fill="#F3F4F6" stroke="#6B7280" stroke-width="1.5" />
  <text x="360" y="136" font-family="Arial" font-size="18" font-weight="bold" text-anchor="middle" dominant-baseline="middle" fill="#111827">
    Back-end
  </text>
  <rect x="290" y="152" width="140" height="28" rx="6" fill="#FFFFFF" stroke="#9CA3AF"/>
  <text x="360" y="166" font-family="Arial" font-size="11" text-anchor="middle" dominant-baseline="middle" fill="#111827">
    API REST
  </text>
  <rect x="290" y="190" width="140" height="28" rx="6" fill="#FFFFFF" stroke="#9CA3AF"/>
  <text x="360" y="204" font-family="Arial" font-size="11" text-anchor="middle" dominant-baseline="middle" fill="#111827">
    Autenticação
  </text>
  <rect x="290" y="228" width="140" height="28" rx="6" fill="#FFFFFF" stroke="#9CA3AF"/>
  <text x="360" y="242" font-family="Arial" font-size="11" text-anchor="middle" dominant-baseline="middle" fill="#111827">
    Regras de negócio
  </text>
  <rect x="290" y="266" width="140" height="28" rx="6" fill="#DBEAFE" stroke="#60A5FA"/>
  <text x="360" y="280" font-family="Arial" font-size="11" font-weight="bold" text-anchor="middle" dominant-baseline="middle" fill="#1D4ED8">
    WebSocket
  </text>
  <text x="360" y="296" font-family="Arial" font-size="9" text-anchor="middle" dominant-baseline="middle" fill="#1D4ED8">
    SignalR
  </text>
  <rect x="290" y="310" width="140" height="34" rx="6" fill="#FFFFFF" stroke="#9CA3AF"/>
  <text x="360" y="320" font-family="Arial" font-size="10" text-anchor="middle" dominant-baseline="middle" fill="#111827">
    Sincronização
  </text>
  <text x="360" y="334" font-family="Arial" font-size="10" text-anchor="middle" dominant-baseline="middle" fill="#111827">
    Offline
  </text>
  <rect x="534" y="150" width="150" height="170" rx="12" fill="#FCE7F3" stroke="#F472B6"/>
  <text x="609" y="175" font-family="Arial" font-size="16" font-weight="bold" text-anchor="middle" dominant-baseline="middle" fill="#831843">
    Painel Web
  </text>
  <rect x="549" y="190" width="120" height="24" rx="6" fill="#FFFFFF" stroke="#D1A3C7"/>
  <text x="609" y="202" font-family="Arial" font-size="11" text-anchor="middle" dominant-baseline="middle" fill="#831843">
    Gestor
  </text>
  <rect x="549" y="224" width="120" height="24" rx="6" fill="#FFFFFF" stroke="#D1A3C7"/>
  <text x="609" y="236" font-family="Arial" font-size="11" text-anchor="middle" dominant-baseline="middle" fill="#831843">
    Mapa
  </text>
  <rect x="549" y="258" width="120" height="46" rx="6" fill="#FDE68A" stroke="#F59E0B"/>
  <text x="609" y="270" font-family="Arial" font-size="11" font-weight="bold" text-anchor="middle" dominant-baseline="middle" fill="#92400E">
    Google Maps
  </text>
  <text x="609" y="286" font-family="Arial" font-size="9" text-anchor="middle" dominant-baseline="middle" fill="#92400E">
    ou Mapbox
  </text>
  <rect x="255" y="404" width="210" height="58" rx="12" fill="#D1FAE5" stroke="#10B981"/>
  <text x="360" y="425" font-family="Arial" font-size="16" font-weight="bold" text-anchor="middle" dominant-baseline="middle" fill="#065F46">
    PostgreSQL
  </text>
  <text x="360" y="440" font-family="Arial" font-size="10" text-anchor="middle" dominant-baseline="middle" fill="#065F46">
    Rotas • Entregas
  </text>
  <text x="360" y="452" font-family="Arial" font-size="10" text-anchor="middle" dominant-baseline="middle" fill="#065F46">
    Localizações • Usuários
  </text>
  <line x1="186" y1="235" x2="270" y2="235" stroke="#2563EB" stroke-width="2" stroke-linecap="round"/>
  <polygon points="262,229 270,235 262,241" fill="#2563EB"/>
  <text x="228" y="220" font-family="Arial" font-size="10" text-anchor="middle" dominant-baseline="middle" fill="#1D4ED8">
    HTTPS
  </text>
  <line x1="450" y1="210" x2="534" y2="210" stroke="#64748B" stroke-width="2" stroke-linecap="round"/>
  <polygon points="526,204 534,210 526,216" fill="#64748B"/>
  <text x="492" y="195" font-family="Arial" font-size="10" text-anchor="middle" dominant-baseline="middle" fill="#475569">
    HTTPS
  </text>
  <line x1="450" y1="280" x2="534" y2="280" stroke="#7C3AED" stroke-width="2" stroke-dasharray="6 5" stroke-linecap="round"/>
  <polygon points="526,274 534,280 526,286" fill="#7C3AED"/>
  <text x="492" y="265" font-family="Arial" font-size="10" text-anchor="middle" dominant-baseline="middle" fill="#6D28D9">
    WebSocket
  </text>
  <line x1="360" y1="360" x2="360" y2="404" stroke="#059669" stroke-width="2" stroke-linecap="round"/>
  <polygon points="354,396 360,404 366,396" fill="#059669"/>
  <line x1="111" y1="320" x2="111" y2="348" stroke="#10B981" stroke-width="1.5" stroke-dasharray="4 4" stroke-linecap="round"/>
  <text x="123" y="338" font-family="Arial" font-size="8" dominant-baseline="middle" fill="#047857">
    Offline
  </text>
</svg>

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

<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 720 420" role="img" aria-label="Diagrama em camadas da organização interna do back-end com API Controllers no topo, camada de serviços no meio, camada de infraestrutura abaixo e banco PostgreSQL na base. Os serviços incluem autenticação, rotas, entregas, localização, sincronização e tempo real.">
  <rect width="720" height="420" rx="18" fill="#FFFFFF" stroke="#D9D9D9"/>
  <rect x="170" y="28" width="380" height="52" rx="10" fill="#DBEAFE" stroke="#60A5FA"/>
  <text x="360" y="49" font-family="Arial" font-size="18" font-weight="bold" text-anchor="middle" fill="#1D4ED8">
    API Controllers
  </text>
  <text x="360" y="65" font-family="Arial" font-size="11" text-anchor="middle" fill="#1E40AF">
    Endpoints REST • Entrada das requisições
  </text>
  <line x1="360" y1="80" x2="360" y2="96" stroke="#64748B" stroke-width="1.5" stroke-dasharray="4 4"/>
  <rect x="110" y="96" width="500" height="138" rx="12" fill="#F3F4F6" stroke="#9CA3AF"/>
  <text x="360" y="118" font-family="Arial" font-size="17" font-weight="bold" text-anchor="middle" fill="#111827">
    Camada de Serviços
  </text>
  <rect x="128" y="136" width="104" height="34" rx="6" fill="#FFFFFF" stroke="#D1D5DB"/>
  <text x="180" y="150" font-family="Arial" font-size="10" font-weight="bold" text-anchor="middle" fill="#111827">
    Auth
  </text>
  <text x="180" y="162" font-family="Arial" font-size="9" text-anchor="middle" fill="#374151">
    Usuários
  </text>
  <rect x="244" y="136" width="104" height="34" rx="6" fill="#FFFFFF" stroke="#D1D5DB"/>
  <text x="296" y="150" font-family="Arial" font-size="10" font-weight="bold" text-anchor="middle" fill="#111827">
    Rotas
  </text>
  <text x="296" y="162" font-family="Arial" font-size="9" text-anchor="middle" fill="#374151">
    Jornada
  </text>
  <rect x="360" y="136" width="104" height="34" rx="6" fill="#FFFFFF" stroke="#D1D5DB"/>
  <text x="412" y="150" font-family="Arial" font-size="10" font-weight="bold" text-anchor="middle" fill="#111827">
    Entregas
  </text>
  <text x="412" y="162" font-family="Arial" font-size="9" text-anchor="middle" fill="#374151">
    Status
  </text>
  <rect x="476" y="136" width="116" height="34" rx="6" fill="#FFFFFF" stroke="#D1D5DB"/>
  <text x="534" y="150" font-family="Arial" font-size="10" font-weight="bold" text-anchor="middle" fill="#111827">
    Localização
  </text>
  <text x="534" y="162" font-family="Arial" font-size="9" text-anchor="middle" fill="#374151">
    GPS
  </text>
  <rect x="186" y="182" width="160" height="34" rx="6" fill="#D1FAE5" stroke="#34D399"/>
  <text x="266" y="196" font-family="Arial" font-size="10" font-weight="bold" text-anchor="middle" fill="#065F46">
    Sincronização
  </text>
  <text x="266" y="208" font-family="Arial" font-size="9" text-anchor="middle" fill="#065F46">
    Offline Batch
  </text>
  <rect x="374" y="182" width="160" height="34" rx="6" fill="#E9D5FF" stroke="#A78BFA"/>
  <text x="454" y="196" font-family="Arial" font-size="10" font-weight="bold" text-anchor="middle" fill="#5B21B6">
    Tempo Real
  </text>
  <text x="454" y="208" font-family="Arial" font-size="9" text-anchor="middle" fill="#5B21B6">
    SignalR Hub
  </text>
  <line x1="360" y1="234" x2="360" y2="250" stroke="#64748B" stroke-width="1.5" stroke-dasharray="4 4"/>
  <rect x="170" y="250" width="380" height="74" rx="10" fill="#FEF3C7" stroke="#F59E0B"/>
  <text x="360" y="272" font-family="Arial" font-size="16" font-weight="bold" text-anchor="middle" fill="#92400E">
    Infraestrutura
  </text>
  <text x="360" y="288" font-family="Arial" font-size="11" text-anchor="middle" fill="#92400E">
    Repositórios • Entity Framework • Integrações externas
  </text>
  <text x="360" y="302" font-family="Arial" font-size="11" text-anchor="middle" fill="#92400E">
    Persistência • APIs geográficas
  </text>
  <line x1="360" y1="324" x2="360" y2="340" stroke="#64748B" stroke-width="1.5" stroke-dasharray="4 4"/>
  <rect x="220" y="340" width="280" height="52" rx="10" fill="#D1FAE5" stroke="#10B981"/>
  <text x="360" y="361" font-family="Arial" font-size="17" font-weight="bold" text-anchor="middle" fill="#065F46">
    PostgreSQL
  </text>
  <text x="360" y="377" font-family="Arial" font-size="10" text-anchor="middle" fill="#065F46">
    Persistência permanente dos dados
  </text>
</svg>

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

<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 720 180" role="img" aria-label="Fluxo REST mostrando aplicativo enviando uma requisição HTTPS para a API, a API validando e salvando no banco PostgreSQL e retornando uma resposta de sucesso ao aplicativo.">
  <rect width="720" height="180" rx="16" fill="#FFFFFF" stroke="#D9D9D9"/>
  <rect x="28" y="58" width="150" height="64" rx="10" fill="#E8F0FE" stroke="#7AA7FF"/>
  <text x="103" y="78" font-family="Arial" font-size="14" font-weight="bold" text-anchor="middle" fill="#1F3A5F">
    Aplicativo
  </text>
  <text x="103" y="94" font-family="Arial" font-size="10" text-anchor="middle" fill="#1F3A5F">
    Motorista
  </text>
  <line x1="178" y1="90" x2="270" y2="90" stroke="#2563EB" stroke-width="2" stroke-linecap="round"/>
  <polygon points="262,84 270,90 262,96" fill="#2563EB"/>
  <text x="224" y="78" font-family="Arial" font-size="10" text-anchor="middle" fill="#1D4ED8">
    HTTPS
  </text>
  <text x="224" y="102" font-family="Arial" font-size="9" text-anchor="middle" fill="#1D4ED8">
    POST /localizações
  </text>
  <rect x="270" y="46" width="180" height="88" rx="10" fill="#F3F4F6" stroke="#9CA3AF"/>
  <text x="360" y="66" font-family="Arial" font-size="15" font-weight="bold" text-anchor="middle" fill="#111827">
    API REST
  </text>
  <text x="360" y="84" font-family="Arial" font-size="10" text-anchor="middle" fill="#374151">
    Validação
  </text>
  <text x="360" y="98" font-family="Arial" font-size="10" text-anchor="middle" fill="#374151">
    Regras de negócio
  </text>
  <text x="360" y="112" font-family="Arial" font-size="10" text-anchor="middle" fill="#374151">
    Persistência
  </text>
  <line x1="450" y1="90" x2="542" y2="90" stroke="#059669" stroke-width="2" stroke-linecap="round"/>
  <polygon points="534,84 542,90 534,96" fill="#059669"/>
  <rect x="542" y="58" width="150" height="64" rx="10" fill="#D1FAE5" stroke="#34D399"/>
  <text x="617" y="78" font-family="Arial" font-size="14" font-weight="bold" text-anchor="middle" fill="#065F46">
    PostgreSQL
  </text>
  <text x="617" y="94" font-family="Arial" font-size="10" text-anchor="middle" fill="#065F46">
    Salva dados
  </text>
  <path d="M 270 126 C 220 150, 150 150, 103 122" fill="none" stroke="#64748B" stroke-width="1.5" stroke-dasharray="5 5"/>
  <polygon points="109,118 101,122 109,126" fill="#64748B"/>
  <text x="185" y="160" font-family="Arial" font-size="9" text-anchor="middle" fill="#475569">
    Resposta HTTP 200/201
  </text>
</svg>

**Figura 3 — Comunicação utilizando API REST.**

A API recebe a requisição, valida as informações e salva os dados antes de retornar uma resposta de sucesso ao aplicativo.

---

## 6.2 WebSocket

Enquanto a API REST trabalha com requisições tradicionais, o WebSocket será utilizado para comunicação contínua em tempo real.

Essa tecnologia permite que o servidor envie novas localizações automaticamente para o painel web imediatamente após recebê-las.

Dessa forma, o gestor não precisa atualizar a página constantemente.

Fluxo:

<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 720 220" role="img" aria-label="Fluxo de tempo real mostrando motorista enviando localização por REST para o back-end, o back-end salvando no banco e publicando por SignalR WebSocket para o painel web que atualiza o marcador no mapa.">
  <rect width="720" height="220" rx="16" fill="#FFFFFF" stroke="#D9D9D9"/>
  <rect x="24" y="74" width="140" height="72" rx="10" fill="#E8F0FE" stroke="#7AA7FF"/>
  <text x="94" y="94" font-family="Arial" font-size="13" font-weight="bold" text-anchor="middle" fill="#1F3A5F">
    Motorista
  </text>
  <text x="94" y="110" font-family="Arial" font-size="10" text-anchor="middle" fill="#1F3A5F">
    App envia GPS
  </text>
  <line x1="164" y1="110" x2="258" y2="110" stroke="#2563EB" stroke-width="2" stroke-linecap="round"/>
  <polygon points="250,104 258,110 250,116" fill="#2563EB"/>
  <text x="211" y="98" font-family="Arial" font-size="9" text-anchor="middle" fill="#1D4ED8">
    REST
  </text>
  <rect x="258" y="54" width="204" height="112" rx="12" fill="#F3F4F6" stroke="#9CA3AF"/>
  <text x="360" y="74" font-family="Arial" font-size="15" font-weight="bold" text-anchor="middle" fill="#111827">
    Back-end
  </text>
  <rect x="278" y="86" width="164" height="22" rx="5" fill="#FFFFFF" stroke="#D1D5DB"/>
  <text x="360" y="101" font-family="Arial" font-size="9" text-anchor="middle" fill="#374151">
    Valida e salva localização
  </text>
  <rect x="278" y="118" width="164" height="30" rx="5" fill="#E9D5FF" stroke="#A78BFA"/>
  <text x="360" y="131" font-family="Arial" font-size="10" font-weight="bold" text-anchor="middle" fill="#5B21B6">
    SignalR Hub
  </text>
  <text x="360" y="141" font-family="Arial" font-size="8" text-anchor="middle" fill="#6D28D9">
    Publica evento
  </text>
  <line x1="462" y1="110" x2="556" y2="110" stroke="#7C3AED" stroke-width="2" stroke-dasharray="6 5" stroke-linecap="round"/>
  <polygon points="548,104 556,110 548,116" fill="#7C3AED"/>
  <text x="509" y="98" font-family="Arial" font-size="9" text-anchor="middle" fill="#6D28D9">
    WebSocket
  </text>
  <rect x="556" y="74" width="140" height="72" rx="10" fill="#FCE7F3" stroke="#F472B6"/>
  <text x="626" y="94" font-family="Arial" font-size="13" font-weight="bold" text-anchor="middle" fill="#831843">
    Painel Web
  </text>
  <text x="626" y="110" font-family="Arial" font-size="10" text-anchor="middle" fill="#831843">
    Atualiza mapa
  </text>
  <line x1="360" y1="166" x2="360" y2="192" stroke="#059669" stroke-width="1.5" stroke-dasharray="4 4"/>
  <rect x="290" y="192" width="140" height="18" rx="4" fill="#D1FAE5" stroke="#34D399"/>
  <text x="360" y="205" font-family="Arial" font-size="9" text-anchor="middle" fill="#065F46">
    PostgreSQL
  </text>
</svg>

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

<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 720 260" role="img" aria-label="Diagrama mostrando dois gestores conectados ao grupo Rota 25 por SignalR e um motorista enviando localização para o back-end, que distribui o evento apenas para o grupo daquela rota.">
  <rect width="720" height="260" rx="16" fill="#FFFFFF" stroke="#D9D9D9"/>
  <rect x="28" y="92" width="120" height="56" rx="10" fill="#E8F0FE" stroke="#7AA7FF"/>
  <text x="88" y="112" font-family="Arial" font-size="12" font-weight="bold" text-anchor="middle" fill="#1F3A5F">
    Motorista
  </text>
  <text x="88" y="126" font-family="Arial" font-size="9" text-anchor="middle" fill="#1F3A5F">
    Envia GPS
  </text>
  <line x1="148" y1="120" x2="250" y2="120" stroke="#2563EB" stroke-width="2" stroke-linecap="round"/>
  <polygon points="242,114 250,120 242,126" fill="#2563EB"/>
  <rect x="250" y="54" width="220" height="132" rx="12" fill="#F3F4F6" stroke="#9CA3AF"/>
  <text x="360" y="74" font-family="Arial" font-size="15" font-weight="bold" text-anchor="middle" fill="#111827">
    Back-end
  </text>
  <rect x="272" y="88" width="176" height="28" rx="6" fill="#FFFFFF" stroke="#D1D5DB"/>
  <text x="360" y="106" font-family="Arial" font-size="10" text-anchor="middle" fill="#374151">
    API recebe localização
  </text>
  <rect x="272" y="126" width="176" height="40" rx="6" fill="#E9D5FF" stroke="#A78BFA"/>
  <text x="360" y="141" font-family="Arial" font-size="10" font-weight="bold" text-anchor="middle" fill="#5B21B6">
    Grupo SignalR
  </text>
  <text x="360" y="153" font-family="Arial" font-size="10" text-anchor="middle" fill="#6D28D9">
    rota-25
  </text>
  <line x1="470" y1="92" x2="556" y2="74" stroke="#7C3AED" stroke-width="2" stroke-dasharray="6 5" stroke-linecap="round"/>
  <polygon points="548,68 556,74 546,78" fill="#7C3AED"/>
  <line x1="470" y1="148" x2="556" y2="186" stroke="#7C3AED" stroke-width="2" stroke-dasharray="6 5" stroke-linecap="round"/>
  <polygon points="548,182 556,186 546,190" fill="#7C3AED"/>
  <rect x="556" y="42" width="136" height="64" rx="10" fill="#FCE7F3" stroke="#F472B6"/>
  <text x="624" y="62" font-family="Arial" font-size="12" font-weight="bold" text-anchor="middle" fill="#831843">
    Gestor A
  </text>
  <text x="624" y="76" font-family="Arial" font-size="9" text-anchor="middle" fill="#831843">
    Grupo rota-25
  </text>
  <rect x="556" y="154" width="136" height="64" rx="10" fill="#FCE7F3" stroke="#F472B6"/>
  <text x="624" y="174" font-family="Arial" font-size="12" font-weight="bold" text-anchor="middle" fill="#831843">
    Gestor B
  </text>
  <text x="624" y="188" font-family="Arial" font-size="9" text-anchor="middle" fill="#831843">
    Grupo rota-25
  </text>
  <text x="360" y="205" font-family="Arial" font-size="10" text-anchor="middle" fill="#374151">
    Apenas usuários daquela rota recebem o evento
  </text>
</svg>

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

<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 720 360" role="img" aria-label="Fluxograma do modo offline mostrando GPS capturando localização, decisão de internet disponível, envio imediato para API quando sim e armazenamento em SQLite quando não. Depois, quando a internet retorna, ocorre sincronização em lote e confirmação para limpeza dos registros locais.">
  <rect width="720" height="360" rx="18" fill="#FFFFFF" stroke="#D9D9D9"/>
  <rect x="260" y="20" width="200" height="42" rx="10" fill="#E8F0FE" stroke="#7AA7FF"/>
  <text x="360" y="38" font-family="Arial" font-size="14" font-weight="bold" text-anchor="middle" fill="#1F3A5F">
    GPS captura
  </text>
  <text x="360" y="50" font-family="Arial" font-size="10" text-anchor="middle" fill="#1F3A5F">
    nova localização
  </text>
  <line x1="360" y1="62" x2="360" y2="82" stroke="#64748B" stroke-width="1.5" stroke-dasharray="4 4"/>
  <polygon points="354,74 360,82 366,74" fill="#64748B"/>
  <polygon points="360,82 430,122 360,162 290,122" fill="#F3F4F6" stroke="#9CA3AF"/>
  <text x="360" y="118" font-family="Arial" font-size="12" font-weight="bold" text-anchor="middle" fill="#111827">
    Internet?
  </text>
  <line x1="290" y1="122" x2="150" y2="122" stroke="#DC2626" stroke-width="1.5" stroke-linecap="round"/>
  <polygon points="158,116 150,122 158,128" fill="#DC2626"/>
  <text x="220" y="110" font-family="Arial" font-size="10" text-anchor="middle" fill="#B91C1C">
    Não
  </text>
  <rect x="24" y="92" width="126" height="60" rx="10" fill="#FEE2E2" stroke="#F87171"/>
  <text x="87" y="110" font-family="Arial" font-size="12" font-weight="bold" text-anchor="middle" fill="#991B1B">
    SQLite
  </text>
  <text x="87" y="124" font-family="Arial" font-size="9" text-anchor="middle" fill="#991B1B">
    Salva ponto
  </text>
  <text x="87" y="136" font-family="Arial" font-size="9" text-anchor="middle" fill="#991B1B">
    Pendente
  </text>
  <line x1="360" y1="162" x2="360" y2="182" stroke="#64748B" stroke-width="1.5" stroke-dasharray="4 4"/>
  <polygon points="354,174 360,182 366,174" fill="#64748B"/>
  <line x1="430" y1="122" x2="570" y2="122" stroke="#16A34A" stroke-width="1.5" stroke-linecap="round"/>
  <polygon points="562,116 570,122 562,128" fill="#16A34A"/>
  <text x="500" y="110" font-family="Arial" font-size="10" text-anchor="middle" fill="#166534">
    Sim
  </text>
  <rect x="570" y="92" width="126" height="60" rx="10" fill="#D1FAE5" stroke="#34D399"/>
  <text x="633" y="110" font-family="Arial" font-size="12" font-weight="bold" text-anchor="middle" fill="#065F46">
    API
  </text>
  <text x="633" y="124" font-family="Arial" font-size="9" text-anchor="middle" fill="#065F46">
    Envio imediato
  </text>
  <text x="633" y="136" font-family="Arial" font-size="9" text-anchor="middle" fill="#065F46">
    HTTP POST
  </text>
  <rect x="220" y="182" width="280" height="54" rx="10" fill="#FEF3C7" stroke="#F59E0B"/>
  <text x="360" y="202" font-family="Arial" font-size="13" font-weight="bold" text-anchor="middle" fill="#92400E">
    Internet retorna
  </text>
  <text x="360" y="218" font-family="Arial" font-size="10" text-anchor="middle" fill="#92400E">
    Inicia sincronização automática
  </text>
  <line x1="87" y1="152" x2="250" y2="209" stroke="#F59E0B" stroke-width="1.5" stroke-dasharray="5 5"/>
  <polygon points="242,203 250,209 240,213" fill="#F59E0B"/>
  <line x1="360" y1="236" x2="360" y2="256" stroke="#64748B" stroke-width="1.5" stroke-dasharray="4 4"/>
  <polygon points="354,248 360,256 366,248" fill="#64748B"/>
  <rect x="220" y="256" width="280" height="78" rx="10" fill="#DBEAFE" stroke="#60A5FA"/>
  <text x="360" y="276" font-family="Arial" font-size="13" font-weight="bold" text-anchor="middle" fill="#1D4ED8">
    Envio em lote
  </text>
  <text x="360" y="292" font-family="Arial" font-size="10" text-anchor="middle" fill="#1E40AF">
    Batch de coordenadas pendentes
  </text>
  <text x="360" y="306" font-family="Arial" font-size="10" text-anchor="middle" fill="#1E40AF">
    Servidor confirma recebimento
  </text>
  <text x="360" y="320" font-family="Arial" font-size="10" text-anchor="middle" fill="#1E40AF">
    Registros locais são removidos
  </text>
</svg>

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

<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 720 280" role="img" aria-label="Diagrama entidade relacionamento simplificado mostrando Usuário ligado um para um com Motorista, Motorista ligado um para muitos com Rota, e Rota ligada um para muitos com Entrega e Localização.">
  <rect width="720" height="280" rx="16" fill="#FFFFFF" stroke="#D9D9D9"/>
  <rect x="36" y="104" width="130" height="56" rx="10" fill="#DBEAFE" stroke="#60A5FA"/>
  <text x="101" y="124" font-family="Arial" font-size="13" font-weight="bold" text-anchor="middle" fill="#1D4ED8">
    Usuário
  </text>
  <text x="101" y="138" font-family="Arial" font-size="9" text-anchor="middle" fill="#1E40AF">
    autenticação
  </text>
  <line x1="166" y1="132" x2="220" y2="132" stroke="#64748B" stroke-width="1.5"/>
  <text x="193" y="122" font-family="Arial" font-size="9" text-anchor="middle" fill="#374151">
    1:1
  </text>
  <rect x="220" y="104" width="130" height="56" rx="10" fill="#E0F2FE" stroke="#38BDF8"/>
  <text x="285" y="124" font-family="Arial" font-size="13" font-weight="bold" text-anchor="middle" fill="#075985">
    Motorista
  </text>
  <text x="285" y="138" font-family="Arial" font-size="9" text-anchor="middle" fill="#075985">
    perfil
  </text>
  <line x1="350" y1="132" x2="404" y2="132" stroke="#64748B" stroke-width="1.5"/>
  <text x="377" y="122" font-family="Arial" font-size="9" text-anchor="middle" fill="#374151">
    1:N
  </text>
  <rect x="404" y="104" width="130" height="56" rx="10" fill="#F3F4F6" stroke="#9CA3AF"/>
  <text x="469" y="124" font-family="Arial" font-size="13" font-weight="bold" text-anchor="middle" fill="#111827">
    Rota
  </text>
  <text x="469" y="138" font-family="Arial" font-size="9" text-anchor="middle" fill="#374151">
    viagem
  </text>
  <line x1="469" y1="160" x2="469" y2="178" stroke="#64748B" stroke-width="1.5" stroke-dasharray="4 4"/>
  <line x1="469" y1="178" x2="394" y2="190" stroke="#64748B" stroke-width="1.5" stroke-dasharray="4 4"/>
  <polygon points="394,196 402,188 402,198" fill="#64748B"/>
  <line x1="469" y1="178" x2="564" y2="190" stroke="#64748B" stroke-width="1.5" stroke-dasharray="4 4"/>
  <polygon points="564,196 556,188 556,198" fill="#64748B"/>
  <text x="428" y="174" font-family="Arial" font-size="9" text-anchor="middle" fill="#374151">
    1:N
  </text>
  <text x="516" y="174" font-family="Arial" font-size="9" text-anchor="middle" fill="#374151">
    1:N
  </text>
  <rect x="334" y="196" width="120" height="52" rx="10" fill="#FDE68A" stroke="#F59E0B"/>
  <text x="394" y="214" font-family="Arial" font-size="12" font-weight="bold" text-anchor="middle" fill="#92400E">
    Entrega
  </text>
  <text x="394" y="226" font-family="Arial" font-size="9" text-anchor="middle" fill="#92400E">
    encomendas
  </text>
  <rect x="484" y="196" width="160" height="52" rx="10" fill="#D1FAE5" stroke="#34D399"/>
  <text x="564" y="214" font-family="Arial" font-size="12" font-weight="bold" text-anchor="middle" fill="#065F46">
    Localização
  </text>
  <text x="564" y="226" font-family="Arial" font-size="9" text-anchor="middle" fill="#065F46">
    histórico GPS
  </text>
</svg>

**Figura 7 — Relacionamento simplificado das principais entidades.**

Cada motorista poderá possuir diversas rotas ao longo do tempo. Cada rota poderá conter várias entregas e milhares de registros de localização.

---

# 11. Estados da Rota

Para garantir a privacidade do motorista, o rastreamento dependerá obrigatoriamente do estado da rota.

Os estados principais são:

<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 720 140" role="img" aria-label="Fluxo de estados da rota mostrando Planejada seguindo para Ativa e depois Finalizada. Abaixo de Ativa aparece GPS permitido em verde e abaixo de Finalizada aparece rastreamento encerrado em vermelho.">
  <rect width="720" height="140" rx="16" fill="#FFFFFF" stroke="#D9D9D9"/>
  <rect x="36" y="34" width="150" height="42" rx="8" fill="#E5E7EB" stroke="#9CA3AF"/>
  <text x="111" y="59" font-family="Arial" font-size="13" font-weight="bold" text-anchor="middle" fill="#374151">
    Planejada
  </text>
  <line x1="186" y1="55" x2="285" y2="55" stroke="#64748B" stroke-width="1.5"/>
  <polygon points="277,49 285,55 277,61" fill="#64748B"/>
  <rect x="285" y="26" width="150" height="58" rx="10" fill="#D1FAE5" stroke="#10B981"/>
  <text x="360" y="48" font-family="Arial" font-size="14" font-weight="bold" text-anchor="middle" fill="#065F46">
    Ativa
  </text>
  <text x="360" y="64" font-family="Arial" font-size="10" text-anchor="middle" fill="#065F46">
    GPS permitido
  </text>
  <line x1="435" y1="55" x2="534" y2="55" stroke="#64748B" stroke-width="1.5"/>
  <polygon points="526,49 534,55 526,61" fill="#64748B"/>
  <rect x="534" y="34" width="150" height="42" rx="8" fill="#FEE2E2" stroke="#F87171"/>
  <text x="609" y="53" font-family="Arial" font-size="13" font-weight="bold" text-anchor="middle" fill="#991B1B">
    Finalizada
  </text>
  <text x="609" y="66" font-family="Arial" font-size="9" text-anchor="middle" fill="#991B1B">
    Rastreamento encerrado
  </text>
</svg>

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

<svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 720 460" role="img" aria-label="Fluxograma completo mostrando início da rota, GPS capturando coordenadas, decisão sobre internet, armazenamento local se offline, sincronização em lote quando a internet volta ou envio imediato por REST se online, validação pelo back-end, armazenamento no PostgreSQL, publicação por SignalR e atualização do painel web no mapa.">
  <rect width="720" height="460" rx="18" fill="#FFFFFF" stroke="#D9D9D9"/>
  <rect x="250" y="18" width="220" height="36" rx="10" fill="#DBEAFE" stroke="#60A5FA"/>
  <text x="360" y="40" font-family="Arial" font-size="14" font-weight="bold" text-anchor="middle" fill="#1D4ED8">
    Iniciar rota
  </text>
  <line x1="360" y1="54" x2="360" y2="70" stroke="#64748B" stroke-width="1.5" stroke-dasharray="4 4"/>
  <polygon points="354,62 360,70 366,62" fill="#64748B"/>
  <rect x="240" y="70" width="240" height="44" rx="10" fill="#E8F0FE" stroke="#7AA7FF"/>
  <text x="360" y="88" font-family="Arial" font-size="13" font-weight="bold" text-anchor="middle" fill="#1F3A5F">
    GPS captura coordenadas
  </text>
  <text x="360" y="101" font-family="Arial" font-size="9" text-anchor="middle" fill="#1F3A5F">
    latitude • longitude • precisão
  </text>
  <line x1="360" y1="114" x2="360" y2="130" stroke="#64748B" stroke-width="1.5" stroke-dasharray="4 4"/>
  <polygon points="354,122 360,130 366,122" fill="#64748B"/>
  <polygon points="360,130 430,170 360,210 290,170" fill="#F3F4F6" stroke="#9CA3AF"/>
  <text x="360" y="166" font-family="Arial" font-size="12" font-weight="bold" text-anchor="middle" fill="#111827">
    Possui internet?
  </text>
  <line x1="290" y1="170" x2="130" y2="170" stroke="#DC2626" stroke-width="1.5" stroke-linecap="round"/>
  <polygon points="138,164 130,170 138,176" fill="#DC2626"/>
  <text x="210" y="158" font-family="Arial" font-size="10" text-anchor="middle" fill="#B91C1C">
    Não
  </text>
  <rect x="24" y="142" width="106" height="56" rx="10" fill="#FEE2E2" stroke="#F87171"/>
  <text x="77" y="160" font-family="Arial" font-size="12" font-weight="bold" text-anchor="middle" fill="#991B1B">
    SQLite
  </text>
  <text x="77" y="174" font-family="Arial" font-size="9" text-anchor="middle" fill="#991B1B">
    Salva pendente
  </text>
  <line x1="77" y1="198" x2="220" y2="258" stroke="#F59E0B" stroke-width="1.5" stroke-dasharray="5 5"/>
  <polygon points="212,252 220,258 210,262" fill="#F59E0B"/>
  <rect x="170" y="248" width="180" height="42" rx="8" fill="#FEF3C7" stroke="#F59E0B"/>
  <text x="260" y="266" font-family="Arial" font-size="11" font-weight="bold" text-anchor="middle" fill="#92400E">
    Sincronização
  </text>
  <text x="260" y="278" font-family="Arial" font-size="9" text-anchor="middle" fill="#92400E">
    Envio em lote
  </text>
  <line x1="430" y1="170" x2="590" y2="170" stroke="#16A34A" stroke-width="1.5" stroke-linecap="round"/>
  <polygon points="582,164 590,170 582,176" fill="#16A34A"/>
  <text x="510" y="158" font-family="Arial" font-size="10" text-anchor="middle" fill="#166534">
    Sim
  </text>
  <rect x="590" y="142" width="106" height="56" rx="10" fill="#D1FAE5" stroke="#34D399"/>
  <text x="643" y="160" font-family="Arial" font-size="12" font-weight="bold" text-anchor="middle" fill="#065F46">
    REST API
  </text>
  <text x="643" y="174" font-family="Arial" font-size="9" text-anchor="middle" fill="#065F46">
    POST GPS
  </text>
  <line x1="260" y1="290" x2="360" y2="320" stroke="#64748B" stroke-width="1.5" stroke-dasharray="4 4"/>
  <polygon points="352,314 360,320 350,324" fill="#64748B"/>
  <line x1="643" y1="198" x2="430" y2="320" stroke="#64748B" stroke-width="1.5" stroke-dasharray="4 4"/>
  <polygon points="438,314 430,320 440,322" fill="#64748B"/>
  <rect x="250" y="320" width="220" height="46" rx="10" fill="#F3F4F6" stroke="#9CA3AF"/>
  <text x="360" y="338" font-family="Arial" font-size="13" font-weight="bold" text-anchor="middle" fill="#111827">
    Back-end valida
  </text>
  <text x="360" y="351" font-family="Arial" font-size="9" text-anchor="middle" fill="#374151">
    rota ativa + dados válidos
  </text>
  <line x1="360" y1="366" x2="360" y2="382" stroke="#64748B" stroke-width="1.5" stroke-dasharray="4 4"/>
  <polygon points="354,374 360,382 366,374" fill="#64748B"/>
  <rect x="250" y="382" width="220" height="28" rx="8" fill="#D1FAE5" stroke="#34D399"/>
  <text x="360" y="400" font-family="Arial" font-size="11" font-weight="bold" text-anchor="middle" fill="#065F46">
    PostgreSQL salva histórico
  </text>
  <line x1="470" y1="396" x2="560" y2="396" stroke="#7C3AED" stroke-width="1.5" stroke-dasharray="6 5"/>
  <polygon points="552,390 560,396 552,402" fill="#7C3AED"/>
  <rect x="560" y="374" width="136" height="44" rx="10" fill="#FCE7F3" stroke="#F472B6"/>
  <text x="628" y="390" font-family="Arial" font-size="11" font-weight="bold" text-anchor="middle" fill="#831843">
    Painel Web
  </text>
  <text x="628" y="402" font-family="Arial" font-size="9" text-anchor="middle" fill="#831843">
    Mapa atualizado
  </text>
</svg>

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

# 20. API Mockada

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

# 19. Conclusão

A arquitetura proposta organiza o sistema em três partes principais: aplicativo mobile, Back-end e painel web. O aplicativo é responsável pela captura das coordenadas GPS e pelo funcionamento offline. O Back-end centraliza toda a lógica do sistema, realiza autenticação, valida rotas, armazena informações e distribui atualizações em tempo real utilizando WebSocket. O painel web recebe essas atualizações e apresenta a localização dos motoristas em um mapa por meio de uma API geográfica.

A utilização de uma arquitetura monolítica modular permite desenvolver o MVP de maneira mais simples, mantendo uma estrutura organizada e preparada para futuras expansões. O modelo também atende às necessidades definidas no projeto, incluindo rastreamento somente durante rotas ativas, armazenamento offline com sincronização posterior e acompanhamento da localização em tempo real pelos gestores. <FileCite ref_id="turn1file0"/>
