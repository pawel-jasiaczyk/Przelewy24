using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Przelewy24
{
    public interface IP24Db
    {
        void SaveTransaction (Transaction transaction);
        Transaction GetTransaction (string sessionId);
    }
}
