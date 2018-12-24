using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFMinistryMemberRepository : IMinistryMemberRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private ministrymember record;
        private IEnumerable<ministrymember> list;

        private List<ministrymember> myRecords = new List<ministrymember>();

        public EFMinistryMemberRepository()
        {
            myRecords = context.ministrymembers.ToList(); 
        }

        public bool AddRecord(ministrymember Record)
        {
            myRecords.Add(record);
            return true;
        }


        public Dictionary<int, string> GetMinistryMemberList(int ministryID)
        {
            Dictionary<int, string> MinistryMemberList = new Dictionary<int,string>();
            ministry ministry = context.ministries.SingleOrDefault(e => e.ministryID == ministryID);
            var memberlist = context.members.Where(e => e.Status == "Active");
            if (ministry.CodeDesc == "Church")
            {
                memberlist = context.members.Where(e => e.Status == "Active");
            }
            else if (ministry.CodeDesc == "Babies")
            {
                constant Membercategory = context.constants.SingleOrDefault(e => e.Category == "Member Category" && e.ConstantName == "Babies");
                int lowerAgeYear = Convert.ToInt16(Membercategory.Value2);
                int upperAgeYear = Convert.ToInt16(Membercategory.Value3);
                int year = System.DateTime.Today.Year;
                int lowerYear = year - lowerAgeYear;
                int upperYear = year - upperAgeYear;
                DateTime aDate = System.DateTime.Today;
                DateTime BeginDate = Convert.ToDateTime(aDate.Month + "/" + aDate.Day + "/" + lowerYear.ToString());
                DateTime EndDate = Convert.ToDateTime(aDate.Month + "/" + aDate.Day + "/" + upperYear.ToString());

                memberlist = context.members.Where(e => e.Status == "Active" && e.DOB >= BeginDate && e.DOB <= EndDate);
            }
            else if (ministry.CodeDesc == "Children")
            {
                constant Membercategory = context.constants.SingleOrDefault(e => e.Category == "Member Category" && e.ConstantName == "Children");
                int lowerAgeYear = Convert.ToInt16(Membercategory.Value2);
                int upperAgeYear = Convert.ToInt16(Membercategory.Value3);
                int year = System.DateTime.Today.Year;
                int upperYear = year - lowerAgeYear;
                int lowerYear = year - upperAgeYear;
                DateTime aDate = System.DateTime.Today;
                DateTime BeginDate = Convert.ToDateTime(aDate.Month + "/" + aDate.Day + "/" + lowerYear.ToString());
                DateTime EndDate = Convert.ToDateTime(aDate.Month + "/" + aDate.Day + "/" + upperYear.ToString());

                memberlist = context.members.Where(e => e.Status == "Active" && e.DOB >= BeginDate && e.DOB <= EndDate);
            }
            else if (ministry.CodeDesc == "Youth")
            {
                constant Membercategory = context.constants.SingleOrDefault(e => e.Category == "Member Category" && e.ConstantName == "Youths");
                int lowerAgeYear = Convert.ToInt16(Membercategory.Value2);
                int upperAgeYear = Convert.ToInt16(Membercategory.Value3);
                int year = System.DateTime.Today.Year;
                int upperYear = year - lowerAgeYear;
                int lowerYear = year - upperAgeYear;
                DateTime aDate = System.DateTime.Today;
                DateTime BeginDate = Convert.ToDateTime(aDate.Month + "/" + aDate.Day + "/" + lowerYear.ToString());
                DateTime EndDate = Convert.ToDateTime(aDate.Month + "/" + aDate.Day + "/" + upperYear.ToString());

                memberlist = context.members.Where(e => e.Status == "Active" && e.DOB >= BeginDate && e.DOB <= EndDate);
            }
            else if (ministry.CodeDesc == "Men")
            {
                constant Membercategory = context.constants.SingleOrDefault(e => e.Category == "Member Category" && e.ConstantName == "Men");
                int lowerAgeYear = Convert.ToInt16(Membercategory.Value2);              
                int year = System.DateTime.Today.Year;
                int lowerYear = year - lowerAgeYear;              
                DateTime aDate = System.DateTime.Today;
                DateTime BeginDate = Convert.ToDateTime(aDate.Month + "/" + aDate.Day + "/" + lowerYear.ToString());
                memberlist = context.members.Where(e => e.Status == "Active" && e.Gender == "Male" && e.DOB >= BeginDate);
            }
            else if (ministry.CodeDesc == "Women")
            {
                constant Membercategory = context.constants.SingleOrDefault(e => e.Category == "Member Category" && e.ConstantName == "Women");
                int lowerAgeYear = Convert.ToInt16(Membercategory.Value2);
                int year = System.DateTime.Today.Year;
                int lowerYear = year - lowerAgeYear;
                DateTime aDate = System.DateTime.Today;
                DateTime BeginDate = Convert.ToDateTime(aDate.Month + "/" + aDate.Day + "/" + lowerYear.ToString());
                memberlist = context.members.Where(e => e.Status == "Active" && e.Gender == "Female" && e.DOB >= BeginDate);
            }
            else if (ministry.CodeDesc == "Seniors")
            {
                constant Membercategory = context.constants.SingleOrDefault(e => e.Category == "Member Category" && e.ConstantName == "Seniors");
                int lowerAgeYear = Convert.ToInt16(Membercategory.Value2);
                int year = System.DateTime.Today.Year;
                int lowerYear = year - lowerAgeYear;
                DateTime aDate = System.DateTime.Today;
                DateTime BeginDate = Convert.ToDateTime(aDate.Month + "/" + aDate.Day + "/" + lowerYear.ToString());
                memberlist = context.members.Where(e => e.Status == "Active" && e.DOB >= BeginDate);
            }
            else if (ministry.CodeDesc == "Marriage")
            {
                var spouse1Group = from g in context.members.Where(e => e.Status == "Active")
                                 join c in context.spouses
                                      on g.memberID equals c.spouse1ID
                                 select g;

                var spouse2Group = from g in context.members.Where(e => e.Status == "Active")
                                   join c in context.spouses
                                        on g.memberID equals c.spouse2ID
                                   select g;

                memberlist = spouse1Group.Concat(spouse2Group);
            }
            else if (ministry.CodeDesc == "Singles")
            {
                ministryID = context.ministries.SingleOrDefault(e => e.CodeDesc == "Singles").ministryID;
                memberlist = from g in context.members.Where(e => e.Status == "Active")
                             join c in context.ministrymembers.Where(e => e.ministryID == ministryID)
                                  on g.memberID equals c.memberID
                                 //where c.ministryID == ministryID
                                 select g;

            }
            else
            {
                memberlist = from g in context.members.Where(e => e.Status == "Active")
                             join c in context.ministrymembers.Where(e => e.ministryID == ministryID)
                                      on g.memberID equals c.memberID
                                 //where c.ministryID == ministryID
                                 select g;
            }
            if (memberlist.Count() == 0)
            {
                return (MinistryMemberList);
            }

            MinistryMemberList = memberlist
            .OrderBy(e => (string)e.FirstName)
            .ToDictionary(e => (int)e.memberID, e => (string)string.Format("{0} {1}",e.FirstName, e.LastName));

            return (MinistryMemberList);
        }

        public IEnumerable<member> GetMinistryRoster(int ministryID)
        {
            var memberlist = from g in context.members.Where(e => e.Status == "Active")
                             join c in context.ministrymembers.Where(e => e.ministryID == ministryID)
                              on g.memberID equals c.memberID
                             //where c.ministryID == ministryID
                             select g;

            return (memberlist.ToList());
        }


        public IEnumerable<ministrymember> GetMemintryMemberByMinistry(int ministryID)
        {
            list = myRecords.Where(e => e.ministryID == ministryID);
            return (list);
        }

        public IEnumerable<ministrymember> GetMinistryMemberByMember(int memberID)
        {
            list = myRecords.Where(e => e.memberID == memberID);
            return (list);
        }

        public IEnumerable<ministrymember> GetMinistryMemberByOfficer(int ministryID)
        {
            list = myRecords.Where(e => e.ministryID == ministryID && e.OfficeTitle != null);
            return (list.OrderBy(e => e.SortOrder));
        }

        public IEnumerable<ministrymember> GetMinistryMemberByNonOfficer(int ministryID)
        {
            list = myRecords.Where(e => e.ministryID == ministryID && e.OfficeTitle == null);
            return (list.OrderBy(e => e.SortOrder));
        }

        public void DeleteRecord(ministrymember record)
        {
            using (churchdatabaseEntities context = new churchdatabaseEntities())
            {
                // myRecords.Remove(record);
                ministrymember aRecord = context.ministrymembers.FirstOrDefault(e => e.MinistryMemberID == record.MinistryMemberID);
                context.ministrymembers.Remove(aRecord);
                context.SaveChanges();
            }
        }

        public ministrymember GetMinistryMemberByID(int ministryID, int memberID)
        {
            record = context.ministrymembers.FirstOrDefault(e => e.ministryID == ministryID && e.memberID == memberID);
            return record;
        }


    }
}
