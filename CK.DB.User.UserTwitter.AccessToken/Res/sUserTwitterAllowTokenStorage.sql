-- SetupConfig: {}
--
create procedure CK.sUserTwitterAllowTokenStorage
    @ActorId int,
    @UserId int,
    @AllowTokenStorage bit /*not null*/
as
begin
    if @ActorId <= 0 throw 50000, 'Security.AnonymousNotAllowed', 1;
    if @UserId = 0 throw 50000, 'Argument.InvalidValue', 1;
    if @AllowTokenStorage is null throw 50000, 'ArgumentNull.AllowTokenStorage', 1;

    --<beginsp>

    declare @CurrentAllowTokenStorage bit;
    select @CurrentAllowTokenStorage = case when AccessToken = '<NoStorage>' then 0 else 1 end
        from CK.tUserTwitter where UserId = @UserId;

    if @CurrentAllowTokenStorage is not null
        and @CurrentAllowTokenStorage <> @AllowTokenStorage 
    begin
        --<PreChange revert />

        update CK.tUserTwitter
            set AccessToken = case when @AllowTokenStorage = 1 then '' else '<NoStorage>' end,
                AccessTokenSecret = case when @AllowTokenStorage = 1 then '' else '<NoStorage>' end
            where UserId = @UserId;

       --<PostChange />
    end

    --<endsp>
end
