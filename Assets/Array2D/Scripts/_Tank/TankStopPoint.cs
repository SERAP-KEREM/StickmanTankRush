using UnityEngine;

public class TankStopPoint : MonoBehaviour
{
    public int StopIndex; // Hangi s?radaki durak oldu?unu belirtir

    private void OnDrawGizmos()
    {
        // Görsel rehberlik için bir küre çiz
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.5f);
    }
}
