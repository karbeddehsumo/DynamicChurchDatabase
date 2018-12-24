using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFMinistryGroupRepository : IMinistryGroupRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private ministrygroup record;
        private IEnumerable<ministrygroup> list;

        private List<ministrygroup> myRecords = new List<ministrygroup>();

        public EFMinistryGroupRepository()
        {
            myRecords = context.ministrygroups.ToList(); 
        }

        public void AddRecord(ministrygroup Record)
        {
            myRecords.Add(record);
        }

        public ministrygroup GetMinistryGroupParent(int childID)
        {
            record = myRecords.FirstOrDefault(e => e.ChildID == childID);
            return (record);
        }

        public IEnumerable<ministrygroup> GetMinistryGrouChild(int parentID)
        {
            list = myRecords.Where(e => e.ParentID == parentID);
            return (list);
        }

        public void DeleteRecord(ministrygroup record)
        {
            myRecords.Remove(record);
            context.ministrygroups.Remove(record);
            context.SaveChanges();
        }

    }
}
