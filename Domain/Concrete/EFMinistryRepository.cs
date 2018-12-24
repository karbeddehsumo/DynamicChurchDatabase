using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFMinistryRepository : IMinistryRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private ministry record;
        private IEnumerable<ministry> list;

        private List<ministry> myRecords = new List<ministry>();
        private List<ministry> myGroupRecords = new List<ministry>();

         public EFMinistryRepository()
        {
            myRecords = context.ministries.Where(e => e.IsGroupMinistry == false).ToList();
            myGroupRecords = context.ministries.Where(e => e.IsGroupMinistry == true).ToList(); 
        }

         public void AddRecord(ministry Record)
        {
            myRecords.Add(record);
        }


        public Dictionary<int, string> GetMinistryList()
        {
            Dictionary<int, string> MinistryList;
            MinistryList = context.ministries.Where(e => e.IsGroupMinistry == false)
            .OrderBy(e => (string)e.MinistryName)
            .ToDictionary(e => (int)e.ministryID, e => (string)e.MinistryName);

            return (MinistryList);
        }

        public Dictionary<int, string> GetPublicMinistryList()
        {
            Dictionary<int, string> MinistryList;
            MinistryList = context.ministries.Where(e => (e.IsGroupMinistry == false)&&(e.IsPublic == true) )
            .OrderBy(e => (string)e.MinistryName)
            .ToDictionary(e => (int)e.ministryID, e => (string)e.MinistryName);

            return (MinistryList);
        }

        public Dictionary<int, string> GetGroupMinistryList()
        {
             Dictionary<int, string> MinistryList;
             MinistryList = myGroupRecords
            .OrderBy(e => (string)e.MinistryName)
            .ToDictionary(e => (int)e.ministryID, e => (string)e.MinistryName);

            return (MinistryList);
        }

        public ministry GetMinistryByID(int ministryID)
        {
            using (churchdatabaseEntities context = new churchdatabaseEntities())
            {
                record = myRecords.FirstOrDefault(e => e.ministryID == ministryID);
            }
            return (record);
        }

        public ministry GetParentMinistryByID(int ministryID)
        {
            using (churchdatabaseEntities context = new churchdatabaseEntities())
            {
                record = myGroupRecords.FirstOrDefault(e => e.ministryID == ministryID);
            }
            return (record);
        }

        public IEnumerable<ministry> GetMinistryByStatus(string status)
        {
            list = myRecords.Where(e => e.Status == status);
            return (list.OrderBy(e => e.MinistryName));
        }

        public IEnumerable<ministry> GetMyMinistries(int memberID)
        {
            var ministryList = from g in context.ministrymembers.Where(e => e.memberID == memberID)
                               join c in context.ministries   //.Where(e => e.DefaultMemberType != "All Members")
                                  on g.ministryID equals c.ministryID
                                 select c;
            return (ministryList.ToList());
        }

        public IEnumerable<ministry> GetMyDefaultMinistries(int memberID)
        {
            List<ministry> ministryList = new List<ministry>();
            var member = context.members.FirstOrDefault(e => e.memberID == memberID);
            double age =  DateTime.Now.Date.Subtract(member.DOB).TotalDays/365;

            
            IEnumerable<ministry> allMinistries = myRecords.Where(e => e.DefaultMemberType != null && e.Status == "Active");
            using (churchdatabaseEntities context2 = new churchdatabaseEntities())
            {
               
                foreach (ministry m in allMinistries)
                {
                    if (m.DefaultMemberType == "Children")
                    {
                        constant c = context2.constants.SingleOrDefault(e => e.Category == "Member Category" && e.ConstantName == "Children");
                        int age1 = Convert.ToInt16(c.Value2);
                        int age2 = Convert.ToInt16(c.Value3);
                        if ((age >= age1) && (age <= age2))
                        {
                            ministryList.Add(m);
                        }
                    }
                    else if (m.DefaultMemberType == "Youths")
                    {
                        constant c = context2.constants.SingleOrDefault(e => e.Category == "Member Category" && e.ConstantName == "Youths");
                        int age1 = Convert.ToInt16(c.Value2);
                        int age2 = Convert.ToInt16(c.Value3);
                        if ((age >= age1) && (age <= age2))
                        {
                            ministryList.Add(m);
                        }
                    }
                    else if (m.DefaultMemberType == "Young Adults")
                    {
                        constant c = context2.constants.SingleOrDefault(e => e.Category == "Member Category" && e.ConstantName == "Young Adults");
                        int age1 = Convert.ToInt16(c.Value2);
                        int age2 = Convert.ToInt16(c.Value3);
                        if ((age >= age1) && (age <= age2))
                        {
                            ministryList.Add(m);
                        }
                    }
                    else if (m.DefaultMemberType == "Adults")
                    {
                        constant c = context2.constants.SingleOrDefault(e => e.Category == "Member Category" && e.ConstantName == "Adults");
                        int age1 = Convert.ToInt16(c.Value2);
                        int age2 = Convert.ToInt16(c.Value3);
                        if ((age >= age1) && (age <= age2))
                        {
                            ministryList.Add(m);
                        }
                    }
                    else if (m.DefaultMemberType == "Men")
                    {
                        constant c = context2.constants.SingleOrDefault(e => e.Category == "Member Category" && e.ConstantName == "Men");
                        int age1 = Convert.ToInt16(c.Value2);
                        if ((age >= age1) && (member.Gender == "Male"))
                        {
                            ministryList.Add(m);
                        }
                    }
                    else if (m.DefaultMemberType == "Women")
                    {
                        constant c = context2.constants.SingleOrDefault(e => e.Category == "Member Category" && e.ConstantName == "Women");
                        int age1 = Convert.ToInt16(c.Value2);
                        if ((age >= age1) && (member.Gender == "Female"))
                        {
                            ministryList.Add(m);
                        }
                    }
                    else if (m.DefaultMemberType == "Seniors")
                    {
                        constant c = context.constants.SingleOrDefault(e => e.Category == "Member Category" && e.ConstantName == "Seniors");
                        int age1 = Convert.ToInt16(c.Value2);
                        int age2 = Convert.ToInt16(c.Value3);
                        if ((age >= age1) && (age >= age2))
                        {
                            ministryList.Add(m);
                        }
                    }
                    else if (m.DefaultMemberType == "All Members")
                    {
                        constant c = context2.constants.SingleOrDefault(e => e.Category == "Member Category" && e.ConstantName == "All Members");
                        int age1 = Convert.ToInt16(c.Value2);
                        int age2 = Convert.ToInt16(c.Value3);
                        if ((age >= age1) && (age <= age2))
                        {
                            ministryList.Add(m);
                        }
                    }
                    else if (m.DefaultMemberType == "Married Couples")
                    {
                        using (churchdatabaseEntities context3 = new churchdatabaseEntities())
                        {
                            var spouse = context3.spouses.FirstOrDefault(e => e.spouse1ID == memberID);
                            if (spouse != null)
                            {
                                ministryList.Add(m);
                            }

                            spouse = context3.spouses.FirstOrDefault(e => e.spouse2ID == memberID);
                            if (spouse != null)
                            {
                                ministryList.Add(m);
                            }
                        }
                    }
                    else
                    {

                    }

                }

            }
            return (ministryList.ToList());
        }

        public ministry GetMinistryByCodeDesc(string CodeDesc)
        {
            record = myRecords.FirstOrDefault(e => e.CodeDesc == CodeDesc);
            return (record);
        }

        public ministry GetMainChurchMinistry()
        {
            record = myRecords.FirstOrDefault(e => e.CodeDesc == "Church");
            return (record);
        }

        public IEnumerable<ministry> GetPublicMinistry()
        {
            list = myRecords.Where(e => e.Status == "Active" & e.IsPublic == true);
            return (list);
        }

        public IEnumerable<ministry> GetDisplayMinistry()
        {
            list = myRecords.Where(e => e.Status == "Active" & e.DisplayBanner == true);
            return (list);
        }

        public void DeleteRecord(ministry record)
        {
            myRecords.Remove(record);
            context.ministries.Remove(record);
            context.SaveChanges();
        }
    }
}
