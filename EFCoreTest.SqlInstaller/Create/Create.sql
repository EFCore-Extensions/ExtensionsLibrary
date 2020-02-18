--Idempotent Create Script

--CREATE SCHEMA [MySchema]
if not exists(select * from sys.schemas where [name] = 'MySchema')
exec('create schema [MySchema];')

--CREATE TABLE [BasicTenant]
if not exists(select * from sys.tables t inner join sys.schemas s on t.schema_id = s.schema_id where t.name = 'BasicTenant' and s.name = 'dbo')
CREATE TABLE [dbo].[BasicTenant] (
[Id] [int] IDENTITY(1,1) NOT NULL ,
[Name] [varchar] (MAX)  NOT NULL ,
[TenantId] [varchar] (50)  NOT NULL ,
CONSTRAINT [PK_BASICTENANT] PRIMARY KEY CLUSTERED
    (
        [Id] ASC
    )
)
GO

--CREATE TABLE [BigDbEntity]
if not exists(select * from sys.tables t inner join sys.schemas s on t.schema_id = s.schema_id where t.name = 'BigDbEntity' and s.name = 'dbo')
CREATE TABLE [dbo].[BigDbEntity] (
[ID] [int] IDENTITY(1,1) NOT NULL ,
[JustAnotherName] [varchar] (50)  NOT NULL ,
[MyBlob1] [text]  NOT NULL ,
[MyBlob2] [ntext]  NULL ,
[MyBool] [bit]  NOT NULL ,
[MyByteArray] [varbinary] (MAX)  NULL ,
[MyChar] [char] (1)  NOT NULL ,
[MyDateTime] [DateTime2] (7)  NOT NULL ,
[MyDecmial] [decimal] (18,2)  NOT NULL ,
[MyDouble] [float]  NOT NULL ,
[MyFloat] [real]  NOT NULL ,
[MyInt16] [smallint]  NOT NULL ,
[MyShort] [smallint]  NOT NULL ,
[MySingle] [real]  NOT NULL ,
[MyXml] [xml]  NULL ,
CONSTRAINT [PK_BIGDBENTITY] PRIMARY KEY CLUSTERED
    (
        [ID] ASC
    )
)
GO

--CREATE TABLE [Car]
if not exists(select * from sys.tables t inner join sys.schemas s on t.schema_id = s.schema_id where t.name = 'Car' and s.name = 'dbo')
CREATE TABLE [dbo].[Car] (
[CarId] [int] IDENTITY(1,1) NOT NULL ,
[IsDeleted] [bit]  NOT NULL CONSTRAINT [DF_CAR_ISDELETED] DEFAULT (0),
[Name] [nvarchar] (50)  NOT NULL CONSTRAINT [DF_CAR_NAME] DEFAULT ('New Car'),
CONSTRAINT [PK_CAR] PRIMARY KEY CLUSTERED
    (
        [CarId] ASC
    )
)
GO

--CREATE TABLE [TenantMaster]
if not exists(select * from sys.tables t inner join sys.schemas s on t.schema_id = s.schema_id where t.name = 'TenantMaster' and s.name = 'dbo')
CREATE TABLE [dbo].[TenantMaster] (
[CategoryId] [int] IDENTITY(1,1) NOT NULL ,
[Name] [varchar] (50)  NOT NULL ,
CONSTRAINT [PK_TENANTMASTER] PRIMARY KEY CLUSTERED
    (
        [CategoryId] ASC
    )
)
GO

--CREATE TABLE [CodeManagedKey]
if not exists(select * from sys.tables t inner join sys.schemas s on t.schema_id = s.schema_id where t.name = 'CodeManagedKey' and s.name = 'dbo')
CREATE TABLE [dbo].[CodeManagedKey] (
[ID] [int]  NOT NULL ,
[Concurrency] [UniqueIdentifier]  NOT NULL ,
[Data] [varchar] (MAX)  NULL ,
[Name] [nvarchar] (50)  NOT NULL ,
[Version] [int]  NOT NULL ,
CONSTRAINT [PK_CODEMANAGEDKEY] PRIMARY KEY CLUSTERED
    (
        [ID] ASC
    )
)
GO

