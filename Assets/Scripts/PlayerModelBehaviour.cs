using UnityEngine;

using FishNet;
using FishNet.Object;

public class PlayerModelBehaviour : NetworkBehaviour
{
    public Transform firstPersonModel;
    public Transform thirdPersonModel;

    private Player player;
    private CharacterController controller;
    public Animator fpsAnimator;

    

    public void Initialize(Player _player)
    {
        player = _player;
        controller = player.GetComponent<CharacterController>();
    }


    void Start()
    {
        fpsAnimator = firstPersonModel.GetComponent<Animator>();
    }

    public override void OnStartClient()
    {
        if (!IsOwner)
        {
            firstPersonModel.gameObject.SetActive(false);
            thirdPersonModel.gameObject.SetActive(true);
        }
        else
        {
            firstPersonModel.gameObject.SetActive(true);
            thirdPersonModel.gameObject.SetActive(false);

            // Child the first person model to the camera
            firstPersonModel.parent = Camera.main.transform;
            firstPersonModel.rotation = Quaternion.identity;
        }
    }
    
    void Update()
    {
        fpsAnimator.SetFloat("VELOCITY", controller.velocity.magnitude);
    }
    
}
