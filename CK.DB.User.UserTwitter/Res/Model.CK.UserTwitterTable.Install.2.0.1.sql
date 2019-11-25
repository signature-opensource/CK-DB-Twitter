--[beginscript]

create table CK.tUserTwitter
(
	UserId int not null,
	-- The Twitter account identifier is the key to identify a Twitter user.
	TwitterAccountId varchar(36) collate Latin1_General_100_BIN2 not null,
	LastLoginTime datetime2(2) not null,
	constraint PK_CK_UserTwitter primary key (UserId),
	constraint FK_CK_UserTwitter_UserId foreign key (UserId) references CK.tUser(UserId),
	constraint UK_CK_UserTwitter_TwitterAccountId unique( TwitterAccountId )
);

insert into CK.tUserTwitter( UserId, TwitterAccountId, LastLoginTime ) 
	values( 0, '', sysutcdatetime() );

--[endscript]
