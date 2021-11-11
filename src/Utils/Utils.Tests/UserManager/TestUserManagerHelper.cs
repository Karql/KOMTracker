using Microsoft.AspNetCore.Identity;
using MockQueryable.NSubstitute;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utils.Tests.UserManager;

public static class TestUserManagerHelper
{
    public static UserManager<TUser> CreateTestUserManager<TUser>()
        where TUser : class
    {
        var userStore = Substitute.For<IUserStore<TUser>>();
        return Substitute.For<UserManager<TUser>>(userStore, null, null, null, null, null, null, null, null);
    }

    public static void MockUsers<TUser>(this UserManager<TUser> userManager, IEnumerable<TUser> users)
        where TUser : class
    {
        var qUsers = users
            .AsQueryable()
            .BuildMock();

        userManager.Users.Returns(qUsers);
    }
}
