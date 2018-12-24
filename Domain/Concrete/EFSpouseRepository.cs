using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFSpouseRepository : ISpouseRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private spouse record;
        private IEnumerable<spouse> list;

        private List<spouse> myRecords = new List<spouse>();

        public EFSpouseRepository()
        {
            myRecords = context.spouses.ToList(); 
        }

        public bool AddRecord(spouse Record)
        {
            myRecords.Add(record);
            return true;
        }

        public spouse GetSpouseByID(int spouseID)
        {
            record = myRecords.FirstOrDefault(e => e.SpouseID == spouseID);
            return (record);
        }

        public spouse GetSpouseByID1(int spouseID)
        {
            record = myRecords.FirstOrDefault(e => e.spouse1ID == spouseID);
            return (record);
        }

        public spouse GetSpouseByID2(int spouseID)
        {
            record = myRecords.FirstOrDefault(e => e.spouse2ID == spouseID);
            return (record);
        }

        public IEnumerable<spouse> GetSpouseByJointTithe(bool joint)
        {
            list = myRecords.Where(e => e.JointTithe == joint);
            return (list);
        }

        public bool IsJointTitle(int memberID)
        {
            record = myRecords.FirstOrDefault(e => e.spouse1ID == memberID);
            if (record == null)
            {
                record = myRecords.FirstOrDefault(e => e.spouse2ID == memberID);
            }

            if (record == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }
        public string JointTitheTitle(int memberID)
        {
            record = myRecords.FirstOrDefault(e => e.spouse1ID == memberID);
            if (record == null)
            {
                record = myRecords.FirstOrDefault(e => e.spouse2ID == memberID);
            }

            if (record == null)
            {
                var member = context.members.FirstOrDefault(e => e.memberID == memberID);
                return member.FullNameTitle;
            }
            else
            {
                return record.JointTitheTitle;
            }
        }


        /*
        public bool AddRecord(spouse Record)
        {
            myRecords.Add(Record);
            context.SaveChanges();
            return true;
        }
        */
        public void DeleteRecord(spouse record)
        {
            myRecords.Remove(record);
            context.spouses.Remove(record);
            context.SaveChanges();
        }
    }
}
