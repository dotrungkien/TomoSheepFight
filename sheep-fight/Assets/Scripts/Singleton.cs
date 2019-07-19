using UnityEngine;

public abstract class Singleton<T_TYPE> : MonoBehaviour where T_TYPE : Singleton<T_TYPE>
{
    public static T_TYPE GetInstance()
    {
        if (_instance == null)
        {
            _instance = GameObject.FindObjectOfType(typeof(T_TYPE)) as T_TYPE;
        }
        return _instance;
    }

    private void Awake()
    {
        T_TYPE instance = GetInstance();
        if (instance == null) { instance = this as T_TYPE; }
        if (instance != (this as T_TYPE))
        {
            Destroy(gameObject);
            return;
        }

        _instance = instance;
        GameObject.DontDestroyOnLoad(gameObject);
    }

    private static T_TYPE _instance = null;
}