namespace DeliciousThings;

public static class Extensions
{
    public static void AutoPopulateTokens(this AchievementDef @this)
    {
        string token = @this.identifier.ToUpperInvariant();
        @this.nameToken = $"ACHIEVEMENT_{token}_NAME";
        @this.descriptionToken = $"ACHIEVEMENT_{token}_DESCRIPTION";
    }

    public static void PopulateUnlockStrings(this UnlockableDef @this, AchievementDef achievementDef)
    {
        @this.getHowToUnlockString = 
            () => Language.GetStringFormatted("UNLOCK_VIA_ACHIEVEMENT_FORMAT", Language.GetString(achievementDef.nameToken), Language.GetString(achievementDef.descriptionToken));
        @this.getUnlockedString = 
            () => Language.GetStringFormatted("UNLOCKED_FORMAT", Language.GetString(achievementDef.nameToken), Language.GetString(achievementDef.descriptionToken));
    }
}
