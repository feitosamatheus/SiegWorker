
# ?? SiegWorker - Processador de Documentos Fiscais

O **SiegWorker** � um microservi�o **.NET** (Worker Service) que atua como a camada de **processamento ass�ncrono** e persist�ncia de **documentos fiscais** (NFe/CTe/NFSe).

Ele **consome eventos** do **RabbitMQ** (publicados pelo projeto `Sieg.Api`), **persiste/atualiza dados** em **SQL Server**, aplica **idempot�ncia** para evitar duplicidades e implementa **resili�ncia** (retries, *exponential backoff* e DLQ).

?? **Reposit�rio:** [github.com/feitosamatheus/SiegWorker](https://www.google.com/search?q=https://github.com/feitosamatheus/SiegWorker)

-----

## ??? Arquitetura (Clean Architecture)

O projeto � estruturado em camadas seguindo a **Clean Architecture** para garantir separa��o de responsabilidades, testabilidade e adaptabilidade.

**Camadas principais:**

| Camada | Responsabilidade Principal | Rela��o com o Desafio |
| :--- | :--- | :--- |
| **Domain** | Entidades, objetos de valor e regras de neg�cio. | Representa a estrutura dos documentos fiscais. |
| **Application** | Casos de uso (**Commands/Handlers**), valida��es e DTOs. | Orquestra a persist�ncia e a l�gica de processamento. |
| **Infrastructure** | Persist�ncia (**EF Core**), reposit�rios e **Unit of Work**. | Implementa acesso ao SQL Server e EF Core. |
| **IoC** | Registro de depend�ncias e *Composition Root*. | Configura��o central de depend�ncias. |
| **ConsumerWorker** | Worker que consome mensagens do **RabbitMQ** e orquestra o processamento. | Implementa a Resili�ncia e Idempot�ncia (itens 7 e 8). |

-----

## ??? Decis�es de Arquitetura e Modelagem

### 1\. Persist�ncia de Dados: SQL Server (Relacional)

**Motivos para a Escolha:**

  * **Integridade e Transa��es:** Essencial para opera��es cr�ticas como *upsert* de documentos e atualiza��es consistentes, garantindo **Atomicidade** e **Isolamento** (ACID).
  * **Modelagem Relacional para Consultas:** A natureza tabular dos documentos fiscais e a necessidade de consulta por m�ltiplos filtros (`CNPJ`, `UF`, `Data`, `Chave do Documento` - *Item 4 do Desafio*) s�o ideais para �ndices e *queries* eficientes em SQL Server.
  * **Ecossistema .NET Maduro:** Suporte robusto e otimizado via **EF Core / Migrations**.

### 2\. Idempot�ncia e Reprocessamento

  * **Estrat�gia:** Garantida por **chave �nica** (`Documento.Chave` ou *hash* do XML normalizado) com **�ndice *unique*** no SQL Server.
  * **Funcionamento:** Ao receber o mesmo evento mais de uma vez, a opera��o de *upsert* (feita via `Unit of Work` e `Repository`) evita a duplica��o. O sistema confirma o estado atual ou realiza uma atualiza��o, tratando reprocessamentos de forma segura.

### 3\. Resili�ncia no Consumo do RabbitMQ 

  * **Estrat�gia:** O **ConsumerWorker** implementa um fluxo de consumo robusto utilizando:
      * **Retries:** Tentativas de reprocessamento em caso de falhas transit�rias.
      * ***Exponential Backoff***: Aumento exponencial do tempo de espera entre as retentativas para evitar sobrecarga no *broker* e no banco de dados.
      * **DLQ (*Dead Letter Queue*)**: Ap�s esgotadas as tentativas de *retry*, a mensagem � enviada para uma fila de falha permanente para an�lise manual e evitar que o consumidor *crash* em loop.

### 4\. Tratamento de Dados Sens�veis 

  * **Dados Considerados Sens�veis:** CNPJ ou qualquer informa��o que possa identificar uma pessoa ou empresa.
  * **Pr�ticas Adotadas:**
      * **Criptografia em Repouso:** A *Connection String* do SQL Server deve ser configurada para usar criptografia (TLS/SSL) ou, em um ambiente Cloud (AWS), as vari�veis de ambiente/Secrets Manager s�o utilizadas.
      * **Filtragem/Anonimiza��o:** Em logs (que n�o est�o no escopo direto do `SiegWorker`, mas s�o uma boa pr�tica), os dados sens�veis devem ser mascarados ou removidos antes do *sink*.
      * **Controle de Acesso:** O `SiegWorker` opera sob um princ�pio de privil�gio m�nimo para acessar apenas os recursos necess�rios (SQL Server e RabbitMQ).

-----

## ? Requisitos e Setup

  * **`.NET SDK 8+`** 
  * **SQL Server** e **RabbitMQ** j� provisionados na AWS (endpoints acess�veis pela rede do worker, utilizando configura��es/vari�veis de ambiente).
### ?? Configura��o de Acesso (Credenciais)

Antes de executar, voc� **deve** garantir que as credenciais do ambiente est�o configuradas. O projeto l� as seguintes configura��es no `appsettings.json` ou via **Vari�veis de Ambiente**:

| Configura��o | Descri��o | Exemplo (*appsettings.json*) |
| :--- | :--- | :--- |
| **`AWS:AccessKey`** | Chave de acesso do usu�rio AWS com permiss�o (ex: S3 e SQS). | `SUA_ACCESS_KEY` |
| **`AWS:SecretKey`** | Chave secreta do usu�rio AWS. | `SUA_SECRET_KEY` |
| **`AWS:Region`** | Regi�o AWS onde est�o os servi�os. | `us-east-2` |
| **`AWS:BucketName`** | Nome do bucket S3 para armazenamento dos XMLs. | `xml-fiscais` |

-----

## ?? Execu��o Local



  * **Navegue at� o diret�rio do projeto principal (`ConsumerWorker`):**

    ```bash
    cd SiegWorker/Sieg.ConsumerWorker
    ```

  * **Restaure as depend�ncias e fa�a o build:**

    ```bash
    dotnet build
    ```

  * **Execute o Worker:**

    ```bash
    dotnet run
    ```

O microservi�o ser� iniciado e come�ar� a escutar por mensagens de documentos fiscais na *queue* do RabbitMQ configurada.

-----


## ?? Pr�ximos Passos e Poss�veis Melhorias

Com a arquitetura do **`SiegWorker`** (consumidor) definida e a **`Sieg.Api`** (produtor) existente, os pr�ximos passos visam aprimorar a qualidade do c�digo, a seguran�a e a automa��o do ciclo de vida do projeto para a conclus�o completa do Desafio T�cnico:

| Item | A��o Necess�ria | Notas |
| :--- | :--- | :--- |
| **Testes Unit�rios/Integra��o** | Implementar testes nas camadas **`Domain`**, **`Application`** e **`Infrastructure`** usando **NUnit** (e bibliotecas auxiliares como `FluentAssertions`). | Essencial para garantir a qualidade, robustez e a correta aplica��o da l�gica de idempot�ncia. |
| **Seguran�a e Configura��o** | Mudar o consumo de credenciais (SQL Server, AWS) de `appsettings.json` para **`.NET User Secrets`** (ambiente local) e **Secrets Manager/Vari�veis de Ambiente** (CI/CD). | **Priorizar a remo��o de chaves** do arquivo de configura��o para aumentar a seguran�a.  |
| **CI/CD B�sico** | Implementar um *pipeline* de Integra��o Cont�nua (CI) e Entrega Cont�nua (CD) (Ex: GitHub Actions, GitLab CI). | O CI deve rodar os testes e garantir que o c�digo *builda*. O CD pode focar apenas no *deployment* do `SiegWorker`. |
| **Teste de Carga** (Opcional) | Utilizar **NBomber** ou **k6** para simular o tr�fego de ingest�o de XMLs (na `Sieg.Api`) e testar a capacidade de processamento do `SiegWorker`. | Medir desempenho sob press�o e identificar *bottlenecks* no consumidor e no SQL Server. |
| **Teste de Arquitetura** (Opcional) | Usar ferramentas como **NetArchTest** para refor�ar a separa��o de responsabilidades e regras de depend�ncia entre as camadas (ex: `Application` n�o pode depender de `Infrastructure`). | Garante a integridade da Clean Architecture ao longo do tempo. |