

--------------------------------------------------------------------------------------------------------------
-- CREATE TABLES                 -----------------------------------------------------------------------------
--------------------------------------------------------------------------------------------------------------


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
	[XmlString] [xml] NULL ,
	[Lang] [nvarchar] (50) NULL ,
	[UserId] [int] NOT NULL CONSTRAINT [DF_{TableName}_UserId] DEFAULT ((-1)),
	[LegacyItemId] [int] NULL,
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

END


if NOT exists (select * from dbo.sysobjects where id = object_id(N'{databaseOwner}[{objectQualifier}{TableName}Idx]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN

-------------------------------------------------------------------------------
--------------              Create Index Table                     ------------
-- Create an index table. This is so we can get performance on sorting         --
-- The standard way of creating a XML index and calculated columns for indexing --
-- works OK for selection, but selection is already fairly quick in a small DB --
-- but sorting by calculated columns seems no quicker than sorting the XML     --
-- directly.  Therefore we are implementing a index table to help speed it up  --
-- NOTE: if extra fields need orderby, they will need to be added + the trigger --
-- Primary Key has been created for Azure install, which always requires a primary key --
--
-- We're trying to create a generic table for all data sort possiblities. --
-- Depending on the data, the trigger to populate this may need altering. --
-------------------------------------------------------------------------------

CREATE TABLE {databaseOwner}[{objectQualifier}{TableName}Idx](
	[ItemId] [int] NOT NULL,
	[Lang] [nvarchar](6) NOT NULL,
	[WordToIdx] [nvarchar](255) NULL,
	[TableCode] [nvarchar](50) NULL,
	[DataField] [nvarchar](50) NULL
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
CREATE  INDEX [IX_{TableName}DataField] ON {databaseOwner}[{objectQualifier}{TableName}Idx]([DataField]) ON [PRIMARY]

END

if not exists (select * from dbo.sysobjects where id = object_id(N'[{TableName}Lang]') and OBJECTPROPERTY(id, N'IsUserTable') = 1)
BEGIN

---------------------------------------------------------------------------------
--------------            Create XML Language Table                  ------------
-- Create an index table for XML merge. This is so we can get performance on   --
-- XML merge.  The format we require for localization is slow, so we pre-build the --
-- merge XML structure in this table so we can return the data quickly.          --
---------------------------------------------------------------------------------

CREATE TABLE {databaseOwner}[{objectQualifier}{TableName}Lang](
	[ParentItemId] [int] NOT NULL,
	[Lang] [nvarchar](50) NOT NULL,
	[XmlString] [xml] NULL,
 CONSTRAINT [PK_{TableName}Lang] PRIMARY KEY CLUSTERED 
(
	[ParentItemId] ASC,
	[Lang] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

END

{GO}


--------------------------------------------------------------------------------------------------------------
-- CREATE FUNCTIONS              -----------------------------------------------------------------------------
--------------------------------------------------------------------------------------------------------------


if exists (select * from dbo.sysobjects where id = object_id(N'{databaseOwner}[{objectQualifier}{TableName}LangMerge]') and OBJECTPROPERTY(id, N'IsScalarFunction') = 1)
drop function {databaseOwner}[{objectQualifier}{TableName}LangMerge]
{GO}

CREATE FUNCTION {databaseOwner}[{objectQualifier}{TableName}LangMerge](@xmllangdata AS XML,@xmlbasedata AS XML)
RETURNS XML
BEGIN

DECLARE @rtndata AS XML

IF NOT @xmlbasedata IS NULL
BEGIN
	IF NOT @xmllangdata IS NULL
	BEGIN
		SET @xmlbasedata.modify('insert <lang/> as last into /genxml[1]')
		SET @xmlbasedata.modify('insert sql:variable("@xmllangdata") as last into /genxml[1]/lang[1]')
	END
	SET @rtndata = @xmlbasedata
END
ELSE
BEGIN
	-- is not a language record so just return the language data
	SET @rtndata = ISNULL(@xmllangdata,'')
END

RETURN @rtndata

END

{GO}

-------------------------------------------------------------------------------
--------------                       SPROCS                        ------------
-------------------------------------------------------------------------------


if exists (select * from dbo.sysobjects where id = object_id(N'{databaseOwner}[{objectQualifier}{TableName}_DeleteTable]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure {databaseOwner}[{objectQualifier}{TableName}_DeleteTable]
{GO}

CREATE   PROCEDURE {databaseOwner}[{objectQualifier}{TableName}_DeleteTable]
@TableCode nvarchar(max)
AS
begin

	DECLARE @ItemId as int

	DECLARE inserted_cursor CURSOR LOCAL FOR
	Select itemid from {databaseOwner}[{objectQualifier}{TableName}] where TableCode = @TableCode

	OPEN inserted_cursor
	FETCH NEXT FROM inserted_cursor INTO @ItemId
	WHILE @@FETCH_STATUS = 0
	BEGIN

	exec {databaseOwner}[{objectQualifier}{TableName}_DeleteKey(@ItemId)]

	END


end


{GO}

if exists (select * from dbo.sysobjects where id = object_id(N'{databaseOwner}[{objectQualifier}{TableName}_DeleteKey]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure {databaseOwner}[{objectQualifier}{TableName}_DeleteKey]
{GO}

CREATE   PROCEDURE {databaseOwner}[{objectQualifier}{TableName}_DeleteKey]
@ItemID int
AS
begin

if @ItemId > 0 Begin

	delete from {databaseOwner}[{objectQualifier}{TableName}] where ItemId = @ItemId
	
	-- Delete all linked child records.
	delete from {databaseOwner}[{objectQualifier}{TableName}] where ParentItemId = @ItemId

	delete from {databaseOwner}[{objectQualifier}{TableName}] where XrefItemId = @ItemId

end
	
end


{GO}


if exists (select * from dbo.sysobjects where id = object_id(N'{databaseOwner}[{objectQualifier}{TableName}_Update]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure {databaseOwner}[{objectQualifier}{TableName}_Update]
{GO}


CREATE   PROCEDURE {databaseOwner}[{objectQualifier}{TableName}_Update]
(
@ItemId int,
@PortalId int, 
@ModuleId int,
@TableCode nvarchar(50),
@KeyData nvarchar(MAX),
@ModifiedDate datetime,
@TextData nvarchar(MAX),
@XrefItemId int,
@ParentItemId int,
@XmlString xml,
@Lang nvarchar(10),
@UserId int
)
AS
BEGIN

	if not exists (select ItemID from {databaseOwner}[{objectQualifier}{TableName}] where ItemID = @ItemID
 )
	begin
		insert into {databaseOwner}[{objectQualifier}{TableName}]
		(
PortalId, 
ModuleId,
TableCode,
KeyData,
ModifiedDate,
TextData,
XrefItemId,
ParentItemId,
XmlString,
Lang,
UserId
		)
		values
		(
@PortalId, 
@ModuleId,
@TableCode,
@KeyData,
@ModifiedDate,
@TextData,
@XrefItemId,
@ParentItemId,
@XmlString,
@Lang,
@UserId
		)
		
		set @ItemID = @@IDENTITY

	end
	else
	begin
		Update {databaseOwner}[{objectQualifier}{TableName}]
		set 
PortalId = @PortalId, 
ModuleId = @ModuleId,
TableCode = @TableCode,
KeyData = @KeyData,
ModifiedDate = @ModifiedDate,
TextData = @TextData,
XrefItemId = @XrefItemId,
ParentItemId = @ParentItemId,
XmlString = @XmlString,
Lang = @Lang,
UserId = @UserId
		where ItemId = @ItemId
 
	end
	
	select @ItemID

END

{GO}


if exists (select * from dbo.sysobjects where id = object_id(N'{databaseOwner}[{objectQualifier}{TableName}_Get]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure {databaseOwner}[{objectQualifier}{TableName}_Get]
{GO}


CREATE     PROCEDURE {databaseOwner}[{objectQualifier}{TableName}_Get]
@ItemID int,
@Lang nvarchar(10)
AS
begin
	select

	NB1.[ItemId],
	NB1.[PortalId],
	NB1.[ModuleId],
	NB1.[TableCode],
	NB1.[KeyData],
	NB1.[ModifiedDate],
	NB1.[TextData],
	NB1.[XrefItemId],
	NB1.[ParentItemId],
	ISNULL(NB2.[XmlString],NB1.[XmlString]) as [XmlString],
	ISNULL(NB2.[Lang],ISNULL(NB1.[Lang],'')) as [Lang],
	NB1.[UserId]
	from {databaseOwner}[{objectQualifier}{TableName}] as NB1
	left join {databaseOwner}[{objectQualifier}{TableName}Lang] as NB2 on NB2.ParentItemId = NB1.ItemId and NB2.lang = @Lang 
	where NB1.ItemId = @ItemId
end

{GO}





if exists (select * from dbo.sysobjects where id = object_id(N'{databaseOwner}[{objectQualifier}{TableName}_GetListLangNoPage]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
DROP PROCEDURE {databaseOwner}[{objectQualifier}{TableName}_GetListLangNoPage]
{GO}

CREATE    PROCEDURE {databaseOwner}[{objectQualifier}{TableName}_GetListLangNoPage]
@PortalId int, 
@ModuleId int,
@TableCode nvarchar(50),
@Filter nvarchar(max),
@OrderBy nvarchar(500),
@ReturnLimit int = 0,
@Lang nvarchar(10) = ''

AS
begin

-- This SPROC return the Get List with LANGAUGE and NO PAGING

	SET NOCOUNT ON
	  DECLARE
		 @STMT nvarchar(max)         -- SQL to execute
		,@rtnFields nvarchar(max)
		,@NB4cascade nvarchar(max)

	IF (@PortalId >= 0) BEGIN

		IF (@ModuleId >= 0) BEGIN
			SET @Filter = ' and (NB1.PortalId = '''  + Convert(nvarchar(10),@PortalId) + ''' or NB1.PortalId = ''-1'') and (NB1.ModuleId = ''' + Convert(nvarchar(10),@ModuleId) + ''' or NB1.ModuleId = ''-1'') ' + @Filter
		END ELSE BEGIN
			SET @Filter = ' and (NB1.PortalId = '''  + Convert(nvarchar(10),@PortalId) + '''  or NB1.PortalId = ''-1'') ' + @Filter
		END 

	END 

	SET @Filter = REPLACE(@Filter,'[XmlString]','ISNULL(NB2.[XmlString],NB1.[XmlString])')
	SET @OrderBy = REPLACE(@OrderBy,'[XmlString]','ISNULL(NB2.[XmlString],NB1.[XmlString])')

	set @rtnFields = ' NB1.[ItemId] '
	set @rtnFields = @rtnFields + ',ISNULL(NB2.[XmlString],NB1.[XmlString]) as [XmlString] '				
	set @rtnFields = @rtnFields + ',ISNULL(NB2.[Lang],ISNULL(NB1.[Lang],'''')) as [Lang] '	 
 
	set @rtnFields = @rtnFields + ',NB1.[PortalId] '
	set @rtnFields = @rtnFields + ',NB1.[ModuleId] '
	set @rtnFields = @rtnFields + ',NB1.[TableCode] '
	set @rtnFields = @rtnFields + ',NB1.[KeyData] '
	set @rtnFields = @rtnFields + ',NB1.[ModifiedDate] '
	set @rtnFields = @rtnFields + ',NB1.[TextData] '
	set @rtnFields = @rtnFields + ',NB1.[XrefItemId] '
	set @rtnFields = @rtnFields + ',NB1.[ParentItemId] '
	set @rtnFields = @rtnFields + ',NB1.[UserId] '


	-- Return records without paging.
	set @STMT = ' SELECT ' 
				
	if @ReturnLimit > 0 
	begin
		set @STMT = @STMT + ' top ' + convert(nvarchar(10),@ReturnLimit)
	end

	set @STMT = @STMT + @rtnFields + ' FROM {databaseOwner}[{objectQualifier}{TableName}] as NB1 '

	set @STMT = @STMT + ' left join  {databaseOwner}[{objectQualifier}{TableName}Idx] as NB3 on NB3.ItemId = NB1.ItemId and NB3.[Lang] = ''' + @Lang + ''''

	set @STMT = @STMT + '  left join {databaseOwner}[{objectQualifier}{TableName}Lang] as NB2 on NB2.ParentItemId = NB1.ItemId and NB2.[Lang] = ''' + @Lang + ''''
	
	IF (@OrderBY like '%{bycategoryproduct}%')
	BEGIN
		DECLARE @categoryid nvarchar(max)
		SET @categoryid = LTRIM(RTRIM(replace(@OrderBY ,'{bycategoryproduct}','')))
		if (@categoryid != '')
		BEGIN
				SET @NB4cascade = ''				
				IF CHARINDEX('CATCASCADE',@filter) > 0  SET @NB4cascade = 'or NB4.TableCode = ''CATCASCADE'''

				SET @OrderBY = ' order by NB4.[XmlString].value(''(genxml/sort)[1]'',''int'')'
				set @STMT = @STMT + '  left join {databaseOwner}[{objectQualifier}{TableName}] as NB4 on (NB4.TableCode = ''CATXREF'' ' + @NB4cascade + ' ) and NB4.ParentItemId = NB1.ItemId and NB4.XrefItemId = ' + @categoryid + ' '
		END 
	END

	IF (RIGHT(@TableCode,1) = '%')
	BEGIN
		set @STMT = @STMT + ' WHERE NB1.TableCode Like ''' + @TableCode + ''' ' + @Filter + ' ' + @OrderBy
	END ELSE
	BEGIN
		IF (@TableCode = '')
		BEGIN
			set @STMT = @STMT + ' WHERE NB1.TableCode != '''' ' + @Filter + @OrderBy
		END ELSE
		BEGIN
			set @STMT = @STMT + ' WHERE NB1.TableCode = ''' + @TableCode + ''' ' + @Filter  + ' ' + @OrderBy
		END
	END
    
	EXEC sp_executeSQL @STMT                 -- return requested records

end
{GO}


if exists (select * from dbo.sysobjects where id = object_id(N'{databaseOwner}[{objectQualifier}{TableName}_GetListLangPage]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
DROP PROCEDURE {databaseOwner}[{objectQualifier}{TableName}_GetListLangPage]
{GO}

CREATE    PROCEDURE {databaseOwner}[{objectQualifier}{TableName}_GetListLangPage]
@PortalId int, 
@ModuleId int,
@TableCode nvarchar(50),
@Filter nvarchar(max),
@OrderBy nvarchar(500),
@ReturnLimit int = 0,
@pageNum int = 0,
@PageSize int = 0,
@RecordCount int = 0,
@Lang nvarchar(10) = ''

AS
begin

-- This SPROC return the Get List with LANGAUGE and PAGING

	SET NOCOUNT ON
	  DECLARE
		 @STMT nvarchar(max)         -- SQL to execute
		,@rtnFields nvarchar(max)
		,@NB4cascade nvarchar(max)

	IF (@PortalId >= 0) BEGIN

		IF (@ModuleId >= 0) BEGIN
			SET @Filter = ' and (NB1.PortalId = '''  + Convert(nvarchar(10),@PortalId) + ''' or NB1.PortalId = ''-1'') and (NB1.ModuleId = ''' + Convert(nvarchar(10),@ModuleId) + ''' or NB1.ModuleId = ''-1'') ' + @Filter
		END ELSE BEGIN
			SET @Filter = ' and (NB1.PortalId = '''  + Convert(nvarchar(10),@PortalId) + '''  or NB1.PortalId = ''-1'') ' + @Filter
		END 

	END 

	SET @Filter = REPLACE(@Filter,'[XmlString]','ISNULL(NB2.[XmlString],NB1.[XmlString])')
	SET @OrderBy = REPLACE(@OrderBy,'[XmlString]','ISNULL(NB2.[XmlString],NB1.[XmlString])')

	set @rtnFields = ' NB1.[ItemId] '
	set @rtnFields = @rtnFields + ',ISNULL(NB2.[XmlString],NB1.[XmlString]) as [XmlString] '				
	set @rtnFields = @rtnFields + ',ISNULL(NB2.[Lang],ISNULL(NB1.[Lang],'''')) as [Lang] '	 
 
	set @rtnFields = @rtnFields + ',NB1.[PortalId] '
	set @rtnFields = @rtnFields + ',NB1.[ModuleId] '
	set @rtnFields = @rtnFields + ',NB1.[TableCode] '
	set @rtnFields = @rtnFields + ',NB1.[KeyData] '
	set @rtnFields = @rtnFields + ',NB1.[ModifiedDate] '
	set @rtnFields = @rtnFields + ',NB1.[TextData] '
	set @rtnFields = @rtnFields + ',NB1.[XrefItemId] '
	set @rtnFields = @rtnFields + ',NB1.[ParentItemId] '
	set @rtnFields = @rtnFields + ',NB1.[UserId] '



			-- Do Paging
		SET @STMT = 'DECLARE @recct int '
		set @STMT = @STMT + ' SET @recct = ' + Convert(nvarchar(5),@RecordCount) 
		
		set @STMT = @STMT + '   DECLARE @lbound int, @ubound int '

		SET @pageNum = ABS(@pageNum)
		SET @pageSize = ABS(@pageSize)
		IF @pageNum < 1 SET @pageNum = 1
		IF @pageSize < 1 SET @pageSize = 1

		set @STMT = @STMT + ' SET @lbound = ' + convert(nvarchar(50),((@pageNum - 1) * @pageSize))
		set @STMT = @STMT + ' SET @ubound = @lbound + ' + convert(nvarchar(50),(@pageSize + 1))
		set @STMT = @STMT + ' IF @lbound >= @recct BEGIN '
		set @STMT = @STMT + '   SET @ubound = @recct + 1 '
		set @STMT = @STMT + '   SET @lbound = @ubound - (' + convert(nvarchar(50),(@pageSize + 1)) + ') ' -- return the last page of records if no records would be on the specified page '
		set @STMT = @STMT + ' END '
		
		-- Default order by clause
		if @OrderBy = '' 
		Begin
			set @OrderBy = ' ORDER BY ModifiedDate DESC '
		End
		
		set @STMT = @STMT + ' SELECT '
		if @ReturnLimit > 0 
		begin
			set @STMT = @STMT + ' top ' + convert(nvarchar(10),@ReturnLimit)
		end
		
		set @STMT = @STMT + @rtnFields		

		DECLARE @categoryid nvarchar(max)
		declare @NB4join nvarchar(max)
		SET @NB4join = ''
		IF (@OrderBY like '%{bycategoryproduct}%')
		BEGIN
			SET @categoryid = LTRIM(RTRIM(replace(@OrderBY ,'{bycategoryproduct}','')))
			if (@categoryid != '')
			BEGIN
				SET @NB4cascade = ''				
				IF CHARINDEX('CATCASCADE',@filter) > 0  SET @NB4cascade = 'or NB4.TableCode = ''CATCASCADE'''

				SET @OrderBY = ' order by NB4.[XmlString].value(''(genxml/sort)[1]'',''int''), NB3.productname  '
				set @NB4join = '  left join {databaseOwner}[{objectQualifier}{TableName}] as NB4 on (NB4.TableCode = ''CATXREF'' ' + @NB4cascade + ' ) and NB4.ParentItemId = NB1.ItemId and NB4.XrefItemId = ' + @categoryid + ' '
			END ELSE
			BEGIN
					SET @OrderBY = ' order by NB3.productname '
			END
		END

		set @STMT = @STMT + ' FROM    (
								SELECT  ROW_NUMBER() OVER(' + @orderBy + ') AS row, '
		set @STMT = @STMT + @rtnFields		
		set @STMT = @STMT + ' FROM {databaseOwner}[{objectQualifier}{TableName}]  as NB1 '
		set @STMT = @STMT + ' left join  {databaseOwner}[{objectQualifier}{TableName}Idx] as NB3 on NB3.ItemId = NB1.ItemId and NB3.[Lang] = ''' + @Lang + ''''
		set @STMT = @STMT + ' left join {databaseOwner}[{objectQualifier}{TableName}Lang] as NB2 on NB2.ParentItemId = NB1.ItemId and NB2.[Lang] = ''' + @Lang + ''' ' 

			set @STMT = @STMT + @NB4join

		
			IF (RIGHT(@TableCode,1) = '%')
			BEGIN
				set @STMT = @STMT + 'WHERE NB1.TableCode Like ''' + @TableCode + ''' ' + @Filter  
			END ELSE
			BEGIN
				IF (@TableCode = '')
				BEGIN
					set @STMT = @STMT + 'WHERE NB1.TableCode != ''''' + @Filter  
				END ELSE
				BEGIN
					set @STMT = @STMT + 'WHERE NB1.TableCode = ''' + @TableCode + ''' ' + @Filter  
				END
			END	                                                              
			
			set @STMT = @STMT + ' ) AS NB1 '
			set @STMT = @STMT + ' left join  {databaseOwner}[{objectQualifier}{TableName}Idx] as NB3 on NB3.ItemId = NB1.ItemId and NB3.[Lang] = ''' + @Lang + ''''
			set @STMT = @STMT + ' left join {databaseOwner}[{objectQualifier}{TableName}Lang] as NB2 on NB2.ParentItemId = NB1.ItemId and NB2.[Lang] = ''' + @Lang + ''' '
			set @STMT = @STMT + @NB4join
			set @STMT = @STMT + ' WHERE row > @lbound AND row < @ubound '

		EXEC sp_executeSQL @STMT                 -- return requested records


end
{GO}



if exists (select * from dbo.sysobjects where id = object_id(N'{databaseOwner}[{objectQualifier}{TableName}_GetList]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure {databaseOwner}[{objectQualifier}{TableName}_GetList]
{GO}

CREATE    PROCEDURE {databaseOwner}[{objectQualifier}{TableName}_GetList]
@PortalId int, 
@ModuleId int,
@TableCode nvarchar(50),
@Filter nvarchar(max),
@OrderBy nvarchar(500),
@ReturnLimit int = 0,
@pageNum int = 0,
@PageSize int = 0,
@RecordCount int = 0,
@Lang nvarchar(10) = ''

AS
begin

	IF @pageSize = 0 BEGIN
		-- NO PAGING
		   exec {databaseOwner}[{objectQualifier}{TableName}_GetListLangNoPage] @PortalId,@ModuleId,@TableCode,@Filter,@OrderBy, @ReturnLimit ,@Lang
	END ELSE BEGIN
		-- PAGING
		   exec {databaseOwner}[{objectQualifier}{TableName}_GetListLangPage] @PortalId,@ModuleId,@TableCode,@Filter,@OrderBy, @ReturnLimit ,@pageNum,@PageSize,@RecordCount,@Lang
	END
end
{GO}



if exists (select * from dbo.sysobjects where id = object_id(N'{databaseOwner}[{objectQualifier}{TableName}_GetData]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure {databaseOwner}[{objectQualifier}{TableName}_GetData]
{GO}

CREATE     PROCEDURE {databaseOwner}[{objectQualifier}{TableName}_GetData]
@ItemID int
AS
begin
	select
ItemId,
PortalId, 
ModuleId,
TableCode,
KeyData,
ModifiedDate,
TextData,
XrefItemId,
ParentItemId,
XmlString,
Lang,
UserId
	from {databaseOwner}[{objectQualifier}{TableName}] as NB1
	where NB1.ItemId = @ItemId
end
{GO}

if exists (select * from dbo.sysobjects where id = object_id(N'{databaseOwner}[{objectQualifier}{TableName}_GetDataLang]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure {databaseOwner}[{objectQualifier}{TableName}_GetDataLang]
{GO}

CREATE     PROCEDURE {databaseOwner}[{objectQualifier}{TableName}_GetDataLang]
@ParentItemId int,
@Lang nvarchar(10)
AS
begin

	declare @TableCodelang nvarchar(max)
	set @TableCodelang = (select TableCode from {databaseOwner}[{objectQualifier}{TableName}] where itemid = @ParentItemId) + 'LANG'

	select
ItemId,
PortalId, 
ModuleId,
TableCode,
KeyData,
ModifiedDate,
TextData,
XrefItemId,
ParentItemId,
XmlString,
Lang,
UserId
	from {databaseOwner}[{objectQualifier}{TableName}] as NB1
	where NB1.ParentItemId = @ParentItemId and NB1.Lang = @Lang and TableCode = @TableCodelang
end
{GO}

if exists (select * from dbo.sysobjects where id = object_id(N'{databaseOwner}[{objectQualifier}{TableName}_GetDataByKey]') and OBJECTPROPERTY(id, N'IsProcedure') = 1)
drop procedure {databaseOwner}[{objectQualifier}{TableName}_GetDataByKey]
{GO}

CREATE     PROCEDURE {databaseOwner}[{objectQualifier}{TableName}_GetDataByKey]
@TableCode nvarchar(50),
@KeyData int,
@Lang nvarchar(10)
AS
begin

	select top 1
ItemId,
PortalId, 
ModuleId,
TableCode,
KeyData,
ModifiedDate,
TextData,
XrefItemId,
ParentItemId,
XmlString,
Lang,
UserId
	from {databaseOwner}[{objectQualifier}{TableName}] as NB1
	where NB1.KeyData = @KeyData and NB1.Lang = @Lang and TableCode = @TableCode
end
{GO}

-------------------------------------------------------------------------------
----------         Triggers for the index table                    ------------
-------------------------------------------------------------------------------

if exists (select * from dbo.sysobjects where id = object_id(N'{databaseOwner}[{objectQualifier}Trigger_{TableName}Idx]') and OBJECTPROPERTY(id, N'IsTrigger') = 1)
DROP TRIGGER {databaseOwner}[{objectQualifier}Trigger_{TableName}Idx]
{GO}

CREATE TRIGGER {databaseOwner}[{objectQualifier}Trigger_{TableName}Idx]
  ON {databaseOwner}[{objectQualifier}{TableName}]
 AFTER INSERT, UPDATE, DELETE
 AS
BEGIN

    SET NOCOUNT ON;

	DECLARE @ParentItemId int
	DECLARE @ItemId int	
	DECLARE @Lang nvarchar(10)
	DECLARE @insertItemId int
	DECLARE @ProductRef nvarchar(max)
	DECLARE @ProductName nvarchar(max)
	DECLARE @Manufacturer nvarchar(max)
	DECLARE @Summary nvarchar(max)
	DECLARE @SEOName nvarchar(max)
	DECLARE @TagWords nvarchar(max)
	DECLARE @SEOPageTitle nvarchar(max)
	DECLARE @FromPrice nvarchar(max)
	DECLARE @Qty nvarchar(max)
	DECLARE @TableCode nvarchar(max)
	DECLARE @Visible bit
	DECLARE @chkarchived nvarchar(max)
	DECLARE @chkisdeleted nvarchar(max)
	DECLARE @chkishidden nvarchar(max)
	
	-- delete all idx records for item
	IF EXISTS(SELECT * FROM DELETED)
	BEGIN
		DECLARE deleted_cursor CURSOR LOCAL FOR
		SELECT ItemId,Lang,TableCode,ParentItemId FROM DELETED 

		OPEN deleted_cursor
		FETCH NEXT FROM deleted_cursor INTO @ItemId,@Lang,@TableCode,@ParentItemId
		WHILE @@FETCH_STATUS = 0
		BEGIN

			delete from {databaseOwner}[{objectQualifier}{TableName}Idx] where ItemId = @itemId
			and itemid not in (select ItemId FROM inserted)

			if (ISNULL(@Lang,'') = '')
			BEGIN
				delete from {databaseOwner}[{objectQualifier}{TableName}Lang] where ParentItemId = @ItemId
			END
			ELSE
			BEGIN
				delete from {databaseOwner}[{objectQualifier}{TableName}Lang] where ParentItemId = @ParentItemId and Lang = @Lang
			END

		FETCH NEXT FROM deleted_cursor INTO @ItemId,@Lang,@TableCode,@ParentItemId
		END

		CLOSE deleted_cursor;
		DEALLOCATE deleted_cursor;

	END

	DECLARE inserted_cursor CURSOR LOCAL FOR
	SELECT ItemId, Lang, TableCode, ParentItemId FROM inserted 

	OPEN inserted_cursor
	FETCH NEXT FROM inserted_cursor INTO @ItemId,@Lang,@TableCode,@ParentItemId
	WHILE @@FETCH_STATUS = 0
	BEGIN

		-----------------------------------------------------------------------
		---- UPDATE {TableName}Lang table ----

		DECLARE @XmlString1 as xml
		DECLARE @XmlString2 as xml
		DECLARE @LangLang nvarchar(10)

		IF ISNULL(@Lang,'') = ''
		BEGIN
			-- Update All language records.

			select @XmlString1 = XmlString from {databaseOwner}[{objectQualifier}{TableName}] where ItemId = @ItemId

			DECLARE idxData2 CURSOR LOCAL FOR
			select Lang, XmlString from {databaseOwner}[{objectQualifier}{TableName}] where ParentItemId = @ItemId and ISNULL(Lang,'') != ''

			OPEN idxData2
			FETCH NEXT FROM idxData2 INTO @LangLang, @XmlString2
			WHILE @@FETCH_STATUS = 0
			BEGIN
				if Exists(Select ParentItemId from {databaseOwner}[{objectQualifier}{TableName}Lang] where ParentItemId = @ItemId and Lang = @LangLang)
				BEGIN
					update {databaseOwner}[{objectQualifier}{TableName}Lang] Set XmlString = {databaseOwner}{objectQualifier}{TableName}LangMerge(@XmlString2,@XmlString1)
					where ParentItemId = @ItemId and Lang = @LangLang 
				END 
				ELSE
				BEGIN
					insert into {databaseOwner}[{objectQualifier}{TableName}Lang] (ParentItemId, Lang, XmlString) values (@ItemId, @LangLang, {databaseOwner}{objectQualifier}{TableName}LangMerge(@XmlString2,@XmlString1))
				END
			FETCH NEXT FROM idxData2 INTO @LangLang, @XmlString2
			END

			CLOSE idxData2;
			DEALLOCATE idxData2;

		END
		ELSE
		BEGIN
			select @XmlString1 = XmlString from {databaseOwner}[{objectQualifier}{TableName}] where ItemId = @ParentItemId
			select @XmlString2 = XmlString from {databaseOwner}[{objectQualifier}{TableName}] where ParentItemId = @ParentItemId and Lang = @Lang 

			-- Update single Language record
			if Exists(Select ParentItemId from {databaseOwner}[{objectQualifier}{TableName}Lang] where ParentItemId = @ParentItemId and Lang = @Lang)
			BEGIN
				update {databaseOwner}[{objectQualifier}{TableName}Lang] Set XmlString = {databaseOwner}{objectQualifier}{TableName}LangMerge(@XmlString2,@XmlString1)
				where ParentItemId = @ParentItemId and Lang = @Lang 
			END 
			ELSE
			BEGIN
				insert into {databaseOwner}[{objectQualifier}{TableName}Lang] (ParentItemId, Lang, XmlString) values (@ParentItemId, @Lang, {databaseOwner}{objectQualifier}{TableName}LangMerge(@XmlString2,@XmlString1))
			END
		END


		-----------------------------------------------------------------------

	
	FETCH NEXT FROM inserted_cursor INTO @ItemId,@Lang,@TableCode,@ParentItemId
	END

	CLOSE inserted_cursor;
	DEALLOCATE inserted_cursor;
	
END
{GO}







