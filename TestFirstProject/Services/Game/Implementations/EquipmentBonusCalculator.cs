using TestFirstProject.Models.Game;

namespace TestFirstProject.Services.Game.Implementations
{
    /// <summary>
    /// Shared equipment bonus aggregation, used by both CombatEngine (battle init) and
    /// CharacterProgressionService (stat sheet display).
    /// </summary>
    public static class EquipmentBonusCalculator
    {
        public static EquipBonuses Calculate(ICollection<EquippedItem> equipped)
        {
            var bonuses = new EquipBonuses();
            foreach (var eq in equipped)
            {
                var item = eq.InventoryItem.Item;
                bonuses.HP += item.BonusHP;
                bonuses.MP += item.BonusMP;
                bonuses.Attack += item.BonusAttack;
                bonuses.Defense += item.BonusDefense;
                bonuses.MagicPower += item.BonusMagicPower;
                bonuses.Speed += item.BonusSpeed;
                bonuses.CritChance += item.BonusCritChance;
                bonuses.DodgeChance += item.BonusDodgeChance;

                if (!string.IsNullOrEmpty(eq.InventoryItem.EnchantmentStat))
                {
                    ApplyEnchantment(bonuses, eq.InventoryItem.EnchantmentStat, eq.InventoryItem.EnchantmentValue);
                }
            }
            return bonuses;
        }

        private static void ApplyEnchantment(EquipBonuses bonuses, string stat, int value)
        {
            switch (stat)
            {
                case "HP": bonuses.HP += value; break;
                case "MP": bonuses.MP += value; break;
                case "Attack": bonuses.Attack += value; break;
                case "Defense": bonuses.Defense += value; break;
                case "MagicPower": bonuses.MagicPower += value; break;
                case "Speed": bonuses.Speed += value; break;
            }
        }
    }

    public class EquipBonuses
    {
        public int HP { get; set; }
        public int MP { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int MagicPower { get; set; }
        public int Speed { get; set; }
        public double CritChance { get; set; }
        public double DodgeChance { get; set; }
    }
}
