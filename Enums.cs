using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RegistryEnforcer
{
    [Flags]
    public enum REG_NOTIFY_CHANGE : uint
    {
        NAME = 0x1,
        ATTRIBUTES = 0x2,
        LAST_SET = 0x4,
        SECURITY = 0x8
    }

    public enum LogLevel
    {
        Trace = 6,
        Debug = 5,
        Info = 4,
        Warn = 3,
        Error = 2,
        Fatal = 1
    }
}
