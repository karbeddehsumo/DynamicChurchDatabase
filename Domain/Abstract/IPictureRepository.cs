using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IPictureRepository
    {
        void AddRecord(picture Record);
        picture GetPictureByID(int pictureID);
        picture GetStaffPicture(int pictureID = 0);
        IEnumerable<picture> GetPictureByMinistry(int ministryID);
        IEnumerable<picture> GetPictureByDate(DateTime aDate);
        IEnumerable<picture> GetPictureByDateRange(DateTime bDate, DateTime eDate);
        IEnumerable<picture> GetPictureByStatus(string status,int ministryID, DateTime bDate, DateTime eDate);
        picture GetLetterHeadImage();
        IEnumerable<picture> GetPictureByGroup(int ministryID, int groupID);
        picture GetPictureByDescription(string Description);
        IEnumerable<picture> GetMinistryPictureGroup(int ministryID);
        IEnumerable<picture> GetMinistryRecentPictures(int ministryID);
        picture GetDefaultPersonPicture(int memberID);
        picture GetMinistryDefaultBanner();
        picture GetMemberPicture(int memberID);
        bool IsDefaultPeoplePicture();
        picture GetDefaultPeoplePicture();
        void DeleteRecord(picture record);
    }
}
