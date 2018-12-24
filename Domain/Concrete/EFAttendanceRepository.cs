using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Abstract;
using System.Data.Entity;


namespace Domain.Concrete
{
    public class EFAttendanceRepository : IAttendanceRepository
    {
        private churchdatabaseEntities context = new churchdatabaseEntities();
        private attendance record;
        private IEnumerable<attendance> list;

        private List<attendance> myRecords = new List<attendance>();

        public EFAttendanceRepository()
        {
            myRecords = context.attendances.ToList(); 
        }

        public bool AddRecord(attendance Record)
        {
            myRecords.Add(record);
            return true;
        }


        public attendance GetAttendanceByID(int attendanceID)
        {
            record = myRecords.FirstOrDefault(e => e.attendanceID == attendanceID);
            return (record);
        }

        public IEnumerable<attendance> GetAttendanceByCalendar(int calendarID)
        {
            list = myRecords.Where(e => e.calendarID == calendarID);
            return (list);
        }

        public IEnumerable<attendance> GetAttencanceByMember(int memberID)
        {
            list = myRecords.Where(e => e.memberID == memberID).Take(100).OrderByDescending(e => e.attendanceID);
            return (list);
        }

        public IEnumerable<attendance> GetAttendanceByDate(DateTime EventDate)
        {
            list = myRecords.Where(e => e.RollCall == EventDate.Date);
            return (list);
        }

        /*
        public bool AddRecord(attendance record)
        {
            myRecords.Add(record);
            context.SaveChanges();
            return true;
        }
        */
        public void DeleteRecord(attendance record)
        {
            myRecords.Remove(record);
            context.attendances.Remove(record);
            context.SaveChanges();
        }
    }
}
