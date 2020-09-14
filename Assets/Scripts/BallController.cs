using System.Collections;
using UnityEngine;

public class BallController : MonoBehaviour
{
    public static BallController ballControl; //Singleton variable

    [Header("Particles")]
    [SerializeField] ParticleSystem sparks; //sparks particle
    [SerializeField] ParticleSystem trail; //trail particle
    [SerializeField] ParticleSystem flame; //flame particle
    [SerializeField] ParticleSystem bullseye; //bullseye particle
    [SerializeField] ParticleSystem death; //death particle

    [Header("Swing Mechanism")]
    [SerializeField] Vector3 connectedAnchor; //connected position
    [SerializeField] GameObject connectedAnchorPrefab; //object to spawn at connected position
    [SerializeField] GameObject ball; //ball mesh
    [SerializeField] GameObject rope; //rope gameobject
    [SerializeField] LayerMask mask; //raycast layer mask
    [SerializeField] float incrementRatio; //rate to increment rope lenght
    [SerializeField] float reduceRopeRatio; //rate to decrement rope lenght
    [SerializeField] float velocityLimit; //velocity threshold
    [SerializeField] float freeForce; //force applied when not swinging
    [SerializeField] float swingForce; //force applied when swinging
    [SerializeField] bool throwRope; //is rope being thrown
    [SerializeField] bool connected; //is rope connected
    [SerializeField] bool swinging; //is swinging
    [SerializeField] bool reThrow; //is throwing rope again
    [SerializeField] bool enableReduceRope; //enable decrement of rope lenght
    [SerializeField] bool enableApplyForce; //enable foces

    [Header("Materials For Switching")]
    [SerializeField] Material glowMaterial; //glowing material for perfect
    [SerializeField] Material nonGlowMaterial; //default ball material
    [SerializeField] Material offCircleMaterial; //non hdr circle material
    [SerializeField] Material offPortalWallMaterial; //non hdr portal material

    [Header("Score")]
    [SerializeField] int bullseyeMultiplier; //perfect multiplier
    [SerializeField] float score; //score variable
    [SerializeField] float posX; //highest x value reached

    [Header("FirstRun")]
    [SerializeField] bool spawnState; //a flag to indicate first run

    [Header("Audio")]
    [SerializeField] AudioClip whoosh1; //whoosh 1
    [SerializeField] AudioClip whoosh2; //whoosh 2
    [SerializeField] AudioClip breaksound; //whoosh 2
    [SerializeField] AudioClip explossion; //explossion


    private void Awake() //on wake
    {
        ballControl = this; //singleton set
    }

    void Start() //on start
    {
        score = 0; //reset score
        bullseyeMultiplier = 0; //reset bullseye

        if(PlayerPrefs.HasKey("BestScore")) //if best score saved
        {
            float bestScore;
            float.TryParse(PlayerPrefs.GetString("BestScore"), out bestScore); //parse saved score into a float variable

            UIController.ui.Best.text = "best:   " + bestScore.ToString("0.0"); //show best score
        }
        else //if not saved
        {
            UIController.ui.Best.text = "best:   0.0"; //show 0
        }

        swingFrom(transform.position + Vector3.right * 5f + Vector3.up * 5f); //swing form pre defined position
        rope.SetActive(true); //enable rope
        swinging = true; //start swinging

        StartCoroutine(sendOutRope()); //start send rope coroutine
        StartCoroutine(reduceRopeLenght()); //start reduce rope coroutine
        StartCoroutine(applyForce()); //start apply force coroutine
        StartCoroutine(addProgressScore()); //start score calculation due to highest x value
    }

