--[beginscript]

-- Note that since 2017, this can be up to 50 characters.
alter table CK.tUserTwitter add
	ScreenName nvarchar( 50 ) collate Latin1_General_100_CI_AS not null constraint DF_TEMP1 default( N'' );

alter table CK.tUserTwitter drop constraint DF_TEMP1;

--[endscript]
