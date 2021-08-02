using CK.Core;
using System;

namespace CK.DB.User.UserTwitter.AuthScope
{
    /// <summary>
    /// Extends <see cref="UserTwitter.IUserTwitterInfo"/> with ScopeSet identifier.
    /// </summary>
    public interface IUserTwitterInfo : UserTwitter.IUserTwitterInfo
    {
        /// <summary>
        /// Gets or sets the scope set identifier.
        /// Note that the ScopeSetId is intrinsic: a new ScopeSetId is acquired 
        /// and set only when a new UserTwitter is created (by copy from 
        /// the default one - the ScopeSet of the UserTwitter 0).
        /// </summary>
        int ScopeSetId { get; set; }
    }
}
