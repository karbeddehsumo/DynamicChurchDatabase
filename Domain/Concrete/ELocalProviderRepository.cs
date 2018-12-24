using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using System.Web;

namespace Domain.Concrete
{
    public class ELocalProviderRepository
    {
        #region Variables
        private churchdatabase_userEntities entities = new churchdatabase_userEntities();

        private IEnumerable<user> Users;
        private IEnumerable<role> Roles;


        private const string MissingRole = "Role does not exist";
        private const string MissingUser = "User does not exist";
        private const string TooManyUser = "User already exists";
        private const string TooManyRole = "Role already exists";
        private const string AssignedRole = "Cannot delete a role with assigned users";
        #endregion

        #region Properties
        public int NumberOfUsers
        {
            get
            {
                return this.entities.users.Count();
            }
        }
        public int NumberOfRoles
        {
            get
            {
                return this.entities.roles.Count();
            }
        }
        #endregion
        #region Constructors
        public ELocalProviderRepository()
        {
            this.entities = new churchdatabase_userEntities();
        }
        #endregion
        #region Query Methods
        public IEnumerable<user> GetAllUsers()
        {

            return (entities.users.OrderBy(e => e.UserName));
        }

        public user GetUser(int id)
        {
            var user = entities.users.SingleOrDefault(e => e.PersonID == id);
            user.role = entities.roles.FirstOrDefault(e => e.roleID == user.RoleID);

            return (user);
        }

        public bool IsUser(string userName)
        {
            if (entities.users.Count(e => e.UserName == userName) > 0)
                return true;
            else
                return false;
        }

        public bool IsUser(int memberID)
        {
            if (entities.users.Where(e => e.PersonID == memberID).Count() > 0)
                return true;
            else
                return false;
        }
        public user GetUser(string userName)
        {
             return (entities.users.Single(e => e.UserName == userName));
        }

        public user GetUserByEmail(string userEmail)
        {
            return (entities.users.Single(e => e.Email == userEmail));
        }

        public IEnumerable<user> GetUsersForRole(string roleName)
        {
            return GetUsersForRole(GetRole(roleName));
        }

        public user GetUsersForRole(int RoleID)
        {
            return (entities.users.Single(e => e.RoleID == RoleID));
        }

        public IEnumerable<user> GetUsersForRole(role role)
        {
            if (!RoleExists(role))
                throw new ArgumentException(MissingRole);
            return (entities.users.Where(e => e.RoleID == role.roleID));
        }

        public IEnumerable<role> GetAllRoles()
        {
            return (entities.roles.OrderBy(e => e.Name));
        }

        public role GetRole(int id)
        {
            return (entities.roles.Single(e => e.roleID == id));
        }

        public role GetRole(string name)
        {

            return entities.roles.SingleOrDefault(role => role.Name == name);
        }

        public role GetRoleForUser(string userName)
        {
            return (entities.users.Single(e => e.UserName == userName).role);
        }

        public role GetRoleForUser(int userID)
        {
            return (entities.users.Single(e => e.userID == userID).role);
        }

        public role GetRoleForUser(user user)
        {
            if (!UserExists(user))
                throw new ArgumentException(MissingUser);
            return (entities.users.Single(e => e.UserName == user.UserName).role);
        }

        #endregion
        #region Insert/Delete
        private void AddUser(user user)
        {
            if (UserExists(user))
                throw new ArgumentException(TooManyUser);
            entities.users.Add(user);
        }

        public void CreateUser(string username, string name, string password, string email, string roleName, string passwordQuestion, string passwordAnswer, int MemberID, string EmailType)
        {
            role role = GetRole(roleName);
            if (string.IsNullOrEmpty(username.Trim()))
                throw new ArgumentException("The user name provided is invalid. Please check the value and try again.");
            if (string.IsNullOrEmpty(name.Trim()))
                throw new ArgumentException("The name provided is invalid. Please check the value and try again.");
            if (string.IsNullOrEmpty(password.Trim()))
                throw new ArgumentException("The password provided is invalid. Please enter a valid password value.");
            if (string.IsNullOrEmpty(email.Trim()))
                throw new ArgumentException("The e-mail address provided is invalid. Please check the value and try again.");
            if (!RoleExists(role))
                throw new ArgumentException("The role selected for this user does not exist! Contact an administrator!");
            if (NumberOfUsers > 0)
            {
                if (entities.users.Any(user => user.UserName == username))
                    throw new ArgumentException("Username already exists. Please enter a different user name.");
            }

            if (string.IsNullOrEmpty(passwordQuestion.Trim()))
                throw new ArgumentException("The Password Question provided is invalid. Please check the value and try again.");
            if (string.IsNullOrEmpty(passwordAnswer.Trim()))
                throw new ArgumentException("The Password Answer provided is invalid. Please check the value and try again.");

            user newUser = new user()
            {
                RoleID = role.roleID,
                Name = name,
                UserName = username,
                Password = FormsAuthentication.HashPasswordForStoringInConfigFile(password.Trim(), "md5"),
                Email = email,
                PasswordQuestion = passwordQuestion,
                PasswordAnswer = passwordAnswer,
                PersonID = MemberID,
                EmailType = EmailType
            };
            /*
                        Role newRole = new Role()
                        {
                            RoleID = roleID,
                            Name = name
                        };
            */
            try
            {
                AddUser(newUser);
            }
            catch (ArgumentException ae)
            {
                throw ae;
            }
            catch (Exception e)
            {
                throw new ArgumentException("The authentication provider returned an error. Please verify your entry and try again. " +
                "If the problem persists, please contact your system administrator.");
            }
            // Immediately persist the user data
            Save();
        }

        public void ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            user user = GetUser(username);
            user.PasswordQuestion = newPasswordQuestion;
            user.PasswordAnswer = newPasswordAnswer;
            Save();
        }

        public void DeleteUser(user user)
        {
            if (!UserExists(user))
                throw new ArgumentException(MissingUser);
            // entities.aspnet_Users.DeleteObject(user);
            entities.users.Remove(user);
            entities.SaveChanges();

        }
        public void DeleteUser(string userName)
        {
            DeleteUser(GetUser(userName));

            user user = entities.users.Single(e => e.UserName == userName);
            DeleteUser(user);
        }

        public void AddRole(role role)
        {
            if (RoleExists(role))
                throw new ArgumentException(TooManyRole);
            //entities.Roles.AddObject(role);
            entities.roles.Add(role);
        }
        public void AddRole(string roleName)
        {
            role role = new role()
            {
                Name = roleName
            };
            AddRole(role);
        }
        public void DeleteRole(role role)
        {
            if (!RoleExists(role))
                throw new ArgumentException(MissingRole);
            if (GetUsersForRole(role).Count() > 0)
                throw new ArgumentException(AssignedRole);
            entities.roles.Remove(role);
            entities.SaveChanges();
        }
        public void DeleteRole(string roleName)
        {
            DeleteRole(GetRole(roleName));
        }
        #endregion


        #region Persistence
        public void Save()
        {
            entities.SaveChanges();
        }
        #endregion
        #region Helper Methods
        public bool UserExists(user user)
        {
            if (user == null)
                return false;
            return (entities.users.SingleOrDefault(u => u.userID == user.userID || u.UserName == user.UserName) != null);
        }
        public bool RoleExists(role role)
        {
            if (role == null)
                return false;
            return (entities.roles.SingleOrDefault(r => r.roleID == role.roleID || r.Name == role.Name) != null);
        }
        #endregion
    }


}