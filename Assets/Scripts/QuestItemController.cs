using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuestItemController : MonoBehaviour {

	public int QuestID;

	public Quest quest;
	public QuestInstance questInstance = null;

	public Text TextQuestName, TextButtonName;
	public Slider SliderProgress;

	// Use this for initialization
	void Start () {
		quest = DataController.Instance.QuestDic [QuestID];
		foreach (QuestInstance qi in DataController.Instance.gameData.QuestList) {
			if (quest.QuestID == qi.QuestID) {
				questInstance = qi;
			}
		}

		if (questInstance == null) {
			questInstance = new QuestInstance ();
			questInstance.QuestID = quest.QuestID;
			questInstance.QuestLevel = 1;
			questInstance.Current = 0;
			questInstance.Goal = quest.GoalPerLevel;
			questInstance.Start = 0;
			DataController.Instance.gameData.QuestList.Add (questInstance);
		}
			
		if ("MonsterKill".Equals (quest.QuestType)) {
			if ("Orc".Equals (quest.TargetName)) {
				NotificationCenter.Instance.Add ("OrcKill", this.UpdateDesc);
			}
		}
			
		UpdateDesc ();

	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void UpdateDesc(){

		if ("MonsterKill".Equals(quest.QuestType)) {
			if ("Orc".Equals (quest.TargetName)) {
				questInstance.Current = DataController.Instance.gameData.OrcKillCount;
				questInstance.Goal = questInstance.QuestLevel * quest.GoalPerLevel;
				questInstance.Start = (questInstance.QuestLevel - 1) * quest.GoalPerLevel;
				questInstance.Progress = 
					(((float)questInstance.Current - (float)questInstance.Start) 
						/ ((float)questInstance.Goal - (float)questInstance.Start));
				questInstance.Reward = questInstance.QuestLevel * quest.RewardAmountPerLevel;

				TextQuestName.text = string.Format (quest.QuestName, questInstance.Goal);
				TextButtonName.text = string.Format (quest.ButtonName, questInstance.Reward);
				SliderProgress.value = questInstance.Progress;

			}
		}
	}

	public void OnClickQuestReward(){
		if ("MonsterKill".Equals(quest.QuestType)) {
			if ("Orc".Equals (quest.TargetName)) {
				questInstance.Current = DataController.Instance.gameData.OrcKillCount;
				questInstance.Goal = questInstance.QuestLevel * quest.GoalPerLevel;
				questInstance.Start = (questInstance.QuestLevel - 1) * quest.GoalPerLevel;
				questInstance.Progress = 
					(((float)questInstance.Current - (float)questInstance.Start) 
						/ ((float)questInstance.Goal - (float)questInstance.Start));
				questInstance.Reward = questInstance.QuestLevel * quest.RewardAmountPerLevel;

				if (questInstance.Current >= questInstance.Goal) {
					
					questInstance.QuestLevel++;
					DataController.Instance.gameData.Gold += questInstance.Reward;

				}

			}
		}

		DataController.Instance.SaveGameData ();
		UpdateDesc ();
	}
}
