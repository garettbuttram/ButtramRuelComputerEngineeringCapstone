using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IrrlichtLime;
using IrrlichtLime.Core;
using IrrlichtLime.Scene;
using IrrlichtLime.Video;
using System.Drawing.Drawing2D;

namespace SpaceLibrary
{
    public class TestShip : Ship
    {
        private List<Ship> m_Allies = new List<Ship>();
        private List<Ship> m_Enemies = new List<Ship>();

        private int m_UpdateListsTotal = Galaxy.m_galaxyGen.Next(20, 80);
        private int m_UpdateTime = 0;

        public TestShip(Faction faction)
            :base(faction)
        {
            m_CurrentHull = m_MaxHull = Galaxy.m_galaxyGen.Next(75, 151);
            m_CurrentShield = m_MaxShield = Galaxy.m_galaxyGen.Next(20, 51);

            m_ShieldCooldownTime = Galaxy.m_galaxyGen.Next(100, 201);
            m_ShieldRechargeRate = Galaxy.m_galaxyGen.Next(1, 6);

            int numGuns = Galaxy.m_galaxyGen.Next(1, 4);
            for(int i = 0; i < numGuns; ++i)
            {
                GunType gType = (GunType)Galaxy.m_galaxyGen.Next((int)GunType.WeakLaser, (int)GunType.WeakMine);
                m_PrimaryWeapons.Add(new Gun(this, gType));
            }
            m_SecondaryWeapon = new Gun(this, GunType.None);
        }

        public TestShip(ShipSave ss)
            :base(ss)
        { }

        public override void Update(GameState currentState)
        {
            if (m_UpdateTime == m_UpdateListsTotal)
            {
                m_Allies.Clear();
                foreach (Ship s in currentState.GetAllGameObjects((go) => go.m_IsAlive && go is Ship &&
                                   MeshGameObject.GetDistanceBetweenMeshes(this, (go as Ship)) < _SHIP_VIEW_RANGE &&
                                   (go as Ship).m_ShipFaction == m_ShipFaction))
                    m_Allies.Add(s);

                m_Enemies.Clear();
                foreach (Ship s in currentState.GetAllGameObjects((go) => go.m_IsAlive && go is Ship &&
                                    MeshGameObject.GetDistanceBetweenMeshes(this, (go as Ship)) < _SHIP_VIEW_RANGE &&
                                    (go as Ship).m_ShipFaction != m_ShipFaction))
                    m_Enemies.Add(s);
                m_UpdateTime = 0;
            }
            else
                m_UpdateTime++;

            float teamStrength = (6 + m_Allies.Count) / (4 + m_Enemies.Count);
            float enemyStrength = (8 + m_Enemies.Count) / (8 + m_Allies.Count);

            //Update which state you are in
            switch (m_ShipState)
            {
                case ShipState.Attack:
                    DecideToAttack(teamStrength, enemyStrength);
                    break;
                case ShipState.Roam:
                    if (m_Enemies.Count != 0)
                    {
                        m_ShipState = ShipState.Attack;
                        PickNewAttackTarget();
                    }
                    break;
                case ShipState.Flee:
                    List<Ship> chaseRange = new List<Ship>();
                    foreach(Ship s in currentState.GetAllGameObjects((go) => go is Ship &&
                                                    MeshGameObject.GetDistanceBetweenMeshes(this, (go as Ship)) <  _CHASE_DISTANCE &&
                                                    (go as Ship).m_ShipFaction != m_ShipFaction))
                        chaseRange.Add(s);
                    if (chaseRange.Count == 0)
                        m_ShipState = ShipState.Roam;
                    if (teamStrength > _STRONG_TEAM_STRENGTH)
                        m_ShipState = ShipState.Attack;
                    break;
                case ShipState.Dodge:
                    if (m_Target == null || !m_Target.m_IsAlive)
                    {
                        PickNewAttackTarget();
                        if (m_Target == null || !m_Target.m_IsAlive || !(m_Target is Ship))
                            break;
                    }

                    if (GetDistanceBetweenMeshes(this, m_Target) > _STOP_DODGE_DISTANCE)
                        m_ShipState = ShipState.Attack;
                    if (teamStrength < _WEAK_TEAM_STRENGTH)
                        m_ShipState = ShipState.Flee;
                    break;
                default:
                    m_ShipState = ShipState.Roam;
                    break;
            }

            //Adjust speed and heading based on state
            switch (m_ShipState)
            {
                case ShipState.Attack:
                    DoAttack();
                    break;
                case ShipState.Roam:
                    DoRoam(currentState);
                    break;
                case ShipState.Flee:
                    DoFlee(currentState);
                    break;
                case ShipState.Dodge:
                    DoDodge();
                    break;
            }

            //Adjust heading slightly towards the sun
            double localXDistance = (-m_xPosition) * Math.Cos(-m_Heading) + (-m_zPosition) * Math.Sin(-m_Heading);
            double localZDistance = -(-m_xPosition) * Math.Sin(-m_Heading) + (-m_zPosition) * Math.Cos(-m_Heading);
            double headingTowardsSun = Math.Atan2(localZDistance, localXDistance);

            if (localXDistance < -_TURN_TOLERENCE)
                m_TotalHeadingChange += 0.01f;
            else if (localXDistance > _TURN_TOLERENCE)
                m_TotalHeadingChange -= 0.01f;

            if (Math.Abs(m_xPosition) > 1300 || Math.Abs(m_zPosition) > 1300)
            {
                float newHeading = (float)(Math.Atan2(-m_xPosition, -m_zPosition));
                m_TotalHeadingChange += m_Heading - newHeading;
                m_Heading = newHeading;
            }
            else if (Math.Abs(m_xPosition) > 1500 || Math.Abs(m_zPosition) > 1500)
                m_Target = (MeshGameObject)currentState.GetAllGameObjects((go) => go is Planet).First();

            base.Update(currentState);
        }

