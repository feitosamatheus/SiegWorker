
# ?? SiegWorker - Processador de Documentos Fiscais

O **SiegWorker** é um microserviço **.NET** (Worker Service) que atua como a camada de **processamento assíncrono** e persistência de **documentos fiscais** (NFe/CTe/NFSe).

Ele **consome eventos** do **RabbitMQ** (publicados pelo projeto `Sieg.Api`), **persiste/atualiza dados** em **SQL Server**, aplica **idempotência** para evitar duplicidades e implementa **resiliência** (retries, *exponential backoff* e DLQ).

?? **Repositório:** [github.com/feitosamatheus/SiegWorker](https://www.google.com/search?q=https://github.com/feitosamatheus/SiegWorker)

-----

## ??? Arquitetura (Clean Architecture)

O projeto é estruturado em camadas seguindo a **Clean Architecture** para garantir separação de responsabilidades, testabilidade e adaptabilidade.

**Camadas principais:**

| Camada | Responsabilidade Principal | Relação com o Desafio |
| :--- | :--- | :--- |
| **Domain** | Entidades, objetos de valor e regras de negócio. | Representa a estrutura dos documentos fiscais. |
| **Application** | Casos de uso (**Commands/Handlers**), validações e DTOs. | Orquestra a persistência e a lógica de processamento. |
| **Infrastructure** | Persistência (**EF Core**), repositórios e **Unit of Work**. | Implementa acesso ao SQL Server e EF Core. |
| **IoC** | Registro de dependências e *Composition Root*. | Configuração central de dependências. |
| **ConsumerWorker** | Worker que consome mensagens do **RabbitMQ** e orquestra o processamento. | Implementa a Resiliência e Idempotência (itens 7 e 8). |

-----

## ??? Decisões de Arquitetura e Modelagem

### 1\. Persistência de Dados: SQL Server (Relacional)

**Motivos para a Escolha:**

  * **Integridade e Transações:** Essencial para operações críticas como *upsert* de documentos e atualizações consistentes, garantindo **Atomicidade** e **Isolamento** (ACID).
  * **Modelagem Relacional para Consultas:** A natureza tabular dos documentos fiscais e a necessidade de consulta por múltiplos filtros (`CNPJ`, `UF`, `Data`, `Chave do Documento` - *Item 4 do Desafio*) são ideais para índices e *queries* eficientes em SQL Server.
  * **Ecossistema .NET Maduro:** Suporte robusto e otimizado via **EF Core / Migrations**.

### 2\. Idempotência e Reprocessamento

  * **Estratégia:** Garantida por **chave única** (`Documento.Chave` ou *hash* do XML normalizado) com **índice *unique*** no SQL Server.
  * **Funcionamento:** Ao receber o mesmo evento mais de uma vez, a operação de *upsert* (feita via `Unit of Work` e `Repository`) evita a duplicação. O sistema confirma o estado atual ou realiza uma atualização, tratando reprocessamentos de forma segura.

### 3\. Resiliência no Consumo do RabbitMQ 

  * **Estratégia:** O **ConsumerWorker** implementa um fluxo de consumo robusto utilizando:
      * **Retries:** Tentativas de reprocessamento em caso de falhas transitórias.
      * ***Exponential Backoff***: Aumento exponencial do tempo de espera entre as retentativas para evitar sobrecarga no *broker* e no banco de dados.
      * **DLQ (*Dead Letter Queue*)**: Após esgotadas as tentativas de *retry*, a mensagem é enviada para uma fila de falha permanente para análise manual e evitar que o consumidor *crash* em loop.

### 4\. Tratamento de Dados Sensíveis 

  * **Dados Considerados Sensíveis:** CNPJ ou qualquer informação que possa identificar uma pessoa ou empresa.
  * **Práticas Adotadas:**
      * **Criptografia em Repouso:** A *Connection String* do SQL Server deve ser configurada para usar criptografia (TLS/SSL) ou, em um ambiente Cloud (AWS), as variáveis de ambiente/Secrets Manager são utilizadas.
      * **Filtragem/Anonimização:** Em logs (que não estão no escopo direto do `SiegWorker`, mas são uma boa prática), os dados sensíveis devem ser mascarados ou removidos antes do *sink*.
      * **Controle de Acesso:** O `SiegWorker` opera sob um princípio de privilégio mínimo para acessar apenas os recursos necessários (SQL Server e RabbitMQ).

-----

## ? Requisitos e Setup

  * **`.NET SDK 8+`** 
  * **SQL Server** e **RabbitMQ** já provisionados na AWS (endpoints acessíveis pela rede do worker, utilizando configurações/variáveis de ambiente).
### ?? Configuração de Acesso (Credenciais)

Antes de executar, você **deve** garantir que as credenciais do ambiente estão configuradas. O projeto lê as seguintes configurações no `appsettings.json` ou via **Variáveis de Ambiente**:

| Configuração | Descrição | Exemplo (*appsettings.json*) |
| :--- | :--- | :--- |
| **`AWS:AccessKey`** | Chave de acesso do usuário AWS com permissão (ex: S3 e SQS). | `SUA_ACCESS_KEY` |
| **`AWS:SecretKey`** | Chave secreta do usuário AWS. | `SUA_SECRET_KEY` |
| **`AWS:Region`** | Região AWS onde estão os serviços. | `us-east-2` |
| **`AWS:BucketName`** | Nome do bucket S3 para armazenamento dos XMLs. | `xml-fiscais` |

-----

## ?? Execução Local



  * **Navegue até o diretório do projeto principal (`ConsumerWorker`):**

    ```bash
    cd SiegWorker/Sieg.ConsumerWorker
    ```

  * **Restaure as dependências e faça o build:**

    ```bash
    dotnet build
    ```

  * **Execute o Worker:**

    ```bash
    dotnet run
    ```

O microserviço será iniciado e começará a escutar por mensagens de documentos fiscais na *queue* do RabbitMQ configurada.

-----


## ?? Próximos Passos e Possíveis Melhorias

Com a arquitetura do **`SiegWorker`** (consumidor) definida e a **`Sieg.Api`** (produtor) existente, os próximos passos visam aprimorar a qualidade do código, a segurança e a automação do ciclo de vida do projeto para a conclusão completa do Desafio Técnico:

| Item | Ação Necessária | Notas |
| :--- | :--- | :--- |
| **Testes Unitários/Integração** | Implementar testes nas camadas **`Domain`**, **`Application`** e **`Infrastructure`** usando **NUnit** (e bibliotecas auxiliares como `FluentAssertions`). | Essencial para garantir a qualidade, robustez e a correta aplicação da lógica de idempotência. |
| **Segurança e Configuração** | Mudar o consumo de credenciais (SQL Server, AWS) de `appsettings.json` para **`.NET User Secrets`** (ambiente local) e **Secrets Manager/Variáveis de Ambiente** (CI/CD). | **Priorizar a remoção de chaves** do arquivo de configuração para aumentar a segurança.  |
| **CI/CD Básico** | Implementar um *pipeline* de Integração Contínua (CI) e Entrega Contínua (CD) (Ex: GitHub Actions, GitLab CI). | O CI deve rodar os testes e garantir que o código *builda*. O CD pode focar apenas no *deployment* do `SiegWorker`. |
| **Teste de Carga** (Opcional) | Utilizar **NBomber** ou **k6** para simular o tráfego de ingestão de XMLs (na `Sieg.Api`) e testar a capacidade de processamento do `SiegWorker`. | Medir desempenho sob pressão e identificar *bottlenecks* no consumidor e no SQL Server. |
| **Teste de Arquitetura** (Opcional) | Usar ferramentas como **NetArchTest** para reforçar a separação de responsabilidades e regras de dependência entre as camadas (ex: `Application` não pode depender de `Infrastructure`). | Garante a integridade da Clean Architecture ao longo do tempo. |