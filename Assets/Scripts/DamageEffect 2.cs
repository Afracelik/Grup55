using UnityEngine;
using UnityEngine.UI;

public class DamageEffect : MonoBehaviour
{
    public Image damageImage;
    public AudioClip damageSound;
    public float flashSpeed = 5f;
    public Color flashColor = new Color(1f, 0f, 0f, 0.3f);
    private AudioSource audioSource;
    private bool damaged = false;
    private float damageTimer = 0f;
    private float damageDuration = 1f;
    private FirstPersonController playerController;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        damageImage.color = Color.clear;
        playerController = FindObjectOfType<FirstPersonController>();
    }

    private bool controll = false;
    void Update()
    {
        // Kırmızı etkisini yönet
        if (damaged && !controll)
        {
            damageImage.color = flashColor;
            damageTimer += Time.deltaTime;
            if (damageTimer >= damageDuration)
            {
                damaged = false;
                damageTimer = 0f;
            }

            if (playerController.currentHealth <= 30)
                controll = true;
        }
        else
        {
            // Sağlık 30'un altındaysa daha belirgin kırmızı
            float targetAlpha = 0f;
            if (playerController.currentHealth < 30)
            {
                targetAlpha = 0.5f;
            }
            else if (playerController.currentHealth >= 45)
            {
                targetAlpha = 0f;
                controll = false; // Sağlık 45'in üstüne çıktığında kırmızılığı kaldır
            }
            damageImage.color = new Color(flashColor.r, flashColor.g, flashColor.b, Mathf.Lerp(damageImage.color.a, targetAlpha, flashSpeed * Time.deltaTime));
        }
    }

    public void TakeDamage(float health)
    {
        damaged = true;
        playerController.currentHealth = health;
        damageTimer = 0f;
        audioSource.PlayOneShot(damageSound);
    }
}