--CREATE TABLE [CompositeStuff]
if not exists(select * from sys.tables t inner join sys.schemas s on t.schema_id = s.schema_id where t.name = 'CompositeStuff' and s.name = 'dbo')
CREATE TABLE [dbo].[CompositeStuff] (
[ID2] [int]  NOT NULL ,
[ID1] [int]  NOT NULL ,
[Name1] [varchar] (50)  NULL ,
[Name2] [varchar] (50)  NULL ,
[Name3] [varchar] (50)  NULL ,
CONSTRAINT [PK_COMPOSITESTUFF] PRIMARY KEY CLUSTERED
    (
        [ID2] ASC, [ID1] ASC
    )
)
GO

--CREATE TABLE [Customer]
if not exists(select * from sys.tables t inner join sys.schemas s on t.schema_id = s.schema_id where t.name = 'Customer' and s.name = 'dbo')
CREATE TABLE [dbo].[Customer] (
[CustomerId] [UniqueIdentifier]  NOT NULL ,
[CreatedBy] [varchar] (50)  NULL ,
[CreatedDate] [DateTime2] (7)  NOT NULL CONSTRAINT [DF_CUSTOMER_CREATEDDATE] DEFAULT (getdate()),
[CustomerTypeId] [int]  NOT NULL ,
[ModifiedBy] [varchar] (50)  NULL ,
[ModifiedDate] [DateTime2] (7)  NOT NULL CONSTRAINT [DF_CUSTOMER_MODIFIEDDATE] DEFAULT (getdate()),
[Name] [varchar] (50)  NOT NULL ,
[TenantId] [varchar] (50)  NOT NULL ,
[Timestamp] [varbinary] (MAX)  NOT NULL ,
CONSTRAINT [PK_CUSTOMER] PRIMARY KEY CLUSTERED
    (
        [CustomerId] ASC
    )
)
GO

--CREATE TABLE [CustomerType]
if not exists(select * from sys.tables t inner join sys.schemas s on t.schema_id = s.schema_id where t.name = 'CustomerType' and s.name = 'dbo')
CREATE TABLE [dbo].[CustomerType] (
[ID] [int] IDENTITY(1,1) NOT NULL ,
[Name] [varchar] (50)  NOT NULL ,
CONSTRAINT [PK_CUSTOMERTYPE] PRIMARY KEY CLUSTERED
    (
        [ID] ASC
    )
)
GO

--CREATE TABLE [HeapTable]
if not exists(select * from sys.tables t inner join sys.schemas s on t.schema_id = s.schema_id where t.name = 'HeapTable' and s.name = 'dbo')
CREATE TABLE [dbo].[HeapTable] (
[ID] [int]  NOT NULL ,
[Name] [varchar] (50)  NULL ,
)
GO

--CREATE TABLE [Order]
if not exists(select * from sys.tables t inner join sys.schemas s on t.schema_id = s.schema_id where t.name = 'Order' and s.name = 'dbo')
CREATE TABLE [dbo].[Order] (
[OrderId5] [UniqueIdentifier]  NOT NULL ,
[CreatedBy] [varchar] (50)  NULL ,
[CreatedDate] [DateTime2] (7)  NOT NULL CONSTRAINT [DF_ORDER_CREATEDDATE] DEFAULT (getdate()),
[CustomerFkId] [UniqueIdentifier]  NOT NULL ,
[DefaultedTime] [DateTime2] (7)  NOT NULL CONSTRAINT [DF_ORDER_DEFAULTEDTIME] DEFAULT (GETDATE()),
[ModifiedBy] [varchar] (50)  NULL ,
[ModifiedDate] [DateTime2] (7)  NOT NULL CONSTRAINT [DF_ORDER_MODIFIEDDATE] DEFAULT (getdate()),
[Quantity] [int]  NOT NULL CONSTRAINT [DF_ORDER_QUANTITY] DEFAULT (5),
[TenantId] [varchar] (50)  NOT NULL ,
[Timestamp] [varbinary] (MAX)  NOT NULL ,
CONSTRAINT [PK_ORDER] PRIMARY KEY CLUSTERED
    (
        [OrderId5] ASC
    )
)
GO

--CREATE TABLE [SchemaTest]
if not exists(select * from sys.tables t inner join sys.schemas s on t.schema_id = s.schema_id where t.name = 'SchemaTest' and s.name = 'MySchema')
CREATE TABLE [MySchema].[SchemaTest] (
[Id] [int] IDENTITY(1,1) NOT NULL ,
[Name] [varchar] (50)  NOT NULL ,
CONSTRAINT [PK_SCHEMATEST] PRIMARY KEY CLUSTERED
    (
        [Id] ASC
    )
)
GO

