<p align="center">
  <img src="https://skillicons.dev/icons?i=cs,dotnet" height="80" alt="C# e .NET" />
</p>

<div align="center">

  <img src="https://img.shields.io/badge/.NET-10-512BD4?style=for-the-badge&logo=dotnet&logoColor=white" />
  <img src="https://img.shields.io/badge/C%23-console-239120?style=for-the-badge" />
  <img src="https://img.shields.io/github/languages/top/imayagmb/stock-quote-alert?style=for-the-badge" />
  <img src="https://img.shields.io/github/repo-size/imayagmb/stock-quote-alert?style=for-the-badge&label=repo%20size" />

  <br/>

  <img src="https://img.shields.io/badge/e--mail-MailKit-0078D4?style=for-the-badge" />
  <img src="https://img.shields.io/badge/cota%C3%A7%C3%B5es-brapi.dev-2EA043?style=for-the-badge" />
  <img src="https://img.shields.io/github/last-commit/imayagmb/stock-quote-alert?style=for-the-badge&label=%C3%BAltimo%20commit" />
  <img src="https://img.shields.io/badge/status-conclu%C3%ADdo-brightgreen?style=for-the-badge" />

</div>

# stock-quote-alert

Aplicação de console em C# que monitora a cotação de um ativo da B3 e envia um e-mail quando o preço ultrapassa os limites de compra ou venda definidos pelo usuário.

O sistema apenas monitora e alerta: não executa ordens de compra ou venda, não possui interface gráfica e não utiliza banco de dados.

## Como funciona

O programa recebe três parâmetros: o código do ativo, o preço de referência para venda e o preço de referência para compra. A cada intervalo configurado, ele consulta a cotação atual e compara com os limites:

| Situação | Ação |
|---|---|
| Cotação **maior** que o preço de venda | Envia e-mail recomendando a venda |
| Cotação **menor** que o preço de compra | Envia e-mail recomendando a compra |
| Cotação entre os dois limites (ou igual a um deles) | Nenhum e-mail |

As comparações são estritas: se a cotação for exatamente igual a um dos limites, nenhum alerta é disparado.

Para evitar e-mails repetidos, cada alerta é enviado apenas uma vez enquanto a cotação permanecer na mesma condição. Quando a cotação volta para a faixa entre os limites, um novo alerta é liberado. Se o envio do e-mail falhar, o alerta não é considerado enviado e o programa tenta novamente na próxima consulta. Esse controle fica em memória durante a execução.

## Tecnologias utilizadas

