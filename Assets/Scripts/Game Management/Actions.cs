using System;
using Character;
using UnityEngine;

namespace Game_Management
{
    public static class Actions
    {
        public static Action ButtonPush;
        public static Action<GameObject> SetNextGrid;
        public static Action PerfectNote;
        public static Action StandardNote;
        public static Action LevelStarted;
        public static Action LevelFinished;
        public static Action LevelFailed;
        public static Action ResetAllGrids;
        public static Action<bool> AudioChanged;
    }
}