        /// <summary>
        /// Decide to attack
        /// </summary>
        private void DecideToAttack(float teamStrength, float enemyStrength)
        {
            if (m_Target == null || !m_Target.m_IsAlive || !(m_Target is Ship))
            {
                PickNewAttackTarget();
                if (m_Target == null || !m_Target.m_IsAlive || !(m_Target is Ship))
                    return;
            }

            if (m_Enemies.Count == 0)
            {
                m_ShipState = ShipState.Roam;
                return;
            }

            Ship target = m_Target as Ship;
            AIData targetData = target.m_ShipData;

            float attack = (float)(Math.Min(Math.Max(targetData.deadHealth, Math.Max(targetData.criticalHealth, targetData.weakHealth)), Math.Max(targetData.pistolStrength, targetData.rifleStrength)));
            float flee = (float)(Math.Min(Math.Max(Math.Max(targetData.okHealth, targetData.healthyHealth), targetData.fullHealth), Math.Max(targetData.partyCannonStrength, targetData.navyRailgunStrength)));

            attack *= 4;
            flee *= 4;

            float probablyAttack = (float)(Math.Max(Math.Max(Math.Min(targetData.partyCannonStrength, targetData.deadHealth), Math.Max(targetData.rifleStrength, Math.Min(targetData.criticalHealth, targetData.weakHealth))), Math.Min(targetData.pistolStrength, targetData.okHealth)));
            if (Math.Min(Math.Max(Math.Max(Math.Max(m_ShipData.okHealth, m_ShipData.healthyHealth), m_ShipData.weakHealth), m_ShipData.fullHealth), Math.Max(Math.Max(m_ShipData.rifleStrength, m_ShipData.partyCannonStrength), m_ShipData.navyRailgunStrength)) > 0.5)
                attack += 3 * probablyAttack;
            else
                flee += 1 * probablyAttack;

            float undecided = (float)(Math.Max(Math.Max(Math.Min(targetData.navyRailgunStrength, Math.Max(targetData.deadHealth, targetData.criticalHealth)), Math.Min(targetData.partyCannonStrength, Math.Max(targetData.criticalHealth, targetData.weakHealth))),
                                               Math.Max(Math.Min(targetData.rifleStrength, Math.Max(targetData.okHealth, targetData.healthyHealth)), Math.Min(targetData.pistolStrength, Math.Max(targetData.healthyHealth, targetData.fullHealth)))));
            if (Math.Min(Math.Max(Math.Max(m_ShipData.okHealth, m_ShipData.healthyHealth), m_ShipData.fullHealth), Math.Max(Math.Max(m_ShipData.rifleStrength, m_ShipData.partyCannonStrength), m_ShipData.navyRailgunStrength)) > 0.5)
                attack += 2 * undecided;
            else
                flee += 2 * undecided;

            float probablyFlee = (float)(Math.Max(Math.Max(Math.Min(targetData.navyRailgunStrength, targetData.weakHealth), Math.Max(targetData.partyCannonStrength, Math.Min(targetData.okHealth, targetData.healthyHealth))), Math.Min(targetData.rifleStrength, targetData.fullHealth)));
            if (Math.Min(Math.Max(Math.Max(m_ShipData.okHealth, m_ShipData.healthyHealth), m_ShipData.fullHealth), Math.Max(m_ShipData.partyCannonStrength, m_ShipData.navyRailgunStrength)) > 0.5)
                attack += 1 * probablyFlee;
            else
                flee += 3 * probablyFlee;

            attack *= teamStrength;
            flee *= enemyStrength;

            if (attack < flee)
                m_ShipState = ShipState.Flee;
            else if (GetDistanceBetweenMeshes(this, m_Target) < _DODGE_DISTANCE)
                m_ShipState = ShipState.Dodge;
        }

