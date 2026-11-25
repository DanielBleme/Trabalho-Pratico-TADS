# Trabalho Prático TADS — LocaOps

Sistema de gestão de locadora de veículos.

Aluno: Daniel Rodrigues Bleme
Período: 4º

## Materiais do projeto

- [Protótipo e wireframes no Figma](https://www.figma.com/make/75f1W4FYd5DyTfiv0Oy4cZ/Wireframes-da-LocaOps?t=kVjtuEElFayCiBVQ-20&fullscreen=1)
- [Relatório técnico (SBC)](https://github.com/user-attachments/files/35189150/SBC-TrabalhoPratico.pdf)
- [Vídeo pitch](https://youtu.be/d6YIH7pf1FI)

## Tecnologias

- Frontend: React, Vite e Lucide React.
- Backend: C#, ASP.NET Core e Entity Framework Core.
- Banco de dados: SQL Server.

## Estrutura

- `backend/Trabalho-Pratico/`: aplicação ASP.NET Core MVC e API.
- `backend/Trabalho-Pratico.sln`: solução .NET.
- `frontend/`: interface React.

## Executar o backend

Com o SQL Server Express local disponível e o banco `LocadoraVeiculosDb` configurado em `backend/Trabalho-Pratico/appsettings.json`:

```powershell
dotnet run --project backend/Trabalho-Pratico/Trabalho-Pratico.csproj --launch-profile http
```

O backend usa `http://localhost:5026`.

## Executar o frontend

Em outro terminal:

```powershell
cd frontend
npm.cmd install
npm.cmd run dev
```

O frontend fica em `http://localhost:5173` e encaminha as chamadas `/api` para o backend.
