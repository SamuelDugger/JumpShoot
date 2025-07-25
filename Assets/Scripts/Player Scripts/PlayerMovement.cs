using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class PlayerMovement : MonoBehaviour
{
	public PlayerData Data;

	#region COMPONENTS
	public Rigidbody2D RB { get; private set; }
	private PlayerHealth playerHealth; // Reference to PlayerHealth

	[SerializeField] private PowerUps powerUps;
	#endregion

	#region STATE PARAMETERS
	public bool IsFacingRight { get; private set; }
	public ParticleSystem jumpDust;
	public bool IsJumping { get; private set; }
	public bool IsWallJumping { get; private set; }
	public bool IsDashing { get; private set; }
	public bool IsSliding { get; private set; }

	//Timers (also all fields, could be private and a method returning a bool could be used)
	public float LastOnGroundTime { get; private set; }
	public float LastOnWallTime { get; private set; }
	public float LastOnWallRightTime { get; private set; }
	public float LastOnWallLeftTime { get; private set; }

	//Jump
	private bool _isJumpCut;
	private bool _isJumpFalling;
	private bool canDoubleJump = false;

	private bool hasDoubleJumped = false;

	//Wall Jump
	private float _wallJumpStartTime;
	private int _lastWallJumpDir;

	//Dash
	private int _dashesLeft;
	private bool _dashRefilling;
	private Vector2 _lastDashDir;
	private bool _isDashAttacking;

	private int remainingDashes;
	private float dashCooldownTimer;

	public TrailRenderer tr;

	//Sound Effects

	public AudioSource jumpSound;

	public AudioSource dashSound;

	#endregion

	#region INPUT PARAMETERS
	private Vector2 _moveInput;
	private PlayerControls _inputActions;

	public float LastPressedJumpTime { get; private set; }
	public float LastPressedDashTime { get; private set; }
	
	[SerializeField] Animator myAnimator;

	#endregion

	#region CHECK PARAMETERS
	//Set all of these up in the inspector
	[Header("Checks")]
	[SerializeField] private Transform _groundCheckPoint;
	//Size of groundCheck depends on the size of your character generally you want them slightly small than width (for ground) and height (for the wall check)
	[SerializeField] private Vector2 _groundCheckSize = new Vector2(0.49f, 0.03f);
	[Space(5)]
	[SerializeField] private Transform _frontWallCheckPoint;
	[SerializeField] private Transform _backWallCheckPoint;
	[SerializeField] private Vector2 _wallCheckSize = new Vector2(0.5f, 1f);
	#endregion

	#region LAYERS & TAGS
	[Header("Layers & Tags")]
	[SerializeField] private LayerMask _groundLayer;
	#endregion

	private void Awake()
	{
		_inputActions = new PlayerControls();
		RB = GetComponent<Rigidbody2D>();
		playerHealth = GetComponent<PlayerHealth>(); // Initialize PlayerHealth reference
	}

	private void OnEnable()
	{
		// Enable the input actions
		_inputActions.Player.Enable();

		// Subscribe to input events
		_inputActions.Player.Move.performed += OnMove;
		_inputActions.Player.Move.canceled += OnMove;
		_inputActions.Player.Jump.performed += OnJump;
		_inputActions.Player.Jump.canceled += OnJumpUp;
		_inputActions.Player.Dash.performed += OnDash;
	}

	private void OnDisable()
	{
		// Unsubscribe from input events
		_inputActions.Player.Move.performed -= OnMove;
		_inputActions.Player.Move.canceled -= OnMove;
		_inputActions.Player.Jump.performed -= OnJump;
		_inputActions.Player.Jump.canceled -= OnJumpUp;
		_inputActions.Player.Dash.performed -= OnDash;

		// Disable the input actions
		_inputActions.Player.Disable();
	}

	private void OnMove(InputAction.CallbackContext context)
	{
		_moveInput = context.ReadValue<Vector2>();
		myAnimator.SetFloat("moveX", _moveInput.x);
	}

	private void OnJump(InputAction.CallbackContext context)
	{
		OnJumpInput();

		myAnimator.SetFloat("moveY", 1f);
		
	}

	private void OnJumpUp(InputAction.CallbackContext context)
	{
		OnJumpUpInput();
		myAnimator.SetFloat("moveY", -1f);
	}

	private void OnDash(InputAction.CallbackContext context)
	{
		OnDashInput();
		myAnimator.SetFloat("moveY", 1f);
	}

	private void Start()
	{
		SetGravityScale(Data.gravityScale);
		CheckPowerUpMovement();
	}

	private void CheckPowerUpMovement()
	{
		// Access the current movement type from the PowerUps script
		powerUps.movement = (PowerUps.Movement)PlayerPrefs.GetInt("SelectedMovement", 0);

		// Perform actions based on the movement type
		if (powerUps.movement == PowerUps.Movement.DoubleJump)
		{
			//Debug.Log("Can double jump. Movement = " + powerUps.movement);
			canDoubleJump = true;
		}
		else if (powerUps.movement == PowerUps.Movement.DoubleDash)
		{
			// Implement behavior for Double Dash if necessary
			//Debug.Log("Double Dash is enabled.");
		}
		else
		{
			canDoubleJump = false;  // Disable double jump
			//Debug.Log("Default movement type (Base) is enabled.");
		}
	}



	private void Update()
	{
		if (playerHealth.IsKnockbackActive()) return;
		#region TIMERS
		LastOnGroundTime -= Time.deltaTime;
		LastOnWallTime -= Time.deltaTime;
		LastOnWallRightTime -= Time.deltaTime;
		LastOnWallLeftTime -= Time.deltaTime;

		LastPressedJumpTime -= Time.deltaTime;
		LastPressedDashTime -= Time.deltaTime;
		#endregion

		#region COLLISION CHECKS
		if (!IsDashing && !IsJumping)
		{
			// Ground Check
			if (Physics2D.OverlapBox(_groundCheckPoint.position, _groundCheckSize, 0, _groundLayer)) //checks if set box overlaps with ground
			{
				if (LastOnGroundTime < -0.1f)
				{
	
					//AnimHandler.justLanded = true;
					myAnimator.SetFloat("moveY", 0f);
					
				}
				
				
				LastOnGroundTime = Data.coyoteTime; //if so sets the lastGrounded to coyoteTime
			}

			// Right Wall Check
			if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)
				|| (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)) && !IsWallJumping)
				LastOnWallRightTime = Data.coyoteTime;

			// Left Wall Check
			if (((Physics2D.OverlapBox(_frontWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && !IsFacingRight)
				|| (Physics2D.OverlapBox(_backWallCheckPoint.position, _wallCheckSize, 0, _groundLayer) && IsFacingRight)) && !IsWallJumping)
				LastOnWallLeftTime = Data.coyoteTime;

			// Two checks needed for both left and right walls since whenever the player turns, the wall check points swap sides
			LastOnWallTime = Mathf.Max(LastOnWallLeftTime, LastOnWallRightTime);
		}
		#endregion

		#region JUMP CHECKS
		if (IsJumping && RB.velocity.y < 0)
		{
			IsJumping = false;
			_isJumpFalling = true;
			myAnimator.SetFloat("moveY", -1f);
		}

		if (IsWallJumping && Time.time - _wallJumpStartTime > Data.wallJumpTime)
		{
			IsWallJumping = false;
		}

		if (LastOnGroundTime > 0 && !IsJumping && !IsWallJumping)
		{
			_isJumpCut = false;
			_isJumpFalling = false;
		}

		if (!IsDashing)
		{
			// Jump
			if (CanJump() && LastPressedJumpTime > 0)
			{
				IsJumping = true;
				IsWallJumping = false;
				_isJumpCut = false;
				_isJumpFalling = false;
				Jump();

				//AnimHandler.startedJumping = true;
			}
			// WALL JUMP
			else if (CanWallJump() && LastPressedJumpTime > 0)
			{
				IsWallJumping = true;
				IsJumping = false;
				_isJumpCut = false;
				_isJumpFalling = false;

				_wallJumpStartTime = Time.time;
				_lastWallJumpDir = (LastOnWallRightTime > 0) ? -1 : 1;

				WallJump(_lastWallJumpDir);
			}
		}
		#endregion

		#region DASH CHECKS
		if (CanDash() && LastPressedDashTime > 0)
		{
			// Freeze game for split second. Adds juiciness and a bit of forgiveness over directional input
			Sleep(Data.dashSleepTime);

			// If no direction pressed, dash forward
			if (_moveInput != Vector2.zero)
				_lastDashDir = _moveInput;
			else
				_lastDashDir = IsFacingRight ? Vector2.right : Vector2.left;

			IsDashing = true;
			IsJumping = false;
			IsWallJumping = false;
			_isJumpCut = false;

			StartCoroutine(nameof(StartDash), _lastDashDir);
		}
		#endregion

		#region SLIDE CHECKS
		if (CanSlide() && ((LastOnWallLeftTime > 0 && _moveInput.x < 0) || (LastOnWallRightTime > 0 && _moveInput.x > 0)))
			IsSliding = true;
		else
			IsSliding = false;
		#endregion

		#region GRAVITY
		if (!_isDashAttacking)
		{
			// Higher gravity if we've released the jump input or are falling
			if (IsSliding)
			{
				SetGravityScale(0);
			}
			else if (RB.velocity.y < 0 && _moveInput.y < 0)
			{
				// Much higher gravity if holding down
				SetGravityScale(Data.gravityScale * Data.fastFallGravityMult);
				// Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
				RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFastFallSpeed));
			}
			else if (_isJumpCut)
			{
				// Higher gravity if jump button released
				SetGravityScale(Data.gravityScale * Data.jumpCutGravityMult);
				RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFallSpeed));
			}
			else if ((IsJumping || IsWallJumping || _isJumpFalling) && Mathf.Abs(RB.velocity.y) < Data.jumpHangTimeThreshold)
			{
				SetGravityScale(Data.gravityScale * Data.jumpHangGravityMult);
			}
			else if (RB.velocity.y < 0 && LastOnGroundTime < 0)
			{
				// Higher gravity if falling
				myAnimator.SetFloat("moveY", -1f);
				SetGravityScale(Data.gravityScale * Data.fallGravityMult);
				// Caps maximum fall speed, so when falling over large distances we don't accelerate to insanely high speeds
				RB.velocity = new Vector2(RB.velocity.x, Mathf.Max(RB.velocity.y, -Data.maxFallSpeed));
			}
			else
			{
				// Default gravity if standing on a platform or moving upwards
				SetGravityScale(Data.gravityScale);
			}
		}
		else
		{
			// No gravity when dashing (returns to normal once initial dash attack phase over)
			SetGravityScale(0);
		}
		#endregion
	}


	private void FixedUpdate()
	{
		if (playerHealth.IsKnockbackActive()) return;
		//Handle Run
		if (!IsDashing)
		{
			if (IsWallJumping)
				Run(Data.wallJumpRunLerp);
			else
				Run(1);
		}
		else if (_isDashAttacking)
		{
			Run(Data.dashEndRunLerp);
		}

		//Handle Slide
		if (IsSliding)
			Slide();
	}

	#region INPUT CALLBACKS
	//Methods which whandle input detected in Update()
	public void OnJumpInput()
	{
		LastPressedJumpTime = Data.jumpInputBufferTime;
	}

	public void OnJumpUpInput()
	{
		if (CanJumpCut() || CanWallJumpCut())
			_isJumpCut = true;
	}

	public void OnDashInput()
	{
		LastPressedDashTime = Data.dashInputBufferTime;
	}
	#endregion

	#region GENERAL METHODS
	public void SetGravityScale(float scale)
	{
		RB.gravityScale = scale;
	}

	private void Sleep(float duration)
	{
		//Method used so we don't need to call StartCoroutine everywhere
		//nameof() notation means we don't need to input a string directly.
		//Removes chance of spelling mistakes and will improve error messages if any
		StartCoroutine(nameof(PerformSleep), duration);
	}

	private IEnumerator PerformSleep(float duration)
	{
		Time.timeScale = 0;
		yield return new WaitForSecondsRealtime(duration); //Must be Realtime since timeScale with be 0 
		Time.timeScale = 1;
	}
	#endregion

	//MOVEMENT METHODS
	#region RUN METHODS
	private void Run(float lerpAmount)
	{
		//Calculate the direction we want to move in and our desired velocity
		float targetSpeed = _moveInput.x * Data.runMaxSpeed;
		//We can reduce are control using Lerp() this smooths changes to are direction and speed
		targetSpeed = Mathf.Lerp(RB.velocity.x, targetSpeed, lerpAmount);

		#region Calculate AccelRate
		float accelRate;

		//Gets an acceleration value based on if we are accelerating (includes turning) 
		//or trying to decelerate (stop). As well as applying a multiplier if we're air borne.
		if (LastOnGroundTime > 0)
			accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount : Data.runDeccelAmount;
		else
			accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? Data.runAccelAmount * Data.accelInAir : Data.runDeccelAmount * Data.deccelInAir;
		#endregion

		#region Add Bonus Jump Apex Acceleration
		//Increase are acceleration and maxSpeed when at the apex of their jump, makes the jump feel a bit more bouncy, responsive and natural
		if ((IsJumping || IsWallJumping || _isJumpFalling) && Mathf.Abs(RB.velocity.y) < Data.jumpHangTimeThreshold)
		{
			accelRate *= Data.jumpHangAccelerationMult;
			targetSpeed *= Data.jumpHangMaxSpeedMult;
		}
		#endregion

		#region Conserve Momentum
		//We won't slow the player down if they are moving in their desired direction but at a greater speed than their maxSpeed
		if (Data.doConserveMomentum && Mathf.Abs(RB.velocity.x) > Mathf.Abs(targetSpeed) && Mathf.Sign(RB.velocity.x) == Mathf.Sign(targetSpeed) && Mathf.Abs(targetSpeed) > 0.01f && LastOnGroundTime < 0)
		{
			//Prevent any deceleration from happening, or in other words conserve are current momentum
			//You could experiment with allowing for the player to slightly increae their speed whilst in this "state"
			accelRate = 0;
		}
		#endregion

		//Calculate difference between current velocity and desired velocity
		float speedDif = targetSpeed - RB.velocity.x;
		//Calculate force along x-axis to apply to thr player

		float movement = speedDif * accelRate;

		//Convert this to a vector and apply to rigidbody
		RB.AddForce(movement * Vector2.right, ForceMode2D.Force);

		/*
		 * For those interested here is what AddForce() will do
		 * RB.velocity = new Vector2(RB.velocity.x + (Time.fixedDeltaTime  * speedDif * accelRate) / RB.mass, RB.velocity.y);
		 * Time.fixedDeltaTime is by default in Unity 0.02 seconds equal to 50 FixedUpdate() calls per second
		*/
	}

	private void CreateJumpDust()
	{
		ParticleSystem particle = Instantiate(jumpDust, transform.position, Quaternion.identity);
		particle.transform.rotation = Quaternion.Euler(0, 0, 0); // Adjust rotation as needed
		particle.Play();

		// Optionally destroy after duration
		Destroy(particle.gameObject, particle.main.duration);
	}
	#endregion

	#region JUMP METHODS
	private void Jump()
	{
		#region Perform Jump
		if (LastOnGroundTime > 0)
		{
			// Normal jump
			PerformJump();
		}
		else if (canDoubleJump && !hasDoubleJumped)
		{
			// Double jump
			PerformJump();
			hasDoubleJumped = true; // Mark double jump as used
		}
		#endregion
	}

	private void PerformJump()
	{
		LastPressedJumpTime = 0;
		LastOnGroundTime = 0;
		float force = Data.jumpForce;
		if (RB.velocity.y < 0)
			force -= RB.velocity.y;
		RB.AddForce(Vector2.up * force, ForceMode2D.Impulse);
		jumpSound.Play();
		CreateJumpDust();
	}

	private void WallJump(int dir)
	{
		//Ensures we can't call Wall Jump multiple times from one press
		LastPressedJumpTime = 0;
		LastOnGroundTime = 0;
		LastOnWallRightTime = 0;
		LastOnWallLeftTime = 0;

		#region Perform Wall Jump
		Vector2 force = new Vector2(Data.wallJumpForce.x, Data.wallJumpForce.y);
		force.x *= dir; //apply force in opposite direction of wall

		if (Mathf.Sign(RB.velocity.x) != Mathf.Sign(force.x))
			force.x -= RB.velocity.x;

		if (RB.velocity.y < 0) //checks whether player is falling, if so we subtract the velocity.y (counteracting force of gravity). This ensures the player always reaches our desired jump force or greater
			force.y -= RB.velocity.y;

		//Unlike in the run we want to use the Impulse mode.
		//The default mode will apply are force instantly ignoring masss
		RB.AddForce(force, ForceMode2D.Impulse);
		#endregion
	}
	#endregion

	#region DASH METHODS
	//Dash Coroutine
	private IEnumerator StartDash(Vector2 dir)
	{
		LastOnGroundTime = 0;
		LastPressedDashTime = 0;

		float startTime = Time.time;

		_dashesLeft--;
		_isDashAttacking = true;

		SetGravityScale(0);

		// Determine the tilt angle based on the dash direction
		float tiltAngle = dir.x > 0 ? -15f : 15f; // Tilts -15 degrees for right dash, 15 degrees for left dash

		// Apply the tilt at the beginning of the dash
		transform.rotation = Quaternion.Euler(0, 0, tiltAngle);
		dashSound.Play();
		// Dash attack phase
		while (Time.time - startTime <= Data.dashAttackTime)
		{
			RB.velocity = dir.normalized * Data.dashSpeed;
			tr.emitting = true;
			yield return null;
		}
		startTime = Time.time;
		_isDashAttacking = false;

		// Return to normal rotation after the dash attack phase
		transform.rotation = Quaternion.Euler(0, 0, 0);

		SetGravityScale(Data.gravityScale);
		RB.velocity = Data.dashEndSpeed * dir.normalized;

		// Dash end phase
		while (Time.time - startTime <= Data.dashEndTime)
		{
			yield return null;
		}

		// Dash over
		tr.emitting = false;
		IsDashing = false;
	}


	//Short period before the player is able to dash again
	private IEnumerator RefillDash(int amount)
	{
		//SHoet cooldown, so we can't constantly dash along the ground, again this is the implementation in Celeste, feel free to change it up
		_dashRefilling = true;
		yield return new WaitForSeconds(Data.dashRefillTime);
		_dashRefilling = false;
		if (powerUps.movement == PowerUps.Movement.DoubleDash)
		{
			Data.dashAmount = 2;
			_dashesLeft = Mathf.Min(Data.dashAmount, _dashesLeft + 2);
		}
		else
		{
			Data.dashAmount = 1;
			_dashesLeft = Mathf.Min(Data.dashAmount, _dashesLeft + 1);
		}
	}
	#endregion

	#region OTHER MOVEMENT METHODS
	private void Slide()
	{
		//We remove the remaining upwards Impulse to prevent upwards sliding
		if (RB.velocity.y > 0)
		{
			RB.AddForce(-RB.velocity.y * Vector2.up, ForceMode2D.Impulse);
		}

		//Works the same as the Run but only in the y-axis
		//THis seems to work fine, buit maybe you'll find a better way to implement a slide into this system
		float speedDif = Data.slideSpeed - RB.velocity.y;
		float movement = speedDif * Data.slideAccel;
		//So, we clamp the movement here to prevent any over corrections (these aren't noticeable in the Run)
		//The force applied can't be greater than the (negative) speedDifference * by how many times a second FixedUpdate() is called. For more info research how force are applied to rigidbodies.
		movement = Mathf.Clamp(movement, -Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime), Mathf.Abs(speedDif) * (1 / Time.fixedDeltaTime));

		RB.AddForce(movement * Vector2.up);
	}
	#endregion


	#region CHECK METHODS

	private bool CanJump()
	{
		if (LastOnGroundTime > 0 && !IsJumping)
		{
			hasDoubleJumped = false;
			return true;
		}
		if (canDoubleJump && !hasDoubleJumped && !IsJumping && LastPressedJumpTime > 0)
		{
			return true;
		}
		return false;
	}

	private bool CanWallJump()
	{
		return LastPressedJumpTime > 0 && LastOnWallTime > 0 && LastOnGroundTime <= 0 && (!IsWallJumping ||
			 (LastOnWallRightTime > 0 && _lastWallJumpDir == 1) || (LastOnWallLeftTime > 0 && _lastWallJumpDir == -1));
	}

	private bool CanJumpCut()
	{
		return IsJumping && RB.velocity.y > 0;
	}

	private bool CanWallJumpCut()
	{
		return IsWallJumping && RB.velocity.y > 0;
	}

	private bool CanDash()
	{
		if (!IsDashing && _dashesLeft < Data.dashAmount && LastOnGroundTime > 0 && !_dashRefilling)
		{
			if (powerUps.movement == PowerUps.Movement.DoubleDash)
			{
				StartCoroutine(nameof(RefillDash), 2);
			}
			else
			{
				StartCoroutine(nameof(RefillDash), 1);
			}
		}

		return _dashesLeft > 0;
	}

	public bool CanSlide()
	{
		if (LastOnWallTime > 0 && !IsJumping && !IsWallJumping && !IsDashing && LastOnGroundTime <= 0)
			return true;
		else
			return false;
	}
	#endregion


	#region EDITOR METHODS
	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireCube(_groundCheckPoint.position, _groundCheckSize);
		Gizmos.color = Color.blue;
		Gizmos.DrawWireCube(_frontWallCheckPoint.position, _wallCheckSize);
		Gizmos.DrawWireCube(_backWallCheckPoint.position, _wallCheckSize);
	}
	#endregion
}