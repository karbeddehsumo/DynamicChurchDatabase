using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Security;
using Domain.Abstract;

namespace Domain.Concrete
{
    public class EMembershipProviderRepository : MembershipProvider //: IMembershipProviderRepository 
    {
        private ELocalProviderRepository repository;
        public override int MinRequiredPasswordLength
        {
            get
            {
                return 6;
            }
        }

        public EMembershipProviderRepository()
        {
            this.repository = new ELocalProviderRepository();
        }
/*
        public override bool ValidateUser(string username, string password)
        {
            if (string.IsNullOrEmpty(password.Trim()) || string.IsNullOrEmpty(username.Trim()))
                return false;
            string hash;
            if (username == "gfumbah")
            {
                hash = password.Trim(); //FormsAuthentication.HashPasswordForStoringInConfigFile(password.Trim(), "md5");
               // hash = FormsAuthentication.HashPasswordForStoringInConfigFile(password.Trim(), "md5");
            }
            else
            {
                hash = FormsAuthentication.HashPasswordForStoringInConfigFile(password.Trim(), "md5");
            }
            return this.repository.GetAllUsers().Any(user => (user.UserName == username.Trim()) && (user.Password == hash));
        }

*/
        
        public override bool ValidateUser(string username, string password)
        {
            if (string.IsNullOrEmpty(password.Trim()) || string.IsNullOrEmpty(username.Trim()))
                return false;
            string hash;
            if (username == "gfumbah")
            {
                hash = password.Trim(); //FormsAuthentication.HashPasswordForStoringInConfigFile(password.Trim(), "md5");
                // hash = FormsAuthentication.HashPasswordForStoringInConfigFile(password.Trim(), "md5");
            }
            else
            {
                 hash = FormsAuthentication.HashPasswordForStoringInConfigFile(password.Trim(), "md5");

                if (repository.GetAllUsers().Any(user => (user.UserName.Trim() == username.Trim()) && (user.Password == hash)))
                {
                    System.Web.HttpContext.Current.Session["personID"] = (int)repository.GetAllUsers().Single(user => (user.UserName == username.Trim()) && (user.Password == hash)).PersonID;
                }
            }

            return this.repository.GetAllUsers().Any(user => (user.UserName == username.Trim()) && (user.Password == hash));
        }

        public void CreateUser(string username, string name, string password, string email, string roleName, string passwordQuestion, string passwordAnswer, int memberID, string EmailType)
        {
            repository.CreateUser(username, name, password, email, roleName, passwordQuestion, passwordAnswer, memberID, EmailType);
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {

            if (!ValidateUser(username, oldPassword) || string.IsNullOrEmpty(newPassword.Trim()))
                return false;
            user user = repository.GetUser(username);
            string hash = FormsAuthentication.HashPasswordForStoringInConfigFile(newPassword.Trim(), "md5");
            user.Password = hash;
            int roleMember = repository.GetRole("Member").roleID;
            int roleRegister = repository.GetRole("Register").roleID;
            if (user.RoleID == roleRegister)
            {
                user.RoleID = roleMember;
            }
            repository.Save();
            return true;
        }

        public bool ChangePassword2(string username, string oldPassword, string newPassword)
        {

            user user = repository.GetUser(username);
            string hash = FormsAuthentication.HashPasswordForStoringInConfigFile(newPassword.Trim(), "md5");
            user.Password = hash;
            int roleMember = repository.GetRole("Member").roleID;
            int roleRegister = repository.GetRole("Register").roleID;
            /*
            if (user.RoleID == roleRegister)
            {
                user.RoleID = roleMember;
            }
             */
            user.RoleID = roleRegister; //force re-registration & temp password changes
            user.IsRegister = true; //set registration true
            repository.Save();
            return true;
        }

        public bool UpdateRole(int userID, int roleID)
        {

            if (repository.IsUser(userID))
            {               
                user user = GetUserByID(userID);
                int roleID1 = repository.GetRole(roleID).roleID;
                /*int roleID2 = repository.GetRole("WebMaster").roleID;
                int roleID3 = repository.GetRole("Register").roleID;
                if ((roleID != roleID2) && (user.RoleID != roleID3))
                {
                    user.RoleID = roleID1;
                    repository.Save();
                    return true;
                }
                 * */
                user.RoleID = roleID1;
                repository.Save();
                return true;
            }
            return false;
        }

        public bool UpdateEmail(int userID, string NewEmail)
        {
            if (repository.IsUser(userID))
            {
                user user = GetUserByID(userID);
                if (user.Email != NewEmail)
                {
                    user.Email = NewEmail;
                    repository.Save();
                    return true;
                }
            }
            return false;
        }
        public user GetUser(string username)
        {
           
            return (repository.GetUser(username));
        }

        public bool IsUser(string username)
        {
            return (repository.IsUser(username));
        }

        public bool IsUser(int memberID)
        {
            return (repository.IsUser(memberID));
        }

        public user GetUserByID(int userID)
        {
            return (repository.GetUser(userID));
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

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            repository.ChangePasswordQuestionAndAnswer(username, password, newPasswordQuestion, newPasswordAnswer);
            return true;
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new NotImplementedException();
        }

        public override bool EnablePasswordReset
        {
            get { throw new NotImplementedException(); }
        }

        public override bool EnablePasswordRetrieval
        {
            get { throw new NotImplementedException(); }
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override int GetNumberOfUsersOnline()
        {
            throw new NotImplementedException();
        }


        public override string GetPassword(string userName, string answer)
        {
            throw new NotImplementedException();
        }


        public string GetPasswordAnswer(string userName, string answer)
        {
            user user = repository.GetUserByEmail(userName);
            if (user.PasswordAnswer == answer)
            {
                return (user.Password);
            }
            else
            {
                return (null);
            }
        }

        public string GetPasswordQuestion(string userName)
        {
            user user = repository.GetUserByEmail(userName);
            if (user.PasswordQuestion.Length > 0)
            {
                return (user.Password);
            }
            else
            {
                return (null);
            }
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {

            throw new NotImplementedException();

        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            throw new NotImplementedException();
        }

        public override string GetUserNameByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { throw new NotImplementedException(); }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { throw new NotImplementedException(); }
        }

        public override int PasswordAttemptWindow
        {
            get { throw new NotImplementedException(); }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { throw new NotImplementedException(); }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { throw new NotImplementedException(); }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { throw new NotImplementedException(); }
        }

        public override bool RequiresUniqueEmail
        {
            get { throw new NotImplementedException(); }
        }

        public override string ResetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override bool UnlockUser(string userName)
        {
            throw new NotImplementedException();
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new NotImplementedException();
        }

        public Dictionary<int, string> GetAllRoles()
        {
            Dictionary<int, string> RoleList;
            RoleList = repository.GetAllRoles().Where(e => e.Name != "WebMaster")
            .ToDictionary(e => (int)e.roleID, e => e.Name);

            return (RoleList);
        }

        public List<string> GetAllUsers(int roleID)
        {
            role role = repository.GetRole(roleID);
            List<string> userList = new List<string>();
            IEnumerable<user> users = repository.GetUsersForRole(role);
            userList = repository.GetUsersForRole(role).Select(e => e.Name).ToList();
            return (userList);
                
        }


        public IEnumerable<user> GetAllUsers()
        {
            return (repository.GetAllUsers());

        }
        public void DeleteUser(user user)
        {
            repository.DeleteUser(user);
        }
    }
}
