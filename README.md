## Objetivo

Desenvolvimento de sistema em C# capaz de avisar, via e-mail, caso a cotação de um ativo da B3 caia mais do que certo nível, ou suba acima de outro. Sem interface gráfica, ele é chamado via linha de comando com 3 parâmetros: 
- O ativo a ser monitorado
- O preço de referência para venda
- O preço de referência para compra

## Requisitos

- [.NET SDK 8.0](https://dotnet.microsoft.com/download/dotnet/8.0) ou superior

## Setup 

Para utilizar o programa é necessário criar um arquivo de configuração, com o nome <configuration.json>, e preenchê-lo corretamente. Para isso: 
- Copie o conteúdo do arquivo <configuration.json.example> no novo <configuration.json>.
- Altere obrigatoriamente as variáveis: `AlertEmail`, `Username`, `Password` e `FromAddress`, de acordo com o que o valor de exemplo dessas variáveis indica.

## API de Cotações

A API escolhida para pegar as cotações em tempo real foi a BRAPI: https://brapi.dev/docs. 
No programa atual, o usuário tem duas opções: 
- Não preencher a variável `BrapiToken` no <configuration.json> e só ter acesso aos seguintes ativos: PETR4 (Petrobras), MGLU3 (Magazine Luiza), VALE3 (Vale) e ITUB4 (Itaú).
- Preencher a variável `BrapiToken` e ter acesso a cotação de todos os ativos da B3 disponíveis na api.*

*Obs: Caso não possua um token, acesse https://brapi.dev/ para criá-lo. 

## Uso

Dentro do repositório `stock-quote-alert`, rode:

```bash
dotnet run -- <ATIVO> <PRECO_VENDA> <PRECO_COMPRA>
```

Exemplo:

```bash
dotnet run -- PETR4 22.67 22.59
```

Parâmetros:

| Parâmetro       | Descrição                                               |
|-----------------|---------------------------------------------------------|
| `ATIVO`         | Código do ativo a ser monitorado (ex: `PETR4`)          |
| `PRECO_VENDA`   | Preço de referência para venda (alerta ao ser atingido) |
| `PRECO_COMPRA`  | Preço de referência para compra (alerta ao ser atingido)|

O preço de compra deve ser sempre menor que o preço de venda, e ambos devem ser valores não negativos.

### Somente Windows
Após compilado (`dotnet publish` ou `dotnet build`), o executável também pode ser chamado diretamente:

```bash
stock-quote-alert.exe PETR4 22.67 22.59
```
