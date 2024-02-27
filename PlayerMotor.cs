using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEditor.Experimental.GraphView;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Animations;

public class PlayerController : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D rb;

    [Header("Horizontal Movement")]
    public float moveSpeed = 10f;   //.95f
    private float defaultMoveSpeed;
    private bool isMoving;

    [Header("Vertical Movement")]
    public float normalJumpFroce = 10f; //11
    public bool canJump = true;
    private float defaultJumpForce;

    [Header("Grounded")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private LayerMask whatIsGround;
    [Range(0f, 1f)]
    [SerializeField] private float groundRadius = .2f;    //.4f
    bool isGrounded = false;

    [Header("Wall Jump")]
    [SerializeField] private float wallJumpTime = .2f;
    [SerializeField] private float wallSlideSpeed = .3f;  //.4f
    [SerializeField] private float wallDistance = .5f;    //.53f
    public bool isWallSliding = false;
    RaycastHit2D wallCheckHit;
    float jumpTime;
    float lastWallSlideTime;

    [Header("Jump")]
    public bool canDoubleJump = true;   // Enable/disable doubleJump feature
    public float fallMultiplier = 2.5f; //3.6
    public float lowJumpMultiplier = 2f;    //3
    [SerializeField] private bool doubleJump;
    public float timeToWait = .5f;

    [Header("Damping")]
    [SerializeField] private float horizontalDampingBasic;        //.4f
    [SerializeField] private float horizontalDamingWhenStopping;  //.55f
    [SerializeField] private float horizontalDamingWhenTurning;   //.9f
    [SerializeField] private float invokeDamping = 1f;            //0f
    private float actucalDamping;

    //Jump buffer time
    float fJumpPressedRemember = 0;
    float fJumpPressRememberTime = 0.2f;
    float fGroundedRemember = 0;
    float fGroundedRemeberTime = 0.2f;

    public bool isFacingRight = true;

    [Header("Dash")]
    public bool canDash = true;
    private bool isDashing;
    public float dashingPower = 24f;
    public float dashingTime = .2f;
    public float dashingCooldown = 1f;      //.6f
    [SerializeField] private TrailRenderer tr;


    [Header("Animations")]
    private Animator anim;
    private float lockedTill;

    private int currentState;
    private bool isStateLocked = false;
    private static readonly int Idle = Animator.StringToHash("Idle");
    private static readonly int Run = Animator.StringToHash("Run");
    private static readonly int anim_Jump = Animator.StringToHash("Jump");
    private static readonly int Fall = Animator.StringToHash("Fall");
    private static readonly int wallSlide = Animator.StringToHash("WallSlide 0");

    private void Start()
    {
        anim = GetComponent<Animator>();

        //Settings for damping
        actucalDamping = horizontalDampingBasic;
        defaultMoveSpeed = moveSpeed;
        defaultJumpForce = normalJumpFroce;

        if (rb == null)
        {
            //Get Rigidbody on Player Object
            rb = GetComponent<Rigidbody2D>();
            Debug.Log("Rigidbody was automatically assigned.");
        }
    }

    private void Update()
    {
        int state = GetState();
        anim.CrossFade(state, 0, 0);

        if (!canDoubleJump)
            doubleJump = false;

        //If is dashing, do not move, Jump, dash...
        if (isDashing)
            return;

        //Heandle coyote-time
        fJumpPressedRemember -= Time.deltaTime;
        if (Input.GetButtonDown("Jump"))
        {
            fJumpPressedRemember = fJumpPressRememberTime;
        }

        if (isGrounded && !Input.GetButton("Jump"))
        {
            doubleJump = false;
        }

        //Handle Jump
        if (isGrounded && (fJumpPressedRemember > 0))
        {
            fJumpPressedRemember = 0;
            //fGroundedRemeber = 0;
            Jump();
            doubleJump = true;
        }
        else if (isWallSliding && (fJumpPressedRemember > 0) && canJump)
        {
            fJumpPressedRemember = 0;
            Jump();
        }
        //double jump from wall
        if (doubleJump && (fJumpPressedRemember > 0))
        {
            fJumpPressedRemember = 0;
            SecondJump();
            doubleJump = false; //!doubleJump
        }

        //Allow to doubleJump if was WallSliding Recently
        if (isWallSliding)
        {
            lastWallSlideTime = Time.time;
        }

        if (WasWallSlidingRecently())
        {
            Debug.Log("Was Wall Sliding");
            doubleJump = true;
        }

        //Add speed when falling
        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }

        //Dashing
        if (Input.GetButtonDown("Fire3") && canDash)
        {
            StartCoroutine(Dash());
        }
    }

    private void FixedUpdate()
    {
        //If dashing stop all movement
        if (isDashing)
            return;

        //Horizontal Movement
        float mx = rb.velocity.x; //mx = movementX
        mx += Input.GetAxisRaw("Horizontal");


        if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) < 0.01f)
            mx *= Mathf.Pow(1f - horizontalDamingWhenStopping, Time.deltaTime * 10f);
        else if (Mathf.Sign(Input.GetAxisRaw("Horizontal")) != Mathf.Sign(mx))
            mx *= Mathf.Pow(1f - horizontalDamingWhenTurning, Time.deltaTime * 10f);
        else
            mx *= Mathf.Pow(1f - horizontalDampingBasic, Time.deltaTime * 10f);

        rb.velocity = new Vector2(mx, rb.velocity.y);

        if(Mathf.Abs(mx) > 0.01f)
            isMoving = true;

        //Facing right/left
        if (mx < 0f) //right == -1f
        {
            isFacingRight = false;
            transform.localScale = new Vector2(-1, transform.localScale.y); //public float PlayerXSize; -PlayerXSize
        }
        else if (mx > 0.0001f)
        {
            isFacingRight = true;
            transform.localScale = new Vector2(1, transform.localScale.y); // PlayerXSize
        }

        rb.velocity = new Vector2(mx * moveSpeed, rb.velocity.y);

        //isGrounded check
        bool touchingGround = Physics2D.OverlapCircle(groundCheckPoint.position, groundRadius, whatIsGround);

        if (touchingGround)
        {
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }

        #region WallJump
        //Wall Jump
        if (isFacingRight)
        {
            wallCheckHit = Physics2D.Raycast(transform.position, new Vector2(wallDistance, 0), wallDistance, whatIsGround);
            Debug.DrawRay(transform.position, new Vector2(wallDistance, 0), Color.red);
        }
        else
        {
            wallCheckHit = Physics2D.Raycast(transform.position, new Vector2(-wallDistance, 0), wallDistance, whatIsGround);
            Debug.DrawRay(transform.position, new Vector2(-wallDistance, 0), Color.red);
        }

        if (wallCheckHit && !isGrounded && mx != 0)
        {
            isWallSliding = true;
        }
        else
        {
            isWallSliding = false;
        }

        #endregion

        //(is)WallSliding
        if (isWallSliding)
        {

            horizontalDampingBasic = 0.0f;
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlideSpeed, float.MaxValue));
        }
        else if (!isWallSliding)
        {
            Invoke("Damping", invokeDamping);
        }
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, normalJumpFroce);

        canJump = false;
        Invoke("UpdateJumping", timeToWait);
    }

    void SecondJump()
    {
        rb.velocity = new Vector2(rb.velocity.x, normalJumpFroce);

        //animator.SetBool("isJumping", true);
    }

    void UpdateJumping()
    {
        //animator.SetBool("isJumping", false);
        canJump = true;
    }

    void Damping()
    {
        if (!isWallSliding)
            horizontalDampingBasic = actucalDamping;
    }

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        //animator.SetBool("isDashing", true);
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.velocity = new Vector2(transform.localScale.x * dashingPower, 0f);
        tr.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        tr.emitting = false;
        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooldown);
        canDash = true;
        //animator.SetBool("isDashing", false);
    }

    private int GetState()
    {
        if (Time.time < lockedTill) return currentState;

        int highestPriotityState = Idle;

        if (isMoving && !isWallSliding)
            highestPriotityState = LockState(Run, 0);
        if (isWallSliding)
            highestPriotityState = Input.GetButtonDown("Jump") == true ? LockState(anim_Jump, 0.23f) : wallSlide;
        if (isGrounded)
            highestPriotityState = Input.GetAxisRaw("Horizontal") == 0 ? Idle : Run;
        if (!isGrounded && !isWallSliding)
        {
            if (rb.velocity.y > 0)
                highestPriotityState = LockState(anim_Jump, 0.17f);
            else
                highestPriotityState = Fall;
        }

        return highestPriotityState;
    }
    private int LockState(int s, float t)
    {
        lockedTill = Time.time + t;
        isStateLocked = true;
        StartCoroutine(UnlockStateAfterDelay(t));
        return s;
    }
    private IEnumerator UnlockStateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        isStateLocked = false;
    }

    private bool WasWallSlidingRecently()
    {
        if (!isWallSliding & (Time.time - lastWallSlideTime) < 0.1f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void OnDrawGizmos()
    {
        //Ground-check Sphere
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(groundCheckPoint.position, groundRadius);
    }
}
