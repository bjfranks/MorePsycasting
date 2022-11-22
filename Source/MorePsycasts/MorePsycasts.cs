using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;
using Verse.AI;
using Verse.Sound;

namespace MorePsycasts
{
    [DefOf]
    public class PsycastDefOf
    {
        public static AbilityDef MorePsycasts_StabilizingTouch;
        public static AbilityDef MorePsycasts_HealingTouch;
        public static AbilityDef MorePsycasts_FlashHeal;
        public static AbilityDef MorePsycasts_HealScars;
        public static AbilityDef MorePsycasts_RegrowBodyParts;
        public static AbilityDef MorePsycasts_RevivingTouch;
        public static AbilityDef MorePsycasts_Ignite;
        public static AbilityDef MorePsycasts_ArcticPinhole;
        public static AbilityDef MorePsycasts_CleanSkip;
        public static AbilityDef MorePsycasts_HaulSkip;
        public static AbilityDef MorePsycasts_Mending;
        public static AbilityDef MorePsycasts_FertilitySkip;
    }

    [DefOf]
    public class HediffDefOf
    {
        public static HediffDef MorePsycasts_PsychicInducedHunger;
        public static HediffDef MorePsycasts_StabilizingTouch;
        public static HediffDef MorePsycasts_AcceleratedHealing;
        public static HediffDef MorePsycasts_PsychicReverberationsScars;
        public static HediffDef MorePsycasts_PsychicReverberationsBodyParts;
        public static HediffDef MorePsycasts_PartiallyGrown;
        public static HediffDef MorePsycasts_PsychicBurn;
        public static HediffDef MorePsycasts_PsychicRessurection;
    }

    public class MorePsycasts_Settings : ModSettings
    {
        public Dictionary<string, bool> psycastStates = new Dictionary<string, bool>();
        public Dictionary<string, List<float>> psycastStats = new Dictionary<string, List<float>>();
        public float? debuff_hunger;
        public float? debuff_rest;
        public float? debuff_pain;

        public float? stabilizing_touch_bleed_factor;
        public float? healing_touch_natural_healing_factor;
        public float? healing_touch_immunity_gain_speed_factor;
        public float? psychic_ressurection_severity_per_day;
        public float? flash_heal_heal_amount = 10f;
        public float? flash_heal_scar_chance = 1.01f;
        public float? heal_scars_healing_speed = 1f;
        public float? regrow_body_parts_healing_speed = 1f;
        public float? reviving_touch_min_proportial_damage = 0.2f;
        public float? reviving_touch_max_proportial_damage = 0.8f;

        public bool reset = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref reset, "MorePsycasts_reset");
            if (reset && Scribe.mode != LoadSaveMode.Saving) return;
            Scribe_Collections.Look(ref psycastStates, "MorePsycasts_psycastStates", LookMode.Value, LookMode.Value, ref psycastKeys, ref boolValues);
            var defNames = psycastStats.Keys.ToList();
            Scribe_Collections.Look(ref defNames, "MorePsycasts_defNames", LookMode.Value);
            var entropies = psycastStats.Select(d => d.Value[0]).ToList();
            Scribe_Collections.Look(ref entropies, "MorePsycasts_entropies", LookMode.Value);
            var psyfocus = psycastStats.Select(d => d.Value[1]).ToList();
            Scribe_Collections.Look(ref psyfocus, "MorePsycasts_psyfocus", LookMode.Value);
            var duration = psycastStats.Select(d => d.Value[2]).ToList();
            Scribe_Collections.Look(ref duration, "MorePsycasts_duration", LookMode.Value);
            if (defNames != null)
                for (var i = 0; i < defNames.Count; i++)
                {
                    var currentList = new List<float>();
                    currentList.Add(entropies[i]);
                    currentList.Add(psyfocus[i]);
                    currentList.Add(duration[i]);
                    psycastStats[defNames[i]] = currentList;
                }

