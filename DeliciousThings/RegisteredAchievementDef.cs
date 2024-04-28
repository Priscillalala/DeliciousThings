using RoR2BepInExPack.VanillaFixes;
using static DeliciousThings.Delicious;

namespace DeliciousThings;

public class RegisteredAchievementDef : AchievementDef
{
    private static readonly List<WeakReference<RegisteredAchievementDef>> weakInstancesList = [];

    static RegisteredAchievementDef()
    {
        Delicious.Logger.LogMessage("RegisteredAchievementDef STATIC!!!");
        SaferAchievementManager.OnCollectAchievementDefs += RegisterAchievements;
    }

    private static void RegisterAchievements(List<string> identifiers, Dictionary<string, AchievementDef> identifierToAchievementDef, List<AchievementDef> achievementDefs)
    {
        for (int i = weakInstancesList.Count - 1; i >= 0; i--)
        {
            WeakReference<RegisteredAchievementDef> weakInstance = weakInstancesList[i];
            if (weakInstance.TryGetTarget(out RegisteredAchievementDef registeredAchievementDef) && registeredAchievementDef != null)
            {
                identifiers.Add(registeredAchievementDef.identifier);
                identifierToAchievementDef.Add(registeredAchievementDef.identifier, registeredAchievementDef);
                achievementDefs.Add(registeredAchievementDef);
            }
            else
            {
                weakInstancesList.RemoveAt(i);
            }
        }
    }

    public RegisteredAchievementDef() : base()
    {
        weakInstancesList.Add(new WeakReference<RegisteredAchievementDef>(this));
    }
}