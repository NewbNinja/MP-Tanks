using PlayFab;
using System.Collections;
using System.Collections.Generic;

namespace CBS
{
    public class BattlePassData
    {
        public List<BattlePassInstance> Instances = new List<BattlePassInstance>();
    }

    public class BattlePassInstance : ICustomData
    {
        public string ID;
        public string DisplayName;
        public string Description;
        public int ExpStep = 100;
        public Period Duration = new Period();
        public DevelopmentState State;
        public List<BattlePassLevel> LevelTree;

        public string CustomDataClassName { get; set; }
        public string CustomRawData { get; set; }

        public virtual T GetCustomData<T>() where T : CBSBattlePassCustomData
        {
            var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
            return jsonPlugin.DeserializeObject<T>(CustomRawData);
        }
    }

    public class BattlePassLevel
    {
        public PrizeObject DefaultReward;
        public PrizeObject PremiumReward;
    }

    [System.Serializable]
    public class BattlePassUserInfo
    {
        public string PlayerID;
        public string BattlePassID;
        public bool PremiumRewardAvailable;
        public string BattlePassName;
        public bool IsActive;
        public int PlayerLevel;
        public int PlayerExp;
        public int ExpOfCurrentLevel;
        public int ExpStep;
        public int RewardBadgeCount;
        public int [] CollectedSimpleReward;
        public int [] CollectedPremiumReward; 
        public long MilisecondsToStart;
        public long MilisecondsToEnd;
        public long MilisecondsActive;

        public string CustomDataClassName;
        public string CustomRawData;

        public virtual T GetCustomData<T>() where T : CBSBattlePassCustomData
        {
            var jsonPlugin = PluginManager.GetPlugin<ISerializerPlugin>(PluginContract.PlayFab_Serializer);
            return jsonPlugin.DeserializeObject<T>(CustomRawData);
        }
    }

    [System.Serializable]
    public class BattlePassPlayerStatesCallback
    {
        public List<BattlePassUserInfo> PlayerStates;
    }

    [System.Serializable]
    public class BattlePassFullInformationCallback
    {
        public BattlePassUserInfo PlayerState;
        public BattlePassInstance Instance;
    }

    [System.Serializable]
    public class BattlePassAddExpirienceCallback
    {
        public Dictionary<string, int> ExpTable;
    }

    [System.Serializable]
    public class BattlePassGrantAwardCallback
    {
        public string InstanceID;
        public PrizeObject RecivedReward;
        public bool IsPremium;
    }

    public class BattlePassPlayerStateRequest
    {
        public string SpecificID;
        public bool IncludeNotStarted;
        public bool IncludeOutdated;
    }

    public class BattlePassLevelInfo
    {
        public string battlePassID;
        public int LevelIndex;
        public BattlePassLevel LevelDetail;
        public int PlayerLevel;
        public int ExpOfCurrentLevel;
        public int ExpStep;
        public bool IsPassActive;
        public bool IsPremium;
        public bool IsPremiumRewardCollected;
        public bool IsDefaultRewardCollected;
        public bool IsLast;
    }
}