--##SECTION BEGIN [RENAME PK]

--RENAME EXISTING PRIMARY KEYS IF NECESSARY
DECLARE @pkfixBasicTenant varchar(500)
SET @pkfixBasicTenant = (SELECT top 1 i.name AS IndexName FROM sys.indexes AS i WHERE i.is_primary_key = 1 AND OBJECT_NAME(i.OBJECT_ID) = 'BasicTenant')
if @pkfixBasicTenant <> '' and (BINARY_CHECKSUM(@pkfixBasicTenant) <> BINARY_CHECKSUM('PK_BASICTENANT')) exec('sp_rename '''+@pkfixBasicTenant+''', ''PK_BASICTENANT''')
DECLARE @pkfixBigDbEntity varchar(500)
SET @pkfixBigDbEntity = (SELECT top 1 i.name AS IndexName FROM sys.indexes AS i WHERE i.is_primary_key = 1 AND OBJECT_NAME(i.OBJECT_ID) = 'BigDbEntity')
if @pkfixBigDbEntity <> '' and (BINARY_CHECKSUM(@pkfixBigDbEntity) <> BINARY_CHECKSUM('PK_BIGDBENTITY')) exec('sp_rename '''+@pkfixBigDbEntity+''', ''PK_BIGDBENTITY''')
DECLARE @pkfixCar varchar(500)
SET @pkfixCar = (SELECT top 1 i.name AS IndexName FROM sys.indexes AS i WHERE i.is_primary_key = 1 AND OBJECT_NAME(i.OBJECT_ID) = 'Car')
if @pkfixCar <> '' and (BINARY_CHECKSUM(@pkfixCar) <> BINARY_CHECKSUM('PK_CAR')) exec('sp_rename '''+@pkfixCar+''', ''PK_CAR''')
DECLARE @pkfixTenantMaster varchar(500)
SET @pkfixTenantMaster = (SELECT top 1 i.name AS IndexName FROM sys.indexes AS i WHERE i.is_primary_key = 1 AND OBJECT_NAME(i.OBJECT_ID) = 'TenantMaster')
if @pkfixTenantMaster <> '' and (BINARY_CHECKSUM(@pkfixTenantMaster) <> BINARY_CHECKSUM('PK_TENANTMASTER')) exec('sp_rename '''+@pkfixTenantMaster+''', ''PK_TENANTMASTER''')
DECLARE @pkfixCodeManagedKey varchar(500)
SET @pkfixCodeManagedKey = (SELECT top 1 i.name AS IndexName FROM sys.indexes AS i WHERE i.is_primary_key = 1 AND OBJECT_NAME(i.OBJECT_ID) = 'CodeManagedKey')
if @pkfixCodeManagedKey <> '' and (BINARY_CHECKSUM(@pkfixCodeManagedKey) <> BINARY_CHECKSUM('PK_CODEMANAGEDKEY')) exec('sp_rename '''+@pkfixCodeManagedKey+''', ''PK_CODEMANAGEDKEY''')
DECLARE @pkfixCompositeStuff varchar(500)
SET @pkfixCompositeStuff = (SELECT top 1 i.name AS IndexName FROM sys.indexes AS i WHERE i.is_primary_key = 1 AND OBJECT_NAME(i.OBJECT_ID) = 'CompositeStuff')
if @pkfixCompositeStuff <> '' and (BINARY_CHECKSUM(@pkfixCompositeStuff) <> BINARY_CHECKSUM('PK_COMPOSITESTUFF')) exec('sp_rename '''+@pkfixCompositeStuff+''', ''PK_COMPOSITESTUFF''')
DECLARE @pkfixCustomer varchar(500)
SET @pkfixCustomer = (SELECT top 1 i.name AS IndexName FROM sys.indexes AS i WHERE i.is_primary_key = 1 AND OBJECT_NAME(i.OBJECT_ID) = 'Customer')
if @pkfixCustomer <> '' and (BINARY_CHECKSUM(@pkfixCustomer) <> BINARY_CHECKSUM('PK_CUSTOMER')) exec('sp_rename '''+@pkfixCustomer+''', ''PK_CUSTOMER''')
DECLARE @pkfixCustomerType varchar(500)
SET @pkfixCustomerType = (SELECT top 1 i.name AS IndexName FROM sys.indexes AS i WHERE i.is_primary_key = 1 AND OBJECT_NAME(i.OBJECT_ID) = 'CustomerType')
if @pkfixCustomerType <> '' and (BINARY_CHECKSUM(@pkfixCustomerType) <> BINARY_CHECKSUM('PK_CUSTOMERTYPE')) exec('sp_rename '''+@pkfixCustomerType+''', ''PK_CUSTOMERTYPE''')
DECLARE @pkfixHeapTable varchar(500)
SET @pkfixHeapTable = (SELECT top 1 i.name AS IndexName FROM sys.indexes AS i WHERE i.is_primary_key = 1 AND OBJECT_NAME(i.OBJECT_ID) = 'HeapTable')
if @pkfixHeapTable <> '' and (BINARY_CHECKSUM(@pkfixHeapTable) <> BINARY_CHECKSUM('PK_HEAPTABLE')) exec('sp_rename '''+@pkfixHeapTable+''', ''PK_HEAPTABLE''')
DECLARE @pkfixOrder varchar(500)
SET @pkfixOrder = (SELECT top 1 i.name AS IndexName FROM sys.indexes AS i WHERE i.is_primary_key = 1 AND OBJECT_NAME(i.OBJECT_ID) = 'Order')
if @pkfixOrder <> '' and (BINARY_CHECKSUM(@pkfixOrder) <> BINARY_CHECKSUM('PK_ORDER')) exec('sp_rename '''+@pkfixOrder+''', ''PK_ORDER''')
DECLARE @pkfixSchemaTest varchar(500)
SET @pkfixSchemaTest = (SELECT top 1 i.name AS IndexName FROM sys.indexes AS i WHERE i.is_primary_key = 1 AND OBJECT_NAME(i.OBJECT_ID) = 'SchemaTest')
if @pkfixSchemaTest <> '' and (BINARY_CHECKSUM(@pkfixSchemaTest) <> BINARY_CHECKSUM('PK_SCHEMATEST')) exec('sp_rename '''+@pkfixSchemaTest+''', ''PK_SCHEMATEST''')
GO

