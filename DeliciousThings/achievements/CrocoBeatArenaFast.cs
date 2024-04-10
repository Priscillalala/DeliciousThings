using RoR2.Achievements;

namespace DeliciousThings;

partial class DeliciousContent
{
    public partial class CrocoBeatArenaFast : AchievementDef
    {
        public static CrocoBeatArenaFast Instance { get; private set; }

        public UnlockableDef unlockableDef = Disembowel.Instance?.unlockableDef;

        public CrocoBeatArenaFast() : base()
        {
            if (unlockableDef)
            {
                Instance = this;

                // Match achievement identifiers from 1.6.1
                identifier = "FSS_CrocoBeatArenaFast";
                this.AutoPopulateTokens();
                unlockableDef.PopulateUnlockStrings(this);
                unlockableRewardIdentifier = unlockableDef.cachedName;
                prerequisiteAchievementIdentifier = "BeatArena";
                type = typeof(Achievement);
                serverTrackerType = typeof(ServerAchievement);
            }
        }

        public class Achievement : BaseAchievement
        {
            public override BodyIndex LookUpRequiredBodyIndex() => BodyCatalog.FindBodyIndex("CrocoBody");

            public override void OnBodyRequirementMet()
            {
                base.OnBodyRequirementMet();
                SetServerTracked(true);
            }

            public override void OnBodyRequirementBroken()
            {
                SetServerTracked(false);
                base.OnBodyRequirementBroken();
            }
        }

        public class ServerAchievement : BaseServerAchievement
        {
            public override void OnInstall()
            {
                base.OnInstall();
                ArenaMissionController.onBeatArena += ArenaMissionController_onBeatArena;
            }

            public override void OnUninstall()
            {
                ArenaMissionController.onBeatArena -= ArenaMissionController_onBeatArena;
                base.OnUninstall();
            }

            private void ArenaMissionController_onBeatArena()
            {
                if (!Run.instance || Run.instance.ambientLevel >= 10f)
                {
                    return;
                }
                DifficultyDef difficultyDef = DifficultyCatalog.GetDifficultyDef(Run.instance.selectedDifficulty);
                if (difficultyDef != null && difficultyDef.countsAsHardMode)
                {
                    Grant();
                }
            }
        }
    }
}