        /// <summary>
        /// Does the attack state
        /// </summary>
        private void DoAttack()
        {
            if (m_Target == null || !m_Target.m_IsAlive || !(m_Target is Ship))
            {
                PickNewAttackTarget();
                if (m_Target == null || !m_Target.m_IsAlive || !(m_Target is Ship))
                    return;
            }

            Ship target = m_Target as Ship;

            //Decide what weapon to use
            //Set m_CurrentPrimWeapon to index of primary weapon list

            //Vector2Dd Speed = new Vector2Dd(Math.Sin(m_Heading) * target.m_CurrentSpeed, Math.Cos(m_Heading) * target.m_CurrentSpeed);
            //Vector2Dd targetSpeed = new Vector2Dd(Math.Sin(target.m_Heading) * target.m_CurrentSpeed, Math.Cos(target.m_Heading) * target.m_CurrentSpeed);
            //Vector2Dd velocDiff = targetSpeed - Speed;
            //Vector2Dd posDiff = new Vector2Dd(target.m_xPosition - m_xPosition, target.m_zPosition - m_zPosition);
            //double timeToClose = posDiff.Length / velocDiff.Length;

            //Vector2Dd targetDest = new Vector2Dd(target.m_xPosition + (Math.Sin(target.m_Heading) * target.m_CurrentSpeed) * timeToClose,
            //                                        target.m_zPosition + (Math.Cos(target.m_Heading) * target.m_CurrentSpeed) * timeToClose);

            //double localXDistance = (targetDest.X - m_xPosition) * Math.Cos(-m_Heading) + (targetDest.Y - m_zPosition) * Math.Sin(-m_Heading);
            //double localZDistance = -(m_Target.m_xPosition - m_xPosition) * Math.Sin(-m_Heading) + (m_Target.m_zPosition - m_zPosition) * Math.Cos(-m_Heading);

            //if (localXDistance < -_TURN_TOLERENCE)
            //{
            //    m_TotalHeadingChange -= 0.02f;
            //    if (m_CurrentSpeed > 2f)
            //        m_CurrentSpeed -= 0.05f;
            //    else
            //        m_CurrentSpeed += 0.05f;
            //}
            //else if (localXDistance > _TURN_TOLERENCE)
            //{
            //    m_TotalHeadingChange += 0.02f;
            //    if (m_CurrentSpeed > 2f)
            //        m_CurrentSpeed -= 0.05f;
            //    else
            //        m_CurrentSpeed += 0.05f;
            //}
            //else
            //    m_CurrentSpeed += 0.05f;

            double leadTime = GetDistanceBetweenMeshes(this, target) / m_CurrentSpeed;
            double targetXPosAfterLead = target.m_xPosition + Math.Cos(target.m_Heading) * target.m_CurrentSpeed * leadTime;
            double targetZPosAfterLead = target.m_zPosition + Math.Cos(target.m_Heading) * target.m_CurrentSpeed * leadTime;

            double localXDistance = (targetXPosAfterLead - m_xPosition) * Math.Cos(-m_Heading) + (targetZPosAfterLead - m_zPosition) * Math.Sin(-m_Heading);
            double localZDistance = -(targetXPosAfterLead - m_xPosition) * Math.Sin(-m_Heading) + (targetZPosAfterLead - m_zPosition) * Math.Cos(-m_Heading);

            if (localXDistance < -_TURN_TOLERENCE)
            {
                m_TotalHeadingChange -= 0.02f;
                if (m_CurrentSpeed > 2f)
                    m_CurrentSpeed -= 0.05f;
                else
                    m_CurrentSpeed += 0.05f;
            }
            else if (localXDistance > _TURN_TOLERENCE)
            {
                m_TotalHeadingChange += 0.02f;
                if (m_CurrentSpeed > 2f)
                    m_CurrentSpeed -= 0.05f;
                else
                    m_CurrentSpeed += 0.05f;
            }
            else
                m_CurrentSpeed += 0.05f;

            //double FiringXDistance = (m_Target.m_xPosition - m_xPosition) * Math.Cos(-m_Heading) + (m_Target.m_zPosition - m_zPosition) * Math.Sin(-m_Heading);
            if (Math.Abs(localXDistance) < _FIRE_GUN_TOLERANCE)
                FireCurrentPrimaryWeapon();
            
        }