--##SECTION END [RENAME PK]

--##SECTION BEGIN [CREATE PK]

--PRIMARY KEY FOR TABLE [BasicTenant]
if not exists(select * from sys.objects where name = 'PK_BASICTENANT' and type = 'PK')
ALTER TABLE [dbo].[BasicTenant] WITH NOCHECK ADD 
CONSTRAINT [PK_BASICTENANT] PRIMARY KEY CLUSTERED
(
	[Id]
)
GO
--PRIMARY KEY FOR TABLE [BigDbEntity]
if not exists(select * from sys.objects where name = 'PK_BIGDBENTITY' and type = 'PK')
ALTER TABLE [dbo].[BigDbEntity] WITH NOCHECK ADD 
CONSTRAINT [PK_BIGDBENTITY] PRIMARY KEY CLUSTERED
(
	[ID]
)
GO
--PRIMARY KEY FOR TABLE [Car]
if not exists(select * from sys.objects where name = 'PK_CAR' and type = 'PK')
ALTER TABLE [dbo].[Car] WITH NOCHECK ADD 
CONSTRAINT [PK_CAR] PRIMARY KEY CLUSTERED
(
	[CarId]
)
GO
--PRIMARY KEY FOR TABLE [TenantMaster]
if not exists(select * from sys.objects where name = 'PK_TENANTMASTER' and type = 'PK')
ALTER TABLE [dbo].[TenantMaster] WITH NOCHECK ADD 
CONSTRAINT [PK_TENANTMASTER] PRIMARY KEY CLUSTERED
(
	[CategoryId]
)
GO
--PRIMARY KEY FOR TABLE [CodeManagedKey]
if not exists(select * from sys.objects where name = 'PK_CODEMANAGEDKEY' and type = 'PK')
ALTER TABLE [dbo].[CodeManagedKey] WITH NOCHECK ADD 
CONSTRAINT [PK_CODEMANAGEDKEY] PRIMARY KEY CLUSTERED
(
	[ID]
)
GO
--PRIMARY KEY FOR TABLE [CompositeStuff]
if not exists(select * from sys.objects where name = 'PK_COMPOSITESTUFF' and type = 'PK')
ALTER TABLE [dbo].[CompositeStuff] WITH NOCHECK ADD 
CONSTRAINT [PK_COMPOSITESTUFF] PRIMARY KEY CLUSTERED
(
	[ID2],[ID1]
)
GO
--PRIMARY KEY FOR TABLE [Customer]
if not exists(select * from sys.objects where name = 'PK_CUSTOMER' and type = 'PK')
ALTER TABLE [dbo].[Customer] WITH NOCHECK ADD 
CONSTRAINT [PK_CUSTOMER] PRIMARY KEY CLUSTERED
(
	[CustomerId]
)
GO
--PRIMARY KEY FOR TABLE [CustomerType]
if not exists(select * from sys.objects where name = 'PK_CUSTOMERTYPE' and type = 'PK')
ALTER TABLE [dbo].[CustomerType] WITH NOCHECK ADD 
CONSTRAINT [PK_CUSTOMERTYPE] PRIMARY KEY CLUSTERED
(
	[ID]
)
GO
GO
--PRIMARY KEY FOR TABLE [Order]
if not exists(select * from sys.objects where name = 'PK_ORDER' and type = 'PK')
ALTER TABLE [dbo].[Order] WITH NOCHECK ADD 
CONSTRAINT [PK_ORDER] PRIMARY KEY CLUSTERED
(
	[OrderId5]
)
GO
--PRIMARY KEY FOR TABLE [SchemaTest]
if not exists(select * from sys.objects where name = 'PK_SCHEMATEST' and type = 'PK')
ALTER TABLE [MySchema].[SchemaTest] WITH NOCHECK ADD 
CONSTRAINT [PK_SCHEMATEST] PRIMARY KEY CLUSTERED
(
	[Id]
)
GO
--##SECTION END [CREATE PK]

