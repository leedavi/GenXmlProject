if NOT exists (select * from dbo.sysobjects where id = object_id(N'{databaseOwner}[{objectQualifier}{TableName}Idx]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN

CREATE TABLE {databaseOwner}[{objectQualifier}{TableName}Idx](
	[ItemId] [int] NOT NULL,
	[Lang] [nvarchar](6) NOT NULL,
	[WordToIdx] [nvarchar](255) NULL,
	[TableCode] [nvarchar](50) NULL
 CONSTRAINT [PK_{TableName}Idx] PRIMARY KEY CLUSTERED 
(
	[ItemId] ASC,
	[Lang] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]

-- Index {TableName}Idx
CREATE  INDEX [IX_{TableName}IdxItemId] ON {databaseOwner}[{objectQualifier}{TableName}Idx]([ItemId]) ON [PRIMARY]
CREATE  INDEX [IX_{TableName}IdxLang] ON {databaseOwner}[{objectQualifier}{TableName}Idx]([Lang]) ON [PRIMARY]
CREATE  INDEX [IX_{TableName}WordToIdx] ON {databaseOwner}[{objectQualifier}{TableName}Idx]([WordToIdx]) ON [PRIMARY]
CREATE  INDEX [IX_{TableName}TableCode] ON {databaseOwner}[{objectQualifier}{TableName}Idx]([TableCode]) ON [PRIMARY]

END