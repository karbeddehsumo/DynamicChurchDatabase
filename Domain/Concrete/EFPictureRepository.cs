using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFPictureRepository :IPictureRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private picture record;
        private IEnumerable<picture> list;

        private List<picture> myRecords = new List<picture>();

        public EFPictureRepository()
        {
            //myRecords = context.pictures.ToList(); 
        }

        public void AddRecord(picture Record)
        {
            myRecords.Add(record);
        }


        public picture GetPictureByID(int pictureID)
        {
            //record = myRecords.FirstOrDefault(e => e.pictureID == pictureID);
            record = context.pictures.FirstOrDefault(e => e.pictureID == pictureID);
            return(record);
        }

        public picture GetStaffPicture(int pictureID = 0)
        {
            //record = myRecords.FirstOrDefault(e => e.pictureID == pictureID);
            record = context.pictures.FirstOrDefault(e => e.pictureID == pictureID);
            if (record == null)
            {
                record = myRecords.FirstOrDefault(e => e.PictureType == "Default People");
            }
            return (record);
        }

        public IEnumerable<picture> GetPictureByMinistry(int ministryID)
        {
            //list = myRecords.Where(e => e.ministryID == ministryID);
            list = context.pictures.Where(e => e.ministryID == ministryID && e.PictureType == null);
            return (list);
        }

        public IEnumerable<picture> GetPictureByDate(DateTime aDate)
        {
           // list = myRecords.Where(e => e.PictureDate == aDate);
            list = context.pictures.Where(e => e.PictureDate == aDate.Date && e.PictureType == null);
            return (list);
        }

        public IEnumerable<picture> GetPictureByDateRange(DateTime bDate, DateTime eDate)
        {
            //list = myRecords.Where(e => e.PictureDate >= bDate & e.PictureDate <= eDate);
            list = context.pictures.Where(e => e.PictureDate >= bDate.Date & e.PictureDate <= eDate.Date && e.PictureType == null);
            return (list);
        }

        public IEnumerable<picture> GetPictureByStatus(string status,int ministryID, DateTime bDate, DateTime eDate)
        {
            if (ministryID == 0)
            {
                //list = myRecords.Where(e => e.Status == status && e.PictureDate >= bDate & e.PictureDate <= eDate);
                list = context.pictures.Where(e => e.Status == status && e.PictureDate >= bDate.Date & e.PictureDate <= eDate.Date && e.PictureType == null);
            }
            else
            {
                //list = myRecords.Where(e => e.Status == status && e.ministryID == ministryID && e.PictureDate >= bDate & e.PictureDate <= eDate);
                list = context.pictures.Where(e => e.Status == status && e.ministryID == ministryID && e.PictureDate >= bDate.Date & e.PictureDate <= eDate.Date && e.PictureType == null);
            }
            return (list);
        }

        public picture GetLetterHeadImage()
        {
            //record = myRecords.FirstOrDefault(e => e.Description == "LetterHeadImage");
            record = context.pictures.FirstOrDefault(e => e.PictureType == "LetterHeadImage");
            return (record);
        }

        public IEnumerable<picture> GetPictureByGroup(int ministryID, int groupID)
        {
            //list = myRecords.Where(e => e.ministryID == ministryID && e.GroupID == groupID);
            list = context.pictures.Where(e => e.ministryID == ministryID && e.GroupID == groupID && e.PictureType == null);
            return (list);
        }

        public picture GetPictureByDescription(string Description)
        {
           // record = myRecords.FirstOrDefault(e => e.Description == Description);
            record = context.pictures.FirstOrDefault(e => e.Description == Description && e.PictureType == null);
            return (record);
        }


        public IEnumerable<picture> GetMinistryPictureGroup(int ministryID)
        {

            //var PictureList = myRecords.Where(e => e.ministryID == ministryID && e.GroupID != null)
            var PictureList = context.pictures.Where(e => e.ministryID == ministryID && e.GroupID != null && e.PictureType == null)
                .GroupBy(e => e.GroupID).ToList().Select(e => e.First())
            .OrderBy(e => e.PictureDate);

            return (PictureList);
        }

        public IEnumerable<picture> GetMinistryRecentPictures(int ministryID)
        {
            //list = myRecords.Where(e => e.ministryID == ministryID && e.GroupID == null).Take(30);
            list = context.pictures.Where(e => e.ministryID == ministryID && e.PictureType == null).Take(30);
            return (list);
        }


        public picture GetMinistryDefaultBanner()
        {

            record = context.pictures.FirstOrDefault(e => e.PictureType == "Default Banner");
            return (record);
        }

        public picture GetMemberPicture(int memberID)
        {
            string picCode = string.Format("MP&{0}", memberID);
            //record = myRecords.FirstOrDefault(e => e.PictureType == "Member Picture" && e.Description == picCode);
            record = context.pictures.FirstOrDefault(e => e.PictureType == "Member Picture" && e.Description == picCode);
            if (record == null)
            {
                record = GetDefaultPersonPicture(memberID);
            }
            return (record);
        }

        public picture GetDefaultPersonPicture(int memberID)
        {
            member Person = context.members.SingleOrDefault(e => e.memberID == memberID);
            if (Person.PictureID > 0)
            {
                return (context.pictures.FirstOrDefault(e => e.pictureID == Person.PictureID));
            }
            else
            {
                return (GetDefaultPeoplePicture());
            }
            /*
            TimeSpan span = DateTime.Now.Subtract(Convert.ToDateTime(Person.DOB));
            double Age = span.TotalDays / 365;
            if (Person.Gender == "Male")
            {
                if (Age < 15)
                {
                    return (context.pictures.FirstOrDefault(e => e.Description == "Boy" && e.PictureType == "Default Member"));
                }
                else if (Age < 25)
                {
                    return (context.pictures.FirstOrDefault(e => e.Description == "Young Man" && e.PictureType == "Default Member"));
                }
                else if (Age < 55)
                {
                    return (context.pictures.FirstOrDefault(e => e.Description == "Man" && e.PictureType == "Default Member"));
                }
                else
                {
                    return (context.pictures.FirstOrDefault(e => e.Description == "Old Man" && e.PictureType == "Default Member"));
                }
            }
            else
            {
                if (Age < 9)
                {
                    return (context.pictures.FirstOrDefault(e => e.Description == "Baby Girl" && e.PictureType == "Default Member"));
                }
                else if (Age < 15)
                {
                    return (context.pictures.FirstOrDefault(e => e.Description == "Girl" && e.PictureType == "Default Member"));
                }
                else if (Age < 25)
                {
                    return (context.pictures.FirstOrDefault(e => e.Description == "Young Woman" && e.PictureType == "Default Member"));
                }
                else if (Age < 55)
                {
                    return (context.pictures.FirstOrDefault(e => e.Description == "Woman" && e.PictureType == "Default Member"));
                }
                else
                {
                    return (context.pictures.FirstOrDefault(e => e.Description == "Old Woman" && e.PictureType == "Default Member"));
                }
            }
             */
        }

        public bool IsDefaultPeoplePicture()
        {
              if(context.pictures.Count(e => e.Description == "Default People") > 0 )
              {
                  return true;
              }
              else
              {
                  return false;
              }


        }
        public picture GetDefaultPeoplePicture()
        {
            return (context.pictures.FirstOrDefault(e => e.Description == "Default People"));
        }

        public void DeleteRecord(picture record)
        {
/*            using (churchdatabaseEntities context2 = new churchdatabaseEntities())
            {
                List<picture> myRecords = new List<picture>();
                myRecords.Add(context2.pictures.FirstOrDefault(e => e.pictureID == record.pictureID));
                myRecords.Remove(record);
                context2.SaveChanges();
            }
 * */
            myRecords.Remove(record);
            context.pictures.Remove(record);
            context.SaveChanges();
        }

    }
}