--##SECTION BEGIN [CREATE INDEXES]

--##SECTION END [CREATE INDEXES]

--##SECTION BEGIN [ADD INDEXES]

--CREATE INDEX FOR [CompositeStuff]
if not exists (select * from sys.indexes where [name] = 'IX_COMPOSITESTUFF_NAME2_NAME1')
CREATE NONCLUSTERED INDEX [IX_COMPOSITESTUFF_NAME2_NAME1]
ON [dbo].[CompositeStuff] ([Name2], [Name1]);
GO

--CREATE INDEX FOR [Customer]
if not exists (select * from sys.indexes where [name] = 'IX_CUSTOMER_CUSTOMERTYPEID')
CREATE NONCLUSTERED INDEX [IX_CUSTOMER_CUSTOMERTYPEID]
ON [dbo].[Customer] ([CustomerTypeId]);
GO

--CREATE INDEX FOR [Order]
if not exists (select * from sys.indexes where [name] = 'IX_ORDER_CUSTOMERFKID')
CREATE NONCLUSTERED INDEX [IX_ORDER_CUSTOMERFKID]
ON [dbo].[Order] ([CustomerFkId]);
GO

--##SECTION END [ADD INDEXES]

--##SECTION BEGIN [ADD RELATIONS]

--ADD FOREIGN KEY FOR [CustomerType] => [Customer]
if not exists(select * from sys.objects where name = 'FK_CUSTOMER_CUSTOMERTYPE_CUSTOMERTYPEID' and type = 'F')
ALTER TABLE [dbo].[Customer]
ADD CONSTRAINT [FK_CUSTOMER_CUSTOMERTYPE_CUSTOMERTYPEID] FOREIGN KEY ([CustomerTypeId]) REFERENCES [dbo].[CustomerType] ([ID]);
GO

--ADD FOREIGN KEY FOR [Customer] => [Order]
if not exists(select * from sys.objects where name = 'FK_ORDER_CUSTOMER_CUSTOMERFKID' and type = 'F')
ALTER TABLE [dbo].[Order]
ADD CONSTRAINT [FK_ORDER_CUSTOMER_CUSTOMERFKID] FOREIGN KEY ([CustomerFkId]) REFERENCES [dbo].[Customer] ([CustomerId]);
GO

--##SECTION END [ADD RELATIONS]

--##SECTION BEGIN [STATIC DATA]

--INSERT STATIC DATA FOR [CustomerType]
SET IDENTITY_INSERT [dbo].[CustomerType] ON
if not exists (select * from [dbo].[CustomerType] where [ID] = 1)
insert into [dbo].[CustomerType] ([ID], [Name]) values (1, 'Big')
if not exists (select * from [dbo].[CustomerType] where [ID] = 2)
insert into [dbo].[CustomerType] ([ID], [Name]) values (2, 'Little')
SET IDENTITY_INSERT [dbo].[CustomerType] OFF
GO

--##SECTION END [STATIC DATA]

