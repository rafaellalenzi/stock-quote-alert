## Objetivo

Desenvolvimento de sistema em C# capaz de avisar, via e-mail, caso a cotação de um ativo da B3 caia mais do que certo nível, ou suba acima de outro. Sem interface gráfica, ele é chamado via linha de comando com 3 parâmetros: 
- O ativo a ser monitorado
- O preço de referência para venda
- O preço de referência para compra

Ex. > stock-quote-alert.exe PETR4 22.67 22.59

## Setup 

Para utilizar o programa é necessário criar um arquivo de configuração, com o nome <configuration.json>, e preenchê-lo corretamente. Para isso: 
- Copie o conteúdo do arquivo <configuration.json.example> no novo <configuration.json>.
- Altere as variáveis: AlertEmail, Username, Password e FromAddress, de acordo com as intruções presentes no arquivo .example.

## API de Cotações

A API escolhida para pegar as cotações em tempo real foi a BRAPI: https://brapi.dev/docs. 
No programa atual, somente os ativos disponíveis sem authentication token estão disponíveis para consulta: PETR4 (Petrobras), MGLU3 (Magazine Luiza), VALE3 (Vale) e ITUB4 (Itaú)
