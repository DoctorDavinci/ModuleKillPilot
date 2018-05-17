using System;
using System.Collections;
using UnityEngine;
using BDArmory.Core.Module;
using BDArmory.Control;
using BDArmory;

namespace ModuleKillPilot
{
    public class ModuleKillPilot : PartModule
    {
        [KSPField(isPersistant = true, guiActiveEditor = true, guiActive = true, guiName = "KILL PILOT"),
         UI_Toggle(scene = UI_Scene.All, disabledText = "", enabledText = "")]
        public bool killPilot = false;

        //        [KSPField(isPersistant = true, guiActive = true, guiActiveEditor = true),
        //         UI_FloatRange(controlEnabled = true, scene = UI_Scene.All, minValue = 0.0f, maxValue = 1f, stepIncrement = 0.01f)]
        public float modifier = 0.5f;

        private bool killed = false;
        private float armorCheck = 0.0f;

        private ProtoCrewMember kerbal;

        public HitpointTracker hpTracker;
        private HitpointTracker GetHP()
        {
            HitpointTracker hp = null;

            hp = part.FindModuleImplementing<HitpointTracker>();

            return hp;
        }


        [KSPAction("Kill Pilot", KSPActionGroup.Abort)]
        public void Kill(KSPActionParam param)
        {
            Kill();
        }

        private void DetectDamage()
        {
            hpTracker = GetHP();
            modifier = hpTracker.Armor / hpTracker.ArmorThickness;

            var bullet = GameObject.FindObjectOfType<PooledBullet>();

            if (bullet.hasPenetrated)
            {
                if (hpTracker.Armor <= armorCheck)
                {
                    armorCheck = hpTracker.Armor;
                    var random = new System.Random().Next(0, 1);

                    if (random >= modifier)
                    {
                        Kill();
                    }
                }
            }
        }

        private void Setup()
        {
            hpTracker = GetHP();
            armorCheck = hpTracker.Armor;
        }

        public void Kill()
        {
            if (!killed)
            {
                Debug.Log("[MKP] Killing Pilot");
                killed = true;
            }
            else
            {
                StartCoroutine(KeepDead());
            }
        }

        IEnumerator KeepDead()
        {
            ProtoCrewMember kerbal;
            kerbal = this.part.protoModuleCrew[0];
            if (!kerbal.outDueToG)
            {
                kerbal.outDueToG = true;
            }

            yield return new WaitForFixedUpdate();
            BDAcLock();
            Kill();
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (HighLogic.LoadedSceneIsFlight)
            {
                part.force_activate();
                Setup();

            }
        }

        private void BDAcLock()
        {
            foreach (Part p in this.vessel.parts)
            {
                var wm = vessel.FindPartModuleImplementing<MissileFire>();
                var PAI = vessel.FindPartModuleImplementing<BDModulePilotAI>();
                var SAI = vessel.FindPartModuleImplementing<BDModuleSurfaceAI>();

                if (wm != null)
                {
                    if (wm.guardMode)
                    {
                        wm.guardMode = false;
                    }
                }

                if (PAI != null)
                {
                    if (PAI.pilotOn)
                    {
                        PAI.pilotOn = false;
                    }
                }


                if (SAI != null)
                {
                    if (SAI.pilotOn)
                    {
                        SAI.pilotOn = false;
                    }
                }
            }
        }

        void Update()
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return;

            if (HighLogic.LoadedSceneIsFlight)
            {
                DetectDamage();

                if (killPilot)
                {
                    if (!killed)
                    {
                        Kill();
                    }
                }
            }
        }
    }
}