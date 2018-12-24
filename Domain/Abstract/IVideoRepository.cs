using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IVideoRepository
    {
        void AddRecord(video Record);
        Dictionary<int, string> GetVideoList(int ministryID);
        video GetVideoByID(int videoID);
        IEnumerable<video> GetVideoByMinistry(int ministryID, DateTime bDate, DateTime eDate);
        IEnumerable<video> GetMinistryVideo(int ministryID);
        IEnumerable<video> GetVideoByStatus(string status);
        IEnumerable<video> GetVideoByMinistryStatus(int ministryID, string status);
        IEnumerable<video> GetVideoByDateRange(DateTime bDate, DateTime eDate);
        void DeleteRecord(video record);
    }
}
