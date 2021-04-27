using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

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
			if (reset&&Scribe.mode!=LoadSaveMode.Saving){return;}
			Scribe_Collections.Look(ref psycastStates, "MorePsycasts_psycastStates", LookMode.Value, LookMode.Value, ref psycastKeys, ref boolValues);
			List<string> defNames = psycastStats.Keys.ToList();
			Scribe_Collections.Look(ref defNames, "MorePsycasts_defNames", LookMode.Value);
			List<float> entropies = psycastStats.Select(d => d.Value[0]).ToList();
			Scribe_Collections.Look(ref entropies, "MorePsycasts_entropies", LookMode.Value);
			List<float> psyfocus = psycastStats.Select(d => d.Value[1]).ToList();
			Scribe_Collections.Look(ref psyfocus, "MorePsycasts_psyfocus", LookMode.Value);
			List<float> duration = psycastStats.Select(d => d.Value[2]).ToList();
			Scribe_Collections.Look(ref duration, "MorePsycasts_duration", LookMode.Value);
			if (defNames != null)
			{
				for (int i = 0; i < defNames.Count; i++)
				{
					List<float> currentList = new List<float>();
					currentList.Add(entropies[i]);
					currentList.Add(psyfocus[i]);
					currentList.Add(duration[i]);
					psycastStats[defNames[i]] = currentList;

				}
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

			Rect rect = inRect.TopPart(0.15f);
			Widgets.DrawLineHorizontal(0f, rect.y, rect.width);
			Widgets.Label(rect.LeftPart(0.7f), "Most changes here require a restart of the game to take effect.");
			Widgets.CheckboxLabeled(new Rect(rect.RightPart(0.3f).x, rect.RightPart(0.3f).y, rect.RightPart(0.3f).width, 25f), "Reset", ref reset);
			Widgets.DrawLineHorizontal(0f, rect.height, rect.width);
			Rect rect2 = inRect.BottomPart(0.85f);

			Rect rect3 = rect2.LeftHalf();
			GUI.BeginGroup(rect3, new GUIStyle(GUI.skin.box));
			Rect rect4 = new Rect(0f, 0f, rect3.width-20f, (keysStats.Count * 4 + 14) * 32 + 1) ;
			Widgets.BeginScrollView(rect3.AtZero(), ref scrollPosition1, rect4);
			Widgets.DrawLineHorizontal(0f, rect4.y * 32f, rect4.width);
			Widgets.Label(getDrawRect(ref rect4), "Psychically induced hunger and exhaustion");
			debuff_hunger = TextFieldNumericLabeled(ref rect4, "hunger rate factor offset", (float)debuff_hunger);
			debuff_rest = TextFieldNumericLabeled(ref rect4, "rest fall factor offset", (float)debuff_rest);
			debuff_pain = TextFieldNumericLabeled(ref rect4, "pain offset", (float)debuff_pain);
			for (int num = keysStats.Count - 1; num >= 0; num--)
			{
				Widgets.DrawLineHorizontal(0f, rect4.y*32f, rect4.width);
				Widgets.Label(getDrawRect(ref rect4), keysStats[num]);
				if (psycastStats.ContainsKey(keysStats[num]))
				{
					List<float> currentList = psycastStats[keysStats[num]];
					psycastStats[keysStats[num]][0] = TextFieldNumericLabeled(ref rect4, "entropy gain", currentList[0]);
					psycastStats[keysStats[num]][1] = TextFieldNumericLabeled(ref rect4, "psyfocus cost", currentList[1]);
					psycastStats[keysStats[num]][2] = TextFieldNumericLabeled(ref rect4, "duration", currentList[2]);
					if (keysStats[num] == PsycastDefOf.MorePsycasts_StabilizingTouch.defName) { stabilizing_touch_bleed_factor = TextFieldNumericLabeled(ref rect4, "bleed factor", (float)stabilizing_touch_bleed_factor); }
					if (keysStats[num] == PsycastDefOf.MorePsycasts_HealingTouch.defName)
                    {
						healing_touch_natural_healing_factor = TextFieldNumericLabeled(ref rect4, "natural healing factor", (float)healing_touch_natural_healing_factor);
						healing_touch_immunity_gain_speed_factor = TextFieldNumericLabeled(ref rect4, "immunity gain speed factor", (float)healing_touch_immunity_gain_speed_factor);
					}
					if (keysStats[num] == PsycastDefOf.MorePsycasts_HealScars.defName) { heal_scars_healing_speed = TextFieldNumericLabeled(ref rect4, "healing speed relative to base speed", (float)heal_scars_healing_speed); }
					if (keysStats[num] == PsycastDefOf.MorePsycasts_RegrowBodyParts.defName) { regrow_body_parts_healing_speed = TextFieldNumericLabeled(ref rect4, "healing speed relative to base speed", (float)regrow_body_parts_healing_speed); }
					if (keysStats[num] == PsycastDefOf.MorePsycasts_FlashHeal.defName)
					{
						flash_heal_heal_amount = TextFieldNumericLabeled(ref rect4, "heal amount", (float)flash_heal_heal_amount);
						flash_heal_scar_chance = TextFieldNumericLabeled(ref rect4, "scar chance", (float)flash_heal_scar_chance);
					}
					if (keysStats[num] == PsycastDefOf.MorePsycasts_RevivingTouch.defName)
                    {
						reviving_touch_min_proportial_damage = TextFieldNumericLabeled(ref rect4, "min proportial damage", (float)reviving_touch_min_proportial_damage);
						reviving_touch_max_proportial_damage = TextFieldNumericLabeled(ref rect4, "max proportial damage", (float)reviving_touch_max_proportial_damage);
						psychic_ressurection_severity_per_day = TextFieldNumericLabeled(ref rect4, "severity per day lost on psychic resurrection", (float)psychic_ressurection_severity_per_day);
					}
				}
			}
			Widgets.DrawLineHorizontal(0f, rect4.y * 32f, rect4.width);
			Widgets.EndScrollView();
			GUI.EndGroup();

			Rect rect5 = rect2.RightHalf();
			GUI.BeginGroup(rect5, new GUIStyle(GUI.skin.box));
			Rect rect6 = new Rect(0f, 0f, rect5.width-20f, keysStates.Count * 24);
			Widgets.BeginScrollView(rect5.AtZero(), ref scrollPosition2, rect6);
			for (int num = keysStates.Count - 1; num >= 0; num--)
			{
				var test = psycastStates[keysStates[num]];
				Widgets.CheckboxLabeled(getDrawRect(ref rect6), keysStates[num], ref test);
				psycastStates[keysStates[num]] = test;
			}
			Widgets.EndScrollView();
			GUI.EndGroup();

			x = 0;
			base.Write();
		}
		private static Vector2 scrollPosition1 = Vector2.zero;
		private static Vector2 scrollPosition2 = Vector2.zero;

		private static List<string> editables = new List<string>();
		private static int x = 0;
		private static float TextFieldNumericLabeled(ref Rect rect, string label, float val)
		{
			if (editables.Count >= x) editables.Add(null);
			string s = editables[x];
			Widgets.TextFieldNumericLabeled<float>(getDrawRect(ref rect), label, ref val, ref s);
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
			{
				if (settings.psycastStates == null) settings.psycastStates = new Dictionary<string, bool>();
				if (!settings.psycastStates.ContainsKey(psycast.defName))
				{
					settings.psycastStates[psycast.defName] = true;
				}
			}
			var morePsycasts_psycasts = typeof(PsycastDefOf).GetFields();
			foreach (var psycast in morePsycasts_psycasts)
			{
				if (settings.psycastStats == null) settings.psycastStats = new Dictionary<string, List<float>>();
				AbilityDef current = DefDatabase<AbilityDef>.GetNamed(psycast.Name);
				if (!settings.psycastStats.ContainsKey(current.defName))
				{
					List<float> currentList = new List<float>();
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
	static class DefsRemover
	{
		static DefsRemover()
		{
			DoDefsRemoval();
		}
		public static void RemoveDef(AbilityDef def)
		{
			try
			{
				if (DefDatabase<AbilityDef>.AllDefsListForReading.Contains(def))
				{
					DefDatabase<AbilityDef>.AllDefsListForReading.Remove(def);
				}
			}
			catch { };
		}
		public static void DoDefsRemoval()
		{
			foreach (var psycastState in MorePsycasts_Mod.settings.psycastStates)
			{
				if (!psycastState.Value)
				{
					var defToRemove = DefDatabase<AbilityDef>.GetNamedSilentFail(psycastState.Key);
					if (defToRemove != null)
					{
						RemoveDef(defToRemove);
					}
				}
			}
		}
	}
	[StaticConstructorOnStartup]
	static class SettingsImplementerExecutorInAConstructor
	{
		static SettingsImplementerExecutorInAConstructor()
		{
			applyChanges();
		}
		public static void applyChanges()
        {
			Dictionary<string, List<float>> psycastStats = MorePsycasts_Mod.settings.psycastStats;
			foreach (string key in psycastStats.Keys)
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
				StatUtility.SetStatValueInList(ref HediffDefOf.MorePsycasts_AcceleratedHealing.stages[0].statFactors, StatDefOf.ImmunityGainSpeedFactor, (float)MorePsycasts_Mod.settings.healing_touch_immunity_gain_speed_factor);
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
	public class HediffStacks : Hediff
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

	}
	public class CompAbilityEffect_MorePsycasts_FlashHeal : CompAbilityEffect
	{
		public new CompProperties_AbilityEffect Props => (CompProperties_AbilityEffect)props;

		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			Props.sound = SoundDefOf.PsycastPsychicEffect;//SoundDefOf.PsycastPsychicPulse;
			base.Apply(target, dest);

			if (target.Pawn == null) { return; }
			for (int i = 0; i < 10*MorePsycasts_Mod.settings.flash_heal_heal_amount; i++)
            {
				Hediff_Injury hediff_Injury = FindInjury(target.Pawn);
				if (hediff_Injury != null)
                {
					hediff_Injury.Heal(0.1f);
					HediffWithComps hediffWithComps = hediff_Injury as HediffWithComps;
					if (hediffWithComps != null)
					{
						hediffWithComps.TryGetComp<HediffComp_GetsPermanent>().Props.becomePermanentChanceFactor *= (float)MorePsycasts_Mod.settings.flash_heal_scar_chance;
					}
				}
			}
			//SoundDefOf.PsycastPsychicEffect.PlayOneShot(new TargetInfo(target.Cell, parent.pawn.Map));
		}

		private Hediff_Injury FindInjury(Pawn pawn)
		{
			if (!pawn.Dead) {
				IEnumerable<Hediff_Injury> hediffs = pawn.health.hediffSet.GetHediffs<Hediff_Injury>();
				Func<Hediff_Injury, bool> predicate = (Hediff_Injury injury) => (injury != null && injury.Visible && injury.def.everCurableByItem && !injury.IsPermanent() && injury.CanHealNaturally());
				IEnumerable<Hediff_Injury> injuryList = hediffs.Where(predicate);
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
			Hediff psychicBreakdown = HediffMaker.MakeHediff(DefDatabase<HediffDef>.GetNamed("PsychicBreakdown"), parent.pawn);
			psychicBreakdown.Severity = 15;
			parent.pawn.health.AddHediff(psychicBreakdown);
			if (target.Thing == null||!(target.Thing is Corpse)) { return; }
			Pawn pawn = ((Corpse)target.Thing).InnerPawn;
			float x2 = ((pawn.Corpse == null) ? 0f : (pawn.Corpse.GetComp<CompRottable>().RotProgress / 60000f));
			ResurrectionUtility.Resurrect(pawn);
			Hediff psychicRessurection = HediffMaker.MakeHediff(DefDatabase<HediffDef>.GetNamed("MorePsycasts_PsychicRessurection"), pawn);
			if (!pawn.health.WouldDieAfterAddingHediff(psychicRessurection))
			{
				pawn.health.AddHediff(psychicRessurection);
			}
			x2 = (x2*(float)MorePsycasts_Mod.settings.reviving_touch_max_proportial_damage)+(float)MorePsycasts_Mod.settings.reviving_touch_min_proportial_damage;
			float toBeDealt = pawn.health.LethalDamageThreshold * x2;
			for (int i=0; i<toBeDealt;i++)
            {
				BodyPartRecord part = pawn.health.hediffSet.GetRandomNotMissingPart(DefDatabase<DamageDef>.GetNamed("Rotting"));
				Hediff hediff = HediffMaker.MakeHediff(DefDatabase<HediffDef>.GetNamed("MorePsycasts_PsychicBurn"), pawn, part);
				hediff.Severity = Rand.Value;
				if (!pawn.health.WouldDieAfterAddingHediff(hediff))
				{
					pawn.health.AddHediff(hediff);
				}
			}

			BodyPartRecord brain = pawn.health.hediffSet.GetBrain();
			if (Rand.Chance(Utilities.DementiaChancePerRotDaysCurve.Evaluate(x2)) && brain != null)
			{
				Hediff hediff2 = HediffMaker.MakeHediff(RimWorld.HediffDefOf.Dementia, pawn, brain);
				if (!pawn.health.WouldDieAfterAddingHediff(hediff2))
				{
					pawn.health.AddHediff(hediff2);
				}
			}
			if (Rand.Chance(Utilities.BlindnessChancePerRotDaysCurve.Evaluate(x2)))
			{
				foreach (BodyPartRecord item in from x in pawn.health.hediffSet.GetNotMissingParts()
												where x.def == BodyPartDefOf.Eye
												select x)
				{
					if (!pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(item))
					{
						Hediff hediff3 = HediffMaker.MakeHediff(RimWorld.HediffDefOf.Blindness, pawn, item);
						pawn.health.AddHediff(hediff3);
					}
				}
			}
			if (brain != null && Rand.Chance(Utilities.ResurrectionPsychosisChancePerRotDaysCurve.Evaluate(x2)))
			{
				Hediff hediff4 = HediffMaker.MakeHediff(RimWorld.HediffDefOf.ResurrectionPsychosis, pawn, brain);
				if (!pawn.health.WouldDieAfterAddingHediff(hediff4))
				{
					pawn.health.AddHediff(hediff4);
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
			return target.Thing is Corpse;
        }
	}
	public class HediffComp_PsychicReverberationsScars : HediffComp
	{

		public override void CompPostTick(ref float severityAdjustment)
		{
			base.CompPostTick(ref severityAdjustment);
			Hediff_Injury hediff_Injury = Utilities.FindPermanentInjury(base.Pawn);
			if (hediff_Injury == null)
			{
				severityAdjustment = -1;
			}

			if (base.Pawn.IsHashIntervalTick(600))
				hediff_Injury.Heal((float)MorePsycasts_Mod.settings.heal_scars_healing_speed*Utilities.getHealingAmount(base.Pawn));
		}
	}
	public class HediffComp_PsychicReverberationsBodyParts : HediffComp
	{

		public override void CompPostTick(ref float severityAdjustment)
		{
			base.CompPostTick(ref severityAdjustment);
			Hediff hediff_Injury = Utilities.FindHealablePart(base.Pawn);
			if (hediff_Injury == null) {
				severityAdjustment = -1;
				return;
			}

			float healingAmount = (float)MorePsycasts_Mod.settings.regrow_body_parts_healing_speed*Utilities.getHealingAmount(base.Pawn);

			if (base.Pawn.IsHashIntervalTick(600))
			{
				if (hediff_Injury.def.defName == "MorePsycasts_PartiallyGrown")
					hediff_Injury.Heal(healingAmount);
				else
				{
					if (Rand.Chance(healingAmount))
                    {
						base.Pawn.health.hediffSet.hediffs.Remove(hediff_Injury);
						Hediff hediff = HediffMaker.MakeHediff(DefDatabase<HediffDef>.GetNamed("MorePsycasts_PartiallyGrown"), base.Pawn, hediff_Injury.Part);
						hediff.Severity = hediff_Injury.Part.def.GetMaxHealth(base.Pawn) - 1;
						hediff.TryGetComp<HediffComp_GetsPermanent>().IsPermanent = true;
						base.Pawn.health.AddHediff(hediff);
					}
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
				IEnumerable<Hediff_Injury> hediffs = pawn.health.hediffSet.GetHediffs<Hediff_Injury>();
				Func<Hediff_Injury, bool> predicate = (Hediff_Injury injury) => (injury != null && injury.def.defName == "MorePsycasts_PartiallyGrown");
				IEnumerable<Hediff_Injury> injuryList = hediffs.Where(predicate);
				if (injuryList.Count() != 0) return injuryList.ElementAt(Rand.Range(0, injuryList.Count()));

				IEnumerable<Hediff_MissingPart> hediffs2 = pawn.health.hediffSet.GetMissingPartsCommonAncestors();
				Func<Hediff_MissingPart, bool> predicate2 = (Hediff_MissingPart injury) => (injury != null && !pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(injury.Part));
				IEnumerable<Hediff_MissingPart> injuryList2 = hediffs2.Where(predicate2);
				if (injuryList2.Count() != 0) return injuryList2.ElementAt(Rand.Range(0, injuryList2.Count()));
			}
			return null;
		}
		public static Hediff_Injury FindPermanentInjury(Pawn pawn, IEnumerable<BodyPartRecord> allowedBodyParts = null)
		{
			if (!pawn.Dead)
			{
				IEnumerable<Hediff_Injury> hediffs = pawn.health.hediffSet.GetHediffs<Hediff_Injury>();
				Func<Hediff_Injury, bool> predicate = (Hediff_Injury injury) => (injury != null && injury.Visible && injury.def.everCurableByItem && injury.IsPermanent());
				IEnumerable<Hediff_Injury> injuryList = hediffs.Where(predicate);
				if (injuryList.Count() == 0) return null;
				return injuryList.ElementAt(Rand.Range(0, injuryList.Count()));
			}
			return null;
		}

		public static float getHealingAmount(Pawn pawn)
        {
			float num = 8f;
			if (pawn.GetPosture() != 0)
			{
				num += 4f;
				Building_Bed building_Bed = pawn.CurrentBed();
				if (building_Bed != null)
				{
					num += building_Bed.def.building.bed_healPerDay;
				}
			}
			foreach (Hediff hediff3 in pawn.health.hediffSet.hediffs)
			{
				HediffStage curStage = hediff3.CurStage;
				if (curStage != null && curStage.naturalHealingFactor != -1f)
				{
					num *= curStage.naturalHealingFactor;
				}
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
	}
}