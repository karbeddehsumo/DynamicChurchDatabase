using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFPledgeRepository : IPledgeRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private IEnumerable<pledge> list;
        private pledge record;

        private List<pledge> myRecords = new List<pledge>();

        public EFPledgeRepository()
        {
            myRecords = context.pledges.ToList(); 
        }

        public void AddRecord(pledge Record)
        {
            myRecords.Add(record);
        }



        public pledge GetPledgeByID(int pledgeID)
        {
            record = myRecords.FirstOrDefault(e => e.pledgeID == pledgeID);
            return (record);
        }

        public IEnumerable<pledge> GetPledgeByMember(int memberID, int PledgeYear)
        {
             var jointTithe = context.spouses.FirstOrDefault(e => e.spouse1ID == memberID || e.spouse2ID == memberID);
             if (jointTithe != null)
             {
                 var listA = myRecords.Where(e => e.memberID == jointTithe.spouse1ID && e.PledgeYear == PledgeYear);
                 var listB = myRecords.Where(e => e.memberID == jointTithe.spouse2ID && e.PledgeYear == PledgeYear);


                 if ((listA.Count() > 0) && (listB.Count() > 0))
                 {
                     list = listA.Concat(listB);
                 }
                 else if (listA.Count() > 0)
                 {
                     list = listA;
                 }
                 else if (listB.Count() > 0)
                 {
                     list = listB;
                 }
             }
             else
             {
                 list = myRecords.Where(e => e.memberID == memberID && e.PledgeYear == PledgeYear);
             }
            return (list);
        }

        public IEnumerable<pledge> GetPledgeByType(string PledgeType, int PledgeYear)
        {
            list = myRecords.Where(e => e.Type == PledgeType && e.PledgeYear == PledgeYear);
            return (list);
        }

        public IEnumerable<pledge> GetPledgeByFund(int fundID, int PledgeYear)
        {
            list = myRecords.Where(e => e.FundID == fundID && e.PledgeYear == PledgeYear);
            return (list);
        }

        public IEnumerable<pledge> GetPledgeByYear(int pledgeYear)
        {
            list = myRecords.Where(e => e.PledgeYear == pledgeYear);
            return (list);
        }

        public void DeleteRecord(pledge record)
        {
            myRecords.Remove(record);
            context.pledges.Remove(record);
            context.SaveChanges();
        }

    }
}
