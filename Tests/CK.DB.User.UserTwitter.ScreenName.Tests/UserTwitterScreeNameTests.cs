using CK.Core;
using CK.DB.Actor;
using CK.SqlServer;
using CK.Testing;
using FluentAssertions;
using NUnit.Framework;
using System;
using static CK.Testing.MonitorTestHelper;

namespace CK.DB.User.UserTwitter.ScreenName.Tests
{
    [TestFixture]
    public class UserTwitterScreenNameTests
    {
        [Test]
        public void screenName_is_handled_like_any_other_Authentication_Provider_property()
        {
            var u = SharedEngine.Map.StObjs.Obtain<UserTwitterTable>();
            var user = SharedEngine.Map.StObjs.Obtain<UserTable>();
            Throw.DebugAssert( user != null && u != null );
            using( var ctx = new SqlStandardCallContext( TestHelper.Monitor ) )
            {
                var twitterAccountId = Guid.NewGuid().ToString( "N" );
                string userName = "Twitter user " + twitterAccountId;
                var idU = user.CreateUser( ctx, 1, userName );
                var info = u.CreateUserInfo<IUserTwitterInfo>();
                info.TwitterAccountId = twitterAccountId;
                info.ScreenName = "Albert";
                u.CreateOrUpdateTwitterUser( ctx, 1, idU, info );
                string rawSelect = $"select ScreenName from CK.tUserTwitter where UserId={idU}";
                u.Database.ExecuteScalar( rawSelect ).Should().Be( "Albert" );
                info.ScreenName = null;
                u.CreateOrUpdateTwitterUser( ctx, 1, idU, info );
                u.Database.ExecuteScalar( rawSelect ).Should().Be( "Albert" );

                info = (IUserTwitterInfo?)u.FindKnownUserInfo( ctx, twitterAccountId )?.Info;
                Throw.DebugAssert( info != null );
                info.ScreenName.Should().Be( "Albert" );
                info.TwitterAccountId.Should().Be( twitterAccountId );
            }
        }

    }
}
