using UnityEngine;

public enum LockState
{
    Locked,
    Opening,
    Unlocked
}


public class LockSystem : MonoBehaviour
{
    [SerializeField] private bool _isLocked;
    [SerializeField] private LockState currentState;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (_isLocked)
        {
            currentState = LockState.Locked;
        }
    }

    public void CheckLockState()
    {
        switch (currentState)
        {
            case LockState.Locked:
                LockedState();
                break;
            case LockState.Opening:
                OpeningDoor();
                break;
            case LockState.Unlocked:
                UnlockState();
                break;
        }
    }

    private void OpeningDoor()
    {
        //Door is opening
        // Play Door Opening anim
        currentState = LockState.Unlocked;
    }

    public void LockedState()
    {
        //Door is locked
        //if found way to open door
            // Set current state to Opening Door
        //else
            // Play lock door sound effect
    }

    public void UnlockState()
    {
        //opens
    }

}
