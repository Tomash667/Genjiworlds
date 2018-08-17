using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Genjiworlds.Stats
{
    public enum ClassId
    {
        Warrior,
        Fighter,
        Ranger,
        Duelist
    }

    public class Class
    {
        public ClassId id;
        public string name;

        public static Class[] classes =
        {
            new Class
            {
                id = ClassId.Warrior,
                name = "warrior"
            },
            new Class
            {
                id = ClassId.Fighter,
                name = "fighter"
            },
            new Class
            {
                id = ClassId.Ranger,
                name = "ranger"
            },
            new Class
            {
                id = ClassId.Duelist,
                name = "duelist"
            }
        };
    }
}