- C# / .NET 10
- `HttpClient` para consumir a API de cotações
- `System.Text.Json` para leitura do JSON da API e do arquivo de configuração
- [MailKit](https://github.com/jstedfast/MailKit) para envio de e-mails via SMTP
- [brapi.dev](https://brapi.dev) como fonte das cotações da B3

## Estrutura do projeto

| Arquivo | Responsabilidade |
|---|---|
| `Program.cs` | Valida os argumentos, carrega a configuração e controla o ciclo de monitoramento |
| `Settings.cs` | Representa e valida as configurações do `appsettings.json` |
| `QuoteService.cs` | Consulta a API da brapi e extrai o preço da cotação |
| `EmailService.cs` | Envia os e-mails de alerta via SMTP |

## Pré-requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download) ou superior
- Uma conta de e-mail com acesso SMTP (os exemplos usam Gmail)
- Git, para clonar o repositório

## Como clonar o repositório

```bash
git clone https://github.com/SEU-USUARIO/stock-quote-alert.git
cd stock-quote-alert
```

## Configuração

O arquivo `appsettings.json` não é versionado, pois contém credenciais. Crie esse arquivo na raiz do projeto com a seguinte estrutura:

```json
{
  "Email": {
    "To": "destinatario@exemplo.com",
    "From": "remetente@exemplo.com"
  },
  "Smtp": {
    "Host": "smtp.gmail.com",
    "Port": 587,
    "User": "remetente@exemplo.com",
    "Password": "sua-senha-de-app"
  },
  "IntervalSeconds": 300
}
```

| Campo | Descrição |
|---|---|
| `Email.To` | E-mail que receberá os alertas |
| `Email.From` | E-mail remetente dos alertas |
| `Smtp.Host` | Endereço do servidor SMTP |
| `Smtp.Port` | Porta do servidor SMTP (587 para STARTTLS, 465 para SSL) |
| `Smtp.User` | Usuário de autenticação no servidor SMTP |
| `Smtp.Password` | Senha de autenticação no servidor SMTP |
| `IntervalSeconds` | Intervalo entre as consultas, em segundos (padrão: 300) |

Se o arquivo estiver ausente, com formato inválido ou com campos obrigatórios vazios, o programa informa o problema e encerra.

### Credenciais SMTP com Gmail

O Gmail não aceita a senha normal da conta em aplicações externas. É necessário gerar uma **senha de app**:

1. Acesse [myaccount.google.com](https://myaccount.google.com) e abra a seção **Segurança**.
2. Ative a **Verificação em duas etapas**, caso ainda não esteja ativa.
3. Procure por **Senhas de app** e gere uma nova senha.
4. Copie a senha gerada (sem espaços) para o campo `Smtp.Password`.

Os campos `Smtp.User` e `Email.From` devem ser o mesmo endereço do Gmail que gerou a senha de app.

### Token da brapi (opcional)

Os ativos PETR4, VALE3, MGLU3 e ITUB4 podem ser consultados sem autenticação. Para outros ativos, é necessário um token:

1. Crie uma conta gratuita em [brapi.dev](https://brapi.dev).
2. No painel, copie a chave exibida em **Sua chave de API**.
3. Defina a variável de ambiente `BRAPI_TOKEN` antes de executar o programa.

Git Bash:

```bash
export BRAPI_TOKEN="seu_token"
```

PowerShell:

```powershell
$env:BRAPI_TOKEN="seu_token"
```

A variável vale apenas para a janela do terminal em que foi definida. O token não é armazenado no código nem no arquivo de configuração.

## Códigos dos ativos

O primeiro parâmetro do programa é o **código de negociação** (ticker) do ativo na B3. Ele é formado por quatro letras, que identificam a empresa, seguidas de um número, que identifica o tipo do ativo:

| Final | Tipo | Exemplo |
|---|---|---|
| **3** | Ação ordinária (ON), com direito a voto | PETR3 |
| **4** | Ação preferencial (PN), com preferência no recebimento de dividendos | PETR4 |
| **11** | Units e fundos imobiliários | TAEE11 |

Ações da mesma empresa com finais diferentes têm cotações diferentes, por isso o código deve ser informado completo.

### Ativos disponíveis sem token

| Código | Empresa |
|---|---|
| PETR4 | Petrobras (preferencial) |
| VALE3 | Vale (ordinária) |
| ITUB4 | Itaú Unibanco (preferencial) |
| MGLU3 | Magazine Luiza (ordinária) |

### Outros exemplos (exigem token)

| Código | Empresa |
|---|---|
| PETR3 | Petrobras (ordinária) |
| BBAS3 | Banco do Brasil |
| BBDC4 | Bradesco (preferencial) |
| ABEV3 | Ambev |
| WEGE3 | WEG |
| B3SA3 | B3 |

### Como encontrar o código de outros ativos

- Pesquisando o nome da empresa no site da [brapi.dev](https://brapi.dev) ou em qualquer site de cotações. O código aparece ao lado do nome da empresa.
- Consultando a lista completa de ativos disponíveis na API, pelo endpoint `https://brapi.dev/api/v2/tickers`.

## Como executar

Pelo terminal, na pasta do projeto:

```bash
dotnet run -- <ATIVO> <PRECO_VENDA> <PRECO_COMPRA>
```

Exemplo:

```bash
dotnet run -- PETR4 22.67 22.59
```

Nesse exemplo, o programa monitora a PETR4, envia um alerta de venda se a cotação passar de R$ 22,67 e um alerta de compra se ficar abaixo de R$ 22,59.

Os preços devem ser informados com **ponto** como separador decimal, independentemente da configuração regional do Windows. O preço de venda deve ser maior que o preço de compra, e ambos devem ser positivos.

Exemplo de saída:

```
Monitorando PETR4 | Venda acima de R$ 22,67 | Compra abaixo de R$ 22,59 | Intervalo: 300 s
Pressione Ctrl+C para encerrar.

[02/11/2026 10:00:00] PETR4: R$ 22,61 | Dentro da faixa
[02/11/2026 10:05:00] PETR4: R$ 22,70 | ALERTA DE VENDA (acima de R$ 22,67)
    E-mail de alerta enviado.
[02/11/2026 10:10:00] PETR4: R$ 22,72 | ALERTA DE VENDA (acima de R$ 22,67)
```

## Como gerar o executável

```bash
dotnet publish -c Release -r win-x64 --self-contained false -o publish
```

O executável `stock-quote-alert.exe` será gerado na pasta `publish`, junto com uma cópia do `appsettings.json`. Para executar:

```bash
cd publish
./stock-quote-alert.exe PETR4 22.67 22.59
```

No PowerShell, use `.\stock-quote-alert.exe` no lugar de `./stock-quote-alert.exe`.

Esse executável depende do .NET 10 Runtime instalado na máquina. Para gerar um executável que funcione sem o .NET instalado:

```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -o publish
```

## Como testar os cenários

Para testar os alertas sem alterar a lógica do programa, consulte a cotação atual do ativo e escolha limites temporários em volta dela. Para os testes, é recomendável reduzir o `IntervalSeconds` para 15 no `appsettings.json`.

Considerando uma cotação atual de R$ 52,71:

| Cenário | Comando | Resultado esperado |
|---|---|---|
| Dentro da faixa | `dotnet run -- PETR4 60 40` | Nenhum e-mail enviado |
| Alerta de venda | `dotnet run -- PETR4 52.50 40` | Um e-mail de venda |
| Alerta de compra | `dotnet run -- PETR4 60 53` | Um e-mail de compra |
| Preço igual ao limite | `dotnet run -- PETR4 52.71 40` | Nenhum e-mail (comparação estrita) |
| Sem repetição | Manter o cenário de venda por vários ciclos | Apenas um e-mail enviado |
| Parâmetros inválidos | `dotnet run -- PETR4 22,67 22.59` | Mensagem de uso |
| Falha na API | Desconectar a internet durante a execução | Mensagem de erro e retomada automática no ciclo seguinte |

Ou seja: para provocar um alerta de venda, use um preço de venda **abaixo** da cotação atual; para um alerta de compra, use um preço de compra **acima** dela.

## Como encerrar o monitoramento

Pressione **Ctrl+C**. O programa interrompe a espera entre as consultas e encerra exibindo a mensagem `Monitoramento encerrado.`. Se uma consulta ou um envio de e-mail estiver em andamento, ele é concluído antes do encerramento (no máximo 15 segundos, que é o tempo limite da consulta).

## Limitações

- **Dados sem token:** os ativos de teste consultados sem token retornam dados de demonstração, que podem não refletir a movimentação real do mercado.
- **Atraso das cotações:** no plano gratuito da brapi, as cotações têm atraso aproximado de 30 minutos. O sistema não opera com cotações em tempo real.
- **Limite de requisições:** o plano gratuito permite 15.000 requisições por mês. Intervalos muito curtos esgotam essa cota rapidamente; o intervalo padrão de 5 minutos mantém o consumo dentro do limite.
- **Horário do pregão:** fora do horário de negociação da B3, a cotação não se altera.
- **Estado em memória:** o controle de alertas enviados não é persistido. Ao reiniciar o programa, um alerta já enviado pode ser enviado novamente.
- **Verificação de revogação de certificado:** a verificação de revogação do certificado SMTP está desativada, pois antivírus que inspecionam conexões de e-mail impedem essa verificação no Windows. A conexão continua criptografada e o certificado do servidor continua sendo validado.

## Utilização de ferramentas de IA

Durante o desenvolvimento, utilizei o **Claude** (Anthropic) como apoio. Como minha experiência é principalmente com JavaScript, TypeScript, React, Angular, Node.js e PHP, usei a ferramenta como um instrutor de C#, para:

- Aprender conceitos de C# e .NET que eu ainda não dominava, como `HttpClient`, `System.Text.Json`, tratamento de exceções tipadas e `CancellationToken`- Sugerir a estrutura do projeto e a divisão de responsabilidades entre os arquivos
- Propor e explicar trechos de código, que revisei, executei e testei etapa por etapa
- Investigar erros durante os testes, como a falha de validação de certificado no envio de e-mails
- Consultar a documentação da brapi e auxiliar na redação deste README

Todo o código foi revisado, executado e testado por mim, etapa por etapa, e os cenários de teste descritos neste documento foram executados manualmente.v
