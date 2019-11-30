using CK.Core;
using CK.DB.Actor;
using CK.SqlServer;
using FluentAssertions;
using NUnit.Framework;
using System;
using static CK.Testing.DBSetupTestHelper;

namespace CK.DB.User.UserTwitter.ScreenName.Tests
{
    [TestFixture]
    public class UserTwitterScreenNameTests
    {
        [Test]
        public void screenName_is_handled_like_any_other_Authentication_Provider_property()
        {
            var u = TestHelper.StObjMap.StObjs.Obtain<UserTwitterTable>();
            var user = TestHelper.StObjMap.StObjs.Obtain<UserTable>();
            using( var ctx = new SqlStandardCallContext() )
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

                info = (ScreenName.IUserTwitterInfo)u.FindKnownUserInfo( ctx, twitterAccountId ).Info;
                info.ScreenName.Should().Be( "Albert" );
                info.TwitterAccountId.Should().Be( twitterAccountId );
            }
        }

    }
}
