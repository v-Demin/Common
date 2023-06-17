using UnityEngine.SceneManagement;

namespace Submodules.Common.Architecture.Providers
{
    public abstract class AbstractSceneProviderService
    {
        protected abstract string SceneName { get;}
    
        public virtual void StartScene()
        {
            PrepareService();
            LoadScene();
        }

        protected abstract void PrepareService();
        
        protected void LoadScene()
        {
            SceneManager.LoadScene(SceneName);
        }
    }
}
