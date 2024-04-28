using RoR2.Skills;
using RoR2BepInExPack.VanillaFixes;

namespace DeliciousThings;

public static class Extensions
{
    public static void AutoPopulateTokens(this AchievementDef @this)
    {
        string token = @this.identifier.ToUpperInvariant();
        @this.nameToken = $"ACHIEVEMENT_{token}_NAME";
        @this.descriptionToken = $"ACHIEVEMENT_{token}_DESCRIPTION";
    }

    private static readonly List<AchievementDef> registeredAchievementDefs = [];

    public static void Register(this AchievementDef @this)
    {
        registeredAchievementDefs.Add(@this);
    }

    public static void Unregister(this AchievementDef @this)
    {
        registeredAchievementDefs.Remove(@this);
    }

    static Extensions()
    {
        SaferAchievementManager.OnCollectAchievementDefs += RegisterAchievements;
    }

    private static void RegisterAchievements(List<string> identifiers, Dictionary<string, AchievementDef> identifierToAchievementDef, List<AchievementDef> achievementDefs)
    {
        foreach (AchievementDef achievementDef in registeredAchievementDefs)
        {
            identifiers.Add(achievementDef.identifier);
            identifierToAchievementDef.Add(achievementDef.identifier, achievementDef);
            achievementDefs.Add(achievementDef);
        }
    }

    public static void PopulateUnlockStrings(this UnlockableDef @this, AchievementDef achievementDef)
    {
        @this.getHowToUnlockString = 
            () => Language.GetStringFormatted("UNLOCK_VIA_ACHIEVEMENT_FORMAT", Language.GetString(achievementDef.nameToken), Language.GetString(achievementDef.descriptionToken));
        @this.getUnlockedString = 
            () => Language.GetStringFormatted("UNLOCKED_FORMAT", Language.GetString(achievementDef.nameToken), Language.GetString(achievementDef.descriptionToken));
    }

    public static void AutoPopulateTokens(this SkillDef @this)
    {
        string token = @this.skillName.ToUpperInvariant();
        @this.skillNameToken = $"SKILL_{token}_NAME";
        @this.skillDescriptionToken = $"SKILL_{token}_DESC";
    }
}