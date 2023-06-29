using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerStorage
{
    public long GetUserValue(int missionType, float[] args = null)
    {
        MissionEnum type = (MissionEnum)missionType;
        switch(type)
        {
            case MissionEnum.Daily_StagePlayCount:
            case MissionEnum.Daily_StageClearCount:
            case MissionEnum.SpecificMapClear:
                return GetDataFromStage(type, args);

            case MissionEnum.Daily_GachaCount:
                return GetDataFromGacha(type, args);

            case MissionEnum.Daily_StatSlotIncrCount:
            case MissionEnum.SpecificStatUpgradeLevel:
            case MissionEnum.TotalSlotUpgradeLevel:
                return GetDataFromStat(type, args);

            case MissionEnum.Daily_GemUpgrdeCount:
            case MissionEnum.Daily_WeaponUpgradeCount:
            case MissionEnum.Daily_GemAwakenCount:
            case MissionEnum.Daily_WeaponAwakenCount:
                return GetDataFromSaves(type, args);

            case MissionEnum.Daily_InfiTowerClearCount:
                return GetDataFromInfi(type, args);

            case MissionEnum.SpecificWeaponEquip:
            case MissionEnum.SpecificRuneEquip:
                return GetDataFromEquips(type, args);

            case MissionEnum.SpecificWeaponAchieveGrade:
            case MissionEnum.WeaponAchieveLevel:
                return GetDataFromWeapons(type, args);

            case MissionEnum.SpecificRuneAchieveGrade:
            case MissionEnum.RuneAchieveLevel:
                return GetDataFromRunes(type, args);

            case MissionEnum.SpecificAssetGain:
                return GetDataFromStorage(type, args);

            case MissionEnum.BuyShopProduct:
                return GetDataFromShops(type, args);
            case MissionEnum.SpecificContentClear:
                return GetDataFromContents(type, args);
        }

        DebugX.LogWarning("MissionObserver : cannot Find MissionType id " + missionType);
        return 0;
    }


    int GetDataFromStage(MissionEnum type, float[] args)
    {
        var db = DB.Get<DatabaseContents_Stage>();
        
        switch (type)
        {
            case MissionEnum.Daily_StagePlayCount:
                return db.Daily_GetPlayCount();
            case MissionEnum.Daily_StageClearCount:
                return db.Daily_GetClearCount();
            case MissionEnum.SpecificMapClear:
                if (args == null || args.Length == 0)
                {
                    DebugX.LogWarning("invalid param.");
                    return 0;
                }

                ContentEntranceStateEnum contentState = db.GetStageStatus((int)args[0], (int)args[1]);
                if (contentState == ContentEntranceStateEnum.Clear) return 1;
                return 0;
        }

        DebugX.Log("Invalid Mission Type " + type.ToString());
        return 0;
    }
    long GetDataFromGacha(MissionEnum type, float[] args)
    {
        var db = DB.Get<DatabaseGacha>();
        if (args == null || args.Length == 0)
        {
            DebugX.LogWarning("invalid param.");
            return 0;
        }

        int.TryParse(args[0].ToString(), out var assetId);

        return db.GetDailyGachaCountWithAsset(assetId);

    }
    int GetDataFromStat(MissionEnum type, float[] args)
    {
        if(type == MissionEnum.Daily_StatSlotIncrCount)
        {
            if (args == null || args.Length == 0) return 0;
            int.TryParse(args[0].ToString(), out var slotId);
            if (slotId == 0) return 0;
            return  DB.Get<DatabaseStat>().GetDailyUpgradeCntOfSlot(slotId);
            
        }
        else if(type == MissionEnum.SpecificStatUpgradeLevel)
        {
            long level = DB.Get<DatabaseStat>().GetStatLevels((int)args[0]);

            return (int)level;
        }
        else if(type == MissionEnum.TotalSlotUpgradeLevel)
        {
            long level = DB.Get<DatabaseStat>().GetTotalStatLevel();
            return (int)level;
        }

        Debug.LogWarning("Mission GetValue Not Implemented");
        return 0;
        
    }

    int GetDataFromSaves(MissionEnum type, float[] args)
    {
        var db = DB.Get<DatabaseSaves>();
        UserSaveEnum saveType = UserSaveEnum.None;
        switch (type)
        {
            case MissionEnum.Daily_GemUpgrdeCount:
                saveType = UserSaveEnum.runeUpgradeCount;
                break;
            case MissionEnum.Daily_WeaponUpgradeCount:
                saveType = UserSaveEnum.weaponUpgradeCount;
                break;
            case MissionEnum.Daily_GemAwakenCount:
                saveType = UserSaveEnum.runeAwakenCount;
                break;
            case MissionEnum.Daily_WeaponAwakenCount:
                saveType = UserSaveEnum.weaponAwakenCount;
                break;
            default: return 0;
        }
        var obj = db.GetValue(saveType);
        if (obj == null) return 0;

        int.TryParse(obj.ToString(), out var intVal);
        return intVal;
    }

    int GetDataFromInfi(MissionEnum type, float[] args)
    {
        DebugX.LogWarning("Not Implemented");
        return 0;
    }

    int GetDataFromEquips(MissionEnum type, float[] args)
    {
        if(type == MissionEnum.SpecificWeaponEquip)
        {
            int length = UserCSVContainer.GetInstance().GetGlobalInt(UserGlobalEnum.battleUnitCount);
            for (int slotNum = 1; slotNum < length + 1; slotNum++)
            {
                int weaponId = DB.Get<DatabaseEquipment>().GetEquipedAssetId(PresetEnum.StagePreset, AssetEnum.Weapon, slotNum);

                if (weaponId == (int)args[0]) return 1;
            }
        }
        else if (type == MissionEnum.SpecificRuneEquip)
        {
            var weaponList = WeaponCSVContainer.GetInstance().GetWeaponCSVWithGroup((int)args[0]);
            
            foreach(var weapon in weaponList)
            {
                var list = DB.Get<DatabaseWeapon>().GetEquipRunes(weapon.id);
                if(list != null)
                {
                    foreach (var rune in list)
                    {
                        if (rune.runeKey == (int)args[1]) return 1;
                    }
                }                
            }
        }

        return 0;
    }


    int GetDataFromWeapons(MissionEnum type, float[] args)
    {
        var db = DB.Get<DatabaseWeapon>();

        if (type == MissionEnum.SpecificWeaponAchieveGrade)
        {
            var list = WeaponCSVContainer.GetInstance().GetWeaponCSVWithGroup((int)args[0]);
            foreach(var item in list)
            {
                int grade = db.GetGrade(item.id);
                if (grade >= args[1]) return 1;
            }
            
            return 0;
        }
        
        return 0;
    }

    int GetDataFromRunes(MissionEnum type, float[] args)
    {
        var db = DB.Get<DatabaseRune>();

        if (type == MissionEnum.SpecificRuneAchieveGrade)
        {
            int grade = db.GetGrade((int)args[0]);
            if (grade == args[1]) return 1;
            else return 0;
        }
        else if (type == MissionEnum.RuneAchieveLevel)
        {
            return db.GetLevel((int)args[0]);
        }

        return 0;
    }

    int GetDataFromStorage(MissionEnum type, float[] args)
    {
        if (type == MissionEnum.SpecificAssetGain)
        {
            long count = GetCount((int)args[0], (int)args[1]);
            if (count == 0) return 0;
            else return 1;
        }

        return 0;
    }

    int GetDataFromShops(MissionEnum type, float[] args)
    {
        var db = DB.Get<DatabaseShop>();
        if(type == MissionEnum.BuyShopProduct)
        {
            long count = db.GetPurchaseCount((int)args[0]);
            if (count == 0) return 0;
            else if (count < args[1]) return 0;
            else return 1;
        }

        return 0;
    }

    int GetDataFromContents(MissionEnum type, float[] args)
    {
        switch ((int)args[0])
        {
            case (int)GameStateEnum.InfiTower:
                int clearCount = DB.Get<DatabaseContents_InfiTower>().GetLastClearFloor(GameStateEnum.InfiTower);
                if (clearCount > 0) return 1;
                else return 0;
            case (int)GameStateEnum.Raid:
                int raidClearCount = DB.Get<DatabaseContents_Raid>().GetTryCount();
                return raidClearCount;
        }

        return 0;
    }
}
