using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpaceLibrary
{
    public class AIData
    {
        public double currentHealth { get; private set; }
        public double deadHealth { get; private set; }
        public double criticalHealth { get; private set; }
        public double weakHealth { get; private set; }
        public double okHealth { get; private set; }
        public double healthyHealth { get; private set; }
        public double fullHealth { get; private set; }

        public double pistolStrength { get; private set; }
        public double rifleStrength { get; private set; }
        public double partyCannonStrength { get; private set; }
        public double navyRailgunStrength { get; private set; }

        public void Update(Ship s)
        {
            currentHealth = (s.m_CurrentHull + s.m_CurrentShield) / (s.m_MaxHull + s.m_MaxShield);
            deadHealth = FuzzyReverseGrade(currentHealth, 0, 0.2);
            criticalHealth = FuzzyTriangle(currentHealth, 0.1, 0.3, 0.25);
            weakHealth = FuzzyTriangle(currentHealth, 0.3, 0.6, 0.45);
            okHealth = FuzzyTriangle(currentHealth, 0.5, 0.8, 0.65);
            healthyHealth = FuzzyTriangle(currentHealth, .7, 1, 0.85);
            fullHealth = FuzzyGrade(currentHealth, 0.9, 1);

            float gunStrength = (float)((s.m_PrimaryWeapons.Sum((g) => g.m_GunStrength)) / Math.Sqrt(s.m_PrimaryWeapons.Count));
            gunStrength /= 300;
            pistolStrength = FuzzyReverseGrade(gunStrength, 0.12, 0.25);
            rifleStrength = FuzzyTriangle(gunStrength, 0.12, 0.37, 0.25);
            partyCannonStrength = FuzzyTriangle(gunStrength, 0.37, 0.67, 0.5);
            navyRailgunStrength = FuzzyGrade(gunStrength, 0.75, 0.87);
        }

        public static double FuzzyTriangle(double position, double left, double right, double centre)
        {
            if (position <= left)
                return 0;
            else if (position == centre)
                return 1;
            else if (position >= right)
                return 0;
            else if (position > left && position < centre)
                return (position / (centre - left)) - (left / (centre - left));
            else
                return (-position / (right - centre)) - (right / (right - centre));
        }

        public static double FuzzyGrade(double position, double beginUp, double endUp)
        {
            if (position <= beginUp)
                return 0;
            else if (position >= endUp)
                return 1;
            else
                return (position / (endUp - beginUp)) - (beginUp / (endUp - beginUp));
        }

        public static double FuzzyReverseGrade(double position, double beginDown, double endDown)
        {
            if (position >= endDown)
                return 0;
            else if (position <= beginDown)
                return 1;
            else
                return (-position / (endDown - beginDown)) + (endDown / (endDown - beginDown));
        }
    }
}
