
using Godot;

namespace MathUtils
{
    public static class Percentage
    {
        /// <summary>
        /// Converts the value passed to a percentage value.
        /// </summary>
        /// <returns> The converted value </returns>
        public static int ConvertNumberToPercentage(float number, float maxValue)
        {
            return (int)(number * 100 / maxValue);
        }
    }

    public static class UnitCalculations
    {
        public static float CalcHealth(int strength)
        {
            return Mathf.Round(10 + strength * 2);
        }
        public static float CalcMana(int magicka)
        {
            return Mathf.Round(magicka * 2.5f);
        }
        public static float CalcBaseMeleeDmg(int strength, int speed)
        {
            return Mathf.Round(strength * 0.5f + speed * 0.25f);
        }
        public static float CalcBaseRangedDmg(int strength, int speed)
        {
            return Mathf.Round(strength * 0.25f + speed * 0.60f);
        }
        public static float CalcBaseMagicDmg(int magicka)
        {
            return Mathf.Round(magicka * 0.75f);
        }
        public static int CalcBaseInitiative(int speed)
        {
            return speed;
        }
        public static float CalcBaseCritChance(int speed)
        {
            return Mathf.Round(speed / 2 + 1);
        }
        public static int CalcUnitMaxStatPoints(int level)
        {
            return Mathf.RoundToInt(27 + level * 3);
        }
        public static int CalcUnitAvailableStatPoints(int level, int strength, int speed, int magicka)
        {
            return CalcUnitMaxStatPoints(level) - (strength + speed + magicka);
        }
    }
}
