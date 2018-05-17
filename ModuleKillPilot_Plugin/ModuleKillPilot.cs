using System;
using System.Collections;
using UnityEngine;
using BDArmory.Core.Module;

namespace ModuleKillPilot
{
    public class ModuleKillPilot : PartModule
    {
        [KSPField(isPersistant = true, guiActiveEditor = true, guiActive = true, guiName = "KILL PILOT"),
         UI_Toggle(scene = UI_Scene.All, disabledText = "", enabledText = "")]
        public bool killPilot = false;

        private bool killed = false;

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
            if (hpTracker.Armor <= hpTracker.ArmorThickness / 0.5f)
            {
                Kill();
            }
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

            Kill();
        }

        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (HighLogic.LoadedSceneIsFlight)
            {
                part.force_activate();
            }
        }

        void Update()
        {
            if (!HighLogic.LoadedSceneIsFlight)
                return;

            if (HighLogic.LoadedSceneIsFlight)
            {

                if (killPilot)
                {
                    if (!killed)
                    {
                        Kill();
                    }

                    DetectDamage();
                }
            }
        }
    }
}