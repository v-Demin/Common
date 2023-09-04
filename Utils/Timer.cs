using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

namespace Submodules.Common.Utils
{
    public class Timer
    {
        public Action<Timer> OnStart;
        public Action<Timer> OnUpdate;
        public Action<Timer> OnPause;
        public Action<Timer> OnEnd;
    
        public float AllSeconds { get; private set; }
        public float CurrentSeconds { get; private set; }
        public float LastedSeconds => AllSeconds - CurrentSeconds;
        public float LastedProportion => CurrentSeconds / AllSeconds;

        
        private Tween _cashedTimer;

        public Timer(float seconds)
        {
            AllSeconds = seconds;
            CurrentSeconds = 0;
        }

        public void Start()
        {
            _cashedTimer = DOVirtual.Float(CurrentSeconds, AllSeconds, AllSeconds, value => OnUpdate?.Invoke(this))
                .OnStart(() => OnStart?.Invoke(this))
                .OnPause(() => OnPause?.Invoke(this))
                .OnComplete(() => OnEnd?.Invoke(this));
        }

        public void Resume()
        {
            _cashedTimer?.Play();
        }
        
        public void Pause()
        {
            _cashedTimer?.Pause();
        }
    }
}
