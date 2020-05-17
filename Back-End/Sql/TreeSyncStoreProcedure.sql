use OneMoreTreeContext
go

create or alter procedure dbo.sync_trees
@json nvarchar(max)
as

declare @fromVersion int = json_value(@json, '$.fromVersion')

set xact_abort on
set transaction isolation level snapshot;  
begin tran

    declare @curVer int = change_tracking_current_version();
    declare @minVer int = change_tracking_min_valid_version(object_id('dbo.Trees'));

    if (@fromVersion < @minVer) begin
        set @fromVersion = 0
    end

    if (@fromVersion = 0)
    begin
        select
            @curVer as 'Metadata.Sync.Version',
            'Full' as 'Metadata.Sync.Type',
            [Data] = json_query((
                select
                    Trees.Id,
                    Trees.Photo,
                    Trees.[Description],
                    Trees.Latitude,
                    Trees.Longitude,
                    Trees.City,
                    Trees.[State],
                    Trees.[Date],
                    Users.Id as 'User.Id',
                    Users.[Name] as 'User.Name',
                    Users.Picture as 'User.Picture'
                from
                    dbo.Trees as Trees
                join
                    dbo.Users as Users on Trees.UserId = Users.Id
                for json path
            ))
        for
            json path, without_array_wrapper
    end else begin
        select
            @curVer as 'Metadata.Sync.Version',
            'Diff' as 'Metadata.Sync.Type',
            [Data] = json_query((
                select 
                    [ChangeTable].SYS_CHANGE_OPERATION as '$operation',
                    [ChangeTable].[Id],
                    Trees.Photo,
                    Trees.[Description],
                    Trees.Latitude,
                    Trees.Longitude,
                    Trees.City,
                    Trees.[State],
                    Trees.[Date],
                    Users.Id as 'User.Id',
                    Users.[Name] as 'User.Name',
                    Users.Picture as 'User.Picture'
                from 
                    dbo.Trees as Trees
                join
                    dbo.Users as Users on Trees.UserId = Users.Id
                right outer join 
                    changetable(changes dbo.Trees, @fromVersion) as [ChangeTable] on [ChangeTable].[Id] = Trees.[id]
                for 
                    json path
            ))
        for
            json path, without_array_wrapper
    end

commit tran