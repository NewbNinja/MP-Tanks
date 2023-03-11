/*  This file is part of the "Tanks Multiplayer" project by FLOBUK.
 *  You are only allowed to use these resources if you've bought them from the Unity Asset Store.
 * 	You shall not license, sublicense, sell, resell, transfer, assign, distribute or
 * 	otherwise make available to any third party the Service or the Content. */

using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

namespace TanksMP
{
    /// <summary>
    /// Networked player class implementing movement control and shooting.
    /// Contains both server and client logic in an authoritative approach.
    /// </summary> 
    public class Player : MonoBehaviourPunCallbacks, IPunObservable
    {
        [Header("Player Info")]
        [SerializeField]
        /// <summary>
        /// UI Text displaying the player name.
        /// </summary>    
        public Text label;

        [SerializeField]
        [Range(10, 1000)]
        /// <summary>
        /// Maximum health value at game start.
        /// </summary>
        public int maxHealth = 10;

        /// <summary>
        /// Current turret rotation and shooting direction.
        /// </summary>
        [HideInInspector]
        public short turretRotation;

        [SerializeField]
        [Range(0.01f, 3f)]
        /// <summary>
        /// Delay between shots.
        /// </summary>
        public float fireRate = 0.75f;


        [Header("PLAYER MOVEMENT")]
        [SerializeField]
        [Range(1f,50f)]
        /// <summary>
        /// Movement speed in all directions.
        /// PUN sets move speed in the Prefab/Resources/ folder for each asset
        /// </summary>
        public float moveSpeed = 20f;

        [Header("Strafe")]
        [SerializeField]
        [Range(1f, 50f)]
        /// <summary>
        /// Player ship strafe speed, left and right of current position.
        /// PUN sets strafe speed in the Prefab/Resources/ folder for each asset
        /// on the Player.cs script -- change base value there if required
        /// </summary>
        public float strafeSpeed = 10f;


        [Header("Rotation")]
        [SerializeField]
        [Range(10f, 200f)]
        /// <summary>
        /// Player rotation speed.
        /// PUN sets rotation speed in the Prefab/Resources/ folder for each asset
        /// </summary>
        public float maxRotationSpeed = 100f;

        [SerializeField]
        [Range(0.1f, 30f)]
        /// <summary>
        /// Rotation acceleration rate.   
        /// How fast the mouse / right stick will build currentRotationSpeed which governs current rotation velocity
        /// </summary>
        float rotationAccelRate = 5f;

        [SerializeField]
        [Range(-150f, 150f)]
        /// <summary>
        /// Player rotation magnitude handles how much rotation should be applied to player on next update
        /// which is determined by the horizontal force received from the mouse over several FixedUpdates
        /// 
        /// Example:   maxRotationSpeed * currentRotationSpeed * Time.deltaTime;
        /// </summary>
        public float currentRotationSpeed = 0f;

        [SerializeField]
        [Range(.5f, 50f)]
        /// <summary>
        /// Gradually slows down the rotation of the player ship if no rotation input is received in future updates
        /// </summary>
        public float rotationDecay = 2f;

        /// <summary>
        /// Handles the roll / tilt on the player ship
        /// </summary>
        [SerializeField] 
        private float rollSpeed = 90f, rollAcceleration = 3.5f, maxRoll = 20f, maxStrafeRoll = 10f, rollInput, RS_rollInput, LS_rollInput;

        /// <summary>
        /// UI Slider visualizing health value.
        /// </summary>
        public Slider healthSlider;

        /// <summary>
        /// UI Slider visualizing shield value.
        /// </summary>
        public Slider shieldSlider;

        /// <summary>
        /// Clip to play when a shot has been fired.
        /// </summary>
        public AudioClip shotClip;

        /// <summary>
        /// Clip to play on player death.
        /// </summary>
        public AudioClip explosionClip;

        /// <summary>
        /// Object to spawn on shooting.
        /// </summary>
        public GameObject shotFX;

        /// <summary>
        /// Object to spawn on player death.
        /// </summary>
        public GameObject explosionFX;

        /// <summary>
        /// Reference to the Player Position Indicator (shows below the player ship)
        /// </summary>
        public Transform playerDirectionIndicator;

        /// <summary>
        /// Reference to the player ship prefab.
        /// </summary>
        public Transform playerShipPrefab;

