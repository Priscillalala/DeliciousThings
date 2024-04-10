using RoR2.Achievements;

namespace DeliciousThings;

partial class DeliciousContent
{
    public partial class CompleteMultiplayerUnknownEnding : AchievementDef, IStaticContent
    {
        public static CompleteMultiplayerUnknownEnding Instance { get; private set; }

        public CompleteMultiplayerUnknownEnding() : base()
        {
            Instance = this;

            // Match achievement identifiers from FreeItemFriday
            identifier = "CompleteMultiplayerUnknownEnding";
            this.AutoPopulateTokens();
            UnlockableDef unlockable = GodlessEye.Instance.unlockableDef;
            unlockableRewardIdentifier = unlockable.cachedName;
            unlockable.PopulateUnlockStrings(this);
            type = typeof(Achievement);
            serverTrackerType = typeof(ServerAchievement);
        }

        public IEnumerator LoadAsync(IProgress<float> progressReceiver, AssetBundle assets)
        {
            var texCompleteMultiplayerUnknownEndingIcon = assets.LoadAssetAsync<Sprite>("texCompleteMultiplayerUnknownEndingIcon");

            yield return texCompleteMultiplayerUnknownEndingIcon;
            SetAchievedIcon((Sprite)texCompleteMultiplayerUnknownEndingIcon.asset);
        }

        public class Achievement : BaseAchievement
        {
            public override void OnInstall()
            {
                base.OnInstall();
                SetServerTracked(true);
            }

            public override void OnUninstall()
            {
                SetServerTracked(false);
                base.OnUninstall();
            }
        }

        public class ServerAchievement : BaseServerAchievement
        {
            public override void OnInstall()
            {
                base.OnInstall();
                Run.onServerGameOver += OnServerGameOver;
            }

            public override void OnUninstall()
            {
                base.OnInstall();
                Run.onServerGameOver -= OnServerGameOver;
            }

            public void OnServerGameOver(Run run, GameEndingDef gameEndingDef)
            {
                if ((gameEndingDef == RoR2Content.GameEndings.ObliterationEnding || gameEndingDef == RoR2Content.GameEndings.LimboEnding) && RoR2Application.isInMultiPlayer)
                {
                    Grant();
                }
            }
        }
    }
}