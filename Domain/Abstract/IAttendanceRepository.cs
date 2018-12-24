using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface IAttendanceRepository
    {
        attendance GetAttendanceByID(int attendanceID);
        IEnumerable<attendance> GetAttendanceByCalendar(int calendarID);
        IEnumerable<attendance> GetAttencanceByMember(int memberID);
        IEnumerable<attendance> GetAttendanceByDate(DateTime EventDate);
        bool AddRecord(attendance record);
        void DeleteRecord(attendance record);
    }
}