        /// <summary>
        /// Turret to rotate with look direction.
        /// </summary>
        public Transform turret;

        /// <summary>
        /// Position to spawn new bullets at.
        /// </summary>
        public Transform shotPos;

        /// <summary>
        /// Array of available bullets for shooting.
        /// </summary>
        public GameObject[] bullets;

        /// <summary>
        /// MeshRenderers that should be highlighted in team color.
        /// </summary>
        public MeshRenderer[] renderers;

        /// <summary>
        /// Last player gameobject that killed this one.
        /// </summary>
        [HideInInspector]
        public GameObject killedBy;

        /// <summary>
        /// Reference to the camera following component.
        /// </summary>
        [HideInInspector]
        public FollowTarget camFollow;

        //timestamp when next shot should happen
        private float nextFire;
        
        //reference to this rigidbody
        #pragma warning disable 0649
		private Rigidbody rb;
		#pragma warning restore 0649


        //initialize server values for this player
        void Awake()
        {
            //only let the master do initialization
            if(!PhotonNetwork.IsMasterClient)
                return;
            
            //set players current health value after joining
            GetView().SetHealth(maxHealth);
        }


        /// <summary>
        /// Initialize synced values on every client.
        /// Initialize camera and input for this local client.
        /// </summary>
        void Start()
        {           
			//get corresponding team and colorize renderers in team color
            Team team = GameManager.GetInstance().teams[GetView().GetTeam()];
            for(int i = 0; i < renderers.Length; i++)
                renderers[i].material = team.material;

            //set name in label
            label.text = GetView().GetName();
            //call hooks manually to update
            OnHealthChange(GetView().GetHealth());
            OnShieldChange(GetView().GetShield());

            //called only for this client 
            if (!photonView.IsMine)
                return;

			//set a global reference to the local player
            GameManager.GetInstance().localPlayer = this;

			//get components and set camera target
            rb = GetComponent<Rigidbody>();        
            camFollow = Camera.main.GetComponent<FollowTarget>();
            camFollow.target = turret;

			//initialize input controls for mobile devices
			//[0]=left joystick for movement, [1]=right joystick for shooting
            #if !UNITY_STANDALONE && !UNITY_WEBGL
            GameManager.GetInstance().ui.controls[0].onDrag += Move;
            GameManager.GetInstance().ui.controls[0].onDragEnd += MoveEnd;

            //GameManager.GetInstance().ui.controls[1].onDragBegin += ShootBegin;
            GameManager.GetInstance().ui.controls[1].onDrag += RotateTurret;
            //GameManager.GetInstance().ui.controls[1].onDrag += Shoot;
            #endif
        }


        /// <summary>
        /// This method gets called whenever player properties have been changed on the network.
        /// </summary>
        public override void OnPlayerPropertiesUpdate(Photon.Realtime.Player player, ExitGames.Client.Photon.Hashtable playerAndUpdatedProps)
        {
            //only react on property changes for this player
            if(player != photonView.Owner)
                return;

            //update values that could change any time for visualization to stay up to date
            OnHealthChange(player.GetHealth());
            OnShieldChange(player.GetShield());
        }

        
        //this method gets called multiple times per second, at least 10 times or more
        void IPunObservable.OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {        
            if (stream.IsWriting)
            {             
                //here we send OUR turret rotation angle to other clients
                stream.SendNext(turretRotation);
            }
            else
            {   
                //here we receive the turret rotation angle from other players and apply it
                this.turretRotation = (short)stream.ReceiveNext();
                OnTurretRotation();
            }
        }


