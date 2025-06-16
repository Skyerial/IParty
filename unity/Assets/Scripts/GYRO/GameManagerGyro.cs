using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Score Tracking")]
    public int moleHits = 0;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddMoleHit()
    {
        moleHits++;
        Debug.Log($"Mole hit! Total: {moleHits}");
    }

    public void RemoveMoleHit()
    {
        if (moleHits != 0)
        {
            moleHits--;
            Debug.Log($"Bomb hit! Total: {moleHits}");
        }
    }
}
