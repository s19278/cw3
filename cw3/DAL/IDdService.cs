using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace cw3.DAL
{
    public interface IDdService
    {

        public IEnumerable<Models.Student> GetStudents();

    }
}
