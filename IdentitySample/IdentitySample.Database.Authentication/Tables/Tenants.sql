CREATE TABLE [dbo].[Tenants](
	[Id] [uniqueidentifier] NOT NULL,
	[ConnectionString] [varchar](4000) NOT NULL,
 [StorageConnectionString] VARCHAR(4000) NULL, 
    [ContainerName] VARCHAR(256) NULL, 
    CONSTRAINT [PK_Tenants] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
))
GO