            Scribe_Values.Look(ref debuff_hunger, "MorePsycasts_debuff_hunger", null);
            Scribe_Values.Look(ref debuff_rest, "MorePsycasts_debuff_rest", null);
            Scribe_Values.Look(ref debuff_pain, "MorePsycasts_debuff_pain", null);
            Scribe_Values.Look(ref stabilizing_touch_bleed_factor, "MorePsycasts_stabilizing_touch_bleed_factor", null);
            Scribe_Values.Look(ref healing_touch_natural_healing_factor, "MorePsycasts_healing_touch_natural_healing_factor", null);
            Scribe_Values.Look(ref healing_touch_immunity_gain_speed_factor, "MorePsycasts_healing_touch_immunity_gain_speed_factor", null);
            Scribe_Values.Look(ref flash_heal_heal_amount, "MorePsycasts_flash_heal_heal_amount");
            if (flash_heal_heal_amount is null) flash_heal_heal_amount = 10f;
            Scribe_Values.Look(ref flash_heal_scar_chance, "MorePsycasts_flash_heal_scar_chance");
            if (flash_heal_scar_chance is null) flash_heal_scar_chance = 1.01f;
            Scribe_Values.Look(ref heal_scars_healing_speed, "MorePsycasts_heal_scars_healing_speed");
            if (heal_scars_healing_speed is null) heal_scars_healing_speed = 1f;
            Scribe_Values.Look(ref regrow_body_parts_healing_speed, "MorePsycasts_regrow_body_parts_healing_speed");
            if (regrow_body_parts_healing_speed is null) regrow_body_parts_healing_speed = 1f;
            Scribe_Values.Look(ref reviving_touch_min_proportial_damage, "MorePsycasts_reviving_touch_min_proportial_damage");
            if (reviving_touch_min_proportial_damage is null) reviving_touch_min_proportial_damage = 0.2f;
            Scribe_Values.Look(ref reviving_touch_max_proportial_damage, "MorePsycasts_reviving_touch_max_proportial_damage");
            if (reviving_touch_max_proportial_damage is null) reviving_touch_max_proportial_damage = 0.8f;
            Scribe_Values.Look(ref psychic_ressurection_severity_per_day, "MorePsycasts_psychic_ressurection_severity_per_day");
            if (psychic_ressurection_severity_per_day is null) psychic_ressurection_severity_per_day = 0.1f;
        }

        private List<string> psycastKeys;
        private List<bool> boolValues;

        public void DoSettingsWindowContents(Rect inRect)
        {
            var keysStates = psycastStates.Keys.ToList().OrderByDescending(x => x).ToList();
            var keysStats = psycastStats.Keys.ToList().OrderByDescending(x => x).ToList();

            var rect = inRect.TopPart(0.15f);
            Widgets.DrawLineHorizontal(0f, rect.y, rect.width);
            Widgets.Label(rect.LeftPart(0.7f), "MorePsycasts_MostChangesRequireRestart".Translate());
            Widgets.CheckboxLabeled(new Rect(rect.RightPart(0.3f).x, rect.RightPart(0.3f).y, rect.RightPart(0.3f).width, 25f), "Reset", ref reset);
            Widgets.DrawLineHorizontal(0f, rect.height, rect.width);
            var rect2 = inRect.BottomPart(0.85f);

            var rect3 = rect2.LeftHalf();
            GUI.BeginGroup(rect3, new GUIStyle(GUI.skin.box));
            var rect4 = new Rect(0f, 0f, rect3.width - 20f, (keysStats.Count * 4 + 14) * 32 + 1);
            Widgets.BeginScrollView(rect3.AtZero(), ref scrollPosition1, rect4);
            Widgets.DrawLineHorizontal(0f, rect4.y * 32f, rect4.width);
            Widgets.Label(getDrawRect(ref rect4), "MorePsycasts_PsychicallyInducedHunger".Translate());
            debuff_hunger = TextFieldNumericLabeled(ref rect4, "MorePsycasts_HungerRate".Translate(), (float)debuff_hunger);
            debuff_rest = TextFieldNumericLabeled(ref rect4, "MorePsycasts_RestFall".Translate(), (float)debuff_rest);
            debuff_pain = TextFieldNumericLabeled(ref rect4, "MorePsycasts_PainOffset".Translate(), (float)debuff_pain);
            for (var num = keysStats.Count - 1; num >= 0; num--)
            {
                Widgets.DrawLineHorizontal(0f, rect4.y * 32f, rect4.width);
                Widgets.Label(getDrawRect(ref rect4), keysStats[num]);
                if (psycastStats.ContainsKey(keysStats[num]))
                {
                    var currentList = psycastStats[keysStats[num]];
                    psycastStats[keysStats[num]][0] = TextFieldNumericLabeled(ref rect4, "MorePsycasts_EntropyGain".Translate(), currentList[0]);
                    psycastStats[keysStats[num]][1] = TextFieldNumericLabeled(ref rect4, "MorePsycasts_PsyFocusCost".Translate(), currentList[1]);
                    psycastStats[keysStats[num]][2] = TextFieldNumericLabeled(ref rect4, "MorePsycasts_Duration".Translate(), currentList[2]);
                    if (keysStats[num] == PsycastDefOf.MorePsycasts_StabilizingTouch.defName)
                        stabilizing_touch_bleed_factor = TextFieldNumericLabeled(ref rect4, "MorePsycasts_BleedFactor".Translate(), (float)stabilizing_touch_bleed_factor);
                    if (keysStats[num] == PsycastDefOf.MorePsycasts_HealingTouch.defName)
                    {
                        healing_touch_natural_healing_factor = TextFieldNumericLabeled(ref rect4, "MorePsycasts_NaturalHealingFactor".Translate(), (float)healing_touch_natural_healing_factor);
                        healing_touch_immunity_gain_speed_factor = TextFieldNumericLabeled(ref rect4, "MorePsycasts_ImmunityGainSpeedFactor".Translate(), (float)healing_touch_immunity_gain_speed_factor);
                    }

                    if (keysStats[num] == PsycastDefOf.MorePsycasts_HealScars.defName)
                        heal_scars_healing_speed = TextFieldNumericLabeled(ref rect4, "MorePsycasts_HealingSpeedRelative".Translate(), (float)heal_scars_healing_speed);
                    if (keysStats[num] == PsycastDefOf.MorePsycasts_RegrowBodyParts.defName)
                        regrow_body_parts_healing_speed = TextFieldNumericLabeled(ref rect4, "MorePsycasts_HealingSpeedRelative".Translate(), (float)regrow_body_parts_healing_speed);
                    if (keysStats[num] == PsycastDefOf.MorePsycasts_FlashHeal.defName)
                    {
                        flash_heal_heal_amount = TextFieldNumericLabeled(ref rect4, "MorePsycasts_HealAmount".Translate(), (float)flash_heal_heal_amount);
                        flash_heal_scar_chance = TextFieldNumericLabeled(ref rect4, "MorePsycasts_ScarChance".Translate(), (float)flash_heal_scar_chance);
                    }

                    if (keysStats[num] == PsycastDefOf.MorePsycasts_RevivingTouch.defName)
                    {
                        reviving_touch_min_proportial_damage = TextFieldNumericLabeled(ref rect4, "MorePsycasts_MinProportionalDamage".Translate(), (float)reviving_touch_min_proportial_damage);
                        reviving_touch_max_proportial_damage = TextFieldNumericLabeled(ref rect4, "MorePsycasts_MaxProportionalDamage".Translate(), (float)reviving_touch_max_proportial_damage);
                        psychic_ressurection_severity_per_day = TextFieldNumericLabeled(ref rect4, "MorePsycasts_SeverityPerDay".Translate(), (float)psychic_ressurection_severity_per_day);
                    }
                }
            }

            Widgets.DrawLineHorizontal(0f, rect4.y * 32f, rect4.width);
            Widgets.EndScrollView();
            GUI.EndGroup();

            var rect5 = rect2.RightHalf();
            GUI.BeginGroup(rect5, new GUIStyle(GUI.skin.box));
            var rect6 = new Rect(0f, 0f, rect5.width - 20f, keysStates.Count * 24);
            Widgets.BeginScrollView(rect5.AtZero(), ref scrollPosition2, rect6);
            for (var num = keysStates.Count - 1; num >= 0; num--)
            {
                var test = psycastStates[keysStates[num]];
                Widgets.CheckboxLabeled(getDrawRect(ref rect6), keysStates[num], ref test);
                psycastStates[keysStates[num]] = test;
            }

            Widgets.EndScrollView();
            GUI.EndGroup();

            x = 0;
            Write();
        }

        private static Vector2 scrollPosition1 = Vector2.zero;
        private static Vector2 scrollPosition2 = Vector2.zero;

        private static List<string> editables = new List<string>();
        private static int x = 0;

        private static float TextFieldNumericLabeled(ref Rect rect, string label, float val)
        {
            if (editables.Count >= x) editables.Add(null);
            var s = editables[x];
            Widgets.TextFieldNumericLabeled<float>(getDrawRect(ref rect), label, ref val, ref s, -1f);
            editables[x++] = s;
            return val;
        }

        private static Rect getDrawRect(ref Rect rect)
        {
            return new Rect(0f, 32f * rect.y++, rect.width, 30f);
        }
    }

    public class MorePsycasts_Mod : Mod
    {
        public static MorePsycasts_Settings settings;

        public MorePsycasts_Mod(ModContentPack content) : base(content)
        {
            settings = GetSettings<MorePsycasts_Settings>();
        }

        public override void DoSettingsWindowContents(Rect inRect)
        {
            base.DoSettingsWindowContents(inRect);
            var psycasts = DefDatabase<AbilityDef>.AllDefsListForReading;
            foreach (var psycast in psycasts)
                if (typeof(Psycast).IsAssignableFrom(psycast.abilityClass))
                {
                    if (settings.psycastStates == null) settings.psycastStates = new Dictionary<string, bool>();
                    if (!settings.psycastStates.ContainsKey(psycast.defName)) settings.psycastStates[psycast.defName] = true;
                }
                else
                {
                    if (settings.psycastStates.ContainsKey(psycast.defName))
                        settings.psycastStates.Remove(psycast.defName);
                }

            var morePsycasts_psycasts = typeof(PsycastDefOf).GetFields();
            foreach (var psycast in morePsycasts_psycasts)
            {
                if (settings.psycastStats == null) settings.psycastStats = new Dictionary<string, List<float>>();
                var current = DefDatabase<AbilityDef>.GetNamed(psycast.Name);
                if (!settings.psycastStats.ContainsKey(current.defName))
                {
                    var currentList = new List<float>();
                    currentList.Add(current.statBases.GetStatValueFromList(StatDefOf.Ability_EntropyGain, 0));
                    currentList.Add(current.statBases.GetStatValueFromList(StatDefOf.Ability_PsyfocusCost, 0));
                    currentList.Add(current.statBases.GetStatValueFromList(StatDefOf.Ability_Duration, 0));
                    settings.psycastStats[current.defName] = currentList;
                }
            }

            settings.DoSettingsWindowContents(inRect);
        }

        public override string SettingsCategory()
        {
            return "More Psycasts";
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            DefsRemover.DoDefsRemoval();
        }
    }

    [StaticConstructorOnStartup]
    internal static class DefsRemover
    {
        static DefsRemover()
        {
            DoDefsRemoval();
        }

        public static void RemoveDef(AbilityDef def)
        {
            try
            {
                if (DefDatabase<AbilityDef>.AllDefsListForReading.Contains(def)) DefDatabase<AbilityDef>.AllDefsListForReading.Remove(def);
            }
            catch
            {
            }

            ;
        }

        public static void DoDefsRemoval()
        {
            foreach (var psycastState in MorePsycasts_Mod.settings.psycastStates)
                if (!psycastState.Value)
                {
                    var defToRemove = DefDatabase<AbilityDef>.GetNamedSilentFail(psycastState.Key);
                    if (defToRemove != null) RemoveDef(defToRemove);
                }
        }
    }

    [StaticConstructorOnStartup]
    internal static class SettingsImplementerExecutorInAConstructor
    {
        static SettingsImplementerExecutorInAConstructor()
        {
            applyChanges();
        }

        public static void applyChanges()
        {
            var psycastStats = MorePsycasts_Mod.settings.psycastStats;
            foreach (var key in psycastStats.Keys)
            {
                StatUtility.SetStatValueInList(ref DefDatabase<AbilityDef>.GetNamed(key).statBases, StatDefOf.Ability_EntropyGain, psycastStats[key][0]);
                StatUtility.SetStatValueInList(ref DefDatabase<AbilityDef>.GetNamed(key).statBases, StatDefOf.Ability_PsyfocusCost, psycastStats[key][1]);
                StatUtility.SetStatValueInList(ref DefDatabase<AbilityDef>.GetNamed(key).statBases, StatDefOf.Ability_Duration, psycastStats[key][2]);
            }

            exchange(ref MorePsycasts_Mod.settings.debuff_hunger, ref HediffDefOf.MorePsycasts_PsychicInducedHunger.stages[0].hungerRateFactorOffset);
            exchange(ref MorePsycasts_Mod.settings.debuff_rest, ref HediffDefOf.MorePsycasts_PsychicInducedHunger.stages[0].restFallFactorOffset);
            exchange(ref MorePsycasts_Mod.settings.debuff_pain, ref HediffDefOf.MorePsycasts_PsychicInducedHunger.stages[0].painOffset);
            exchange(ref MorePsycasts_Mod.settings.stabilizing_touch_bleed_factor, ref HediffDefOf.MorePsycasts_StabilizingTouch.stages[0].totalBleedFactor);
            exchange(ref MorePsycasts_Mod.settings.healing_touch_natural_healing_factor, ref HediffDefOf.MorePsycasts_AcceleratedHealing.stages[0].naturalHealingFactor);
            if (MorePsycasts_Mod.settings.healing_touch_immunity_gain_speed_factor is null)
                MorePsycasts_Mod.settings.healing_touch_immunity_gain_speed_factor = HediffDefOf.MorePsycasts_AcceleratedHealing.stages[0].statFactors.GetStatFactorFromList(StatDefOf.ImmunityGainSpeedFactor);
            else
                StatUtility.SetStatValueInList(ref HediffDefOf.MorePsycasts_AcceleratedHealing.stages[0].statFactors, StatDefOf.ImmunityGainSpeedFactor,
                    (float)MorePsycasts_Mod.settings.healing_touch_immunity_gain_speed_factor);
            if (MorePsycasts_Mod.settings.psychic_ressurection_severity_per_day is null)
                MorePsycasts_Mod.settings.psychic_ressurection_severity_per_day = -((HediffCompProperties_SeverityPerDay)HediffDefOf.MorePsycasts_PsychicRessurection.comps[0]).severityPerDay;
            else
                ((HediffCompProperties_SeverityPerDay)HediffDefOf.MorePsycasts_PsychicRessurection.comps[0]).severityPerDay = -(float)MorePsycasts_Mod.settings.psychic_ressurection_severity_per_day;
        }

        private static void exchange(ref float? nullable, ref float def)
        {
            if (nullable is null)
                nullable = def;
            else
                def = (float)nullable;
        }
    }

    /*public class HediffStacks : Hediff
    {
        public List<HediffStacks> stacksList = new List<HediffStacks>();
        public int duration = 0;
        public int stacks = 0;
        public Hediff link;
        public bool done = false;
        public override HediffStage CurStage
        {
            get
            {
                HediffStage result = new HediffStage();
                result.label = Severity + (Severity == 1 ? " stack" : " stacks");
                result.hungerRateFactorOffset = Severity * def.stages[0].hungerRateFactorOffset;
                result.restFallFactorOffset = Severity * def.stages[0].restFallFactorOffset;
                result.painOffset = Severity * def.stages[0].painOffset;
                return result;
            }
        }

        public override int CurStageIndex => (int)severityInt;

        public override void Tick()
        {
            base.Tick();
            if (!done && (duration <= ageTicks||(link!=null && link.ShouldRemove)))
            {
                Severity -= stacks;
                done = true;
            }
            for (int i = stacksList.Count - 1; i >= 0; i--)
            {
                HediffStacks stack = stacksList[i];
                stack.Tick();
                if (stack.ShouldRemove)
                {
                    Severity -= stack.stacks;
                    stacksList.RemoveAt(i);
                }
            }
        }
        public override bool TryMergeWith(Hediff other)
        {
            if (other == null || other.def != def || other.Part != Part)
            {
                return false;
            }
            HediffStacks addition = (HediffStacks) other;
            stacksList.Add(addition);
            Severity += addition.stacks;
            return true;
        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref stacksList, "MorePsycasts_stacksList", LookMode.Deep);
            Scribe_Values.Look(ref duration, "MorePsycasts_duration");
            Scribe_Values.Look(ref stacks, "MorePsycasts_stacks");
            Scribe_Deep.Look(ref link, "MorePsycasts_link");
            Scribe_Values.Look(ref done, "MorePsycasts_done");
        }
    }
    public class CompProperties_MorePsycasts_AbilityGiveHediffWithStacks : CompProperties_AbilityGiveHediff
    {
        public HediffDef hediffDef2;
        public int stacks = 1;
    }
    public class CompAbilityEffect_MorePsycasts_GiveHediffWithStacks : CompAbilityEffect_GiveHediff
    {
        public new CompProperties_MorePsycasts_AbilityGiveHediffWithStacks Props => (CompProperties_MorePsycasts_AbilityGiveHediffWithStacks)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Pawn target2 = Props.applyToSelf ? parent.pawn : target.Pawn;
            if (target2 == null){return;}
            Hediff hediff = Props.hediffDef != null ? HediffMaker.MakeHediff(Props.hediffDef, target2, Props.onlyBrain ? target2.health.hediffSet.GetBrain() : null) : null;
            if (hediff != null)
            {
                HediffComp_Disappears hediffComp_Disappears = hediff.TryGetComp<HediffComp_Disappears>();
                if (hediffComp_Disappears != null)
                {
                    hediffComp_Disappears.ticksToDisappear = GetDurationSeconds(target2).SecondsToTicks();
                }
                target2.health.AddHediff(hediff);
            }

            Hediff hediff2 = HediffMaker.MakeHediff(Props.hediffDef2, target2, Props.onlyBrain ? target2.health.hediffSet.GetBrain() : null);
            hediff2.Severity = Props.stacks;
            if (!(hediff2 is HediffStacks))
                return;
            ((HediffStacks)hediff2).stacks = Props.stacks;
            ((HediffStacks)hediff2).duration = GetDurationSeconds(target2).SecondsToTicks();
            ((HediffStacks)hediff2).link = hediff;
            target2.health.AddHediff(hediff2);
        }

    }*/
    public class CompAbilityEffect_MorePsycasts_FlashHeal : CompAbilityEffect
    {
        public new CompProperties_AbilityEffect Props => (CompProperties_AbilityEffect)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Props.sound = SoundDefOf.PsycastPsychicEffect; //SoundDefOf.PsycastPsychicPulse;
            base.Apply(target, dest);

            if (target.Pawn == null) return;
            for (var i = 0; i < 10 * MorePsycasts_Mod.settings.flash_heal_heal_amount; i++)
            {
                var hediff_Injury = FindInjury(target.Pawn);
                if (hediff_Injury != null)
                {
                    var hediffWithComps = hediff_Injury as HediffWithComps;
                    var getsPermanent = hediffWithComps.TryGetComp<HediffComp_GetsPermanent>();
                    hediff_Injury.Heal(0.1f);
                    getsPermanent.Props.becomePermanentChanceFactor *= (float)MorePsycasts_Mod.settings.flash_heal_scar_chance;
                }
            }
            //SoundDefOf.PsycastPsychicEffect.PlayOneShot(new TargetInfo(target.Cell, parent.pawn.Map));
        }

        private Hediff_Injury FindInjury(Pawn pawn)
        {
            if (!pawn.Dead)
            {
                var hediffs = new List<Hediff_Injury>();
                pawn.health.hediffSet.GetHediffs<Hediff_Injury>(ref hediffs);
                Func<Hediff_Injury, bool> predicate = (Hediff_Injury injury) => injury != null && injury.Visible && injury.def.everCurableByItem && !injury.IsPermanent() && injury.CanHealNaturally() &&
                                                                                injury is HediffWithComps && ((HediffWithComps)injury).TryGetComp<HediffComp_GetsPermanent>() != null;
                var injuryList = hediffs.Where(predicate);
                if (injuryList.Count() == 0) return null;
                return injuryList.ElementAt(Rand.Range(0, injuryList.Count()));
            }

            return null;
        }
    }

    public class CompAbilityEffect_MorePsycasts_RevivingTouch : CompAbilityEffect
    {
        public new CompProperties_AbilityEffect Props => (CompProperties_AbilityEffect)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            var psychicBreakdown = HediffMaker.MakeHediff(DefDatabase<HediffDef>.GetNamed("PsychicBreakdown"), parent.pawn);
            psychicBreakdown.Severity = 15;
            parent.pawn.health.AddHediff(psychicBreakdown);
            if (target.Thing == null || !(target.Thing is Corpse)) return;
            var pawn = ((Corpse)target.Thing).InnerPawn;
            var x2 = pawn.Corpse == null ? 0f : pawn.Corpse.GetComp<CompRottable>().RotProgress / 60000f;
            var daysToDessicated = pawn.Corpse == null ? 0f : pawn.Corpse.GetComp<CompRottable>().PropsRot.daysToDessicated;
            ResurrectionUtility.Resurrect(pawn);

            var psychicRessurection = HediffMaker.MakeHediff(DefDatabase<HediffDef>.GetNamed("MorePsycasts_PsychicRessurection"), pawn);
            if (!pawn.health.WouldDieAfterAddingHediff(psychicRessurection)) pawn.health.AddHediff(psychicRessurection);
            var brain = pawn.health.hediffSet.GetBrain();
            if (Rand.Chance(Utilities.DementiaChancePerRotDaysCurve.Evaluate(x2)) && brain != null)
            {
                var hediff2 = HediffMaker.MakeHediff(RimWorld.HediffDefOf.Dementia, pawn, brain);
                if (!pawn.health.WouldDieAfterAddingHediff(hediff2)) pawn.health.AddHediff(hediff2);
            }

            if (Rand.Chance(Utilities.BlindnessChancePerRotDaysCurve.Evaluate(x2)))
                foreach (var item in from x in pawn.health.hediffSet.GetNotMissingParts()
                         where x.def == BodyPartDefOf.Eye
                         select x)
                    if (!pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(item))
                    {
                        var hediff3 = HediffMaker.MakeHediff(RimWorld.HediffDefOf.Blindness, pawn, item);
                        pawn.health.AddHediff(hediff3);
                    }

            if (brain != null && Rand.Chance(Utilities.ResurrectionPsychosisChancePerRotDaysCurve.Evaluate(x2)))
            {
                var hediff4 = HediffMaker.MakeHediff(RimWorld.HediffDefOf.ResurrectionPsychosis, pawn, brain);
                if (!pawn.health.WouldDieAfterAddingHediff(hediff4)) pawn.health.AddHediff(hediff4);
            }

            if (true) //!ModLister.HasActiveModWithName("Pawns Just Don't Die"))
            {
                x2 /= daysToDessicated;
                var parts1 = pawn.health.hediffSet.GetNotMissingParts();
                var parts = parts1.ToList();
                parts.Shuffle();
                var current = 0;
                while (current < parts.Count())
                {
                    var part = parts.ElementAt(current++);

                    if (part.coverage > 0)
                    {
                        var hediff = HediffMaker.MakeHediff(DefDatabase<HediffDef>.GetNamed("MorePsycasts_PsychicBurn"), pawn, part);
                        hediff.Severity = Rand.Value * (pawn.health.hediffSet.GetPartHealth(part) * x2 * 2);
                        if (!pawn.health.hediffSet.PartIsMissing(part) && !pawn.health.WouldDieAfterAddingHediff(hediff) && (part.def != BodyPartDefOf.Head || hediff.Severity < pawn.health.hediffSet.GetPartHealth(part)))
                            pawn.health.AddHediff(hediff);
                    }
                }
            }

            if (pawn.Dead)
            {
                Log.Error("The pawn has died while being resurrected.");
                return;
            }

            Messages.Message("MessagePawnResurrected".Translate(pawn), pawn, MessageTypeDefOf.PositiveEvent);
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return target.Thing is Corpse corpse && corpse.GetRotStage() != RotStage.Dessicated;
        }
    }

    public class CompAbilityEffect_Ignite : CompAbilityEffect
    {
        public new CompProperties_AbilityEffect Props => (CompProperties_AbilityEffect)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);

            if (target == null)
            {
                Log.Message("Tried to apply ignite to nothing.");
                return;
            }

            FireUtility.TryStartFireIn(target.Cell, parent.pawn.Map, 0.1f);
        }

        public override bool CanApplyOn(LocalTargetInfo target, LocalTargetInfo dest)
        {
            return FireUtility.ChanceToStartFireIn(target.Cell, parent.pawn.Map) > 0f;
        }
    }

    public class CompAbilityEffect_CleanSkip : CompAbilityEffect
    {
        public new CompProperties_AbilityEffect Props => (CompProperties_AbilityEffect)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            var map = parent.pawn.Map;
            foreach (var item in Utilities.AffectedCells(target, map, parent))
            {
                var thingList = item.GetThingList(map);
                for (var i = 0; i < thingList.Count; i++)
                {
                    var filth = thingList[i] as Filth;
                    if (filth != null)
                    {
                        filth.Destroy();
                        SoundDefOf.Psycast_Skip_Exit.PlayOneShot(new TargetInfo(item, map));
                        Utilities.SpawnFleck(new LocalTargetInfo(item), FleckDefOf.PsycastSkipInnerExit, map);
                        Utilities.SpawnFleck(new LocalTargetInfo(item), FleckDefOf.PsycastSkipOuterRingExit, map);
                        Utilities.SpawnEffecter(new LocalTargetInfo(item), EffecterDefOf.Skip_Exit, map, 60, parent);
                        //Mote mote = MoteMaker.MakeStaticMote(item.ToVector3Shifted(), map, ThingDefOf.Mote_WaterskipSplashParticles);
                    }
                }
            }
        }
    }

    public class CompAbilityEffect_HaulSkip : CompAbilityEffect
    {
        public new CompProperties_AbilityEffect Props => (CompProperties_AbilityEffect)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            var map = parent.pawn.Map;
            foreach (var item in Utilities.AffectedCells(target, map, parent))
            {
                var thingList = item.GetThingList(map);
                for (var i = 0; i < thingList.Count; i++)
                {
                    var thing = thingList[i];
                    var max = thing.stackCount;
                    for (var j = 0; j < max; j++)
                        if (thing.def.EverHaulable && !thing.IsForbidden(parent.pawn) && HaulAIUtility.PawnCanAutomaticallyHaulFast(parent.pawn, thing, false))
                        {
                            IntVec3 foundCell;
                            if (StoreUtility.TryFindBestBetterStoreCellFor(thing, parent.pawn, parent.pawn.Map, StoreUtility.CurrentStoragePriorityOf(thing), parent.pawn.Faction, out foundCell))
                            {
                                //Mote mote = MoteMaker.MakeStaticMote(item.ToVector3Shifted(), map, ThingDefOf.Mote_WaterskipSplashParticles);
                                SoundDefOf.Psycast_Skip_Exit.PlayOneShot(new TargetInfo(item, map));
                                Utilities.SpawnFleck(new LocalTargetInfo(item), FleckDefOf.PsycastSkipInnerExit, map);
                                Utilities.SpawnFleck(new LocalTargetInfo(item), FleckDefOf.PsycastSkipOuterRingExit, map);
                                Utilities.SpawnEffecter(new LocalTargetInfo(item), EffecterDefOf.Skip_Exit, map, 60, parent);

                                var count = GridsUtility.GetItemStackSpaceLeftFor(foundCell, parent.pawn.Map, thing.def);
                                count = Math.Min(count, thing.stackCount);
                                //ThingOwner innerContainer = parent.pawn.Map.thingGrid.ThingAt(foundCell, ThingCategory.Building).holdingOwner;
                                Thing resultingThing;
                                var thing2 = thing.SplitOff(count);
                                GenDrop.TryDropSpawn(thing2, foundCell, parent.pawn.Map, ThingPlaceMode.Direct, out resultingThing, playDropSound: false);

                                //thing.stackCount -= count;

                                //if (thing.stackCount == 0)
                                //	thing.DeSpawn();

                                /*Thing otherThing = parent.pawn.Map.thingGrid.ThingAt(foundCell, thing.def);
                                if (otherThing != null)
                                {
                                    int count = thing.def.stackLimit;
                                    count -= otherThing.stackCount;
                                    count = Math.Min(count, thing.stackCount);
                                    otherThing.stackCount += count;
                                    thing.stackCount -= count;
                                    if (thing.stackCount == 0)
                                        thing.DeSpawn();
                                }
                                else
                                {
                                    thing.Position = foundCell;
                                }*/
                                //Mote mote2 = MoteMaker.MakeStaticMote(foundCell.ToVector3Shifted(), map, ThingDefOf.Mote_WaterskipSplashParticles);
                                SoundDefOf.Psycast_Skip_Exit.PlayOneShot(new TargetInfo(foundCell, map));
                                Utilities.SpawnFleck(new LocalTargetInfo(foundCell), FleckDefOf.PsycastSkipInnerExit, map);
                                Utilities.SpawnFleck(new LocalTargetInfo(foundCell), FleckDefOf.PsycastSkipOuterRingExit, map);
                                Utilities.SpawnEffecter(new LocalTargetInfo(foundCell), EffecterDefOf.Skip_Exit, map, 60, parent);
                            }
                            else
                            {
                                break;
                            }
                        }
                }
            }
        }
    }

    public class CompAbilityEffect_Mending : CompAbilityEffect
    {
        public new CompProperties_AbilityEffect Props => (CompProperties_AbilityEffect)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            var map = parent.pawn.Map;
            Thing thing = null;
            if (target.Thing != null)
                thing = target.Thing;
            else
                thing = target.Cell.GetThingList(map).RandomElement<Thing>();
            var toHeal = Math.Min(thing.MaxHitPoints - thing.HitPoints, 300);
            var fraction = (float)toHeal / thing.MaxHitPoints;
            var quality = thing.TryGetComp<CompQuality>();
            if (quality != null && quality.Quality > 0 && Rand.Chance(fraction)) quality.SetQuality(quality.Quality - 1, ArtGenerationContext.Colony);
            thing.HitPoints += toHeal;
            SoundDefOf.Psycast_Skip_Exit.PlayOneShot(new TargetInfo(target.Cell, map));
            Utilities.SpawnFleck(target, FleckDefOf.PsycastSkipInnerExit, map);
            Utilities.SpawnFleck(target, FleckDefOf.PsycastSkipOuterRingExit, map);
            Utilities.SpawnEffecter(target, EffecterDefOf.Skip_Exit, map, 60, parent);
        }
    }

    public class CompAbilityEffect_FertilitySkip : CompAbilityEffect
    {
        public new CompProperties_AbilityEffect Props => (CompProperties_AbilityEffect)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            base.Apply(target, dest);
            var map = parent.pawn.Map;
            foreach (var location in Utilities.AffectedCells(target, map, parent))
            {
                var terrain = map.terrainGrid.TerrainAt(location);
                if (terrain.IsRiver) continue;
                if (terrain.IsWater)
                {
                    var mud = DefDatabase<TerrainDef>.GetNamed("Mud");
                    if (mud != null)
                        map.terrainGrid.SetTerrain(location, mud);
                    else
                        map.terrainGrid.SetTerrain(location, TerrainDefOf.Sand);
                }
                else
                {
                    var terrains = new List<TerrainDef>();
                    terrains.Add(TerrainDefOf.Gravel);
                    terrains.Add(TerrainDefOf.Soil);
                    foreach (var t in map.Biome.terrainsByFertility)
                        if (!terrains.Contains(t.terrain))
                            terrains.Add(t.terrain);
                    foreach (var p in map.Biome.terrainPatchMakers)
                    foreach (var t in p.thresholds)
                        if (!terrains.Contains(t.terrain))
                            terrains.Add(t.terrain);
                    var sorted = terrains.FindAll(e => e.fertility > terrain.fertility && e.fertility <= 1).OrderBy(e => e.fertility);
                    if (sorted.Count() == 0) continue;
                    var newTerrain = sorted.First();
                    map.terrainGrid.SetTerrain(location, newTerrain);
                }
            }
        }
    }

    public class HediffComp_PsychicReverberationsScars : HediffComp
    {
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            var hediff_Injury = Utilities.FindPermanentInjury(Pawn);
            if (hediff_Injury == null) severityAdjustment = -1;

            if (Pawn.IsHashIntervalTick(600))
                hediff_Injury.Heal((float)MorePsycasts_Mod.settings.heal_scars_healing_speed * Utilities.getHealingAmount(Pawn));
        }
    }

    public class HediffComp_PsychicReverberationsBodyParts : HediffComp
    {
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);
            var hediff_Injury = Utilities.FindHealablePart(Pawn);
            if (hediff_Injury == null)
            {
                severityAdjustment = -1;
                return;
            }

            var healingAmount = (float)MorePsycasts_Mod.settings.regrow_body_parts_healing_speed * Utilities.getHealingAmount(Pawn);

            if (Pawn.IsHashIntervalTick(600))
            {
                if (hediff_Injury.def.defName == "MorePsycasts_PartiallyGrown")
                {
                    hediff_Injury.Heal(healingAmount);
                }
                else
                {
                    if (Rand.Chance(healingAmount))
                    {
                        Pawn.health.hediffSet.hediffs.Remove(hediff_Injury);
                        var hediff = HediffMaker.MakeHediff(DefDatabase<HediffDef>.GetNamed("MorePsycasts_PartiallyGrown"), Pawn, hediff_Injury.Part);
                        hediff.Severity = hediff_Injury.Part.def.GetMaxHealth(Pawn) - 1;
                        hediff.TryGetComp<HediffComp_GetsPermanent>().IsPermanent = true;
                        Pawn.health.AddHediff(hediff);
                    }
                }
            }
        }
    }

    public class CompAbilityEffect_CureDisease : CompAbilityEffect
    {
        public new CompProperties_AbilityEffect Props => (CompProperties_AbilityEffect)props;

        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            Props.sound = SoundDefOf.PsycastPsychicEffect;
            base.Apply(target, dest);

            if (target.Pawn == null) return;

            var HediffWithComps = Utilities.FindDisease(target.Pawn);

            if (HediffWithComps == null) return;

            var tendComp = HediffWithComps.TryGetComp<HediffComp_TendDuration>();
            var part = HediffWithComps.Part;
            if (part == null)
            {
                for (var i = 0; i < 50; i++)
                {
                    part = target.Pawn.health.hediffSet.GetRandomNotMissingPart(DefDatabase<DamageDef>.GetNamed("SurgicalCut"));
                    if (!part.def.IsSolid(null, null))
                    {
                        var hediff = HediffMaker.MakeHediff(DefDatabase<HediffDef>.GetNamed("MorePsycasts_PsychicBurn"), target.Pawn, part);
                        hediff.Severity = Rand.Value;
                        if (Rand.Value < 0.95)
                            hediff.Tended(1, 2);
                        target.Pawn.health.AddHediff(hediff);
                    }
                }
            }
            else
            {
                var hediff = HediffMaker.MakeHediff(DefDatabase<HediffDef>.GetNamed("MorePsycasts_PsychicBurn"), target.Pawn, HediffWithComps.Part);
                hediff.Severity = 0.5f * Rand.Value * target.Pawn.health.hediffSet.GetPartHealth(HediffWithComps.Part);
                target.Pawn.health.AddHediff(hediff);
            }

            target.Pawn.health.RemoveHediff(HediffWithComps);
        }
    }

    public class HediffComp_ReverseAging : HediffComp
    {
        public override void CompPostTick(ref float severityAdjustment)
        {
            base.CompPostTick(ref severityAdjustment);

            if (Pawn == null) return;

            if (Pawn.IsHashIntervalTick(GenDate.TicksPerDay)) //once per day
            {
                var pawn = Pawn;

                var num = (int)(4 * GenDate.TicksPerDay * pawn.ageTracker.AdultAgingMultiplier); //4 days
                var val = (long)(GenDate.TicksPerYear * pawn.ageTracker.AdultMinAge); //code from Rimworld age reversal
                var oldAge = pawn.ageTracker.AgeBiologicalYears;
                pawn.ageTracker.AgeBiologicalTicks = Math.Max(val, pawn.ageTracker.AgeBiologicalTicks - num);
                var newAge = pawn.ageTracker.AgeBiologicalYears;
                pawn.ageTracker.ResetAgeReversalDemand(Pawn_AgeTracker.AgeReversalReason.ViaTreatment); //reset age reversal demand

                if (newAge < oldAge) // tries to remove each chronic illness coming from old age
                    foreach (Hediff hediff in Utilities.AllChronic(Pawn))
                    {
                        HediffGiver responsibleHediffGiver = null;
                        foreach (var hediffGiverSet in pawn.RaceProps.hediffGiverSets)
                        {
                            foreach (var hediffGiver in hediffGiverSet.hediffGivers)
                                if (hediffGiver is HediffGiver_Birthday && hediffGiver.hediff == hediff.def)
                                {
                                    responsibleHediffGiver = hediffGiver;
                                    break;
                                }

                            if (responsibleHediffGiver != null)
                                break;
                        }

                        var birthday = (HediffGiver_Birthday)responsibleHediffGiver;

                        var chance = 0f;
                        var notHappenedYet = 1f;
                        var sum = 0f;
                        for (var i = 1; i <= oldAge; i++)
                        {
                            notHappenedYet *= 1f - chance;
                            var x = (float)i / pawn.RaceProps.lifeExpectancy;
                            chance = birthday.ageFractionChanceCurve.Evaluate(x);
                            sum += notHappenedYet * chance;
                        }

                        if (sum == 0 || Rand.Value < notHappenedYet * chance / sum) Pawn.health.RemoveHediff(hediff);
                    }
            }
        }
    }

    public class Utilities
    {
        public static Hediff FindHealablePart(Pawn pawn)
        {
            if (!pawn.Dead)
            {
                var hediffs = new List<Hediff_Injury>();
                pawn.health.hediffSet.GetHediffs<Hediff_Injury>(ref hediffs);
                Func<Hediff_Injury, bool> predicate = (Hediff_Injury injury) => injury != null && injury.def.defName == "MorePsycasts_PartiallyGrown";
                var injuryList = hediffs.Where(predicate);
                if (injuryList.Count() != 0) return injuryList.ElementAt(Rand.Range(0, injuryList.Count()));

                IEnumerable<Hediff_MissingPart> hediffs2 = pawn.health.hediffSet.GetMissingPartsCommonAncestors();
                Func<Hediff_MissingPart, bool> predicate2 = (Hediff_MissingPart injury) => injury != null && !pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(injury.Part);
                var injuryList2 = hediffs2.Where(predicate2);
                if (injuryList2.Count() != 0) return injuryList2.ElementAt(Rand.Range(0, injuryList2.Count()));
            }

            return null;
        }

        public static Hediff_Injury FindPermanentInjury(Pawn pawn, IEnumerable<BodyPartRecord> allowedBodyParts = null)
        {
            if (!pawn.Dead)
            {
                var hediffs = new List<Hediff_Injury>();
                pawn.health.hediffSet.GetHediffs<Hediff_Injury>(ref hediffs);
                Func<Hediff_Injury, bool> predicate = (Hediff_Injury injury) => injury != null && injury.Visible && injury.def.everCurableByItem && injury.IsPermanent();
                var injuryList = hediffs.Where(predicate);
                if (injuryList.Count() == 0) return null;
                return injuryList.ElementAt(Rand.Range(0, injuryList.Count()));
            }

            return null;
        }

        public static Hediff FindDisease(Pawn pawn)
        {
            if (!pawn.Dead)
            {
                var hediffs = new List<HediffWithComps>();
                pawn.health.hediffSet.GetHediffs<HediffWithComps>(ref hediffs);
                Func<HediffWithComps, bool> predicate = (HediffWithComps injury) => injury != null && (injury.TryGetComp<HediffComp_Immunizable>() != null ||
                                                                                                       (injury.TryGetComp<HediffComp_TendDuration>() != null &&
                                                                                                        injury.TryGetComp<HediffComp_TendDuration>().TProps.disappearsAtTotalTendQuality > 0));
                var injuryList = hediffs.Where(predicate);
                if (injuryList.Count() != 0) return injuryList.ElementAt(Rand.Range(0, injuryList.Count()));
            }

            return null;
        }

        public static IEnumerable<HediffWithComps> AllChronic(Pawn pawn)
        {
            if (!pawn.Dead)
            {
                var hediffs = new List<HediffWithComps>();
                pawn.health.hediffSet.GetHediffs<HediffWithComps>(ref hediffs);
                Func<HediffWithComps, bool> predicate = (HediffWithComps injury) => injury != null && injury.def.chronic;
                var injuryList = hediffs.Where(predicate);
                return injuryList;
            }

            return null;
        }

        public static float getHealingAmount(Pawn pawn)
        {
            var num = 8f;
            if (pawn.GetPosture() != 0)
            {
                num += 4f;
                var building_Bed = pawn.CurrentBed();
                if (building_Bed != null) num += building_Bed.def.building.bed_healPerDay;
            }

            foreach (var hediff3 in pawn.health.hediffSet.hediffs)
            {
                var curStage = hediff3.CurStage;
                if (curStage != null && curStage.naturalHealingFactor != -1f) num *= curStage.naturalHealingFactor;
            }

            return num * pawn.HealthScale * 0.01f;
        }

        public static SimpleCurve DementiaChancePerRotDaysCurve = new SimpleCurve
        {
            new CurvePoint(0.1f, 0.02f),
            new CurvePoint(5f, 0.8f)
        };

        public static SimpleCurve BlindnessChancePerRotDaysCurve = new SimpleCurve
        {
            new CurvePoint(0.1f, 0.02f),
            new CurvePoint(5f, 0.8f)
        };

        public static SimpleCurve ResurrectionPsychosisChancePerRotDaysCurve = new SimpleCurve
        {
            new CurvePoint(0.1f, 0.02f),
            new CurvePoint(5f, 0.8f)
        };

        public static IEnumerable<IntVec3> AffectedCells(LocalTargetInfo target, Map map, Ability parent)
        {
            if (target.Cell.Filled(parent.pawn.Map)) yield break;
            foreach (var item in GenRadial.RadialCellsAround(target.Cell, parent.def.EffectRadius, true))
                if (item.InBounds(map) && GenSight.LineOfSightToEdges(target.Cell, item, map, true))
                    yield return item;
        }

        public static bool CheckForMod(string s)
        {
            foreach (var d in ModsConfig.ActiveModsInLoadOrder)
                if (d.PackageId.EqualsIgnoreCase(s))
                    return true;
            return false;
        }

        public static void SpawnFleck(LocalTargetInfo target, FleckDef def, Map map)
        {
            if (target.HasThing)
                FleckMaker.AttachedOverlay(target.Thing, def, Vector3.zero, 1f);
            else
                FleckMaker.Static(target.Cell, map, def, 1f);
        }

        public static void SpawnEffecter(LocalTargetInfo target, EffecterDef def, Map map, int maintainForTicks, Ability parent)
        {
            Effecter effecter = null;
            effecter = !target.HasThing ? def.Spawn(target.Cell, map, 1f) : def.Spawn(target.Thing, map, 1f);
            parent.AddEffecterToMaintain(effecter, target.Cell, maintainForTicks);
        }
    }
}