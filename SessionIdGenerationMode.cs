using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Przelewy24
{
    public enum SessionIdGenerationMode
    {
        time,
        plain,
        addPrefix,
        addPostfix,
        md5
    }
}
