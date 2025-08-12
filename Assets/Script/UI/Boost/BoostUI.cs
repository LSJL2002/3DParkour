using System.Collections.Generic;
using UnityEngine;

public class BoostUI : MonoBehaviour
{
    [System.Serializable]
    public struct BoostPrefabEntry
    {
        public BoostType boostType;
        public GameObject prefab;
    }

    public List<BoostPrefabEntry> boostPrefabs;

    public Transform boostTimerContainer;

    private Dictionary<BoostType, BoostTimerUI> activeBoostTimers = new Dictionary<BoostType, BoostTimerUI>();

    private Dictionary<BoostType, GameObject> prefabLookup;

    void Awake()
    {
        prefabLookup = new Dictionary<BoostType, GameObject>();
        foreach (var entry in boostPrefabs)
        {
            prefabLookup[entry.boostType] = entry.prefab;
        }
    }

    public void ShowOrRefreshBoostTimer(BoostType boostType, float duration)
    {
        if (activeBoostTimers.ContainsKey(boostType))
        {
            activeBoostTimers[boostType].StartTimer(duration, () => RemoveBoostTimer(boostType));
        }
        else
        {
            if (!prefabLookup.TryGetValue(boostType, out GameObject prefab))
            {
                Debug.LogWarning($"No prefab assigned for boost type {boostType}");
                return;
            }

            GameObject timerGO = Instantiate(prefab, boostTimerContainer);
            BoostTimerUI timerUI = timerGO.GetComponent<BoostTimerUI>();
            activeBoostTimers.Add(boostType, timerUI);
            timerUI.StartTimer(duration, () => RemoveBoostTimer(boostType));
        }
    }

    private void RemoveBoostTimer(BoostType boostType)
    {
        if (activeBoostTimers.TryGetValue(boostType, out BoostTimerUI timerUI))
        {
            activeBoostTimers.Remove(boostType);
            Destroy(timerUI.gameObject);
        }
    }
}
