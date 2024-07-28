using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class FirstPersonController : MonoBehaviour
{
    public bool CanMove {get; private set; } = true;
    private bool IsSprinting => canSprint && Input.GetKey(sprintKey);
    private bool ShouldJump => Input.GetKeyDown(jumpKey) && characterController.isGrounded;
    private bool ShouldCrouch => Input.GetKeyDown(crouchKey) && !duringCrouchAnimation && characterController.isGrounded;
    private bool ShouldCombat => Input.GetKeyDown(combatKey) && characterController.isGrounded && canCombat;
    private bool ShouldParry => Input.GetKeyDown(blockKey) && characterController.isGrounded && canCombat;

    [Header("Functional Options")]
    [SerializeField] private bool canSprint = true;
    [SerializeField] private bool canJump = true;
    [SerializeField] private bool canCrouch = true;
    [SerializeField] private bool canUseHeadbob = true;
    [SerializeField] private bool willSlideOnSlopes = true;
    [SerializeField] private bool useFootSteps = true;
    [SerializeField] private bool useStamina = true;
    [SerializeField] private bool canCombat = true;

    [Header("Controls")]
    [SerializeField] private KeyCode sprintKey = KeyCode.LeftShift;
    [SerializeField] private KeyCode jumpKey = KeyCode.Space;
    [SerializeField] private KeyCode crouchKey = KeyCode.LeftControl;
    [SerializeField] private KeyCode combatKey = KeyCode.Mouse0;
    [SerializeField] private KeyCode blockKey = KeyCode.Mouse1;

    [Header("Movement Parameters")]
    [SerializeField] private float walkSpeed = 4.5f;
    [SerializeField] private float sprintSpeed = 7f;
    [SerializeField] private float crouchSpeed = 2.2f;
    [SerializeField] private float slopeSpeed = 5f;

    [Header("Dash Parameters")]
    [SerializeField] public float dashDistance = 5f;
    [SerializeField] public float dashCooldown = 1f;
    [SerializeField] public float dashStaminaCost = 10f;
    [SerializeField] public AudioClip dashSoundClip;
    [SerializeField] public Canvas dashCanvas;

    private bool canDash = true;
    private bool isDashing = false;
    private int aKeyPressCount = 0;
    private int dKeyPressCount = 0;
    private float keyPressInterval = 0.25f; // Hızlı tuş basım süresi
    private float lastAPressTime;
    private float lastDPressTime;
    private AudioSource audioSource; 
    

    [Header("Look Parameters")]
    [SerializeField, Range(1, 10)] private float lookSpeedX = 2.0f;
    [SerializeField, Range(1, 10)] private float lookSpeedY = 2.0f;
    [SerializeField, Range(1, 180)] private float upperLookLimit = 80.0f;
    [SerializeField, Range(1, 180)] private float lowerLookLimit = 80.0f;

    [Header("Jumping Parameters")]
    [SerializeField] private float jumpForce = 8.0f;
    [SerializeField] private float gravity = 20.0f;

    [Header("Crouch Parameters")]
    [SerializeField] private float crouchHeight = 0.5f;
    [SerializeField] private float standingHeight = 1.8f;
    [SerializeField] private float timeToCrouch = 0.25f;
    [SerializeField] private Vector3 crouchingCenter = new Vector3(0,0.5f,0);
    [SerializeField] private Vector3 standingCenter = new Vector3(0,0,0);
    private bool isCrouching;
    private bool duringCrouchAnimation;

    [Header("Headbob Parameters")]
    [SerializeField] private float walkBobSpeed = 14f;
    [SerializeField] private float walkBobAmount = 0.05f;
    [SerializeField] private float sprintBobSpeed = 18f;
    [SerializeField] private float sprintBobAmount = 0.11f;
    [SerializeField] private float crouchBobSpeed = 8f;
    [SerializeField] private float crouchBobAmount = 0.025f;
    private float defaultYPos = 0;
    private float timer;

    [Header("Health Parameters")]
    [SerializeField] private float maxHealth = 100;
    [SerializeField] private float timeBeforeRegenStarts = 8;
    [SerializeField] private float healthValueIncrement = 1;
    [SerializeField] private float healthTimeIncrement = 0.15f;
    public float currentHealth;
    private Coroutine regeneratingHealth;
    public static Action<float> onTakeDamage;
    public static Action<float> onDamage;
    public static Action<float> onHeal;

    [Header("Stamina Parameters")]
    [SerializeField] private float maxStamina = 100;
    [SerializeField] private float staminaUseMultiplier = 5;
    [SerializeField] private float timeBeforeStaminaRegenStarts = 5;
    [SerializeField] private float staminaValueIncrement = 2;
    [SerializeField] private float staminaTimeIncrement = 0.1f;
    private float currentStamina;
    private Coroutine regeneratingStamina;
    public static Action<float> onStaminaChange;

    [Header("Combat Parameters")]
    public GameObject hitEffect;
    public AudioClip attack1Sound;
    public AudioClip attack2Sound;
    public AudioClip attack3Sound;
    public AudioClip blockSound;
    public AudioClip damageSound;
    public AudioClip hitSound;
    public float attackDistance = 3f; // Combo pencere süresi
    public float attackDelay = 0.4f; // Saldırı cool down süresi
    private float attackSpeed = 0.4f;
    public int attackDamage = 20;
    public float parryDuration = 0.25f; // Parry süresi
    public LayerMask attackLayer;
    bool attacking = false;
    bool readyToAttack = true;
    int attackCount;

    //Animations
    public const string IDLE = "Idle";
    public const string ATTACK1 = "Attack1";
    public const string ATTACK2 = "Attack2";
    public const string ATTACK3 = "Attack3";
    public const string BLOCK = "Block";

    
    string currentAnimationState;



    [Header("Footstep Parameters")]
    [SerializeField] private float baseStepSpeed = 0.5f;
    [SerializeField] private float crouchStepMultipler = 1.5f;
    [SerializeField] private float sprintStepMultipler = 0.6f;
    [SerializeField] private AudioSource footstepAudioSource = default;
    [SerializeField] private AudioClip[] defaultStepSound = default; //u can change the variable name as your ground origin (woodClip etc.)
    //[SerializeField] private AudioClip[] clip2 = default;
    //[SerializeField] private AudioClip[] clip3 = default;
    private float footstepTimer = 0;
    private float GetCurrentOffset => isCrouching ? baseStepSpeed * crouchStepMultipler : IsSprinting ? baseStepSpeed * sprintStepMultipler : baseStepSpeed;
    
    //sliding parameters
    private Vector3 hitPointNormal;
    private bool isSliding 
    {
        get {
            if(characterController.isGrounded && Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, 2f))
            {
                hitPointNormal = slopeHit.normal;
                return Vector3.Angle(hitPointNormal, Vector3.up) > characterController.slopeLimit;
            }
            else
            {
                return false;
            }
        }
    }

    
    
    private Camera playerCamera;
    private Animator animator;
    private CharacterController characterController;

    private Vector3 moveDirection;
    private Vector2 currentInput;

    private float rotationX = 0;

    public bool IsJumping { get; private set; } = false; // sonradan duruma göre kullanmak için bıraktım
    public bool IsCrouching { get; private set; } = false; // sonradan duruma göre kullanmak için bıraktım

    
    // damage almak
    private void OnEnable()
    {
        onTakeDamage += ApplyDamage;
    }

    private void Enable()
    {
        onTakeDamage -= ApplyDamage;
    }

    
    
    
    void Awake()
    {
        if(damageEffect == null)
        {
            damageEffect = GetComponent<DamageEffect>();
        }
        if (dashCanvas != null)
        {
            dashCanvas.gameObject.SetActive(false); // Başlangıçta Canvas'ı gizle
        }
        playerCamera = GetComponentInChildren<Camera>();
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
        defaultYPos = playerCamera.transform.localPosition.y;
        currentHealth = maxHealth;
        currentStamina = maxStamina;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        if(CanMove)
        {
            HandleMovementInput();
            HandleMouseLook();
            HandleDash();
            HandleBlock();
            if(canJump)
                HandleJump();
            if(canCrouch)
                HandleCrouch();
            if(canUseHeadbob)
                HandleHeadbob();
            if(useFootSteps)
                HandleFootsteps();
            if(useStamina)
                HandleStamina();
            if(canCombat)
                HandleAttack();
            SetAnimations();
            if(willSlideOnSlopes)
                HandleSliding();
            ApplyFinalMovements();
        }
    }

    
    
    //----------------//
    //   MOVEMENT    //
    //               //
    private void HandleMovementInput()
    {
        currentInput = new Vector2((isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Vertical"), (isCrouching ? crouchSpeed : IsSprinting ? sprintSpeed : walkSpeed) * Input.GetAxis("Horizontal"));

        float moveDirectionY = moveDirection.y;
        moveDirection = (transform.TransformDirection(Vector3.forward) * currentInput.x) + (transform.TransformDirection(Vector3.right) * currentInput.y);
        moveDirection.y = moveDirectionY;
    }

    
    //Dash
    private void HandleDash()
    {
        if (canDash == true && currentStamina >= dashStaminaCost)
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                if (Time.time - lastAPressTime < keyPressInterval)
                {
                    aKeyPressCount++;
                }
                else
                {
                    aKeyPressCount = 1;
                }

                lastAPressTime = Time.time;

                if (aKeyPressCount >= 3 && canDash)
                {
                    Dash(-1);
                    SetStamina(currentStamina - dashStaminaCost); // Stamina azaltma
                    aKeyPressCount = 0;
                }
            }

            if (Input.GetKeyDown(KeyCode.D))
            {
                if (Time.time - lastDPressTime < keyPressInterval)
                {
                    dKeyPressCount++;
                }
                else
                {
                    dKeyPressCount = 1;
                }

                lastDPressTime = Time.time;

                if (dKeyPressCount >= 3 && canDash)
                {
                    Dash(1);
                    SetStamina(currentStamina - dashStaminaCost); // Stamina azaltma
                    dKeyPressCount = 0;
                }
            }
        }
    }

    private void Dash(int direction)
    {
        Vector3 dashDirection = Camera.main.transform.right * direction;
        Vector3 dashPosition = transform.position + dashDirection.normalized * dashDistance;
        
        characterController.Move(dashDirection.normalized * dashDistance);
        canDash = false;

        if (dashSoundClip != null)
        {
            audioSource.clip = dashSoundClip;
            audioSource.Play();
        }

        if (dashCanvas != null)
        {
            dashCanvas.gameObject.SetActive(true);
            Invoke("HideDashCanvas", 0.2f); // Canvas'ı 0.2 saniye sonra gizle
        }

        Invoke(nameof(ResetDash), dashCooldown);
    }

    private void ResetDash()
    {
        canDash = true;
    }

    private void HideDashCanvas()
    {
        if (dashCanvas != null)
        {
            dashCanvas.gameObject.SetActive(false);
        }
    }
    //--------------//



    
    //---------------//
    //   MOUSELOOK   //
    //               //
    private void HandleMouseLook()
    {
        rotationX -= Input.GetAxis("Mouse Y") * lookSpeedY;
        rotationX = Mathf.Clamp(rotationX, -upperLookLimit, lowerLookLimit);
        playerCamera.transform.localRotation = Quaternion.Euler(rotationX, 0, 0);
        transform.rotation *= Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeedX, 0);
    }
    //-------------//



    
    //----------------//
    //                //
    //                //
    private void ApplyFinalMovements()
    {
        if (!characterController.isGrounded)
            moveDirection.y -= gravity * Time.deltaTime;

        if (willSlideOnSlopes && isSliding)
            moveDirection += new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * slopeSpeed;

        characterController.Move(moveDirection * Time.deltaTime);
    }
    //--------------//
    
    
    
    
    //---------------//
    //     JUMP      //
    //               //
    private void HandleJump()
    {
        if (ShouldJump){
            moveDirection.y = jumpForce;
            IsJumping = true;
        }else{
            IsJumping = false;
        }
    }
    //--------------//



    
    //---------------//
    //    CROUCH     //
    //               //
    private void HandleCrouch()
    {
        if (ShouldCrouch)
            StartCoroutine(CrouchStand());
    }

    private IEnumerator CrouchStand()
    {
        if (isCrouching && Physics.Raycast(playerCamera.transform.position, Vector3.up, 1f))
            yield break;

        duringCrouchAnimation = true;

        float timeElapsed = 0;
        float targetHeight = isCrouching ? standingHeight : crouchHeight;
        float currentHeight = characterController.height;
        Vector3 targetCenter = isCrouching ? standingCenter : crouchingCenter;
        Vector3 currentCenter = characterController.center;

        while(timeElapsed < timeToCrouch)
        {
            characterController.height = Mathf.Lerp(currentHeight, targetHeight, timeElapsed/timeToCrouch);
            characterController.center = Vector3.Lerp(currentCenter, targetCenter, timeElapsed/timeToCrouch);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        characterController.height = targetHeight;
        characterController.center = targetCenter;

        isCrouching = !isCrouching;
        IsCrouching = isCrouching;

        duringCrouchAnimation = false;
    }
    //------------//
    
    
    
    
    //------------------//
    //     HEADBOB      //
    //                  //
    private void HandleHeadbob()
    {
        if (!characterController.isGrounded) return;

        if(Mathf.Abs(moveDirection.x) > 0.1f || Mathf.Abs(moveDirection.z) > 0.1f)
        {
            timer += Time.deltaTime * (isCrouching ? crouchBobSpeed : IsSprinting ? sprintBobSpeed : walkBobSpeed);
            playerCamera.transform.localPosition = new Vector3(
                playerCamera.transform.localPosition.x,
                defaultYPos + Mathf.Sin(timer) * (isCrouching ? crouchBobAmount : IsSprinting ? sprintBobAmount : walkBobAmount),
                playerCamera.transform.localPosition.z);
        }
    }
    //-----------------//
    
    
    
    
    //---------------------//
    //      SLIDING        //
    //                     //
    private void HandleSliding()
    {
        if (isSliding)
        {
            moveDirection += new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * slopeSpeed;
        }
    }
    //-------------------//



    
    //--------------------//
    //       HEALTH       //
    //                    //
    private IEnumerator RegenerateHealth()
    {
        yield return new WaitForSeconds(timeBeforeRegenStarts);
        WaitForSeconds timeToWait = new WaitForSeconds(healthTimeIncrement);

        while(currentHealth < maxHealth)
        {
            currentHealth += healthValueIncrement;
            if (currentHealth > maxHealth)
                currentHealth = maxHealth;
            
            onHeal?.Invoke(currentHealth);
            
            yield return timeToWait;
        }

        regeneratingHealth = null;
    }
    //----------------------//



    
    //------------------//
    //     STAMINA      //
    //                  //
    private void HandleStamina()
    {
        if(IsSprinting && currentInput != Vector2.zero)
        {
            if(regeneratingStamina != null)
            {
                StopCoroutine(regeneratingStamina);
                regeneratingStamina = null;
            }
            
            currentStamina -= staminaUseMultiplier * Time.deltaTime;
            SetStamina(currentStamina);

            onStaminaChange?.Invoke(currentStamina);
            
            if (currentStamina <= 0)
                canSprint = false;
        }

        if (!IsSprinting && currentStamina < maxStamina && regeneratingStamina == null)
        {
            regeneratingStamina = StartCoroutine(RegenerateStamina());
        }
    }

    private IEnumerator RegenerateStamina()
    {
        yield return new WaitForSeconds(timeBeforeStaminaRegenStarts);
        WaitForSeconds timeToWait = new WaitForSeconds(staminaTimeIncrement);

        while(currentStamina < maxStamina)
        {
            if (currentStamina > 0)
                canSprint = true;
            currentStamina += staminaValueIncrement;
            SetStamina(currentStamina);
            
            onStaminaChange?.Invoke(currentStamina);
            yield return timeToWait;
        }

        regeneratingStamina = null;
    }

    private void SetStamina(float value)
    {
        currentStamina = Mathf.Clamp(value, 0, maxStamina);
    }
    //------------------//
    


    
    //-----------------//
    //    FOOTSTEPS    //
    //                 //
    private void HandleFootsteps()
    {
        if (!characterController.isGrounded) return;
        if (currentInput == Vector2.zero) return;

        footstepTimer -= Time.deltaTime;

        if (footstepTimer <= 0)
        {
            if(Physics.Raycast(playerCamera.transform.position, Vector3.down, out RaycastHit hit, 3))
            {
                switch (hit.collider.tag)
                {
                    case "Footsteps/Normal":
                        footstepAudioSource.PlayOneShot(defaultStepSound[UnityEngine.Random.Range(0, defaultStepSound.Length - 1)]);
                        break;
                    default:
                        footstepAudioSource.PlayOneShot(defaultStepSound[UnityEngine.Random.Range(0, defaultStepSound.Length - 1)]);
                        break;
                }
            }
            footstepTimer = GetCurrentOffset;
        }
    }
    //----------------//



    
    //-----------------//
    //     ATTACK      //
    //                 //
    private void HandleAttack()
    {
        if(ShouldCombat)
        {
            if(!readyToAttack || attacking) return;

            readyToAttack = false;
            attacking = true;

            Invoke(nameof(ResetAttack), attackSpeed);
            Invoke(nameof(AttackRaycast), attackDelay);

            audioSource.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
            audioSource.PlayOneShot(attack1Sound);

            if(attackCount == 0)
            {
                ChangeAnimationState(ATTACK1);
                attackCount++;
            }
            else if(attackCount == 1){
                ChangeAnimationState(ATTACK2);
                attackCount++;
            }
            else{
                ChangeAnimationState(ATTACK3);
                attackCount = 0;
            }
        }
        
    }
    void ResetAttack()
    {
        attacking = false;
        readyToAttack = true;
    }

//onemli burası hasar alınırken kullanılacak, değerlerin 0 ın altına düşmesini engelliyor  
    private void SetHealth(float value)
{
    currentHealth = Mathf.Clamp(value, 0, maxHealth);
}
//

    
    private bool blockSoundPlayed = false; // Block sesi çalınıp çalınmadığını takip eden bayrak
    private void ApplyDamage(float dmg)
    {
        // Eğer block yapıyorsak
        if (IsParrying())
        {
            RaycastHit hit;
            blockSoundPlayed = false; // Sesi çalınmadığını varsayıyoruz
            // Block sırasında çarpışmayı kontrol et
            if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, attackDistance))
            {
                if (hit.transform.CompareTag("Enemy"))
                {
                    // Enemy tag'ine sahip objelere karşı block yapıldığında hasar alınmaz
                    PlayBlockSound(); // Block sesini çal
                    return; // Hasarı uygulama
                }
            }
        }

        currentHealth -= dmg;
        SetHealth(currentHealth);

        onDamage?.Invoke(currentHealth);

        if (damageEffect != null)
        {
            damageEffect.TakeDamage(currentHealth);
        }
            
        if (currentHealth <= 0)
            KillPlayer();
        else if (regeneratingHealth != null)
            StopCoroutine(regeneratingHealth);
        
        regeneratingHealth = StartCoroutine(RegenerateHealth());
    }
    private void KillPlayer()
    {
        SetHealth(0);
        
        if (regeneratingHealth != null)
            StopCoroutine(regeneratingHealth);
        
        print("DEAD");
    }

    
    void AttackRaycast() //HITCONTROL
    {
        GameObject hitEffectInstance;
        if(Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out RaycastHit hit, attackDistance, attackLayer))
        {
            HitTarget(hit.point);

            if(hit.transform.TryGetComponent<Enemy>(out Enemy T))
                {
                    T.TakeDamage(attackDamage);
                    hitEffectInstance = Instantiate(hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
                    Destroy(hitEffectInstance, 0.3f);
                }
        }
        /*GameObject hitEffectInstance = Instantiate(hitEffect, hit.point, Quaternion.LookRotation(hit.normal));
        Destroy(hitEffectInstance, 0.3f);*/
    }

    void HitTarget(Vector3 pos){
        audioSource.pitch = 1;
        audioSource.PlayOneShot(hitSound);

        GameObject GO = Instantiate(hitEffect, pos, Quaternion.identity);
        Destroy(GO, 20);
    }
    //---------------//

    
    
    
    //--------------//
    //    Block     //
    //              //
    private void HandleBlock()
    {
        if (ShouldParry)
        {
            if (Input.GetMouseButtonDown(1))
            {
                if (!animator.GetBool("Blocked"))
                {
                    // Parry animasyonunu başlat
                    animator.SetBool("Blocked", true);

                    // Diğer saldırıları iptal et
                    attacking = false;
                    readyToAttack = true;
                    attackCount = 0;

                    StartCoroutine(CheckParryCollision());
                }
            }
            if (Input.GetMouseButtonUp(1)) // Sağ mouse tuşu bırakıldığında
            {
                if (animator.GetBool("Blocked"))
                {
                    // Parry animasyonunu durdur ve idle'a dön
                    animator.SetBool("Blocked", false);
                }
            }
        }
    }

    private IEnumerator ResetToIdleAfterParry() //parry = block
    {
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);
        animator.SetBool("Blocked", false);
    }

    private IEnumerator CheckParryCollision()
    {
        yield return new WaitForSeconds(parryDuration);

        RaycastHit hit;
        if (Physics.Raycast(playerCamera.transform.position, playerCamera.transform.forward, out hit, attackDistance))
        {
            if (hit.transform.CompareTag("Enemy"))
            {
                // Çarpışma sesi çal
                audioSource.PlayOneShot(blockSound);
            }
        }
    }

    public bool IsParrying()
    {
        return animator.GetBool("Blocked");
    }
    
    public DamageEffect damageEffect;
    
    
    private void PlayBlockSound()
    {
        audioSource.PlayOneShot(blockSound);
        blockSoundPlayed = true; // Sesi çaldı olarak işaretle
    }
    //-----------// 

    
    
    
    // ----------------- //
    //  Animation state  //
    //                   //
    public void ChangeAnimationState(string newState)
    {   
        if(currentAnimationState == newState) return;

        currentAnimationState = newState;
        animator.CrossFadeInFixedTime(currentAnimationState, 0.2f);
    }
    //---------------//
    
    
    
    
    //-----------------//
    //  Set Animation  // 
    //                 //
    void SetAnimations()
    {
        if(!attacking)
        {
            if(moveDirection.x == 0 && moveDirection.z == 0){
                ChangeAnimationState(IDLE);
            }
        }
    }
    //------------------//

}