        /// <summary>
        /// Does flee state
        /// </summary>
        private void DoFlee(GameState currentState)
        {
            if (m_Target == null || !m_Target.m_IsAlive || m_Target is Ship)
            {
                List<MeshGameObject> meshGameObjects = new List<MeshGameObject>();
                foreach (MeshGameObject mgo in (currentState.GetAllGameObjects((go) => go is MeshGameObject && !(go is Ship))))
                    meshGameObjects.Add(mgo);
                m_Target = meshGameObjects[Galaxy.m_galaxyGen.Next(meshGameObjects.Count)];
            }

            double localXDistance = (m_Target.m_zPosition - m_xPosition) * Math.Cos(-m_Heading) + (m_Target.m_zPosition - m_zPosition) * Math.Sin(-m_Heading);
            double localZDistance = -(m_Target.m_xPosition - m_xPosition) * Math.Sin(-m_Heading) + (m_Target.m_zPosition - m_zPosition) * Math.Cos(-m_Heading);

            if (localXDistance < -_TURN_TOLERENCE)
            {
                m_TotalHeadingChange -= 0.02f;
                if (m_CurrentSpeed > 2f)
                    m_CurrentSpeed -= 0.05f;
                else
                    m_CurrentSpeed += 0.05f;
            }
            else if (localXDistance > _TURN_TOLERENCE)
            {
                m_TotalHeadingChange += 0.02f;
                if (m_CurrentSpeed > 3f)
                    m_CurrentSpeed -= 0.05f;
                else
                    m_CurrentSpeed += 0.05f;
            }
            else
                m_CurrentSpeed += 0.05f;

        }

