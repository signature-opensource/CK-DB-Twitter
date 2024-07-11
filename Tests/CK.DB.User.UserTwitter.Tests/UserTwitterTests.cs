using System;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using CK.Core;
using CK.DB.Actor;
using CK.SqlServer;
using NUnit.Framework;
using System.Linq;
using CK.DB.Auth;
using System.Collections.Generic;
using FluentAssertions;
using static CK.Testing.MonitorTestHelper;

namespace CK.DB.User.UserTwitter.Tests
{
    [TestFixture]
    public class UserTwitterTests
    {
        [Test]
        public void create_Twitter_user_and_check_read_info_object_method()
        {
            var u = SharedEngine.Map.StObjs.Obtain<UserTwitterTable>();
            var user = SharedEngine.Map.StObjs.Obtain<UserTable>();
            var infoFactory = SharedEngine.Map.StObjs.Obtain<IPocoFactory<IUserTwitterInfo>>();
            Throw.DebugAssert( user != null && u != null && infoFactory != null );
            using( var ctx = new SqlStandardCallContext() )
            {
                var userName = Guid.NewGuid().ToString();
                int userId = user.CreateUser( ctx, 1, userName );
                var twitterAccountId = Guid.NewGuid().ToString( "N" );

                var info = infoFactory.Create();
                info.TwitterAccountId = twitterAccountId;
                var created = u.CreateOrUpdateTwitterUser( ctx, 1, userId, info );
                created.OperationResult.Should().Be( UCResult.Created );
                var info2 = u.FindKnownUserInfo( ctx, twitterAccountId );
                Throw.DebugAssert( info2 != null );
                info2.UserId.Should().Be( userId );
                info2.Info.TwitterAccountId.Should().Be( twitterAccountId );

                u.FindKnownUserInfo( ctx, Guid.NewGuid().ToString() ).Should().BeNull();
                user.DestroyUser( ctx, 1, userId );
                u.FindKnownUserInfo( ctx, twitterAccountId ).Should().BeNull();
            }
        }

        [Test]
        public async Task create_Twitter_user_and_check_read_info_object_method_Async()
        {
            var u = SharedEngine.Map.StObjs.Obtain<UserTwitterTable>();
            var user = SharedEngine.Map.StObjs.Obtain<UserTable>();
            var infoFactory = SharedEngine.Map.StObjs.Obtain<IPocoFactory<IUserTwitterInfo>>();
            Throw.DebugAssert( user != null && u != null && infoFactory != null );
            using( var ctx = new SqlStandardCallContext() )
            {
                var userName = Guid.NewGuid().ToString();
                int userId = await user.CreateUserAsync( ctx, 1, userName );
                var TwitterAccountId = Guid.NewGuid().ToString( "N" );

                var info = infoFactory.Create();
                info.TwitterAccountId = TwitterAccountId;
                var created = await u.CreateOrUpdateTwitterUserAsync( ctx, 1, userId, info );
                created.OperationResult.Should().Be( UCResult.Created );
                var info2 = await u.FindKnownUserInfoAsync( ctx, TwitterAccountId );
                Throw.DebugAssert( info2 != null );
                info2.UserId.Should().Be( userId );
                info2.Info.TwitterAccountId.Should().Be( TwitterAccountId );

                (await u.FindKnownUserInfoAsync( ctx, Guid.NewGuid().ToString() )).Should().BeNull();
                await user.DestroyUserAsync( ctx, 1, userId );
                (await u.FindKnownUserInfoAsync( ctx, TwitterAccountId )).Should().BeNull();
            }
        }

        [Test]
        public void Twitter_AuthProvider_is_registered()
        {
            Auth.Tests.AuthTests.CheckProviderRegistration( "Twitter" );
        }