    private void OnTriggerEnter(Collider other) //on trigger enter 
    {
        if(other.gameObject.tag == "PortalWall") //if trigger is portal wall
        {
            bullseye.Play(); //play bullseye particle
            score += 20; //add 20 points
            bullseyeMultiplier++; //increment bullseye multiplier
            Camera.main.backgroundColor = 
                other.gameObject.GetComponent<MeshRenderer>().material.color + Color.white * 0.01f; //set camera background
            other.gameObject.GetComponent<MeshRenderer>().material = offPortalWallMaterial; //set portal wall material to non hdr

            AudioSource audioSource = gameObject.AddComponent<AudioSource>(); //add audio source component
            audioSource.clip = breaksound; //set clip
            audioSource.time = 0.5f; //set start
            audioSource.Play(); //play
            Destroy(audioSource, 1f); //destroy after 1 second
        }

        if(other.gameObject.tag == "Circle") //if trigger is circle
        {
            if(Vector3.Distance(transform.position, other.gameObject.transform.position) < 1f) //if distance is less than 1
            {
                bullseye.Play(); //play bullseye particle
                score += 2 + bullseyeMultiplier; //add 2 points + bullseye multiplier to score
                bullseyeMultiplier++; //increment bullseye multiplier


                Destroy(other.gameObject, 5f); //destroy circle 5 seconds later
                Destroy(other.gameObject.transform.GetChild(0).gameObject, 5f); //destroy inner circle 5 seconds later
                other.gameObject.transform.GetChild(0).GetComponent<MeshRenderer>().material = 
                    offCircleMaterial; //set inner circle material to non hdr
                other.gameObject.transform.GetChild(0).GetComponent<BoxCollider>().enabled = true; //enable collider
                other.gameObject.transform.GetChild(0).GetComponent<BoxCollider>().isTrigger = true; //enable trigger
                other.gameObject.layer = 13; //set collision layer
                other.gameObject.transform.GetChild(0).gameObject.AddComponent<Rigidbody>(); //add rigidbody
                other.gameObject.transform.GetChild(0).gameObject.GetComponent<Rigidbody>().AddForce(
                    Vector3.right * Random.Range(1f, 5f) + Vector3.up * Random.Range(-1f, 1f),
                    ForceMode.VelocityChange
                    ); //add random forward force
                other.gameObject.transform.GetChild(0).gameObject.GetComponent<Rigidbody>().AddTorque(
                    new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 1f)),
                    ForceMode.VelocityChange
                    ); //add random torque
                other.gameObject.transform.GetChild(0).gameObject.GetComponent<Rigidbody>().mass = 0.0001f; //set mass to low
                other.gameObject.transform.GetChild(0).transform.parent = null; //set parent to null

                AudioSource audioSource = gameObject.AddComponent<AudioSource>(); //add audio source component
                audioSource.clip = breaksound; //set clip
                audioSource.time = 0.5f; //set start
                audioSource.Play(); //play
                Destroy(audioSource, 1f); //destroy after 1 second

                StartCoroutine(incrementHDR()); //start perfect material coroutine
            }else if(Vector3.Distance(transform.position, other.gameObject.transform.position) < 2.5f) //if distance is less than 2.5f
            {
                Destroy(other.gameObject, 5f); //destroy circle 5 seconds later
                Destroy(other.gameObject.transform.GetChild(1).gameObject, 5f); //destroy outer circle 5 seconds later
                other.gameObject.transform.GetChild(1).GetComponent<MeshRenderer>().material = 
                    offCircleMaterial; //set outer circle material to non hdr
                other.gameObject.transform.GetChild(1).GetComponent<BoxCollider>().enabled = true; //enable collider
                other.gameObject.transform.GetChild(1).GetComponent<BoxCollider>().isTrigger = true; //enable trigger
                other.gameObject.layer = 13; //set collision layer
                other.gameObject.transform.GetChild(1).gameObject.AddComponent<Rigidbody>(); //add rigidbody
                other.gameObject.transform.GetChild(1).gameObject.GetComponent<Rigidbody>().AddForce(
                    Vector3.right * Random.Range(1f, 5f) + Vector3.up * Random.Range(-1f, 1f),
                    ForceMode.VelocityChange
                    ); //add random forward force
                other.gameObject.transform.GetChild(1).gameObject.GetComponent<Rigidbody>().AddTorque(
                    new Vector3(Random.Range(-5f, 5f), 0f, Random.Range(-5f, 1f)),
                    ForceMode.VelocityChange
                    ); //add random torque
                other.gameObject.transform.GetChild(1).gameObject.GetComponent<Rigidbody>().mass = 0.0001f; //set mass to low
                other.gameObject.transform.GetChild(1).transform.parent = null; //set parent to null

                score++; //increment score
                bullseyeMultiplier = 0; //reset bullseye

                AudioSource audioSource = gameObject.AddComponent<AudioSource>(); //add audio source component
                audioSource.clip = breaksound; //set clip
                audioSource.time = 0.5f; //set start
                audioSource.Play(); //play
                Destroy(audioSource, 1f); //destroy after 1 second
            }
            else
            {
                bullseyeMultiplier = 0; //reset bullseye
            }
        }
    }

    private void OnCollisionEnter(Collision collision) //on collision
    {
        if (collision.gameObject.tag == "Ceiling") //if tag is ceiling
        {
            GetComponentInChildren<MeshRenderer>().enabled = false; //disable mesh renderer
            GetComponent<Rigidbody>().isKinematic = true; //set kinematic
            rope.SetActive(false); //disable rope
            if(GetComponent<HingeJoint>() != null) //if hinge joint exists
            {
                Destroy(GetComponent<HingeJoint>()); //destroy hinge joint
            }

            sparks.Stop(); //disable sparks particle
            trail.Stop(); //disable trail particle
            flame.Stop(); //disable flame particle
            death.Play(); //enable death particle
            UIController.ui.touchEnabled = false;
            UIController.ui.setEndgameScene(); //load endgame scene

            AudioSource audioSource = gameObject.AddComponent<AudioSource>(); //add audio source component
            audioSource.clip = explossion; //set clip
            audioSource.time = 0.55f; //set start
            audioSource.Play(); //play
            Destroy(audioSource, 1f); //destroy after 1 second

            if (PlayerPrefs.HasKey("BestScore")) //if score is saved
            {
                float bestScore;
                float.TryParse(PlayerPrefs.GetString("BestScore"), out bestScore); //parse saved score into a float variable
                if (score > bestScore) //if score is higher than saved score
                {
                    PlayerPrefs.SetString("BestScore", score.ToString()); //save score
                    UIController.ui.Best.text = "best:   " + score.ToString("0.0"); //show score
                }
                else //if not higher
                {
                    UIController.ui.Best.text = "best:   " + bestScore.ToString("0.0"); //show best score
                }
            }
            else //if not saved
            {
                PlayerPrefs.SetString("BestScore", score.ToString()); //save score
                UIController.ui.Best.text = "best:   " + score.ToString("0.0"); //show score
            }
        }
    }

    void Update() //works every frame
    {
        if(UIController.ui.touchEnabled) //if touch is enabled
        {
            // ! I normally use Input.getTouch but it does not work on the editor screen !
            if (Input.GetMouseButtonDown(0)) //if left mouse button is down
            {
                if (spawnState) //if first run
                {
                    posX = transform.position.x; //set position to highest x value travelled
                    Camera.main.GetComponent<CameraFollowManager>().follow = true; //enable camera follow coroutine
                    UIController.ui.setIngameScene(); //set ingame ui
                    spawnState = false; //disable first run
                }

                throwRope = true; //ebable rope throw
                reThrow = true; //its a re throw
                swinging = false; //not swinging
                connected = false; //not connected

                if (GetComponent<SpringJoint>() != null) //if spring joint exists
                {
                    Destroy(GetComponent<SpringJoint>()); //destroy spring joint
                }
            }

            if (Input.GetMouseButtonUp(0)) //if left mouse button is up
            {
                rope.SetActive(false); //disable rope
                throwRope = false; //disable rope throw
                reThrow = false; //disable re throw
                swinging = false; //disable swinging
                connected = false; //disable connected

                GetComponent<Rigidbody>().drag = 0.33f; //set drag to 0.33

                if (GetComponent<SpringJoint>() != null) //if spring joint exists
                {
                    Destroy(GetComponent<SpringJoint>()); //destroy spring joint
                }
            }
        }

        if(swinging) // if swinging
        {
            rope.transform.localScale = 
                new Vector3(rope.transform.localScale.x, 
                Vector3.Distance(transform.position, connectedAnchor), 
                rope.transform.localScale.z); //set rope size

            rope.transform.position = 
                transform.position + ((connectedAnchor - transform.position) / 2f); //set rope position

            rope.transform.rotation = 
                Quaternion.LookRotation(Vector3.forward, 
                new Vector2((connectedAnchor.x - transform.position.x), 
                connectedAnchor.y - transform.position.y).normalized); //set rope rotation
        }
    }

    IEnumerator incrementHDR() //increment hdr coroutine
    {
        ball.GetComponent<MeshRenderer>().material = glowMaterial; //set glow material
        yield return new WaitForSeconds(2.5f); //wait 2.5 seconds
        ball.GetComponent<MeshRenderer>().material = nonGlowMaterial; //set default ball material
        yield return new WaitForEndOfFrame(); //wait for end of frame
    }

    IEnumerator addProgressScore() //add score related to highest x value coroutine
    {
        while(true) //infinity loop almost like fixedupdate
        {
            if(!spawnState) //if not first run
            {
                if (transform.position.x > posX) //if position is higher than the highest recorded x value
                {
                    score += 
                        (float)(Mathf.Floor(transform.position.x) - Mathf.Floor(posX)) * 0.1f; //increase score by 0.1 per 1 unity unit
                    posX = transform.position.x; //update highest x value
                    UIController.ui.Score.text = score.ToString("0.0"); //show score
                }
            }

            yield return new WaitForFixedUpdate(); //wait for fixedupdate
        }
    }

    IEnumerator sendOutRope() //send out rope coroutine
    {
        //definitions before while loop work at startup
        RaycastHit hit; //raycast hit variable
        float ropeLenght = 0f; //reset rope lenght
        throwRope = false; //disable throw rope
        reThrow = false; //disable rethrow
        connected = false; //disable connected

        while (true) //infinity loop almost like fixedupdate
        {
            if (reThrow) //if rethrowing
            {
                rope.transform.localScale = new Vector3(
                    rope.transform.localScale.x,
                    0.0001f,
                    rope.transform.localScale.z); //reset rope lenght
                ropeLenght = 0f; //reset rope lenght materiaş
                reThrow = false; //disable rethrow
                rope.SetActive(true); //enable rope
            }

            if (throwRope) //if throw rope
            {
                if (!(Physics.Raycast(transform.position, 
                    Vector3.right + Vector3.up * 1.5f, out hit, ropeLenght, mask))) //if raycast doesn't hit
                {
                    ropeLenght += incrementRatio; //increase rope lenght

                    rope.transform.localScale = new Vector3(
                        rope.transform.localScale.x,
                        ropeLenght,
                        rope.transform.localScale.z); //set rope lenght

                    rope.transform.rotation =
                        Quaternion.LookRotation(Vector3.forward,
                        Vector3.right + Vector3.up * 1.5f); //set rope rotation

                    rope.transform.position =
                        transform.position + 
                        ((Vector3.right + Vector3.up * 1.5f).normalized * ropeLenght * 0.5f); //set rope position
                }
                else //if hits
                {
                    if (!connected) //if not connected
                    {
                        swingFrom(hit.point); //swing from hit point
                        connected = true; //set connected
                    }
                }
            }

            yield return new WaitForFixedUpdate(); //wait for fixedupdate
        }
    }

    public void swingFrom(Vector3 target) //swinf rom method
    {
        GetComponent<Rigidbody>().drag = 0.25f;//set drag to 0.25

        connectedAnchor = target; //set connected anchor to target

        GameObject connectedAnchorObject = 
            Instantiate(connectedAnchorPrefab, target, connectedAnchorPrefab.transform.rotation); //instantiate connection sphere object
        Destroy(connectedAnchorObject, 10f); //destroy sphere object

        gameObject.AddComponent<SpringJoint>(); //add spring joint
        GetComponent<SpringJoint>().anchor = Vector3.zero; //reset anchor
        GetComponent<SpringJoint>().autoConfigureConnectedAnchor = false; //disable auto configuration
        GetComponent<SpringJoint>().connectedAnchor = target; //set connected anchor to target
        GetComponent<SpringJoint>().axis = new Vector3(1, 1, 0); //enable x, y axis
        GetComponent<SpringJoint>().spring = 25; //set spring value
        GetComponent<SpringJoint>().damper = 5; //set damper value 
        GetComponent<SpringJoint>().minDistance = 
            Vector3.Distance(transform.position, target) - 1f; //set minimum distance
        GetComponent<SpringJoint>().maxDistance = 
            Vector3.Distance(transform.position, target); //set maximum distance

        throwRope = false; //disable throw rope
        swinging = true; //enable swining

        AudioSource audioSource = gameObject.AddComponent<AudioSource>(); //add audio source component
        Destroy(audioSource, 1f); //destroy after 1 second

        if (Random.Range(0, 2) == 0) //Randomize sound to play
        {
            audioSource.clip = whoosh1; //set clip
            audioSource.time = 0.30f; //set start
        }
        else
        {
            audioSource.clip = whoosh2; //set clip
            audioSource.time = 0.15f; //set start
        }

        audioSource.Play(); //play
    }

    IEnumerator reduceRopeLenght() //reduce rope lenght coroutine
    {
        while(true) //infinity loop almost like fixedupdate
        {
            if(enableReduceRope) //if rope reduction is enabled
            {
                if (swinging && !spawnState) //if swinging and not first run
                {
                    GetComponent<SpringJoint>().minDistance -= reduceRopeRatio; //decrease minimum distance
                    GetComponent<SpringJoint>().maxDistance -= reduceRopeRatio; //decrease maximum distance
                }
            }

            yield return new WaitForFixedUpdate(); //wait for fixedupdate
        }
    }

    IEnumerator applyForce() //apply force coroutine
    {
        while(true) //infinity loop almost like fixedupdate
        {
            if(enableApplyForce) //if force is applied
            {
                if (!swinging) //if not swinging
                {
                    if (GetComponent<Rigidbody>().velocity.magnitude < velocityLimit) //if velocity is less than threshold
                    {
                        GetComponent<Rigidbody>().AddForce(Vector3.right * freeForce); //add free force
                    }
                }
                else //if swinging
                {
                    if(!spawnState) //if not first run
                    {
                        GetComponent<Rigidbody>().AddForce(
                            (Quaternion.AngleAxis(-90, Vector3.forward) *
                            (connectedAnchor - transform.position)).normalized * swingForce
                            ); //add swing force
                    }
                }
            }

            yield return new WaitForFixedUpdate(); //wait for fixedupdate
        }
    }
}