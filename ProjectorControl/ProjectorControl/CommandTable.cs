using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProjectorControl
{
    class CommandTable
    {
        public static string haha = "hahastring";

        public static int getPort(string type)
        {
            int port = 4352;
            switch (type)
            {
                case "Z15WST":
                    port = 23;
                    break;
                default:
                    port = 4352;
                    break;
            }
            return port;
        }

        public static string getPowerOnCommand(string type)
        {
            switch (type)
            {
                case "Z15WST":
                    return "0x7E, 0x30, 0x30, 0x30, 0x30, 0x20, 0x31, 0x0D";
                default:
                    return "0x25, 0x31, 0x50, 0x4F, 0x57, 0x52, 0x20,  0x31, 0x0D";
            }
        }

        public static string getPowerOffCommand(string type)
        {
            switch (type)
            {
                case "Z15WST":
                    return "0x7E, 0x30, 0x30, 0x30, 0x30, 0x20, 0x30, 0x0D";
                default:
                    return "0x25, 0x31, 0x50, 0x4F, 0x57, 0x52, 0x20,  0x30, 0x0D";
            }
        }

        public static string getPowerStateCommand(string type)
        {
            switch (type)
            {
                case "Z15WST":
                    return "0x7E, 0x30, 0x30, 0x31, 0x32, 0x34, 0x20, 0x31, 0x0D";
                default:
                    return "0x25, 0x31, 0x50, 0x4F, 0x57, 0x52, 0x20,  0x3F, 0x0D";
            }
        }
    }
}