        [Test]
        public void vUserAuthProvider_reflects_the_user_Twitter_authentication()
        {
            var u = SharedEngine.Map.StObjs.Obtain<UserTwitterTable>();
            var user = SharedEngine.Map.StObjs.Obtain<UserTable>();
            Throw.DebugAssert( user != null && u != null );

            using( var ctx = new SqlStandardCallContext() )
            {
                string userName = "Twitter auth - " + Guid.NewGuid().ToString();
                var TwitterAccountId = Guid.NewGuid().ToString( "N" );
                var idU = user.CreateUser( ctx, 1, userName );
                u.Database.ExecuteReader( $"select * from CK.vUserAuthProvider where UserId={idU} and Scheme='Twitter'" )!
                    .Rows.Should().BeEmpty();
                var info = u.CreateUserInfo<IUserTwitterInfo>();
                info.TwitterAccountId = TwitterAccountId;
                u.CreateOrUpdateTwitterUser( ctx, 1, idU, info );
                u.Database.ExecuteScalar( $"select count(*) from CK.vUserAuthProvider where UserId={idU} and Scheme='Twitter'" )
                    .Should().Be( 1 );
                u.DestroyTwitterUser( ctx, 1, idU );
                u.Database.ExecuteReader( $"select * from CK.vUserAuthProvider where UserId={idU} and Scheme='Twitter'" )!
                    .Rows.Should().BeEmpty();
            }
        }

        [Test]
        public void standard_generic_tests_for_Twitter_provider()
        {
            var auth = SharedEngine.Map.StObjs.Obtain<Auth.Package>();
            // With IUserTwitterInfo POCO.
            var f = SharedEngine.Map.StObjs.Obtain<IPocoFactory<IUserTwitterInfo>>();
            Throw.DebugAssert( auth != null && f != null );

            CK.DB.Auth.Tests.AuthTests.StandardTestForGenericAuthenticationProvider(
                auth,
                "Twitter",
                payloadForCreateOrUpdate: ( userId, userName ) => f.Create( i => i.TwitterAccountId = "TwitterAccountIdFor:" + userName ),
                payloadForLogin: ( userId, userName ) => f.Create( i => i.TwitterAccountId = "TwitterAccountIdFor:" + userName ),
                payloadForLoginFail: ( userId, userName ) => f.Create( i => i.TwitterAccountId = "NO!" + userName )
                );
            // With a KeyValuePair.
            CK.DB.Auth.Tests.AuthTests.StandardTestForGenericAuthenticationProvider(
                auth,
                "Twitter",
                payloadForCreateOrUpdate: ( userId, userName ) => new[]
                {
                    new KeyValuePair<string,object>( "TwitterAccountId", "IdFor:" + userName)
                },
                payloadForLogin: ( userId, userName ) => new[]
                {
                    new KeyValuePair<string,object>( "TwitterAccountId", "IdFor:" + userName)
                },
                payloadForLoginFail: ( userId, userName ) => new[]
                {
                    new KeyValuePair<string,object>( "TwitterAccountId", ("IdFor:" + userName).ToUpperInvariant())
                }
                );
        }

        [Test]
        public async Task standard_generic_tests_for_Twitter_provider_Async()
        {
            var auth = SharedEngine.Map.StObjs.Obtain<Auth.Package>();
            var f = SharedEngine.Map.StObjs.Obtain<IPocoFactory<IUserTwitterInfo>>();
            Throw.DebugAssert( auth != null && f != null );
            await Auth.Tests.AuthTests.StandardTestForGenericAuthenticationProviderAsync(
                auth,
                "Twitter",
                payloadForCreateOrUpdate: ( userId, userName ) => f.Create( i => i.TwitterAccountId = "TwitterAccountIdFor:" + userName ),
                payloadForLogin: ( userId, userName ) => f.Create( i => i.TwitterAccountId = "TwitterAccountIdFor:" + userName ),
                payloadForLoginFail: ( userId, userName ) => f.Create( i => i.TwitterAccountId = "NO!" + userName )
                );
        }

    }

}

