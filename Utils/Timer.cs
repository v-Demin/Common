using System;
using DG.Tweening;

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
        private bool _isLooped = false;
        
        public Timer(bool isLooped)
        {
            AllSeconds = 0;
            CurrentSeconds = 0;
            _isLooped = isLooped;
        }

        public void Start(float seconds)
        {
            AllSeconds = seconds;
            
            _cashedTimer = DOVirtual.Float(CurrentSeconds, AllSeconds, AllSeconds, value => OnUpdate?.Invoke(this))
                .OnStart(() => OnStart?.Invoke(this))
                .OnPause(() => OnPause?.Invoke(this))
                .OnComplete(() => OnEnd?.Invoke(this));

            if (_isLooped)
            {
                _cashedTimer.SetLoops(-1);
            }
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
