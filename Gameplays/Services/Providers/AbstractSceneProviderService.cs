using UnityEngine.SceneManagement;

namespace Submodules.Common.Gameplays.Services.Providers
{
    public abstract class AbstractSceneProviderService<T>
    {
        protected abstract string SceneName { get;}
        public T Data { get; protected set; }
    
        public virtual void StartScene(T data)
        {
            Data = data;
            LoadScene();
        }

        protected void LoadScene()
        {
            SceneManager.LoadScene(SceneName);
        }
    }
}
