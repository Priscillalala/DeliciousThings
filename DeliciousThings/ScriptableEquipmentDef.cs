namespace DeliciousThings;

public abstract class ScriptableEquipmentDef : EquipmentDef
{
    static ScriptableEquipmentDef()
    {
        Delicious.Logger.LogMessage("ScriptableEquipmentDef STATIC!!!");
        On.RoR2.EquipmentSlot.PerformEquipmentAction += PerformScriptableEquipmentActions;
    }

    private static bool PerformScriptableEquipmentActions(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
    {
        if (equipmentDef is ScriptableEquipmentDef scriptableEquipmentDef)
        {
            return scriptableEquipmentDef.fireFunction != null && scriptableEquipmentDef.fireFunction(self);
        }
        return orig(self, equipmentDef);
    }

    public Func<EquipmentSlot, bool> fireFunction;

    public ScriptableEquipmentDef() : base()
    {
        canDrop = true;
    }
}