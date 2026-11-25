 # LocaOps

## Links do Projeto

* **Protótipo no Figma:** [[[Acessar Figma](https://www.figma.com/seu-link-aqui)](https://www.figma.com/make/vcF9xv49TbsRO1UEw7gqvN/Loja-Virtual-Sells-and-Tells?t=dTRsUmrzhF6axzDu-20&fullscreen=1&preview-route=%2F%23historia)](https://www.figma.com/make/75f1W4FYd5DyTfiv0Oy4cZ/Wireframes-da-LocaOps?t=kVjtuEElFayCiBVQ-20&fullscreen=1)
  
* **Relatório Técnico (SBC):** [SBC-TrabalhoPratico.pdf](https://github.com/user-attachments/files/35189150/SBC-TrabalhoPratico.pdf)

## Video-Pitch 
Disponível no youtube: 
https://youtu.be/d6YIH7pf1FI


## Tecnologias

* **Front-end:** React, Tailwind CSS, Lucide React
* **Back-end:** | C#, .NET | 
* **Banco de Dados:** SQL Server

## Como rodar o projeto

### 1. Banco de Dados
Banco rodando na tabela `LocadoraVeiculoDb` no SQL Server e ajuste o usuário e senha no arquivo `dbconfig.js`.

### 2. API (Back-end)
Na pasta raiz, instale as dependências e inicie o servidor:
```PowerShell
dotnet run
```
A API vai rodar em `(http://localhost:5026)`.

### 3. Front-end
Em outro terminal, entre na pasta do React e inicie a aplicação:
```bash
cd front
npm install
npm run dev
```
O Front vai rodar em ` http://127.0.0.1:5173`
