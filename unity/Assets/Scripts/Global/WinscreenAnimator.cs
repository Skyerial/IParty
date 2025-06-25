using UnityEngine;
using UnityEngine.InputSystem;

public class WinscreenAnimator : MonoBehaviour
{
    private Animator animator;
    private int playerID;
    private PlayerInput playerInput;

    void Start()
    {
        animator = GetComponent<Animator>();
        playerInput = GetComponent<PlayerInput>();

        if (PlayerManager.playerStats.ContainsKey(playerInput.devices[0]))
        {
            playerID = PlayerManager.playerStats[playerInput.devices[0]].playerID;
            PlayAnimationBasedOnRank();
        }
        else
        {
            Debug.LogWarning("Player device not found in playerStats.");
        }
    }


    void PlayAnimationBasedOnRank()
    {
        var ranking = PlayerManager.instance.rankGameboard;

        if (!ranking.Contains(playerID))
        {
            Debug.LogWarning($"Player {playerID} not found in rankGameboard.");
            return;
        }
        
        int position = ranking.IndexOf(playerID) + 1; 

        switch (position)
        {
            case 1:
                animator.Play("Win");
                break;
            case 2:
                animator.Play("Defeated");
                break;
            case 3:
                animator.Play("Crying");
                break;
            case 4:
                animator.Play("Dying");
                break;
            default:
                Debug.LogWarning("Invalid ranking position: " + position);
                break;
        }
    }
}

