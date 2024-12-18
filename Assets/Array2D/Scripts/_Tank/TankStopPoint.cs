using UnityEngine;

public class TankStopPoint : MonoBehaviour
{
    public int StopIndex; // Hangi s?radaki durak oldu?unu belirtir

    private void OnDrawGizmos()
    {
        // G�rsel rehberlik i�in bir k�re �iz
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.5f);
    }
}
