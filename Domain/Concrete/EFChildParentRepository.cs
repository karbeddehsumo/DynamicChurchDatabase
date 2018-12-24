using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFChildParentRepository : IChildParentRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private childparent record;
        private IEnumerable<childparent> list;

        private List<childparent> myRecords = new List<childparent>();

        public EFChildParentRepository()
        {
            myRecords = context.childparents.ToList(); 
        }


        public bool AddRecord(childparent Record)
        {
            myRecords.Add(record);
            return true;
        }

        public member GetParent(int childID, int parentID)
        {
            record = myRecords.FirstOrDefault(e => e.ChildID == childID && e.ParentID == parentID);
            var parent = context.members.FirstOrDefault(e => e.Status == "Active" && e.memberID == record.ParentID);
            return parent;
        }

        public IEnumerable<member> GetParents(int childID)
        {
            var parents = from g in context.members.Where(e => e.Status == "Active")
                          join c in context.childparents.Where(e => e.ChildID == childID)
                              on g.memberID equals c.ParentID
                         select g;

            return (parents);
        }

        public IEnumerable<childparent> GetParentRecords(int childID)
        {
            list = context.childparents.Where(e => e.ChildID == childID);
            return (list);
        }

        public IEnumerable<member> Getchildren(int parentID)
        {
            var children = from g in context.members.Where(e => e.Status == "Active")
                           join c in context.childparents.Where(e => e.ParentID == parentID)
                               on g.memberID equals c.ChildID
                          select g;

            return (children);
        }

        public bool HasParent(int childID, int parentID)
        {
            record = myRecords.FirstOrDefault(e => e.ChildID == childID && e.ParentID == parentID);
            if (record == null)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void DeleteRecord(childparent record)
        {
            using (churchdatabaseEntities context = new churchdatabaseEntities())
            {
                // myRecords.Remove(record);
                childparent aRecord = context.childparents.FirstOrDefault(e => e.ChildParentID == record.ChildParentID);
                context.childparents.Remove(aRecord);
                context.SaveChanges();
            }
        }
    }
}
