using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Linq;
using UnityEngine;

public class PlayerController2D : MonoBehaviour
{
    [Header("References")]
    public Rigidbody2D rb;

    [Header("Horizontal Movement")]
    public float moveSpeed = 10f;   //.95f
    private float defaultMoveSpeed;

    [Header("Vertical Movement")]
    public float normalJumpFroce = 10f; //11
    private float defaulJumpForce;
    public bool canJump = true;

    [Header("Grounded")]
    public Transform groundCheckPoint;
    public LayerMask whatIsGround;
    public float groundRadius = .2f;    //.4f
    bool isGrounded = false;

    //public float fCutJumpHeight = 0.5f;

    [Header("Wall Jump")]
    public float wallJumpTime = .2f;
    public float wallSlideSpeed = .3f;  //.4f
    public float wallDistance = .5f;    //.53f
    public float xForce = 5f;
    public bool isWallSliding = false;
    RaycastHit2D wallCheckHit;
    float jumpTime;

    [Header("Jump")]
    public bool canDoubleJump = true;
    public float fallMultiplier = 2.5f; //3.6
    public float lowJumpMultiplier = 2f;    //3
    public bool doubleJump;
    public float timeToWait = .5f;

    [Header("Is Falling")]
    public bool isFalling;

    [Header("Damping")]
    //float horizontalDaming = 0.22f;
    public float horizontalDampingBasic;        //.4f
    private float actucalDamping;
    public float horizontalDamingWhenStopping;  //.55f
    public float horizontalDamingWhenTurning;   //.9f
    public float invokeDamping = 1f;            //0f

    //Jump buffer time
    float fJumpPressedRemember = 0;
    float fJumpPressPememberTime = 0.2f;
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

    //PlayerAnimator playerAnimator;
    private void Start()
    {
        //Settings for damping
        actucalDamping = horizontalDampingBasic;
        defaultMoveSpeed = moveSpeed;
        defaulJumpForce = normalJumpFroce;

        //playerAnimator = GetComponent<PlayerAnimator>();
    }

    private void Update()
    {
        if (!canDoubleJump)
            doubleJump = false;

        //If is dashing, do not move or Jump etc...
        if (isDashing)
            return;

        fJumpPressedRemember -= Time.deltaTime;
        if (Input.GetButtonDown("Jump"))
        {

            fJumpPressedRemember = fJumpPressPememberTime;
        }

        if (isGrounded && !Input.GetButton("Jump"))
        {
            doubleJump = false;
        }

        //Handle Jump
        if (isGrounded && (fJumpPressedRemember > 0) || isWallSliding && (fJumpPressedRemember > 0) && canJump)
        {
            fJumpPressedRemember = 0;
            //fGroundedRemeber = 0;
            Jump();
            doubleJump = true;
        }
        //IS able to double jump from wall
        if (doubleJump && (fJumpPressedRemember > 0))
        {
            fJumpPressedRemember = 0;
            SecondJump();
            doubleJump = false; //!doubleJump
        }


        //Disable double jump if is Wallsliding/jumping
        //if (isWallSliding && doubleJump)
        //doubleJump = false;


        //Add Falling speed
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
        if (isDashing)
            return;


        float mx = rb.velocity.x;
        mx += Input.GetAxisRaw("Horizontal");
        //Debug.Log(mx);

        //Animate player movement
        //animator.SetFloat("Speed", Mathf.Abs(mx));

        if (Mathf.Abs(Input.GetAxisRaw("Horizontal")) < 0.01f)
            mx *= Mathf.Pow(1f - horizontalDamingWhenStopping, Time.deltaTime * 10f);
        else if (Mathf.Sign(Input.GetAxisRaw("Horizontal")) != Mathf.Sign(mx))
            mx *= Mathf.Pow(1f - horizontalDamingWhenTurning, Time.deltaTime * 10f);
        else
            mx *= Mathf.Pow(1f - horizontalDampingBasic, Time.deltaTime * 10f);

        rb.velocity = new Vector2(mx, rb.velocity.y);



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
            //canDash = false;
            //jumpTime = Time.time + wallJumpTime;
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
            //canDash = false;
        }
        else if (!isWallSliding)
        {
            Invoke("Damping", invokeDamping);
        }
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, normalJumpFroce);

        //animator.SetBool("isJumping", true);
        canJump = false;
        //jumpKey = KeyCode.None;
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

    private void OnDrawGizmos()
    {
        //Ground-check Sphere
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(groundCheckPoint.position, groundRadius);
    }
}
