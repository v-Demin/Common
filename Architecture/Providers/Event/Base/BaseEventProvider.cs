using System;
using System.Collections.Generic;

namespace Submodules.Common.Architecture.Providers
{
    public abstract class BaseEventProvider<T> : AbstractEventProvider where T : Enum
    {
        protected Dictionary<T, RefAction> ActionDictionary = new Dictionary<T, RefAction>();
    
        public BaseEventProvider()
        {
            var tValues = (T[]) Enum.GetValues(typeof(T));
            foreach (var value in tValues)
            {
                ActionDictionary.Add(value, new RefAction());
            }
            LoadActions();
        }
    
        public void ActionAchieved(T value)
        {
            ActionDictionary[value]?.Action?.Invoke();
        }
    
        public RefAction GetRefAction(T value)
        {
            return ActionDictionary[value];
        }
    
        protected abstract void LoadActions();


        #region InnerClasses

        public class RefAction
        {
            public Action Action { get; set; }
        
            public RefAction(): this(default) { }

            public RefAction(Action action)
            {
                Action = action;
            }
        
            public static RefAction operator +(RefAction me, Action action)
            {
                return new RefAction(me.Action + action);
            }
        
            public static RefAction operator -(RefAction me, Action action)
            {
                return new RefAction(me.Action - action);
            }
        }

        #endregion
    }
}
