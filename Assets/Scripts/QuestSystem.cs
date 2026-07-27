using UnityEngine;
using System.Collections.Generic;

public class QuestSystem : MonoBehaviour
{
    public struct Quest
    {
        public string title;
        public string description;
        public int targetIsland;
        public bool isActive;
        public bool isCompleted;
        public int rewardCoins;
        public int rewardReputation;
        public int rewardKnowledge;
    }

    private List<Quest> quests = new List<Quest>();
    private int currentQuestIndex = 0;

    public void InitializeQuests()
    {
        quests.Clear();

        quests.Add(new Quest
        {
            title = "Добро пожаловать!",
            description = "Посетите Факультет Информатики",
            targetIsland = 0,
            isActive = true,
            isCompleted = false,
            rewardCoins = 30,
            rewardReputation = 10,
            rewardKnowledge = 5
        });

        quests.Add(new Quest
        {
            title = "Медицинская экспедиция",
            description = "Доставьте документы на Медицинский Факультет",
            targetIsland = 1,
            isActive = false,
            isCompleted = false,
            rewardCoins = 50,
            rewardReputation = 15,
            rewardKnowledge = 10
        });

        quests.Add(new Quest
        {
            title = "Юридический кейс",
            description = "Найдите ancient scrolls на Юридическом Факультете",
            targetIsland = 2,
            isActive = false,
            isCompleted = false,
            rewardCoins = 75,
            rewardReputation = 20,
            rewardKnowledge = 15
        });

        quests.Add(new Quest
        {
            title = "Арт-миссия",
            description = "Привезите краски на Факультет Искусств",
            targetIsland = 3,
            isActive = false,
            isCompleted = false,
            rewardCoins = 40,
            rewardReputation = 25,
            rewardKnowledge = 5
        });

        quests.Add(new Quest
        {
            title = "Инженерный вызов",
            description = "Доставьте детали на Инженерный Факультет",
            targetIsland = 4,
            isActive = false,
            isCompleted = false,
            rewardCoins = 60,
            rewardReputation = 15,
            rewardKnowledge = 20
        });

        quests.Add(new Quest
        {
            title = "Финальный экзамен",
            description = "Посетите все факультеты и сдайте экзамен",
            targetIsland = 5,
            isActive = false,
            isCompleted = false,
            rewardCoins = 200,
            rewardReputation = 50,
            rewardKnowledge = 50
        });

        if (quests.Count > 0)
            quests[0] = quests[0]; // First quest is active
    }

    public void CheckQuestProgress(int islandIndex)
    {
        if (currentQuestIndex >= quests.Count) return;

        Quest quest = quests[currentQuestIndex];

        if (!quest.isActive || quest.isCompleted) return;

        if (quest.targetIsland == islandIndex)
        {
            CompleteQuest(currentQuestIndex);
        }
    }

    void CompleteQuest(int index)
    {
        Quest quest = quests[index];
        quest.isCompleted = true;
        quest.isActive = false;
        quests[index] = quest;

        GameManager.Instance?.AddCoins(quest.rewardCoins);
        GameManager.Instance?.AddReputation(quest.rewardReputation);
        GameManager.Instance?.AddKnowledge(quest.rewardKnowledge);

        GameManager.Instance?.uiController?.ShowNotification(
            $"Quest Complete: {quest.title}\n" +
            $"+{quest.rewardCoins} Coins, +{quest.rewardReputation} Rep, +{quest.rewardKnowledge} Knowledge");

        AdvanceToNextQuest();
    }

    void AdvanceToNextQuest()
    {
        currentQuestIndex++;

        if (currentQuestIndex < quests.Count)
        {
            Quest next = quests[currentQuestIndex];
            next.isActive = true;
            quests[currentQuestIndex] = next;

            GameManager.Instance?.uiController?.ShowNotification(
                $"New Quest: {next.title}\n{next.description}");
        }
        else
        {
            GameManager.Instance?.uiController?.ShowNotification(
                "All quests completed! You are a true Campus Voyager!");
        }
    }

    public Quest? GetCurrentQuest()
    {
        if (currentQuestIndex < quests.Count)
            return quests[currentQuestIndex];
        return null;
    }

    public int GetCompletedCount()
    {
        int count = 0;
        foreach (var q in quests)
            if (q.isCompleted) count++;
        return count;
    }

    public int GetTotalCount()
    {
        return quests.Count;
    }
}
