using System.Collections.Generic;

namespace Genjiworlds.Stats
{
    public enum ClassId
    {
        /// <summary>
        /// Weapon
        /// </summary>
        Warrior,
        /// <summary>
        /// Armor
        /// </summary>
        Fighter,
        /// <summary>
        /// Bow & dex
        /// </summary>
        Ranger,
        /// <summary>
        /// Weapon & dex
        /// </summary>
        Duelist
    }

    public class Class
    {
        public ClassId id;
        public string name, desc;
        private Attribute prefered_attribute;
        public ItemType[] item_choice;
        private int[] attribute_chance;
        private List<Attribute> attribute_choice;

        public Attribute GetAttribute()
        {
            return attribute_choice[Utils.Rand() % attribute_choice.Count];
        }

        public static Class[] classes =
        {
            new Class
            {
                id = ClassId.Warrior,
                name = "warrior",
                desc = "weapon & strength",
                prefered_attribute = Attribute.Strength,
                item_choice = new ItemType[]{ItemType.Weapon, ItemType.Armor, ItemType.Bow },
                attribute_chance = new int[]{2,1,1 }
            },
            new Class
            {
                id = ClassId.Fighter,
                name = "fighter",
                desc = "armor & endurance",
                prefered_attribute = Attribute.Endurance,
                item_choice = new ItemType[]{ItemType.Armor, ItemType.Weapon, ItemType.Bow },
                attribute_chance = new int[]{1,1,2 }
            },
            new Class
            {
                id = ClassId.Ranger,
                name = "ranger",
                desc = "bow & dexterity",
                prefered_attribute = Attribute.Dexterity,
                item_choice = new ItemType[]{ItemType.Bow, ItemType.Weapon, ItemType.Armor },
                attribute_chance = new int[]{1,2,1 }
            },
            new Class
            {
                id = ClassId.Duelist,
                name = "duelist",
                desc = "weapon & dexterity",
                prefered_attribute = Attribute.Dexterity,
                item_choice = new ItemType[]{ItemType.Weapon, ItemType.Armor, ItemType.Bow },
                attribute_chance = new int[]{1,2,1 }
            }
        };

        private static Dictionary<Attribute, List<Class>> class_map;

        static Class()
        {
            class_map = new Dictionary<Attribute, List<Class>>();
            for (int i = 0; i < (int)Attribute.Max; ++i)
            {
                List<Class> choices = new List<Class>();
                foreach (Class c in classes)
                {
                    choices.Add(c);
                    if (c.prefered_attribute == (Attribute)i)
                        choices.Add(c);
                }
                class_map[(Attribute)i] = choices;
            }

            foreach (Class c in classes)
            {
                c.attribute_choice = new List<Attribute>();
                for (int i = 0; i < c.attribute_chance.Length; ++i)
                {
                    for (int j = 0; j < c.attribute_chance[i]; ++j)
                        c.attribute_choice.Add((Attribute)i);
                }
            }
        }

        public static Class Get(Attribute prefered_attribute)
        {
            List<Class> choices = class_map[prefered_attribute];
            return choices[Utils.Rand() % choices.Count];
        }

        public static Class Get(ClassId id)
        {
            return classes[(int)id];
        }
    }
}
