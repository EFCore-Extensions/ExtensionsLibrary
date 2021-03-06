--Idempotent Create Script

--CREATE SCHEMA [MySchema]
CREATE SCHEMA IF NOT EXISTS MySchema;

--CREATE TABLE [BasicTenant]
CREATE TABLE IF NOT EXISTS public."BasicTenant" (
	"Id" INTEGER GENERATED ALWAYS AS IDENTITY NOT NULL,
	"Name" TEXT NOT NULL,
	"TenantId" VARCHAR (50) NOT NULL
);


--CREATE TABLE [BigDbEntity]
CREATE TABLE IF NOT EXISTS public."BigDbEntity" (
	"ID" INTEGER GENERATED ALWAYS AS IDENTITY NOT NULL,
	"MyBlob1" TEXT NOT NULL,
	"MyBlob2" TEXT NULL,
	"MyBool" BOOLEAN NOT NULL,
	"MyByteArray" BYTEA NULL,
	"MyChar" CHAR NOT NULL,
	"MyDateTime" TIMESTAMP NOT NULL,
	"MyDecmial" DOUBLE PRECISION NOT NULL,
	"MyDouble" DOUBLE PRECISION NOT NULL,
	"MyFloat" DOUBLE PRECISION NOT NULL,
	"MyInt16" SMALLINT NOT NULL,
	"MyShort" SMALLINT NOT NULL,
	"MySingle" DOUBLE PRECISION NOT NULL,
	"MyXml" TEXT NULL,
	"JustAnotherName" VARCHAR (50) NOT NULL
);


--CREATE TABLE [Car]
CREATE TABLE IF NOT EXISTS public."Car" (
	"CarId" INTEGER GENERATED ALWAYS AS IDENTITY NOT NULL,
	"IsDeleted" BOOLEAN NOT NULL,
	"Name" VARCHAR (50) NOT NULL
);


--CREATE TABLE [TenantMaster]
CREATE TABLE IF NOT EXISTS public."TenantMaster" (
	"CategoryId" INTEGER GENERATED ALWAYS AS IDENTITY NOT NULL,
	"Name" VARCHAR (50) NOT NULL
);


--CREATE TABLE [CodeManagedKey]
CREATE TABLE IF NOT EXISTS public."CodeManagedKey" (
	"ID" INTEGER NOT NULL,
	"Concurrency" VARCHAR NOT NULL,
	"Data" TEXT NULL,
	"Name" VARCHAR (50) NOT NULL,
	"Version" INTEGER NOT NULL
);


--CREATE TABLE [CompositeStuff]
CREATE TABLE IF NOT EXISTS public."CompositeStuff" (
	"ID2" INTEGER NOT NULL,
	"ID1" INTEGER NOT NULL,
	"Name1" VARCHAR (50) NULL,
	"Name2" VARCHAR (50) NULL,
	"Name3" VARCHAR (50) NULL,
	PRIMARY KEY 
	(
		"Name2", "Name1"
	)
);


--CREATE TABLE [Customer]
CREATE TABLE IF NOT EXISTS public."Customer" (
	"CustomerId" VARCHAR NOT NULL,
	"CreatedBy" VARCHAR (50) NULL,
	"CreatedDate" TIMESTAMP NOT NULL,
	"CustomerTypeId" INTEGER NOT NULL,
	"ModifiedBy" VARCHAR (50) NULL,
	"ModifiedDate" TIMESTAMP NOT NULL,
	"Name" VARCHAR (50) NOT NULL,
	"TenantId" VARCHAR (50) NOT NULL,
	"Timestamp" BYTEA NOT NULL,
	PRIMARY KEY 
	(
		"CustomerTypeId"
	)
);


--CREATE TABLE [CustomerType]
CREATE TABLE IF NOT EXISTS public."CustomerType" (
	"ID" INTEGER GENERATED ALWAYS AS IDENTITY NOT NULL,
	"Name" VARCHAR (50) NOT NULL
);


--CREATE TABLE [HeapTable]
CREATE TABLE IF NOT EXISTS public."HeapTable" (
	"ID" INTEGER NOT NULL,
	"Name" VARCHAR (50) NULL
);


--CREATE TABLE [Order]
CREATE TABLE IF NOT EXISTS public."Order" (
	"OrderId5" VARCHAR NOT NULL,
	"CreatedBy" VARCHAR (50) NULL,
	"CreatedDate" TIMESTAMP NOT NULL,
	"CustomerFkId" VARCHAR NOT NULL,
	"DefaultedTime" TIMESTAMP NOT NULL,
	"ModifiedBy" VARCHAR (50) NULL,
	"ModifiedDate" TIMESTAMP NOT NULL,
	"Quantity" INTEGER NOT NULL,
	"TenantId" VARCHAR (50) NOT NULL,
	"Timestamp" BYTEA NOT NULL,
	PRIMARY KEY 
	(
		"CustomerFkId"
	)
);


--CREATE TABLE [SchemaTest]
CREATE TABLE IF NOT EXISTS MySchema."SchemaTest" (
	"Id" INTEGER GENERATED ALWAYS AS IDENTITY NOT NULL,
	"Name" VARCHAR (50) NOT NULL
);


