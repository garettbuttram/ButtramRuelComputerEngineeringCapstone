using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Printing;
using System.Drawing.Text;

using IrrlichtLime;
using IrrlichtLime.Core;
using IrrlichtLime.Scene;
using IrrlichtLime.Video;

namespace SpaceLibrary
{
    //Possible types of guns
    public enum GunType
    {
        None = 0,
        MiningLaser = 1000,
        WeakLaser,
        Laser,
        StrongLaser,
        WeakBullet,
        Bullet,
        StrongBullet,
        WeakMissile,
        Missile,
        StrongMissile,
        WeakMine,
        Mine,
        StrongMine
    };

    public class Gun
    {
        //The ship that the gun is attached to
        public Ship m_WhosFiring { get; private set; }

        //Which gun to fire from
        public bool m_firingRight { get; private set; }
        public static float GUN_OFFSET = 4;
        public static float BULLET_ANGLE_OFFSET = .6f;

        //How many updates before the gun can fire again
        private int m_FireRate;
        //The variable used to hold the current cooldown
        private int m_FireCooldownLeft; 

        //The base damage the bullet does to anything
        public int m_BulletDamage { get; private set; }
        //Modifier for the damage to deal damage to the Hull
        public float m_BulletHullModifier { get; private set; }
        //Modifier for the damage to deal damage to the Shield
        public float m_BulletShieldModifier { get; private set; }

        //The speed at which the bullet moves
        public float m_BulletSpeed { get; private set; }

        //The number of update ticks the bullet will last for
        public int m_BulletLife { get; private set; }

        //The direction in which the gun is facing
        public float m_GunHeading { get; private set; }

        //The type of gun
        public GunType m_GunType { get; private set; }

        public float m_GunStrength { get; private set; }

        public Gun(Ship firingShip, GunType type)
        {
            m_GunType = type;

            if (m_GunType == GunType.None)
                return;

            m_WhosFiring = firingShip;
            m_GunHeading = firingShip.m_Heading;

            //Load in different gun stats based on the type of gun
            switch (type)
            {
                case GunType.MiningLaser:
                    m_FireRate = 1;
                    m_BulletDamage = 50;
                    m_BulletHullModifier = 0.04f;
                    m_BulletShieldModifier = 0.04f;
                    m_BulletSpeed = 0; //Length of beam model
                    m_BulletLife = 1;
                    break;
                case GunType.WeakLaser:
                    m_FireRate = 1;
                    m_BulletDamage = 2;
                    m_BulletHullModifier = 0.8f;
                    m_BulletShieldModifier = 0.8f;
                    m_BulletSpeed = 0; //length of beam model
                    m_BulletLife = 1;
                    break;
                case GunType.Laser:
                    m_FireRate = 1;
                    m_BulletDamage = 5;
                    m_BulletHullModifier = 1f;
                    m_BulletShieldModifier = 1;
                    m_BulletSpeed = 0; //length of beam model
                    m_BulletLife = 1;
                    break;
                case GunType.StrongLaser:
                    m_FireRate = 1;
                    m_BulletDamage = 8;
                    m_BulletHullModifier = 1.2f;
                    m_BulletShieldModifier = 1.1f;
                    m_BulletSpeed = 0; //length of beam model
                    m_BulletLife = 1;
                    break;
                case GunType.WeakBullet:
                    m_FireRate = 10;
                    m_BulletDamage = 10;
                    m_BulletHullModifier = 1;
                    m_BulletShieldModifier = 1;
                    m_BulletSpeed = 3;
                    m_BulletLife = 150;
                    break;
                case GunType.Bullet:
                    m_FireRate = 9;
                    m_BulletDamage = 15;
                    m_BulletHullModifier = 1;
                    m_BulletShieldModifier = 1;
                    m_BulletSpeed = 3;
                    m_BulletLife = 200;
                    break;
                case GunType.StrongBullet:
                    m_FireRate = 8;
                    m_BulletDamage = 20;
                    m_BulletHullModifier = 1.1f;
                    m_BulletShieldModifier = 1;
                    m_BulletSpeed = 3;
                    m_BulletLife = 200;
                    break;
                case GunType.WeakMissile:
                    m_FireRate = 30;
                    m_BulletDamage = 50;
                    m_BulletHullModifier = 1.2f;
                    m_BulletShieldModifier = .9f;
                    m_BulletSpeed = 2;
                    m_BulletLife = 200;
                    break;
                case GunType.Missile:
                    m_FireRate = 30;
                    m_BulletDamage = 60;
                    m_BulletHullModifier = 1.2f;
                    m_BulletShieldModifier = .9f;
                    m_BulletSpeed = 2;
                    m_BulletLife = 200;
                    break;
                case GunType.StrongMissile:
                    m_FireRate = 30;
                    m_BulletDamage = 75;
                    m_BulletHullModifier = 1.2f;
                    m_BulletShieldModifier = .9f;
                    m_BulletSpeed = 3;
                    m_BulletLife = 200;
                    break;
                case GunType.WeakMine:
                    m_FireRate = 30;
                    m_BulletDamage = 80;
                    m_BulletHullModifier = 1;
                    m_BulletShieldModifier = 1;
                    m_BulletSpeed = 0;
                    m_BulletLife = 400;
                    break;
                case GunType.Mine:
                    m_FireRate = 30;
                    m_BulletDamage = 90;
                    m_BulletHullModifier = 1.1f;
                    m_BulletShieldModifier = 1;
                    m_BulletSpeed = 0;
                    m_BulletLife = 400;
                    break;
                case GunType.StrongMine:
                    m_FireRate = 30;
                    m_BulletDamage = 100;
                    m_BulletHullModifier = 1.2f;
                    m_BulletShieldModifier = 1;
                    m_BulletSpeed = 0;
                    m_BulletLife = 400;
                    break;
            }

            m_GunStrength = ((m_BulletSpeed * m_BulletLife) * (m_BulletDamage * (m_BulletHullModifier + m_BulletShieldModifier)/2)) / m_FireRate;
        }

        /// <summary>
        /// Updates the gun
        /// </summary>
        public void Update()
        {
            if (m_GunType == GunType.None)
                return;

            m_GunHeading = m_WhosFiring.m_Heading;
        }

        /// <summary>
        /// Creates the bullet the gun fires
        /// </summary>
        /// <returns>The bullet the gun fired</returns>
        public Bullet FireGun()
        {
            if (m_GunType == GunType.None)
                return null;

            if (m_FireCooldownLeft <= 0)
            {
                m_firingRight = !m_firingRight;
                m_FireCooldownLeft = m_FireRate;
                return new Bullet(this);
            }
            else
                --m_FireCooldownLeft;

            return null;
        }
    }
}
