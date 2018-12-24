using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Threading.Tasks;
using System.Web.Security;
using Domain.Abstract;

namespace Domain.Concrete
{
    public class ERoleProviderRepository : RoleProvider
    {

        private ELocalProviderRepository repository;
        private string UserName;

        public ERoleProviderRepository()
        {
            this.repository = new ELocalProviderRepository();

        }

        public ERoleProviderRepository(string userName)
        {
            UserName = userName;
            this.repository = new ELocalProviderRepository();
        }

        public override string[] GetRolesForUser(string userName)
        {
            if (userName.Trim() == "")
                userName = UserName;
            role role = this.repository.GetRoleForUser(userName);
            if (!this.repository.RoleExists(role))
                return new string[] { string.Empty };
            return new string[] { role.Name };
        }



        public override bool IsUserInRole(string userName, string roleName)
        {
            user user = repository.GetUser(userName);
            role role = repository.GetRole(roleName);
            if (!repository.UserExists(user))
                return false;
            if (!repository.RoleExists(role))
                return false;

            if (!(user.RoleID == role.roleID))
                return false;
            return true;
        }

        public role GetRole(int roleID)
        {
            return (repository.GetRole(roleID));
        }

        public role GetRole(string roleName)
        {
            return (repository.GetRole(roleName));
        }


        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override string ApplicationName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            throw new NotImplementedException();
        }



        
        public override string[] GetAllRoles()
        {
            throw new NotImplementedException();
        }
         

        public override string[] GetUsersInRole(string roleName)
        {
            throw new NotImplementedException();
        }



        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            throw new NotImplementedException();
        }
    }
}