        /// <summary>
        /// Does roam state
        /// </summary>
        /// <param name="currentState"></param>
        private void DoRoam(GameState currentState)
        {
            if (m_Target == null || !m_Target.m_IsAlive || GetDistanceBetweenMeshes(this, m_Target) < 5)
            {
                List<MeshGameObject> meshGameObjects = new List<MeshGameObject>();
                foreach (MeshGameObject mgo in (currentState.GetAllGameObjects((go) => go is MeshGameObject)))
                    meshGameObjects.Add(mgo);
                m_Target = meshGameObjects[Galaxy.m_galaxyGen.Next(meshGameObjects.Count)];
            }

            double localXDistance = (m_Target.m_xPosition - m_xPosition) * Math.Cos(-m_Heading) + (m_Target.m_zPosition - m_zPosition) * Math.Sin(-m_Heading);
            double localZDistance = -(m_Target.m_xPosition - m_xPosition) * Math.Sin(-m_Heading) + (m_Target.m_zPosition - m_zPosition) * Math.Cos(-m_Heading);

            if (localXDistance < -_TURN_TOLERENCE)
            {
                m_TotalHeadingChange -= 0.02f;
                if (m_CurrentSpeed > 2f)
                    m_CurrentSpeed -= 0.05f;
                else
                    m_CurrentSpeed += 0.05f;
            }
            else if (localXDistance > _TURN_TOLERENCE)
            {
                m_TotalHeadingChange += 0.02f;
                if (m_CurrentSpeed > 2f)
                    m_CurrentSpeed -= 0.05f;
                else
                    m_CurrentSpeed += 0.05f;
            }
            else
                m_CurrentSpeed += 0.05f;
        }

        /// <summary>
        /// Does dodging state
        /// </summary>
        private void DoDodge()
        {
            if (m_Target == null || !m_Target.m_IsAlive)
            {
                PickNewAttackTarget();
                if (m_Target == null || !m_Target.m_IsAlive && !(m_Target is Ship))
                    return;
            }

            Ship target = m_Target as Ship;
            Vector2Dd Speed = new Vector2Dd(Math.Sin(m_Heading) * target.m_CurrentSpeed, Math.Cos(m_Heading) * target.m_CurrentSpeed);
            Vector2Dd targetSpeed = new Vector2Dd(Math.Sin(target.m_Heading) * target.m_CurrentSpeed, Math.Cos(target.m_Heading) * target.m_CurrentSpeed);
            Vector2Dd velocDiff = targetSpeed - Speed;
            Vector2Dd posDiff = new Vector2Dd(target.m_xPosition - m_xPosition, target.m_zPosition - m_zPosition);
            double timeToClose = posDiff.Length / velocDiff.Length;
            Vector2Dd targetDest = new Vector2Dd(target.m_xPosition + (Math.Sin(target.m_Heading) * target.m_CurrentSpeed) * timeToClose,
                                                    target.m_zPosition + (Math.Cos(target.m_Heading) * target.m_CurrentSpeed) * timeToClose);
                
            double localXDistance = (targetDest.X - m_xPosition) * Math.Cos(-m_Heading) + (targetDest.Y - m_zPosition) * Math.Sin(-m_Heading);
            //double localZDistance = -(m_Target.m_xPosition - m_xPosition) * Math.Sin(-m_Heading) + (m_Target.m_zPosition - m_zPosition) * Math.Cos(-m_Heading);
            if (localXDistance < -_TURN_TOLERENCE)
            {
                m_TotalHeadingChange += 0.02f;
                if (m_CurrentSpeed > 2f)
                    m_CurrentSpeed -= 0.005f;
                else
                    m_CurrentSpeed += 0.005f;
            }
            else if (localXDistance > _TURN_TOLERENCE)
            {
                m_TotalHeadingChange -= 0.02f;
                if (m_CurrentSpeed > 2f)
                    m_CurrentSpeed -= 0.05f;
                else
                    m_CurrentSpeed += 0.05f;
            }
            else
                m_CurrentSpeed += 0.05f;
        }

        /// <summary>
        /// Picks a new target to attack
        /// </summary>
        private void PickNewAttackTarget()
        {
            IEnumerable<Ship> check = m_Enemies.Where((ship) => ship.m_IsAlive);
            if (check.Count() == 0)
                return;
            float min = check.Min((s) => GetDistanceBetweenMeshes(this, s));
            m_Target = check.First((shippy) => GetDistanceBetweenMeshes(this, shippy) == min);
        }
    }
}
