using System;
using UnityEngine;

namespace Game_Management
{
    public static class Actions
    {
        public static Action<GameObject> SetNextGrid;
        public static Action GameStarted;
        public static Action LevelStarted;
        public static Action LevelFinished;
        public static Action ResetAllGrids;
        public static Action<bool> AudioChanged;
    }
}
