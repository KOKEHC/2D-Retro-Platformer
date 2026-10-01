using UnityEngine;

public class InfiniteBackground : MonoBehaviour
{
    [Header(" амера и скорость движени€")]
    public GameObject cam;
    public float parallaxEffect;

    private float length, startpos;
    private float yOffset; // «апоминаем разницу по высоте между фоном и камерой

    void Start()
    {
        startpos = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;

        // јвтоматически запоминаем начальный сдвиг по высоте относительно камеры
        if (cam != null)
        {
            yOffset = transform.position.y - cam.transform.position.y;
        }
    }

    void LateUpdate()
    {
        if (cam != null)
        {
            float temp = (cam.transform.position.x * (1 - parallaxEffect));
            float dist = (cam.transform.position.x * parallaxEffect);

            // ѕо оси X двигаем с эффектом параллакса
            // ѕо оси Y жестко следуем за камерой с учетом начального смещени€ (yOffset)
            transform.position = new Vector3(
                startpos + dist,
                cam.transform.position.y + yOffset,
                transform.position.z
            );

            if (temp > startpos + length)
            {
                startpos += length;
            }
            else if (temp < startpos - length)
            {
                startpos -= length;
            }
        }
    }
}