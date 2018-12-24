//------------------------------------------------------------------------------
// <auto-generated>
//    This code was generated from a template.
//
//    Manual changes to this file may cause unexpected behavior in your application.
//    Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Domain
{
    using System;
    using System.Collections.Generic;
    using System.Web;
    
    public partial class member
    {
        public int memberID { get; set; }
        public Nullable<int> familyID { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string LastName { get; set; }
        public string Suffix { get; set; }
        public System.DateTime DOB { get; set; }
        public string Gender { get; set; }
        public Nullable<System.DateTime> MembershipDate { get; set; }
        public string ChurchTitle { get; set; }
        public int ContactTypeID { get; set; }
        public string PhoneNumber { get; set; }
        public Nullable<int> PhoneProviderID { get; set; }
        public string EmailAddress { get; set; }
        public Nullable<int> PictureID { get; set; }
        public string Status { get; set; }
        public string EnteredBy { get; set; }
        public System.DateTime DateEntered { get; set; }

         public string MemberName { get { return LastName + ", " + FirstName; } }

        public string FullName
        {
            get
            {
                if ((MiddleName == null) && (Suffix == null))
                {
                    return FirstName + " " + LastName;
                }
                else if (MiddleName == null)
                {
                    return FirstName + " " + LastName + ", " + Suffix;
                }
                else if (Suffix == null)
                {
                   return FirstName + " " + MiddleName + " " + LastName;
                }
                else
                    return FirstName + " " + MiddleName + " " + LastName + ", " + Suffix;
            }
        }


        public string FullNameLastFirstMiddle
        {
            get
            {
                if ((MiddleName == null) && (Suffix == null))
                {
                    return LastName + ", " + FirstName;
                }
                else if (MiddleName == null)
                {
                    return LastName + ", " + FirstName + ", " + Suffix;
                }
                else if (Suffix == null)
                {
                    return LastName + ", " + FirstName + " " + MiddleName;
                }
                else
                    return LastName + ", " + FirstName + " " + MiddleName + ", " + Suffix;
            }
        }


        public string FullNameTitle
        {
            get
            {
               if ((MiddleName == null) && (Suffix == null))
                {
                    return ChurchTitle + " " + FirstName + " " + LastName;
                }
                else if (MiddleName == null)
                {
                    return ChurchTitle + " " + FirstName + " " + LastName + ", " + Suffix;
                }
                else if (Suffix == null)
                {
                    return ChurchTitle + " " + FirstName + " " + MiddleName + " " + LastName;
                }
                else
                   return ChurchTitle + " " + FirstName + " " + MiddleName + " " + LastName + ", " + Suffix;
            }
        }
        public string FirstNameTitle { get { return ChurchTitle + " " + FirstName; } }

        public virtual ICollection<attendance> Attendances { get; set; }
        public virtual ICollection<contribution> Contributions { get; set; }
        public virtual ICollection<ministrymember> MinistryMembers { get; set; }
        public virtual ICollection<pledge> Pledges { get; set; }

        public virtual spouse spouse { get; set; }
        public virtual family family { get; set; }

        public virtual string PhoneList { get; set; }
        public virtual string EmailList { get; set; }

        public string Address { get; set; }
        public string Address2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string LastNameTitle { get { return ChurchTitle + " " + LastName; } }


        public string ContactType { get; set; }
 	
         public IEnumerable<HttpPostedFileBase> files { get; set; }
    }
}