using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFVideoRepository : IVideoRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private video record;
        private IEnumerable<video> list;

        private List<video> myRecords = new List<video>();

        public EFVideoRepository()
        {
            myRecords = context.videos.ToList(); 
        }

        public void AddRecord(video Record)
        {
            myRecords.Add(record);
        }

        public Dictionary<int, string> GetVideoList(int ministryID)
        {
            Dictionary<int, string> VideoList;
            VideoList = context.videos.Where(e => e.ministryID == ministryID)
            .Where(e => e.ministryID == ministryID)
            .OrderBy(e => (string)e.VideoTitle)
            .ToDictionary(e => (int)e.videoID, e => (string)e.VideoTitle);

            return (VideoList);
        }

        public video GetVideoByID(int videoID)
        {
            record = myRecords.FirstOrDefault(e => e.videoID == videoID);
            return (record);
        }

        public IEnumerable<video> GetVideoByMinistry(int ministryID, DateTime bDate, DateTime eDate)
        {
            list = myRecords.Where(e => e.ministryID == ministryID && e.VideoDate >= bDate && e.VideoDate <= eDate && e.Status == "Active");
            return (list);
        }

        public IEnumerable<video> GetMinistryVideo(int ministryID)
        {
            list = myRecords.Where(e => e.ministryID == ministryID);
            return (list);
        }

        public IEnumerable<video> GetVideoByStatus(string status)
        {
            list = myRecords.Where(e => e.Status == status);
            return (list);
        }

        public IEnumerable<video> GetVideoByMinistryStatus(int ministryID, string status)
        {
            list = myRecords.Where(e => e.ministryID == ministryID && e.Status == status);
            return (list);
        }

        public IEnumerable<video> GetVideoByDateRange(DateTime bDate, DateTime eDate)
        {
            list = myRecords.Where(e => e.VideoDate >= bDate.Date && e.VideoDate <= eDate.Date);
            return (list);
        }

        public void DeleteRecord(video record)
        {
            myRecords.Remove(record);
            context.videos.Remove(record);
            context.SaveChanges();
        }
    }
}
