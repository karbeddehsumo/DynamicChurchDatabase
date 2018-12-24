using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFStoryRepository : IStoryRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private story record;
        private IEnumerable<story> list;

        private List<story> myRecords = new List<story>();

        public EFStoryRepository()
        {
            myRecords = context.stories.ToList(); 
        }

        public void AddRecord(story Record)
        {
            myRecords.Add(record);
        }

        public Dictionary<int, string> GetStoryList(int ministryID)
        {
            Dictionary<int, string> StoryList;
            StoryList = context.stories.Where(e => e.MinistryID == ministryID).Take(20)
            //.OrderBy(e => (string)e.Header)
            .OrderByDescending(e => e.StoryDate)
            .ToDictionary(e => (int)e.StoryID, e => (string)e.Header);

            return (StoryList);
        }

        public Dictionary<int, string> GetStoryStatusList(int ministryID)
        {
            Dictionary<int, string> StoryList;
            StoryList = context.stories.Where(e => e.MinistryID == ministryID && e.Status == "Active").Take(20)
                //.OrderBy(e => (string)e.Header)
            .OrderByDescending(e => e.StoryDate)
            .ToDictionary(e => (int)e.StoryID, e => (string)e.Header);

            return (StoryList);
        }

        public story GetStoryByID(int storyID)
        {
            record = myRecords.FirstOrDefault(e => e.StoryID == storyID);
            return (record);
        }

        public IEnumerable<story> GetStoryByMinistry(int ministryID)
        {
            list = myRecords.Where(e => e.MinistryID == ministryID).OrderByDescending(e => e.StoryDate);
            return (list);
        }

        public IEnumerable<story> GetStoryByMinistryStatus(int ministryID, string Status)
        {
            list = myRecords.Where(e => e.MinistryID == ministryID && e.Status == Status).OrderByDescending(e => e.StoryDate);
            return (list);
        }

        public IEnumerable<story> GetStoryByType(int ministryID, int storyTypeID)
        {
            list = myRecords.Where(e => e.MinistryID == ministryID && e.StoryTypeID == storyTypeID).OrderByDescending(e => e.StoryDate);
            return (list);
        }

        public IEnumerable<story> GetStoryByMinistryDateRange(int ministryID, DateTime BeginDate, DateTime EndDate)
        {
            list = myRecords.Where(e => e.MinistryID == ministryID && e.StoryDate >= BeginDate.Date && e.StoryDate <= EndDate.Date);
            return (list);
        }

        public IEnumerable<story> GetStoryByDateRange(DateTime BeginDate, DateTime EndDate)
        {
            list = myRecords.Where(e => e.StoryDate >= BeginDate.Date && e.StoryDate <= EndDate.Date);
            return (list);
        }

        public IEnumerable<story> GetStory()
        {
            list = myRecords.Where(e => e.Status == "Active").OrderByDescending(e => e.StoryDate).Take(20);
            return list;
        }

        public void DeleteRecord(story record)
        {
            myRecords.Remove(record);
            context.stories.Add(record);
            context.SaveChanges();
        }
    }
}
