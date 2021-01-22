﻿IF NOT EXISTS -- if temporal table doesn't exist
(
	SELECT 1
	FROM   sys.tables
	WHERE  object_id = OBJECT_ID('{TABLE_WITH_SCHEMA}', 'u') AND temporal_type = 2
)
AND
EXISTS  -- if the base table exists
(
	SELECT 1
	FROM   sys.tables
	WHERE  object_id = OBJECT_ID('{TABLE_WITH_SCHEMA}', 'u')
)
BEGIN
    ALTER TABLE {TABLE_WITH_SCHEMA}
    ADD
            SysStartTime datetime2(0) GENERATED ALWAYS AS ROW START HIDDEN
                CONSTRAINT {SYS_TIME_CONSTRAINT}_DF_SysStart DEFAULT DATEADD(SECOND, -10, SYSUTCDATETIME())
            , SysEndTime datetime2(0) GENERATED ALWAYS AS ROW END HIDDEN
                CONSTRAINT {SYS_TIME_CONSTRAINT}_DF_SysEnd DEFAULT CONVERT(datetime2 (0), '9999-12-31 23:59:59'),
            PERIOD FOR SYSTEM_TIME (SysStartTime, SysEndTime);

    ALTER TABLE {TABLE_WITH_SCHEMA}
    SET (SYSTEM_VERSIONING = ON (HISTORY_TABLE = {HISTORY_TABLE_NAME}));
END