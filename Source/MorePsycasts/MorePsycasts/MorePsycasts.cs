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
			if (duration == ageTicks)
            {
				Severity -= stacks;
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
	}
	public class CompProperties_MorePsycasts_AbilityGiveHediffStacks : CompProperties_AbilityGiveHediff
	{
		public int stacks = 1;
	}
	public class CompAbilityEffect_MorePsycasts_GiveHediffStacks : CompAbilityEffect_GiveHediff
	{
		public new CompProperties_MorePsycasts_AbilityGiveHediffStacks Props => (CompProperties_MorePsycasts_AbilityGiveHediffStacks)props;

		public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
		{
			Pawn target2 = Props.applyToSelf ? parent.pawn : target.Pawn;
			Hediff hediff = HediffMaker.MakeHediff(Props.hediffDef, target2, Props.onlyBrain ? target2.health.hediffSet.GetBrain() : null);
			hediff.Severity = Props.stacks;
			if (!(hediff is HediffStacks))
				return;
			((HediffStacks)hediff).stacks = Props.stacks;
			((HediffStacks)hediff).duration = GetDurationSeconds(target2).SecondsToTicks();
			target2.health.AddHediff(hediff);
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
			//Props.sound = SoundDefOf.PsycastPsychicEffect;//SoundDefOf.PsycastPsychicPulse;
			base.Apply(target, dest);
			if (target.Thing == null) { return; }
			Pawn innerPawn = ((Corpse)target.Thing).InnerPawn;
			ResurrectionUtility.ResurrectWithSideEffects(innerPawn);
			Messages.Message("MessagePawnResurrected".Translate(innerPawn), innerPawn, MessageTypeDefOf.PositiveEvent);
			//SoundDefOf.PsycastPsychicEffect.PlayOneShot(new TargetInfo(target.Cell, parent.pawn.Map));
		}
	}

	public class HediffComp_PsychicReverberationsScars : HediffComp
	{

		public override void CompPostTick(ref float severityAdjustment)
		{
			base.CompPostTick(ref severityAdjustment);
			Hediff_Injury hediff_Injury = FindPermanentInjury(base.Pawn);
			if (hediff_Injury == null) { return; }
			if (Rand.Value<0.001)
				hediff_Injury.Heal(0.1f);
		}

		public override void CompExposeData()
		{
			base.CompExposeData();
		}

		private Hediff_Injury FindPermanentInjury(Pawn pawn, IEnumerable<BodyPartRecord> allowedBodyParts = null)
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
	}

	public class HediffComp_PsychicReverberationsBodyParts : HediffComp
	{

		public override void CompPostTick(ref float severityAdjustment)
		{
			base.CompPostTick(ref severityAdjustment);
			Hediff_MissingPart hediff_Injury = FindPermanentInjury(base.Pawn);
			if (hediff_Injury == null) { return; }
			if (Rand.Value < (0.00001/(float)hediff_Injury.Part.def.hitPoints))
            {
				HealthUtility.CureHediff(hediff_Injury);
				//Hediff hediff = HediffMaker.MakeHediff(DefDatabase<HediffDef>.GetNamed("MorePsycasts_Growing"), base.Pawn);
				//hediff.Severity = 1;
				//base.Pawn.health.AddHediff(hediff, hediff_Injury.Part);
			}
		}

		public override void CompExposeData()
		{
			base.CompExposeData();
		}

		private Hediff_MissingPart FindPermanentInjury(Pawn pawn)
		{
			if (!pawn.Dead)
			{
				IEnumerable<Hediff_MissingPart> hediffs = pawn.health.hediffSet.GetMissingPartsCommonAncestors();
				Func<Hediff_MissingPart, bool> predicate = (Hediff_MissingPart injury) => (!pawn.health.hediffSet.PartOrAnyAncestorHasDirectlyAddedParts(injury.Part));
				IEnumerable<Hediff_MissingPart> injuryList = hediffs.Where(predicate);
				if (injuryList.Count() == 0) return null;
				return injuryList.ElementAt(Rand.Range(0, injuryList.Count()));
			}
			return null;
		}
	}
}
