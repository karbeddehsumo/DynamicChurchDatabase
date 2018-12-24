using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFAnnouncementRepository : IAnnouncementRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private announcement record;
        private IEnumerable<announcement> list;

        private List<announcement> myRecords = new List<announcement>();

         public EFAnnouncementRepository()
        {
            myRecords = context.announcements.ToList(); 
        }

        public void AddRecord(announcement Record)
        {
            myRecords.Add(record);
        }


        public Dictionary<int,string> GetAnnouncementList(int ministryID)
        {
            Dictionary<int,string> AnnouncementList;
            AnnouncementList = myRecords
            .Where(e => e.ministryID == ministryID)
            .OrderBy(e => (string)e.Title)
            .ToDictionary(e => (int)e.announcementID, e => (string)e.Title);

            return (AnnouncementList);
        }
        public Dictionary<int,string> GetAllAnnouncementList()
        {
            Dictionary<int,string> AnnouncementList;
            AnnouncementList = myRecords
            .OrderBy(e => (string)e.Title)
            .ToDictionary(e => (int)e.announcementID,e => (string)e.Title);

            return (AnnouncementList);
        }
        public announcement GetAnnouncementByID(int announcementID)
        {
            record = myRecords.FirstOrDefault(e => e.announcementID == announcementID);
            return (record);
        }


        public IEnumerable<announcement> GetAnnouncementByDateRange(DateTime BeginDate, DateTime EndDate, string CallerType = "")
        {
            if ((CallerType == "Officer") || (CallerType == "Ministry"))
            {
                var a = myRecords.Where(e => e.BeginDate >= BeginDate.Date && e.BeginDate <= EndDate.Date);
                var b = myRecords.Where(e => e.EndDate >= BeginDate.Date && e.EndDate <= EndDate.Date);
                list = a.Concat(b);
            }
            else
            {
                var a = myRecords.Where(e => e.BeginDate >= BeginDate && e.BeginDate <= EndDate && e.Status == "Active");
                var b = myRecords.Where(e => e.EndDate >= BeginDate && e.EndDate <= EndDate && e.Status == "Active");
                list = a.Concat(b);
            }
            

            return (list.Distinct());
        }

        public IEnumerable<announcement> GetAnnouncementByMinistry(int ministryID, DateTime BeginDate, DateTime EndDate, string CallerType = "")
        {
            if ((CallerType == "Officer") || (CallerType == "Ministry"))
            {
                var a = myRecords.Where(e => e.ministryID == ministryID && e.BeginDate >= BeginDate.Date && e.BeginDate <= EndDate.Date);
                var b = myRecords.Where(e => e.ministryID == ministryID && e.EndDate >= BeginDate.Date && e.EndDate <= EndDate.Date);
                list = a.Concat(b);
            }
            else
            {
                var a = myRecords.Where(e => e.ministryID == ministryID && e.BeginDate >= BeginDate.Date && e.BeginDate <= EndDate.Date && e.Status == "Active");
                var b = myRecords.Where(e => e.ministryID == ministryID && e.EndDate >= BeginDate.Date && e.EndDate <= EndDate.Date && e.Status == "Active");
                list = a.Concat(b);
            }

            return (list.Distinct());
        }

        public IEnumerable<announcement> GetAllAnnouncement(DateTime BeginDate, DateTime EndDate)
        {
            var a = myRecords.Where(e => e.BeginDate >= BeginDate.Date && e.BeginDate <= EndDate.Date && e.Status == "Active");
            var b = myRecords.Where(e => e.EndDate >= BeginDate.Date && e.EndDate <= EndDate.Date && e.Status == "Active");
            list = a.Concat(b);

            return (list.Distinct());
        }

        public IEnumerable<announcement> GetAnnouncementByMonth(int monthID)
        {
            list = myRecords.Where(e => e.BeginDate.Month == monthID || e.EndDate.Month == monthID);
            return (list);
        }


        public void DeleteRecord(announcement record)
        {
            myRecords.Remove(record);
            context.announcements.Remove(record);
            context.SaveChanges();
        }

    }
}
