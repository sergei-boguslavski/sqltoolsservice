﻿//
// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
//

namespace Microsoft.SqlTools.ServiceLayer.Connection.ReliableConnection
{
    static class SqlConnectionHelperScripts
    {
        public const string EngineEdition = "SELECT SERVERPROPERTY('EngineEdition'), SERVERPROPERTY('productversion'), SERVERPROPERTY ('productlevel'), SERVERPROPERTY ('edition'), SERVERPROPERTY ('MachineName'), (SELECT CASE WHEN EXISTS (SELECT TOP 1 1 from [sys].[all_columns] WITH (NOLOCK) WHERE name = N'xml_index_type' AND OBJECT_ID(N'sys.xml_indexes') = object_id) THEN 1 ELSE 0 END AS SXI_PRESENT)";
        public const string EngineEditionWithLock = "SELECT SERVERPROPERTY('EngineEdition'), SERVERPROPERTY('productversion'), SERVERPROPERTY ('productlevel'), SERVERPROPERTY ('edition'), SERVERPROPERTY ('MachineName'), (SELECT CASE WHEN EXISTS (SELECT TOP 1 1 from [sys].[all_columns] WHERE name = N'xml_index_type' AND OBJECT_ID(N'sys.xml_indexes') = object_id) THEN 1 ELSE 0 END AS SXI_PRESENT)";

        public const string CheckDatabaseReadonly = @"EXEC sp_dboption '{0}', 'read only'";

        public const string GetDatabaseFilePathAndName = @"
DECLARE @filepath		nvarchar(260),
		@rc 			int

EXEC master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE',N'Software\Microsoft\MSSQLServer\MSSQLServer',N'DefaultData', @filepath output, 'no_output' 

IF ((@filepath IS NOT NULL) AND (CHARINDEX(N'\', @filepath, len(@filepath)) = 0))
    SELECT @filepath = @filepath + N'\'

IF (@filepath IS NULL)
    SELECT	@filepath = [sdf].[physical_name]
	FROM	[master].[sys].[database_files] AS [sdf]
	WHERE	[file_id] = 1

SELECT @filepath AS FilePath
";

        public const string GetDatabaseLogPathAndName = @"
DECLARE @filepath		nvarchar(260),
		@rc 			int

EXEC master.dbo.xp_instance_regread N'HKEY_LOCAL_MACHINE',N'Software\Microsoft\MSSQLServer\MSSQLServer',N'DefaultLog', @filepath output, 'no_output' 

IF ((@filepath IS NOT NULL) AND (CHARINDEX(N'\', @filepath, len(@filepath)) = 0))
    SELECT @filepath = @filepath + N'\'

IF (@filepath IS NULL)
    SELECT	@filepath = [ldf].[physical_name]
	FROM	[master].[sys].[database_files] AS [ldf]
	WHERE	[file_id] = 2

SELECT @filepath AS FilePath
";

    public const string GetOsVersion = @"SELECT OSVersion = RIGHT(@@version, LEN(@@version)- 3 -charindex (' on ', LOWER(@@version)))";
    // TODO: #847 remove the Throw condition from endpoints query when removing the GetClusterEndpoints_CTP code.
    // This is needed for 1 monthly release, to support back compatibility with CTP 3.2
    public const string GetClusterEndpoints = @"IF OBJECT_ID (N'sys.dm_cluster_endpoints') IS NOT NULL
SELECT [name], [description], [endpoint], [protocol_desc] FROM .[sys].[dm_cluster_endpoints];
ELSE THROW 50000, 'Use older DMV', 1";
    public const string GetClusterEndpoints_CTP = @"IF OBJECT_ID (N'master.dbo.cluster_endpoint_info') IS NOT NULL
SELECT [service_name], [ip_address], [port] FROM [master].[dbo].[cluster_endpoint_info];";
    }
}
