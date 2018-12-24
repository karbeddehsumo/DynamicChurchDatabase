using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IMembershipProviderRepository //: MembershipProvider
    {
        bool ValidateUser(string username, string password);
        void CreateUser(string username, string fullName, string password, string email, string roleName);
        bool ChangePassword(string username, string oldPassword, string newPassword);
        bool ChangePassword2(string username, string oldPassword, string newPassword);
        void DeleteUser(user user);
    }
}
