using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IRoleProviderRepository //: RoleProvider
    {
        string[] GetRolesForUser(string userName);
        bool IsUserInRole(string userName, string roleName);
        int GetRole(string roleName);
    }
}
