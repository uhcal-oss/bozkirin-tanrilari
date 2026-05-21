using UnityEngine;

/// <summary>
/// Attach to the boss GameObject. Receives TakeDamage from SwordAttack
/// and forwards it to BossFightManager.
/// </summary>
public class BossHealth : MonoBehaviour
{
    public void TakeDamage(int damage)
    {
        if (BossFightManager.Instance != null)
        {
            BossFightManager.Instance.BossTakeDamage(damage);
        }
    }
}
