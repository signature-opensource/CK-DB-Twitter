using CK.Core;
using CK.DB.Actor;
using CK.SqlServer;
using CK.Testing;
using Shouldly;
using NUnit.Framework;
using System;
using System.Threading;
using static CK.Testing.MonitorTestHelper;

namespace CK.DB.User.UserTwitter.AccessToken.Tests;

[TestFixture]
public class UserTwitterAccessTokenTests
{
    [Test]
    public void AccessToken_and_Secret_track_the_LastWriteTime()
    {
        var twitter = SharedEngine.Map.StObjs.Obtain<UserTwitterTable>();
        var user = SharedEngine.Map.StObjs.Obtain<UserTable>();
        Throw.DebugAssert( user != null && twitter != null );
        using( var ctx = new SqlStandardCallContext( TestHelper.Monitor ) )
        {
            string userName = "Test Twitter AccessToken - " + Guid.NewGuid().ToString();
            var twitterAccountId = Guid.NewGuid().ToString( "N" );
            var idU = user.CreateUser( ctx, 1, userName );

            var info = twitter.CreateUserInfo<IUserTwitterInfo>();
            info.TwitterAccountId = twitterAccountId;
            twitter.CreateOrUpdateTwitterUser( ctx, 1, idU, info );
            string rawSelect = $"select AccessToken+'|'+cast(LastWriteTime as varchar) from CK.tUserTwitter where UserId={idU}";
            twitter.Database.ExecuteScalar( rawSelect )
                .ShouldBe( "|0001-01-01 00:00:00.00" );

            info.AccessToken = "an access token";
            info.AccessTokenSecret = "...and its secret...";
            twitter.CreateOrUpdateTwitterUser( ctx, 1, idU, info );
            rawSelect = $"select AccessToken + AccessTokenSecret from CK.tUserTwitter where UserId={idU}";
            twitter.Database.ExecuteScalar( rawSelect )
                .ShouldBe( "an access token...and its secret..." );

            info = (IUserTwitterInfo?)twitter.FindKnownUserInfo( ctx, twitterAccountId )?.Info;
            Throw.DebugAssert( info != null );
            info.LastWriteTime.ShouldBeGreaterThan( DateTime.UtcNow.AddMonths( -1 ) );
            info.AccessToken.ShouldBe( "an access token" );
            info.AccessTokenSecret.ShouldBe( "...and its secret..." );

            var lastUpdate = info.LastWriteTime;
            Thread.Sleep( 500 );
            info.AccessToken = null;
            info.AccessTokenSecret = null;
            twitter.CreateOrUpdateTwitterUser( ctx, 1, idU, info );
            info = (IUserTwitterInfo?)twitter.FindKnownUserInfo( ctx, twitterAccountId )?.Info;
            Throw.DebugAssert( info != null );
            info.LastWriteTime.ShouldBe( lastUpdate );
            info.AccessToken.ShouldBe( "an access token" );
            info.AccessTokenSecret.ShouldBe( "...and its secret..." );
        }
    }

    [Test]
    public void AllowTokenStorage_disables_updates_of_tokens()
    {
        var twitter = SharedEngine.Map.StObjs.Obtain<UserTwitterTable>();
        var tokens = SharedEngine.Map.StObjs.Obtain<Package>();
        var user = SharedEngine.Map.StObjs.Obtain<UserTable>();
        Throw.DebugAssert( twitter != null && tokens != null && user != null );
        using( var ctx = new SqlStandardCallContext( TestHelper.Monitor ) )
        {
            string userName = "Test Twitter <NoStorage> " + Guid.NewGuid().ToString();
            var twitterAccountId = Guid.NewGuid().ToString( "N" );
            var idU = user.CreateUser( ctx, 1, userName );

            var info = twitter.CreateUserInfo<IUserTwitterInfo>();
            info.TwitterAccountId = twitterAccountId;
            info.AccessToken = info.AccessTokenSecret = "token info";
            twitter.CreateOrUpdateTwitterUser( ctx, 1, idU, info );
            twitter.Database.ExecuteScalar( $"select AccessToken + '|' + AccessTokenSecret from CK.tUserTwitter where UserId={idU}" )
                .ShouldBe( "token info|token info" );

            tokens.SetAllowTokenStorage( ctx, 1, idU, true );
            twitter.Database.ExecuteScalar( $"select AccessToken + '|' + AccessTokenSecret from CK.tUserTwitter where UserId={idU}" )
                .ShouldBe( "token info|token info", "Setting to true preserves the existing tokens (if any)." );

            tokens.SetAllowTokenStorage( ctx, 1, idU, false );
            twitter.Database.ExecuteScalar( $"select AccessToken + '|' + AccessTokenSecret from CK.tUserTwitter where UserId={idU}" )
                .ShouldBe( UserTwitterInfoExtensions.NoAccessTokenMarker + '|' + UserTwitterInfoExtensions.NoAccessTokenMarker,
                              "Setting to false sets both markers." );

            info.AccessToken = "Will be ignored.";
            info.AccessTokenSecret = "Will be ignored either.";
            twitter.CreateOrUpdateTwitterUser( ctx, 1, idU, info );
            twitter.Database.ExecuteScalar( $"select AccessToken + '|' + AccessTokenSecret from CK.tUserTwitter where UserId={idU}" )
                .ShouldBe( UserTwitterInfoExtensions.NoAccessTokenMarker + '|' + UserTwitterInfoExtensions.NoAccessTokenMarker,
                              "Markers are here to stay..." );

            tokens.SetAllowTokenStorage( ctx, 1, idU, true );
            twitter.Database.ExecuteScalar( $"select AccessToken + '|' + AccessTokenSecret from CK.tUserTwitter where UserId={idU}" )
                .ShouldBe( "|", "Setting to true clears the markers." );

            info.AccessToken = "Will NOT be ignored.";
            info.AccessTokenSecret = "HERE.";
            twitter.CreateOrUpdateTwitterUser( ctx, 1, idU, info );
            twitter.Database.ExecuteScalar( $"select AccessToken + '|' + AccessTokenSecret from CK.tUserTwitter where UserId={idU}" )
                .ShouldBe( "Will NOT be ignored.|HERE.", "Back to normal." );

        }
    }

}
