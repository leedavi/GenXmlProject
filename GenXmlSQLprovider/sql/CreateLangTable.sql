if not exists (select * from dbo.sysobjects where id = object_id(N'[{TableName}Lang]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN

CREATE TABLE {databaseOwner}[{objectQualifier}{TableName}Lang](
	[ParentItemId] [int] NOT NULL,
	[Lang] [nvarchar](50) NOT NULL,
	[XMLData] [xml] NULL,
 CONSTRAINT [PK_{TableName}Lang] PRIMARY KEY CLUSTERED 
(
	[ParentItemId] ASC,
	[Lang] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

END