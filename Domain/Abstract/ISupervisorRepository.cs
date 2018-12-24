using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Abstract
{
    public interface ISupervisorRepository
    {
        void AddRecord(supervisor Record);
        IEnumerable<supervisor> GetSupervisorByStaff(int bossID);
        supervisor GetSupervisorByBoss(int staffID);
        void DeleteRecord(supervisor record);

    }
}
