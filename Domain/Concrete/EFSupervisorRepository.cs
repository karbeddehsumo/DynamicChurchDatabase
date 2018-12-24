using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFSupervisorRepository : ISupervisorRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private supervisor record;
        private IEnumerable<supervisor> list;

        private List<supervisor> myRecords = new List<supervisor>();

        public EFSupervisorRepository()
        {
            myRecords = context.supervisors.ToList(); 
        }

        public void AddRecord(supervisor Record)
        {
            myRecords.Add(record);
        }


        public IEnumerable<supervisor> GetSupervisorByStaff(int bossID)
        {
            list = myRecords.Where(e => e.bossID == bossID);
            return (list);
        }

        public supervisor GetSupervisorByBoss(int staffID)
        {
            record = myRecords.FirstOrDefault(e => e.staffID == staffID);
            return (record);
        }

        public void DeleteRecord(supervisor record)
        {
            myRecords.Remove(record);
            context.supervisors.Remove(record);
            context.SaveChanges();
        }
    }
}
