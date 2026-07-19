using System;
using UnityEngine;

namespace Sirvival
{
    /// <summary>
    /// Owns the whole run: state machine, live stats, xp/level/timer/kills, and events.
    /// Systems read from here; they never talk to each other directly.
    /// </summary>
    public class RunManager : MonoBehaviour
    {
        public static RunManager Instance { get; private set; }

        [SerializeField] private PlayerStats baseStats = new PlayerStats();
        public PlayerStats Stats { get; private set; }

        public RunState State { get; private set; } = RunState.Playing;
        public float Elapsed { get; private set; }
        public int Level { get; private set; } = 1;
        public int Xp { get; private set; }
        public int XpToNext { get; private set; }
        public int Kills { get; private set; }
        public Transform Player { get; private set; }

        public event Action OnStatsChanged;        // hp/xp/level/kills changed
        public event Action<Upgrade[]> OnLevelUp;  // choices to present
        public event Action OnGameOver;

        private void Awake()
        {
            Instance = this;
            Stats = baseStats.Clone();
            XpToNext = Leveling.XpForLevel(Level);
            var chef = GameObject.Find("Chef");
            if (chef != null) Player = chef.transform;
        }

        private void Update()
        {
            if (State == RunState.Playing) Elapsed += Time.deltaTime;
        }

        public void RaiseStatsChanged() => OnStatsChanged?.Invoke();

        public void AddKill()
        {
            Kills++;
            OnStatsChanged?.Invoke();
        }

        public void AddXp(int amount)
        {
            if (State != RunState.Playing) return;
            Xp += amount;
            while (Xp >= XpToNext)
            {
                Xp -= XpToNext;
                Level++;
                XpToNext = Leveling.XpForLevel(Level);
                EnterLevelUp();
            }
            OnStatsChanged?.Invoke();
        }

        private void EnterLevelUp()
        {
            State = RunState.LevelUp;
            Time.timeScale = 0f;
            OnLevelUp?.Invoke(UpgradePool.RollChoices(3, Level * 7919 + Kills));
        }

        public void ChooseUpgrade(Upgrade u)
        {
            u.Apply(Stats);
            State = RunState.Playing;
            Time.timeScale = 1f;
            OnStatsChanged?.Invoke();
        }

        public void EndRun()
        {
            if (State == RunState.GameOver) return;
            State = RunState.GameOver;
            Time.timeScale = 0f;
            OnGameOver?.Invoke();
        }
    }
}
