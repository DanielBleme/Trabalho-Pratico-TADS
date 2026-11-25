IF DB_ID(N'LocadoraVeiculosDb') IS NULL
    CREATE DATABASE LocadoraVeiculosDb;
GO

USE LocadoraVeiculosDb;
GO

IF OBJECT_ID(N'dbo.Fabricantes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Fabricantes
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Fabricantes PRIMARY KEY,
        Nome nvarchar(max) NOT NULL,
        CNPJ nvarchar(18) NOT NULL,
        CONSTRAINT UQ_Fabricantes_CNPJ UNIQUE (CNPJ)
    );
END;
GO

IF OBJECT_ID(N'dbo.Clientes', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Clientes
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Clientes PRIMARY KEY,
        Nome nvarchar(150) NOT NULL,
        CPF nvarchar(14) NOT NULL,
        Email nvarchar(150) NOT NULL,
        Telefone nvarchar(20) NOT NULL,
        CONSTRAINT UQ_Clientes_CPF UNIQUE (CPF)
    );
END;
GO

IF OBJECT_ID(N'dbo.Veiculos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Veiculos
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Veiculos PRIMARY KEY,
        Modelo nvarchar(100) NOT NULL,
        AnoFabricacao int NOT NULL,
        Placa nvarchar(10) NOT NULL,
        Quilometragem int NOT NULL,
        FabricanteId int NOT NULL,
        CONSTRAINT UQ_Veiculos_Placa UNIQUE (Placa),
        CONSTRAINT FK_Veiculos_Fabricantes_FabricanteId FOREIGN KEY (FabricanteId)
            REFERENCES dbo.Fabricantes (Id) ON DELETE NO ACTION
    );
END;
GO

IF OBJECT_ID(N'dbo.Alugueis', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Alugueis
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Alugueis PRIMARY KEY,
        DataInicio datetime2 NOT NULL,
        DataFimPrevista datetime2 NOT NULL,
        DataDevolucao datetime2 NULL,
        QuilometragemInicial int NOT NULL,
        QuilometragemFinal int NULL,
        ValorDiaria decimal(10,2) NOT NULL,
        ValorTotal decimal(10,2) NULL,
        ClienteId int NOT NULL,
        VeiculoId int NOT NULL,
        CONSTRAINT FK_Alugueis_Clientes_ClienteId FOREIGN KEY (ClienteId)
            REFERENCES dbo.Clientes (Id) ON DELETE NO ACTION,
        CONSTRAINT FK_Alugueis_Veiculos_VeiculoId FOREIGN KEY (VeiculoId)
            REFERENCES dbo.Veiculos (Id) ON DELETE NO ACTION
    );
END;
GO

IF OBJECT_ID(N'dbo.Pagamentos', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Pagamentos
    (
        Id int IDENTITY(1,1) NOT NULL CONSTRAINT PK_Pagamentos PRIMARY KEY,
        AluguelId int NOT NULL,
        DataPagamento datetime2 NOT NULL,
        ValorPago decimal(10,2) NOT NULL,
        MetodoPagamento nvarchar(50) NOT NULL,
        Status nvarchar(30) NOT NULL,
        CONSTRAINT FK_Pagamentos_Alugueis_AluguelId FOREIGN KEY (AluguelId)
            REFERENCES dbo.Alugueis (Id) ON DELETE CASCADE
    );
END;
GO
