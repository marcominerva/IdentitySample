CREATE TABLE [dbo].[Tenants](
	[Id] [uniqueidentifier] NOT NULL,
	[ConnectionString] [varchar](4000) NOT NULL,
 CONSTRAINT [PK_Tenants] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
))
GO