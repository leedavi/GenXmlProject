
if NOT exists (select * from dbo.sysobjects where id = object_id(N'{databaseOwner}[{objectQualifier}{TableName}]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN


CREATE TABLE {databaseOwner}[{objectQualifier}{TableName}] (
	[ItemId] [int] IDENTITY (1, 1) NOT NULL ,
	[PortalId] [int] NULL ,
	[ModuleId] [int] NULL ,
	[TableCode] [nvarchar](50) NULL ,
	[KeyData] [nvarchar](MAX) NULL ,
	[ModifiedDate] [datetime] NULL ,
	[TextData] [nvarchar](MAX) NULL ,
	[XrefItemId] [int] NULL ,
	[ParentItemId] [int] NULL ,
	[XxmlString] [xml] NULL ,
	[Lang] [nvarchar] (50) NULL ,
	[UserId] [int] NOT NULL CONSTRAINT [DF_{TableName}_UserId] DEFAULT ((-1)),
	[FreeTextIndexData] [nvarchar](MAX) NULL ,
	[LegacyItemId] [int] NULL,
	[Visible] [bit] NULL,
	CONSTRAINT [PK_{TableName}] PRIMARY KEY  CLUSTERED 
	(
		[ItemId]
	)  ON [PRIMARY] 
) ON [PRIMARY]

-- Index {TableName}
 CREATE  INDEX [IX_{TableName}Xref] ON {databaseOwner}[{objectQualifier}{TableName}]([XrefItemId]) ON [PRIMARY]
 CREATE  INDEX [IX_{TableName}Mod] ON {databaseOwner}[{objectQualifier}{TableName}]([ModuleId]) ON [PRIMARY]
 CREATE  INDEX [IX_{TableName}Parent] ON {databaseOwner}[{objectQualifier}{TableName}]([ParentItemId]) ON [PRIMARY]
 CREATE  INDEX [IX_{TableName}Portal] ON {databaseOwner}[{objectQualifier}{TableName}]([PortalId]) ON [PRIMARY]
 CREATE  INDEX [IX_{TableName}Type] ON {databaseOwner}[{objectQualifier}{TableName}]([TableCode]) ON [PRIMARY]
 CREATE  INDEX [IX_{TableName}UserId] ON {databaseOwner}[{objectQualifier}{TableName}]([UserId]) ON [PRIMARY]
 CREATE  INDEX [IX_{TableName}LegacyItemId] ON {databaseOwner}[{objectQualifier}{TableName}]([LegacyItemId]) ON [PRIMARY]
 CREATE  INDEX [IX_{TableName}Lang] ON {databaseOwner}[{objectQualifier}{TableName}]([Lang]) ON [PRIMARY]
 CREATE  INDEX [IX_{TableName}Visible] ON {databaseOwner}[{objectQualifier}{TableName}]([Visible]) ON [PRIMARY]

END
