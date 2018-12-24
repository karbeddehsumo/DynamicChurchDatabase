using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;
using System.Web;

namespace Domain.Concrete
{
    public class EFMemberRepository : IMemberRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private member record;
        private IEnumerable<member> list;

        private List<member> myRecords = new List<member>();

         public EFMemberRepository()
        {
            myRecords = context.members.ToList(); 
        }

         public void AddRecord(member Record)
        {
            myRecords.Add(record);
        }


        public Dictionary<int, string> GetMemberList()
        {
            Dictionary<int, string> MemberList;
            MemberList = context.members.Where(e => e.Status == "Active")
            .OrderBy(e => (string)e.LastName.Trim() + ", " + e.FirstName.Trim() + " " + e.MiddleName.Trim())
            //.ToDictionary(e => (int)e.memberID, e => (string)e.LastName.Trim() + ", " + e.FirstName.Trim() + " " + e.MiddleName.Trim());
            .ToDictionary(e => (int)e.memberID, e => (string)e.FullNameLastFirstMiddle);
            return (MemberList);
        }

        public member GetMemberByID(int memberID)
        {
            using (churchdatabaseEntities context = new churchdatabaseEntities())
            {
                record = myRecords.FirstOrDefault(e => e.memberID == memberID);
            }
            return (record);
        }

        public IEnumerable<member> GetMemberByDOB(DateTime DOB)
        {
            DateTime DOB2 = DOB.AddDays(32);

                list = myRecords
                    .Where(e => e.DOB.Month == DOB.Month && e.DOB.Day >= DOB.Day)
                    .Where(e => e.Status == "Active");

                var list2 = myRecords
                 .Where(e => e.DOB.Month == DOB2.Month && e.DOB.Day <= DOB2.Day)
                    .Where(e => e.Status == "Active");

                var list3 = list.Concat(list2);
                return (list3.OrderBy(e => (string.Format("{0}/{1}", e.DOB.Month, e.DOB.Date))));
              
        }

        public IEnumerable<member> GetMemberByDOBRange(DateTime bDate, DateTime eDate)
        {
            list = myRecords.Where(e => e.DOB.Month >= bDate.Month && e.DOB.Day >= bDate.Day && e.DOB.Month <= eDate.Month && e.DOB.Day <= eDate.Day );
            return (list);
        }

        public IEnumerable<member> GetMemberByGender(string Gender)
        {
            list = myRecords.Where(e => e.Gender == Gender);
            return (list);
        }

        public IEnumerable<member> GetMemberByMembershipDate(DateTime aDate)
        {
            list = myRecords.Where(e => e.MembershipDate == aDate.Date);
            return (list);
        }

        public IEnumerable<member> GetMemberByMembershipDateRange(DateTime bDate, DateTime eDate)
        {
            list = myRecords.Where(e => e.MembershipDate >= bDate.Date && e.MembershipDate <= eDate.Date);
            return (list);
        }
        public IEnumerable<member> GetMemberByChurchTitle(string churchTitle)
        {
            list = myRecords.Where(e => e.ChurchTitle == churchTitle);
            return (list);
        }
        public IEnumerable<member> GetMemberByFamily(int familyID)
        {
            list = myRecords.Where(e => e.familyID == familyID);
            return (list);
        }

        public IEnumerable<member> GetMemberByStatus(string status)
        {
            list = myRecords.Where(e => e.Status == status);
            return (list);
        }

        public IEnumerable<member> GetMemberByAge(int BeginningYear, int EndingYear)
        {
            int thisyear = DateTime.Now.Year;
            int beginnningYear = thisyear - BeginningYear;
            int endingYear = thisyear -  EndingYear;
            DateTime todayDate = DateTime.Now;
            DateTime bDate =Convert.ToDateTime(string.Format("{0}/{1}/{2}", todayDate.Month, todayDate.Date.Day, beginnningYear));
            DateTime eDate = Convert.ToDateTime(string.Format("{0}/{1}/{2}", todayDate.Month, todayDate.Date.Day, endingYear));

            list = myRecords.Where(e => e.DOB >= eDate.Date && e.DOB <= bDate.Date && e.Status == "Active");
            return (list);
        }

