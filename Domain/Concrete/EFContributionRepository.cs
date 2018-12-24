using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;

namespace Domain.Concrete
{
    public class EFContributionRepository : IContributionRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private contribution record;
        private IEnumerable<contribution> list;

        private List<contribution> myRecords = new List<contribution>();

        public EFContributionRepository()
        {
            myRecords = context.contributions.ToList(); 
        }

        public void AddRecord(contribution Record)
        {
            myRecords.Add(record);
        }

        public contribution GetContributionByID(int contributionID)
        {
            record = myRecords.FirstOrDefault(e => e.contributionID == contributionID);
            return (record);
        }

        public contribution GetContributionByCheckNumber(string CheckNumber)
        {
            record = myRecords.FirstOrDefault(e => e.CheckNumber == CheckNumber);
            return (record);
        }

        public IEnumerable<contribution> GetContributionByDate(DateTime aDate)
        {
            list = myRecords.Where(e => e.ContributionDate == aDate.Date);
            return (list);
        }

        public IEnumerable<contribution> GetContributionByDateRange(DateTime bDate, DateTime eDate)
        {
            list = myRecords.Where(e => e.ContributionDate >= bDate.Date && e.ContributionDate <= eDate.Date);
            return (list);
        }

        public IEnumerable<contribution> GetContributionByMemberDate(int memberID, DateTime aDate)
        {
            var jointTithe = context.spouses.FirstOrDefault(e => e.spouse1ID == memberID || e.spouse2ID == memberID);
            if (jointTithe != null)
            {
                if (jointTithe.spouse1ID == memberID)
                {
                    list = myRecords.Where(e => e.ContributionDate == aDate.Date && (e.memberID == memberID) || (e.memberID == jointTithe.spouse2ID));
                }
                else
                {
                    list = myRecords.Where(e => e.ContributionDate == aDate.Date && (e.memberID == memberID) || (e.memberID == jointTithe.spouse1ID));
                }
            }
            else
            {
                list = myRecords.Where(e => e.ContributionDate == aDate.Date && e.memberID == memberID);
            }
            return (list);
        }

        public IEnumerable<contribution> GetContributionByMemberDateRange(int memberID, DateTime bDate, DateTime eDate)
        {
            var jointTithe = context.spouses.FirstOrDefault(e => e.spouse1ID == memberID || e.spouse2ID == memberID);
            if (jointTithe != null)
            {
                var listA = list = myRecords.Where(e => e.ContributionDate >= bDate.Date && e.ContributionDate <= eDate.Date && (e.memberID == jointTithe.spouse1ID));
                var listB = list = myRecords.Where(e => e.ContributionDate >= bDate.Date && e.ContributionDate <= eDate.Date && (e.memberID == jointTithe.spouse2ID));
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
                list = myRecords.Where(e => e.ContributionDate >= bDate.Date && e.ContributionDate <= eDate.Date && e.memberID == memberID);
            }

            return (list.OrderBy(e => e.ContributionDate));
        }

        public IEnumerable<contribution> GetContributionByCategoryRange(int categoryID, DateTime bDate, DateTime eDate)
        {

            list = myRecords.Where(e => e.ContributionDate >= bDate.Date && e.ContributionDate <= eDate.Date && e.subCategoryID == categoryID);
            return (list);
        }

        public IEnumerable<contribution> GetContributionByMemberCategoryRange(int memberID, int categoryID, DateTime bDate, DateTime eDate)
        {
            var jointTithe = context.spouses.FirstOrDefault(e => e.spouse1ID == memberID || e.spouse2ID == memberID);
            if (jointTithe != null)
            {
                //if (jointTithe.spouse1ID == memberID)
                //{
                //    list = myRecords.Where(e => e.ContributionDate >= bDate.Date && e.ContributionDate <= eDate.Date && e.subCategoryID == categoryID && (e.memberID == memberID) || (e.memberID == jointTithe.spouse2ID));
                //}
                //else
                //{
                //    list = myRecords.Where(e => e.ContributionDate >= bDate.Date && e.ContributionDate <= eDate.Date && e.subCategoryID == categoryID && (e.memberID == memberID) || (e.memberID == jointTithe.spouse1ID));
                //}

                //
                var listA = list = myRecords.Where(e => e.ContributionDate >= bDate.Date && e.ContributionDate <= eDate.Date && e.subCategoryID == categoryID && (e.memberID == jointTithe.spouse1ID));
                var listB = list = myRecords.Where(e => e.ContributionDate >= bDate.Date && e.ContributionDate <= eDate.Date && e.subCategoryID == categoryID && (e.memberID == jointTithe.spouse2ID));
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
                list = myRecords.Where(e => e.ContributionDate >= bDate.Date && e.ContributionDate <= eDate.Date && e.subCategoryID == categoryID && e.memberID == memberID);
            }


            //list = myRecords.Where(e => e.memberID == memberID && e.ContributionDate >= bDate && e.ContributionDate <= eDate && e.subCategoryID == categoryID);
            return (list);
        }

        public int GetContributionByMember(int memberID)
        {
            return (myRecords.Where(e => e.memberID == memberID).Count());
        }


        public void DeleteRecord(contribution record)
        {
            myRecords.Remove(record);
            context.contributions.Remove(record);
            context.SaveChanges();
        }
    }
}
