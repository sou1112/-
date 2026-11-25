using System;
using System.Collections.Generic;

[Serializable]
public class LoginRequest
{
    public string email;
    public string password;
}

[Serializable]
public class LoginResponse
{
    public bool success;
    public string userId;
    public string childName;
    public string message;
}

[Serializable]
public class UserStatusData
{
    public string userId;
    public string characterId;
    public int level;
    public int exp;
    public int coins;
    public string costume;
}

[Serializable]
public class QuestData
{
    public string questId;
    public string userId;
    public string questName;
    public int timeLimit;
    public int rewardCoins;
    public int rewardExp;
    public string extraReward;
    public bool isUrgent;
    public string illustration;
    public string color;
}

[Serializable]
public class QuestListResponse
{
    public List<QuestData> items;
}

[Serializable]
public class QuestLogData
{
    public string logId;
    public string userId;
    public string questId;
    public string executedAt;
    public int durationMinutes;
    public string result;
    public bool parentApprovalStatus;
}

[Serializable]
public class QuestLogListResponse
{
    public List<QuestLogData> items;
}