        public IEnumerable<member> GetWeddingAnniversary(DateTime TodayDate)
        {
            DateTime TodayDate2 = TodayDate.AddDays(32);

            var list = from s in context.spouses
                .Where(e => e.AnniversaryDate.Month == TodayDate.Month && e.AnniversaryDate.Day >= TodayDate.Day)
                join m in context.members.Where(e => e.Status == "Active")
                 on s.spouse1ID equals m.memberID
                 select s;


            var list2 = from s in context.spouses
                .Where(e => e.AnniversaryDate.Month == TodayDate2.Month && e.AnniversaryDate.Day <= TodayDate2.Day)
                        join m in context.members.Where(e => e.Status == "Active")
                        on s.spouse1ID equals m.memberID
                       select s;

            var list3 = list.Concat(list2);
            var list4 = from s in list3.OrderByDescending(e => e.AnniversaryDate)
                        join m in context.members.Where(e => e.Status == "Active")
                        on s.spouse1ID equals m.memberID
                        select m;
            return (list4);
      

        }

        public IEnumerable<member> GetMemberListCategory(string MemberGroup)
        {
            var memberlist = myRecords.Where(e => e.Status == "Active");
            if (MemberGroup == "Babies")
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

                memberlist = myRecords.Where(e => e.Status == "Active" && e.DOB >= BeginDate && e.DOB <= EndDate);
            }
            else if (MemberGroup == "Children")
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

