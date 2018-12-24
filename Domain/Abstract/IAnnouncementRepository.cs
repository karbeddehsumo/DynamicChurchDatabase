using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IAnnouncementRepository
    {
        void AddRecord(announcement Record);
        Dictionary<int, string> GetAnnouncementList(int ministryID);
        Dictionary<int, string> GetAllAnnouncementList();
        announcement GetAnnouncementByID(int announcementID);
        IEnumerable<announcement> GetAnnouncementByDateRange(DateTime BeginDate, DateTime EndDate, string CallerType = "");
        IEnumerable<announcement> GetAnnouncementByMinistry(int ministryID, DateTime BeginDate, DateTime EndDate, string CallerType = "");
        IEnumerable<announcement> GetAllAnnouncement(DateTime BeginDate, DateTime EndDate);
        IEnumerable<announcement> GetAnnouncementByMonth(int monthID);
        void DeleteRecord(announcement record);

    }
}