        //continously check for input on desktop platforms
        #if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
        void FixedUpdate()
		{
			//skip further calls for remote clients    
            if (!photonView.IsMine)
            {
                //keep turret rotation updated for all clients
                OnTurretRotation();
                return;
            }

            // CODE BELOW EXECUTED ON LOCAL CLIENT ONLY
            // vvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvvv

            //movement variables
            Vector2 moveDir;
            Vector2 turnDir;
            Vector3 shipTurnDir;

            //reset moving input when no arrow keys are pressed down
            if (Input.GetAxisRaw("Horizontal") == 0 && Input.GetAxisRaw("Vertical") == 0)
            {
                moveDir.x = 0;
                moveDir.y = 0;
            }
            else
            {
                //read out moving directions and calculate force
                moveDir.x = Input.GetAxis("Horizontal");
                moveDir.y = Input.GetAxis("Vertical");
                //Move(moveDir);
                HandleMovement(moveDir);
            }

            //cast a ray on a plane at the mouse position for detecting where to shoot 
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            Plane plane = new Plane(Vector3.up, Vector3.up);
            float distance = 0f;
            Vector3 hitPos = Vector3.zero;
            //the hit position determines the mouse position in the scene
            if (plane.Raycast(ray, out distance))
            {
                hitPos = ray.GetPoint(distance) - transform.position;
            }

            // DEBUGGING
            float rightStickHorizontal = Input.GetAxis("Right Stick Horizontal");
            //Debug.Log($"RSH:  {rightStickHorizontal}");


            //we've converted the mouse position to a direction
            turnDir = new Vector2(hitPos.x, hitPos.z);
            shipTurnDir = new Vector3(hitPos.x, 0, 0);

            //rotate turret to look at the mouse direction
            //RotateTurret(new Vector2(hitPos.x, hitPos.z));

            //rotate the ship to look at the mouse / right stick position
            RotateShip3();

            //shoot bullet on left mouse click
            if (Input.GetButton("Fire1"))
                Shoot();

			//replicate input to mobile controls for illustration purposes
			#if UNITY_EDITOR
				GameManager.GetInstance().ui.controls[0].position = moveDir;
				GameManager.GetInstance().ui.controls[1].position = turnDir;
			#endif
        }
        #endif


        void RotateShip3()
        {
            bool isRightKeyPressed = Input.GetKey(KeyCode.RightArrow);
            bool isLeftKeyPressed = Input.GetKey(KeyCode.LeftArrow);

            //slow down any existing rotation if Left/Right are not being pressed
            if (!isRightKeyPressed && !isLeftKeyPressed && currentRotationSpeed != 0f)
            {
                if (currentRotationSpeed > rotationDecay)
                    currentRotationSpeed -= rotationDecay;
                else if (currentRotationSpeed < (rotationDecay * -1f))
                    currentRotationSpeed += rotationDecay;
                else
                    currentRotationSpeed = 0f;
            }


            //if both left/right are being pressed, ignore the input
            else if (isRightKeyPressed && isLeftKeyPressed)
                return;

            //if left OR right are being pressed, apply rotate.  Don't exceed maxRotationSpeed
            else
            {
                if (isRightKeyPressed)
                {
                    //if already at max rotation value, ignore this keypress, else apply rotational force
                    if (currentRotationSpeed <= maxRotationSpeed)
                    {
                        if (currentRotationSpeed + rotationAccelRate > maxRotationSpeed)
                            currentRotationSpeed = maxRotationSpeed;
                        else
                            currentRotationSpeed += rotationAccelRate;
                    }
                }
                if (isLeftKeyPressed)
                {
                    if (currentRotationSpeed >= maxRotationSpeed * -1)
                    {
                        if (currentRotationSpeed - rotationAccelRate < (maxRotationSpeed * -1))
                            currentRotationSpeed = maxRotationSpeed * -1;
                        else
                            currentRotationSpeed -= rotationAccelRate;
                    }
                }
            }


            // Calculate the rotation
            //Vector3 rotation = new Vector3(0f, dir.x, 0f).normalized;
            //float currentRotateSpeed = maxRotationSpeed * currentRotationSpeed;
            transform.Rotate(new Vector3(0f, currentRotationSpeed, 0f) * Time.deltaTime);
            //Debug.Log($"CRS:  {currentRotationSpeed}");

            // Calculate the roll amount - invert the number so the ship pitches the correct way
            //RS_rollInput = maxRoll * dir.x * -1f;


            //transform.Rotate(maxRotationSpeed * Time.deltaTime);

            //transform.Rotate(rotation * rotationMagnitudeX * Time.unscaledDeltaTime);           // Rotate
            //playerShip.transform.localRotation = Quaternion.Euler(0, 0, rollInput);             // Roll
            //Vector3 move = transform.right * direction.x + transform.forward * direction.z;     // Move
            //controller.Move(move * currentSpeed * Time.unscaledDeltaTime);

            //// Calculate the rotation
            //rotation = new Vector3(0f, dir.x, 0f).normalized;
            //currentRotateSpeed = rotateSpeed * magnitude;

            //// Calculate the roll amount - invert the number so the ship pitches the correct way
            //RS_rollInput = maxRoll * dir.x * -1f;
        }