                memberlist = myRecords.Where(e => e.Status == "Active" && e.DOB >= BeginDate && e.DOB <= EndDate);
            }
            else if (MemberGroup == "Youth")
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

                memberlist = myRecords.Where(e => e.Status == "Active" && e.DOB >= BeginDate && e.DOB <= EndDate);
            }
            else if (MemberGroup == "Young Adults")
            {
                constant Membercategory = context.constants.SingleOrDefault(e => e.Category == "Member Category" && e.ConstantName == "Young Adults");
                int lowerAgeYear = Convert.ToInt16(Membercategory.Value2);
                int upperAgeYear = Convert.ToInt16(Membercategory.Value3);
                int year = System.DateTime.Today.Year;
                int upperYear = year - lowerAgeYear;
                int lowerYear = year - upperAgeYear;
                DateTime aDate = System.DateTime.Today;
                DateTime BeginDate = Convert.ToDateTime(aDate.Month + "/" + aDate.Day + "/" + lowerYear.ToString());
                DateTime EndDate = Convert.ToDateTime(aDate.Month + "/" + aDate.Day + "/" + upperYear.ToString());

                memberlist = myRecords.Where(e => e.Status == "Active" && e.DOB >= BeginDate && e.DOB <= EndDate);
            }
            else if (MemberGroup == "Adults")
            {
                constant Membercategory = context.constants.SingleOrDefault(e => e.Category == "Member Category" && e.ConstantName == "Adults");
                int lowerAgeYear = Convert.ToInt16(Membercategory.Value2);
                int upperAgeYear = Convert.ToInt16(Membercategory.Value3);
                int year = System.DateTime.Today.Year;
                int upperYear = year - lowerAgeYear;
                int lowerYear = year - upperAgeYear;
                DateTime aDate = System.DateTime.Today;
                DateTime BeginDate = Convert.ToDateTime(aDate.Month + "/" + aDate.Day + "/" + lowerYear.ToString());
                DateTime EndDate = Convert.ToDateTime(aDate.Month + "/" + aDate.Day + "/" + upperYear.ToString());

                memberlist = myRecords.Where(e => e.Status == "Active" && e.DOB >= BeginDate && e.DOB <= EndDate);
            }
            else if (MemberGroup == "Men")
            {
                constant Membercategory = context.constants.SingleOrDefault(e => e.Category == "Member Category" && e.ConstantName == "Men");
                int lowerAgeYear = Convert.ToInt16(Membercategory.Value2);
                int year = System.DateTime.Today.Year;
                int lowerYear = year - lowerAgeYear;
                DateTime aDate = System.DateTime.Today;
                DateTime BeginDate = Convert.ToDateTime(aDate.Month + "/" + aDate.Day + "/" + lowerYear.ToString());
                memberlist = myRecords.Where(e => e.Status == "Active" && e.Gender == "Male" && e.DOB <= BeginDate);
            }
            else if (MemberGroup == "Women")
            {
                constant Membercategory = context.constants.SingleOrDefault(e => e.Category == "Member Category" && e.ConstantName == "Women");
                int lowerAgeYear = Convert.ToInt16(Membercategory.Value2);
                int year = System.DateTime.Today.Year;
                int lowerYear = year - lowerAgeYear;
                DateTime aDate = System.DateTime.Today;
                DateTime BeginDate = Convert.ToDateTime(aDate.Month + "/" + aDate.Day + "/" + lowerYear.ToString());
                memberlist = myRecords.Where(e => e.Status == "Active" && e.Gender == "Female" && e.DOB <= BeginDate);
            }
            else if (MemberGroup == "Seniors")
            {
                constant Membercategory = context.constants.SingleOrDefault(e => e.Category == "Member Category" && e.ConstantName == "Seniors");
                int lowerAgeYear = Convert.ToInt16(Membercategory.Value2);
                int year = System.DateTime.Today.Year;
                int lowerYear = year - lowerAgeYear;
                DateTime aDate = System.DateTime.Today;
                DateTime BeginDate = Convert.ToDateTime(aDate.Month + "/" + aDate.Day + "/" + lowerYear.ToString());
                memberlist = myRecords.Where(e => e.Status == "Active" && e.DOB <= BeginDate);
            }
            else if (MemberGroup == "Marriage")
            {
                var spouse = from g in context.members.Where(e => e.Status == "Active")
                                   join c in context.spouses
                                        on g.memberID equals c.spouse1ID
                                   select g;

                memberlist = spouse;
            }
            else if (MemberGroup == "Singles")
            {
                //get all adults
                constant Membercategory = context.constants.SingleOrDefault(e => e.Category == "Member Category" && e.ConstantName == "Adults");
                int lowerAgeYear = Convert.ToInt16(Membercategory.Value2);
                int upperAgeYear = Convert.ToInt16(Membercategory.Value3);
                int year = System.DateTime.Today.Year;
                int upperYear = year - lowerAgeYear;
                int lowerYear = year - upperAgeYear;
                DateTime aDate = System.DateTime.Today;
                DateTime BeginDate = Convert.ToDateTime(aDate.Month + "/" + aDate.Day + "/" + lowerYear.ToString());
                DateTime EndDate = Convert.ToDateTime(aDate.Month + "/" + aDate.Day + "/" + upperYear.ToString());

                var All_memberlist = myRecords.Where(e => e.Status == "Active" && e.DOB >= BeginDate && e.DOB <= EndDate);

                //get all married couples
                var spouse = from g in context.members.Where(e => e.Status == "Active")
                             join c in context.spouses
                                  on g.memberID equals c.spouse1ID
                             select g;

                var Married_memberlist1 = spouse;

                var spouse2 = from g in context.members.Where(e => e.Status == "Active")
                             join c in context.spouses
                                  on g.memberID equals c.spouse2ID
                             select g;

                var Married_memberlist2 = spouse2;
                //remove married couples from adult list
                var excludeIds = new HashSet<int>(Married_memberlist1.Select(x => x.memberID));
                var excludeIds2 = new HashSet<int>(Married_memberlist2.Select(x => x.memberID));

                var MemberPartialList = All_memberlist.Where(x => !excludeIds.Contains(x.memberID)).ToList();
                var MemberPartialList2 = MemberPartialList.Where(x => !excludeIds2.Contains(x.memberID)).ToList();

                memberlist = MemberPartialList2.Distinct();
            }
            else
            {
                memberlist = myRecords.Where(e => e.Status == "Active");
            }
            return (memberlist);
        }

        public IEnumerable<member> GetChildParent(int childID)
        {
            using (churchdatabaseEntities context = new churchdatabaseEntities())
            {
                var mylist = from p in context.members.Where(e => e.Status == "Active")
                              join c in context.childparents.Where(e => e.ChildID == childID)
                                   on p.memberID equals c.ParentID
                              select p;
               if (mylist.Any() == false)
               {
                   return null;
               }
               else
               {
                   return mylist;
               }
            }
        }

        public Dictionary<int, string> GetMwmberByFamilyList(int memberID)
        {
            record = myRecords.FirstOrDefault(e => e.memberID == memberID);
            Dictionary<int, string> MemberList;
            MemberList = myRecords.Where(e => e.familyID == record.familyID)
            .OrderBy(e => (string)e.FirstName)
            .ToDictionary(e => (int)e.memberID, e => (string)e.FullNameTitle);

            return (MemberList);
        }

        public Dictionary<int, string> GetAllMembers()
        {
            Dictionary<int, string> MemberList;
            MemberList = context.members
            .OrderBy(e => (string)e.LastName.Trim() + ", " + e.FirstName.Trim() + " " + e.MiddleName.Trim())
            .ToDictionary(e => (int)e.memberID, e => (string)e.LastName.Trim() + ", " + e.FirstName.Trim() + " " + e.MiddleName.Trim());

            return (MemberList);
        }

        public void DeleteRecord(member record)
        {
            myRecords.Remove(record);
            context.members.Remove(record);
            context.SaveChanges();
        }

    }
}
