namespace DeliciousThings
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

    namespace Items
    {
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
    }

    namespace Equipment
    {
        partial class GodlessEye : English
        {
            Dictionary<string, string> English.Language => new()
            {
                [nameToken] = $"Godless Eye",
                [pickupToken] = $"Obliterate all nearby enemies from existence, then yourself. Consumed on use.",
                [descriptionToken] = $"Obliterate enemies within <style=cIsUtility>{range}m</style> from existence. Then, <style=cIsHealth>obliterate yourself from existence</style>. Equipment is <style=cIsUtility>consumed</style> on use.",
                [consumedDef.nameToken] = $"Godless Eye (Consumed)",
                [consumedDef.pickupToken] = $"Still shocking to the touch. Does nothing.",
                [consumedDef.descriptionToken] = $"Still shocking to the touch. Does nothing.",
            };
        }
    }

    namespace Skills
    {
        partial class Disembowel : English
        {
            Dictionary<string, string> English.Language => new()
            {
                [skillNameToken] = $"Disembowel",
                [skillDescriptionToken] = $"<style=cIsHealing>Poisonous</style>. <style=cIsDamage>Slayer</style>. Lacerate an enemy for <style=cIsDamage>3x{damageCoefficient:0%} damage</style>, causing <style=cIsDamage>bleeding</style> and <style=cIsHealth>hemorrhaging</style>.",
                ["FSS_KEYWORD_BLEED"] = $"<style=cKeywordName>Bleed</style><style=cSub>Deal <style=cIsDamage>320%</style> base damage over 4s. <i>Bleed can stack.</i></style>",
            };
        }
    }

    namespace Achievements
    {
        partial class BurnMultipleEnemies : English
        {
            Dictionary<string, string> English.Language => new()
            {
                [nameToken] = $"Burn To Kill",
                [descriptionToken] = $"Ignite 10 enemies simultaneously.",
            };
        }

        partial class CompleteMultiplayerUnknownEnding : English
        {
            Dictionary<string, string> English.Language => new()
            {
                [nameToken] = $"Fly Away Together",
                [descriptionToken] = $"In multiplayer, obliterate at the Obelisk with a fellow survivor..",
            };
        }

        partial class CrocoBeatArenaFast : English
        {
            Dictionary<string, string> English.Language => new()
            {
                [nameToken] = $"Acrid: Virulence",
                [descriptionToken] = $"As Acrid, clear the Void Fields on Monsoon before monsters reach Lv. 10.",
            };
        }
    }
}