--[beginscript]

alter table CK.tUserTwitter add
    -- There is no official statement about the tokens' max length. Take no risk and use max.
	AccessToken varchar(max) collate Latin1_General_100_CI_AS not null constraint DF_TEMP1 default(N''),
	AccessTokenSecret varchar(max) collate Latin1_General_100_CI_AS not null constraint DF_TEMP2 default(N''),
	LastWriteTime datetime2(2) not null constraint DF_TEMP3 default('0001-01-01');

alter table CK.tUserTwitter drop constraint DF_TEMP1;
alter table CK.tUserTwitter drop constraint DF_TEMP2;
alter table CK.tUserTwitter drop constraint DF_TEMP3;

--[endscript]
