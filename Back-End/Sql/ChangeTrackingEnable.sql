alter database OneMoreTreeContext
set change_tracking = on
(change_retention = 8 days, auto_cleanup = on);

alter database OneMoreTreeContext
set allow_snapshot_isolation on;

alter database OneMoreTreeContext
set read_committed_snapshot on;

use OneMoreTreeContext
alter table dbo.Trees
enable change_tracking;