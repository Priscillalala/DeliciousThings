namespace DeliciousThings;

partial class DeliciousContent
{
    public interface English
    {
        public Dictionary<string, string> Language { get; }
    }

    partial class Expansion : English
    {
        Dictionary<string, string> English.Language => new()
        {
            [nameToken] = $"Delicious Things",
            [descriptionToken] = $"Adds content from the 'Delicious Things' mod to the game.",
        };
    }

    #region items
    partial class Theremin : English
    {
        Dictionary<string, string> English.Language => new()
        {
            [nameToken] = $"Theremin",
            [pickupToken] = $"Increase attack speed near the Teleporter.",
            [descriptionToken] = $"Increase <style=cIsDamage>attack speed</style> by up to <style=cIsDamage>{attackSpeedBonus:0%} <style=cStack>(+{attackSpeedBonusPerStack:0%} per stack)</style></style> the closer you are to a Teleporter.",
        };
    }

    partial class FlintArrowhead : English
    {
        Dictionary<string, string> English.Language => new()
        {
            [nameToken] = $"Flint Arrowhead",
            [pickupToken] = $"Burn enemies for flat damage on hit.",
            [descriptionToken] = $"<style=cIsDamage>100%</style> chance to <style=cIsDamage>burn</style> on hit for <style=cIsDamage>{damage} <style=cStack>(+{damagePerStack} per stack)</style></style> damage.",
        };
    }
    #endregion

    #region achievements
    partial class BurnMultipleEnemies : English
    {
        Dictionary<string, string> English.Language => new()
        {
            [nameToken] = $"Burn To Kill",
            [descriptionToken] = $"Ignite 10 enemies simultaneously.",
        };
    }
    #endregion
}
