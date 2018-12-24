using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IStoryRepository
    {
        void AddRecord(story Record);
        Dictionary<int, string> GetStoryList(int ministryID);
        Dictionary<int, string> GetStoryStatusList(int ministryID);
        story GetStoryByID(int storyID);
        IEnumerable<story> GetStoryByMinistry(int ministryID);
        IEnumerable<story> GetStoryByMinistryStatus(int ministryID, string Status);
        IEnumerable<story> GetStoryByType(int ministryID, int storyTypeID);
        IEnumerable<story> GetStoryByMinistryDateRange(int ministryID, DateTime BeginDate, DateTime EndDate);
        IEnumerable<story> GetStoryByDateRange(DateTime BeginDate, DateTime EndDate);
        IEnumerable<story> GetStory();
        void DeleteRecord(story record);
    }
}
