using System;
using System.Collections.Generic;
using System.Linq;

namespace Submodules.Common.Utils
{
    public static partial class CompletionAdapters
    {
        public static void WaitCompletion(
            this IEnumerable<Action<Action>> source,
            Action onComplete)
        {
            source.WaitCompletion((action, done) => action?.Invoke(done), onComplete);
        }
        
        public static void WaitCompletion<T>(
            this IEnumerable<T> source,
            Action<T, Action> subscribe,
            Action onComplete)
        {
            var list = source.ToList();
            int remaining = list.Count;

            if (remaining == 0)
            {
                onComplete?.Invoke();
                return;
            }

            bool completed = false;

            void ItemDone()
            {
                if (completed) return;

                remaining--;

                if (remaining <= 0)
                {
                    completed = true;
                    onComplete?.Invoke();
                }
            }

            foreach (var item in list)
            {
                subscribe(item, ItemDone);
            }
        }
        
        public static void WaitCompletionEvent<T>(
            this IEnumerable<T> source,
            Action<T, Action<Action>> subscribe,
            Action onComplete)
        {
            var list = source.ToList();
            int remaining = list.Count;

            if (remaining == 0)
            {
                onComplete?.Invoke();
                return;
            }

            bool completed = false;

            void ItemDone()
            {
                if (completed) return;

                remaining--;

                if (remaining <= 0)
                {
                    completed = true;
                    onComplete?.Invoke();
                }
            }

            foreach (var item in list)
            {
                subscribe(item, handler =>
                {
                    void Wrapper()
                    {
                        handler();
                        ItemDone();
                    }

                    handler = Wrapper;
                });
            }
        }
        
        public static Action<T, Action> FromEvent<T>(
            Action<T, Action> subscribe,
            Action<T, Action> unsubscribe)
        {
            return (obj, done) =>
            {
                void Handler()
                {
                    unsubscribe(obj, Handler);
                    done();
                }
        
                subscribe(obj, Handler);
            };
        }
    }
}