        //rotates the player ship to face right stick / mouse pointer position
        void RotateShip(Vector3 targetDirection = default(Vector3))
        {
            //don't rotate without values
            if (targetDirection == Vector3.zero)
                return;

            //==============================================================================================================
            // THIS ROTATION IS WORKING - BUT 360 DEGREES, TRYING TO CLAMP IT TO HORIZONTAL ROTATION ADDITION, NOT LOOK AT
            //==============================================================================================================

            // ROTATION INDICATOR:  Set the direction indicator to face the direction we're turning the ship towards
            short requiredRotation = (short)Quaternion.LookRotation(targetDirection, Vector3.up).eulerAngles.y;
            playerDirectionIndicator.rotation = Quaternion.Euler(90, requiredRotation - 90, 0);

            // ROTATE TOWARDS LOOK ROTATION:  Get targetRotation and RotateTowards it
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(playerShipPrefab.transform.rotation, targetRotation, maxRotationSpeed * Time.deltaTime);

            //==============================================================================================================
        }

        // Trying to fix the rotateship method
        void RotateShip2(float magnitude)
        {
            // Rotate ship - clamp at 10f instead of 1f to help with sensitivity
            float magX = Mathf.Clamp(magnitude, -10f, 10f);
            float currentRotationSpeed = (maxRotationSpeed * magX) * 0.1f;

            transform.Rotate(new Vector3(0f, currentRotationSpeed, 0f) * Time.deltaTime);

            Debug.Log($"Rotation Magnitude | Speed:  {magX} | {currentRotationSpeed}");

        }








        /// <summary>
        /// Helper method for getting the current object owner.
        /// </summary>
        public PhotonView GetView()
        {
            return this.photonView;
        }


        //moves rigidbody in the direction passed in
        void Move(Vector2 direction = default(Vector2))
        {
            //if direction is not zero, rotate player in the moving direction relative to camera
            if (direction != Vector2.zero)
                transform.rotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y))
                                     * Quaternion.Euler(0, camFollow.camTransform.eulerAngles.y, 0);

