using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.Sound;

namespace MorePsycasts
{
	
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
			Scribe_Values.Look(ref stacksList, "MorePsycasts_stacksList");
			Scribe_Values.Look(ref duration, "MorePsycasts_duration");
			Scribe_Values.Look(ref stacks, "MorePsycasts_stacks");
			Scribe_Values.Look(ref link, "MorePsycasts_link");
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
	public class CompProperties_MorePsycasts_FlashHeal : CompProperties_AbilityEffect
	{
		public CompProperties_MorePsycasts_FlashHeal()
		{
			this.compClass = typeof(CompAbilityEffect_MorePsycasts_FlashHeal);
		}

		public int damageAmount = 0;
		public float scarringMultiplier = 1;

	}
	public class CompAbilityEffect_MorePsycasts_FlashHeal : CompAbilityEffect
	{
		public new CompProperties_MorePsycasts_FlashHeal Props => (CompProperties_MorePsycasts_FlashHeal)props;

		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			Props.sound = SoundDefOf.PsycastPsychicEffect;//SoundDefOf.PsycastPsychicPulse;
			base.Apply(target, dest);

			if (target.Pawn == null) { return; }
			for (int i = 0; i < 10*this.Props.damageAmount; i++)
            {
				Hediff_Injury hediff_Injury = FindInjury(target.Pawn);
				if (hediff_Injury != null)
                {
					hediff_Injury.Heal(0.1f);
					HediffWithComps hediffWithComps = hediff_Injury as HediffWithComps;
					if (hediffWithComps != null)
					{
						hediffWithComps.TryGetComp<HediffComp_GetsPermanent>().Props.becomePermanentChanceFactor *= this.Props.scarringMultiplier;
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
	public class CompProperties_MorePsycasts_RevivingTouch : CompProperties_AbilityEffect
	{
		public CompProperties_MorePsycasts_RevivingTouch()
		{
			this.compClass = typeof(CompAbilityEffect_MorePsycasts_RevivingTouch);
		}
	}
	public class CompAbilityEffect_MorePsycasts_RevivingTouch : CompAbilityEffect
	{
		public new CompProperties_MorePsycasts_RevivingTouch Props => (CompProperties_MorePsycasts_RevivingTouch)props;

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
			x2 = (x2*0.8f)+0.2f;
			float toBeDealt = pawn.health.LethalDamageThreshold * x2;
			for (int i=0; i<toBeDealt;i++)
            {
				BodyPartRecord part = pawn.health.hediffSet.GetRandomNotMissingPart(DefDatabase<DamageDef>.GetNamed("Rotting"));
				Hediff hediff = HediffMaker.MakeHediff(DefDatabase<HediffDef>.GetNamed("MorePsycasts_PsychicBurn"), pawn, part);
				hediff.Severity = Rand.Value;
				//if (!pawn.health.WouldDieAfterAddingHediff(hediff))
				//{
					pawn.health.AddHediff(hediff);
				//}
			}

			BodyPartRecord brain = pawn.health.hediffSet.GetBrain();
			if (Rand.Chance(Utilities.DementiaChancePerRotDaysCurve.Evaluate(x2)) && brain != null)
			{
				Hediff hediff2 = HediffMaker.MakeHediff(HediffDefOf.Dementia, pawn, brain);
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
						Hediff hediff3 = HediffMaker.MakeHediff(HediffDefOf.Blindness, pawn, item);
						pawn.health.AddHediff(hediff3);
					}
				}
			}
			if (brain != null && Rand.Chance(Utilities.ResurrectionPsychosisChancePerRotDaysCurve.Evaluate(x2)))
			{
				Hediff hediff4 = HediffMaker.MakeHediff(HediffDefOf.ResurrectionPsychosis, pawn, brain);
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
				hediff_Injury.Heal(Utilities.getHealingAmount(base.Pawn));
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

			float healingAmount = Utilities.getHealingAmount(base.Pawn);

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