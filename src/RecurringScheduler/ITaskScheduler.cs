using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecurringScheduler
{
    public interface ITaskScheduler : IDisposable
    {
        void Start();
        void Stop();
    }
}