            //create movement vector based on current rotation and speed
            Vector3 movementDir = transform.forward * moveSpeed * Time.deltaTime;
            //apply vector to rigidbody position
            rb.MovePosition(rb.position + movementDir);
        }

        // SG Spaceship Movement
        private void HandleMovement(Vector2 LS_direction = default(Vector2))
        {
            // === THIS WORKED ===
            //Vector3 strafe = new Vector3(direction.x, 0, 0);
            float strafe = LS_direction.x *strafeSpeed * Time.deltaTime;
            float accel = LS_direction.y * moveSpeed * Time.deltaTime;

            transform.Translate(strafe, 0, accel);
        }


        //on movement drag ended
        void MoveEnd()
        {
            //reset rigidbody physics values
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }


        //rotates turret to the direction passed in
        void RotateTurret(Vector2 direction = default(Vector2))
        {
            //don't rotate without values
            if (direction == Vector2.zero)
                return;

            //get rotation value as angle out of the direction we received
            turretRotation = (short)Quaternion.LookRotation(new Vector3(direction.x, 0, direction.y)).eulerAngles.y;
            //Debug.Log($"RotateTurret():  turretRotationAngle = {turretRotation}");
            OnTurretRotation();
        }

 


        //on shot drag start set small delay for first shot
        void ShootBegin()
        {
            nextFire = Time.time + 0.25f;
        }


        //shoots a bullet in the direction passed in
        //we do not rely on the current turret rotation here, because we send the direction
        //along with the shot request to the server to absolutely ensure a synced shot position
        protected void Shoot(Vector2 direction = default(Vector2))
        {
            //if shot delay is over  
            if (Time.time > nextFire)
            {
                //set next shot timestamp
                nextFire = Time.time + fireRate;
                
                //send current client position and turret rotation along to sync the shot position
                //also we are sending it as a short array (only x,z - skip y) to save additional bandwidth
                short[] pos = new short[] { (short)(shotPos.position.x * 10), (short)(shotPos.position.z * 10)};
                //send shot request with origin to server
                this.photonView.RPC("CmdShoot", RpcTarget.AllViaServer, pos, turretRotation);
            }
        }
        
        
        //called on the server first but forwarded to all clients
        [PunRPC]
        protected void CmdShoot(short[] position, short angle)
        {   
            //get current bullet type
            int currentBullet = GetView().GetBullet();

            //calculate center between shot position sent and current server position (factor 0.6f = 40% client, 60% server)
            //this is done to compensate network lag and smoothing it out between both client/server positions
            Vector3 shotCenter = Vector3.Lerp(shotPos.position, new Vector3(position[0]/10f, shotPos.position.y, position[1]/10f), 0.6f);
            Quaternion syncedRot = turret.rotation = Quaternion.Euler(0, angle, 0);

            //spawn bullet using pooling
            GameObject obj = PoolManager.Spawn(bullets[currentBullet], shotCenter, syncedRot);
            obj.GetComponent<Bullet>().owner = gameObject;

            //check for current ammunition
            //let the server decrease special ammunition, if present
            if (PhotonNetwork.IsMasterClient && currentBullet != 0)
            {
                //if ran out of ammo: reset bullet automatically
                GetView().DecreaseAmmo(1);
            }

            //send event to all clients for spawning effects
            if (shotFX || shotClip)
                RpcOnShot();
        }


        //called on all clients after bullet spawn
        //spawn effects or sounds locally, if set
        protected void RpcOnShot()
        {
            if (shotFX) PoolManager.Spawn(shotFX, shotPos.position, Quaternion.identity);
            if (shotClip) AudioManager.Play3D(shotClip, shotPos.position, 0.1f);
        }


        //hook for updating turret rotation locally
        void OnTurretRotation()
        {
            //we don't need to check for local ownership when setting the turretRotation,
            //because OnPhotonSerializeView PhotonStream.isWriting == true only applies to the owner
            turret.rotation = Quaternion.Euler(0, turretRotation, 0);
            
        }

        //hook for updating health locally
        //(the actual value updates via player properties)
        protected void OnHealthChange(int value)
        {
            healthSlider.value = (float)value / maxHealth;
        }


        //hook for updating shield locally
        //(the actual value updates via player properties)
        protected void OnShieldChange(int value)
        {
            shieldSlider.value = value;
        }


        /// <summary>
        /// Server only: calculate damage to be taken by the Player,
		/// triggers score increase and respawn workflow on death.
        /// </summary>
        public void TakeDamage(Bullet bullet)
        {
            //store network variables temporary
            int health = GetView().GetHealth();
            int shield = GetView().GetShield();

            //reduce shield on hit
            if (shield > 0)
            {
                GetView().DecreaseShield(1);
                return;
            }

            //substract health by damage
            //locally for now, to only have one update later on
            health -= bullet.damage;

            //bullet killed the player
            if (health <= 0)
            {
                //the game is already over so don't do anything
                if(GameManager.GetInstance().IsGameOver()) return;

                //get killer and increase score for that enemy team
                Player other = bullet.owner.GetComponent<Player>();
                int otherTeam = other.GetView().GetTeam();
                if(GetView().GetTeam() != otherTeam)
                    GameManager.GetInstance().AddScore(ScoreType.Kill, otherTeam);

                //the maximum score has been reached now
                if(GameManager.GetInstance().IsGameOver())
                {
                    //close room for joining players
                    PhotonNetwork.CurrentRoom.IsOpen = false;
                    //tell all clients the winning team
                    this.photonView.RPC("RpcGameOver", RpcTarget.All, (byte)otherTeam);
                    return;
                }

                //the game is not over yet, reset runtime values
                //also tell all clients to despawn this player
                GetView().SetHealth(maxHealth);
                GetView().SetBullet(0);

                //clean up collectibles on this player by letting them drop down
                Collectible[] collectibles = GetComponentsInChildren<Collectible>(true);
                for (int i = 0; i < collectibles.Length; i++)
                {
                    PhotonNetwork.RemoveRPCs(collectibles[i].spawner.photonView);
                    collectibles[i].spawner.photonView.RPC("Drop", RpcTarget.AllBuffered, transform.position);
                }

                //tell the dead player who killed him (owner of the bullet)
                short senderId = 0;
                if (bullet.owner != null)
                    senderId = (short)bullet.owner.GetComponent<PhotonView>().ViewID;

                this.photonView.RPC("RpcRespawn", RpcTarget.All, senderId);
            }
            else
            {
                //we didn't die, set health to new value
                GetView().SetHealth(health);
            }
        }


        //called on all clients on both player death and respawn
        //only difference is that on respawn, the client sends the request
        [PunRPC]
        protected virtual void RpcRespawn(short senderId)
        {
            //toggle visibility for player gameobject (on/off)
            gameObject.SetActive(!gameObject.activeInHierarchy);
            bool isActive = gameObject.activeInHierarchy;
            killedBy = null;

            //the player has been killed
            if (!isActive)
            {
                //find original sender game object (killedBy)
                PhotonView senderView = senderId > 0 ? PhotonView.Find(senderId) : null;
                if (senderView != null && senderView.gameObject != null) killedBy = senderView.gameObject;

                //detect whether the current user was responsible for the kill, but not for suicide
                //yes, that's my kill: increase local kill counter
                if (this != GameManager.GetInstance().localPlayer && killedBy == GameManager.GetInstance().localPlayer.gameObject)
                {
                    GameManager.GetInstance().ui.killCounter[0].text = (int.Parse(GameManager.GetInstance().ui.killCounter[0].text) + 1).ToString();
                    GameManager.GetInstance().ui.killCounter[0].GetComponent<Animator>().Play("Animation");
                }

                if (explosionFX)
                {
                    //spawn death particles locally using pooling and colorize them in the player's team color
                    GameObject particle = PoolManager.Spawn(explosionFX, transform.position, transform.rotation);
                    ParticleColor pColor = particle.GetComponent<ParticleColor>();
                    if (pColor) pColor.SetColor(GameManager.GetInstance().teams[GetView().GetTeam()].material.color);
                }

                //play sound clip on player death
                if (explosionClip) AudioManager.Play3D(explosionClip, transform.position);
            }

            if (PhotonNetwork.IsMasterClient)
            {
                //send player back to the team area, this will get overwritten by the exact position from the client itself later on
                //we just do this to avoid players "popping up" from the position they died and then teleporting to the team area instantly
                //this is manipulating the internal PhotonTransformView cache to update the networkPosition variable
                GetComponent<PhotonTransformView>().OnPhotonSerializeView(new PhotonStream(false, new object[] { GameManager.GetInstance().GetSpawnPosition(GetView().GetTeam()),
                                                                                                                 Vector3.zero, Quaternion.identity }), new PhotonMessageInfo());
            }

            //further changes only affect the local client
            if (!photonView.IsMine)
                return;

            //local player got respawned so reset states
            if (isActive == true)
                ResetPosition();
            else
            {
                //local player was killed, set camera to follow the killer
                if (killedBy != null) camFollow.target = killedBy.transform;
                //hide input controls and other HUD elements
                camFollow.HideMask(true);
                //display respawn window (only for local player)
                GameManager.GetInstance().DisplayDeath();
            }
        }


        /// <summary>
        /// Command telling the server and all others that this client is ready for respawn.
        /// This is when the respawn delay is over or a video ad has been watched.
        /// </summary>
        public void CmdRespawn()
        {
            this.photonView.RPC("RpcRespawn", RpcTarget.AllViaServer, (short)0);
        }


        /// <summary>
        /// Repositions in team area and resets camera & input variables.
        /// This should only be called for the local player.
        /// </summary>
        public void ResetPosition()
        {
            //start following the local player again
            camFollow.target = turret;
            camFollow.HideMask(false);

            //get team area and reposition it there
            transform.position = GameManager.GetInstance().GetSpawnPosition(GetView().GetTeam());

            //reset forces modified by input
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            transform.rotation = Quaternion.identity;
            //reset input left over
            GameManager.GetInstance().ui.controls[0].OnEndDrag(null);
            GameManager.GetInstance().ui.controls[1].OnEndDrag(null);
        }


        //called on all clients on game end providing the winning team
        [PunRPC]
        protected void RpcGameOver(byte teamIndex)
        {
            //display game over window
            GameManager.GetInstance().DisplayGameOver(teamIndex);
        }
    }
}