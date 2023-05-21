using UnityEngine.SceneManagement;

public abstract class AbstractGameplayProviderService<T>
{
    protected abstract string SceneName { get;}
    public T Data { get; protected set; }
    
    public virtual void StartGameplay(T data)
    {
        Data = data;
        LoadScene();
    }

    protected void LoadScene()
    {
        SceneManager.LoadScene(SceneName);
    }
}
