using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Domain.Abstract
{
    public interface ILocalProviderRepository
    {
        IEnumerable<user> GetAllUsers();
        user GetUser(Guid id);
        user GetUser(string userName);
        IEnumerable<user> GetUsersForRole(string roleName);
        IEnumerable<user> GetUsersForRole(Guid id);
        IEnumerable<user> GetUsersForRole(role role);
        IEnumerable<role> GetAllRoles();
        role GetRole(Guid id);
        role GetRole(string name);
        role GetRoleForUser(string userName);
        role GetRoleForUser(Guid id);
        role GetRoleForUser(user user);
        void AddUser(user user);
        void CreateUser(string username, string name, string password, string email, string roleName);
        void DeleteUser(user user);
        void DeleteUser(string userName);
        void AddRole(role role);
        void AddRole(string roleName);
        void DeleteRole(role role);
        void DeleteRole(string roleName);
        void Save();
        bool UserExists(user user);
        bool RoleExists(role role);


    }
}
