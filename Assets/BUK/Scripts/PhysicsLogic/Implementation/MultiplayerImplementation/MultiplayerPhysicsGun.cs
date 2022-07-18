using System;
using Buk.PhysicsLogic.Interfaces;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Buk.PhysicsLogic.Implementation.Multiplayer
{
  public class MultiplayerPhysicsGun : NetworkBehaviour, IGun
  {
    // The type of bullet this gun shoots.
    public GameObject bulletType;
    // The button to use as input.
    public InputAction trigger;
    // How fast the bullet is launched
    public float maxMuzzleSpeed = 10.0f;
    // Seconds before you can shoot a new bullet.
    public float coolDown = .25f;
    // At what time was the last bullet shot.
    protected float lastShotTime = 0.0f;

    private Rigidbody shooterBody;
    public bool CanShoot { get => Time.fixedTime - lastShotTime >= coolDown; }

    protected virtual void TriggerPressed(InputAction.CallbackContext _)
    {
      if (CanShoot)
      {
        Shoot(maxMuzzleSpeed);
      }
    }

    protected virtual void TriggerReleased(InputAction.CallbackContext _)
    {
      // Do nothing
    }

    public void Awake()
    {

      if (bulletType == null)
      {
        throw new Exception("You must add a bullet type!");
      }

      if (bulletType.GetComponentInChildren<Rigidbody>() == null)
      {
        throw new Exception("Your bullet must have a Rigidbody to use it with this gun!");
      }
      shooterBody = GetComponentInParent<Rigidbody>();
      InputSetup();
    }

    public virtual void InputSetup()
    {
      if (trigger != null)
      {
        trigger.started += TriggerPressed;
        trigger.canceled += TriggerReleased;
        trigger.Enable();
      }
    }

    public void OnDestroy()
    {
      if (trigger != null)
      {
        trigger.started -= TriggerPressed;
        trigger.canceled -= TriggerReleased;
      }
    }

    // This is what the server communication looks like for this implementation
    // This communication is done with Remote Actions, read more here
    // https://mirror-networking.gitbook.io/docs/guides/communications/remote-actions
    /*
                   ┌───────────────────────┐
                   │Server                 │
                   │ ┌────────┐ ┌────────┐ │
            ┌──────┼─┤Server  | |Server  | │
            │  ┌───┼─►Player 1| |Player 2| │
            │  │   │ └───────┬┘ └────────┘ │
            │  │   └─────────┼─────────────┘
            │  │             │
    RpcShoot│  │Shoot        └──────────┐RpcShoot
            │  │                        │
     ┌──────┼──┼─────────────┐   ┌──────┼────────────────┐
     │Client│1 │             │   │Client|2               │
     │ ┌────▼──┴┐ ┌────────┐ │   │ ┌────▼───┐ ┌────────┐ │
     │ │Client  │ │Remote  │ │   │ │Remote  │ |Client  │ │
     │ │Player 1│ │Player 2│ │   │ │Player 1│ |Player 2│ │
     │ └────────┘ └────────┘ │   │ └────────┘ └────────┘ │
     └───────────────────────┘   └───────────────────────┘
    */

    /// <summary>
    /// This function only gets run on the server.
    /// The server can tell all instances of this player to all fire a bullet
    /// </summary>
    [Command]
    public void Shoot(float speed)
    {
      if (!isServer) return;

      // Call the function to tell all instances of this player to shoot a bullet
      RpcShoot(speed);
    }

    /// <summary>
    /// This function gets run on all instances of a player at the same time
    /// This is the code that actually spawns and fires the bullet for that player on every other player's, and its own, screen
    /// </summary>
    [ClientRpc]
    public void RpcShoot(float speed)
    {
      lastShotTime = Time.fixedTime;
      // Create a new copy of bulletType using the gun's position and rotation.
      var bulletBody = Instantiate(bulletType, transform.position, transform.rotation)
        // Get the Rigidbody of that bullet, so that we can apply physics to it.
        .GetComponentInChildren<Rigidbody>();
      // If possible
      if (shooterBody)
      {
        // Make the bullet start moving just as fast as the shooter.
        // This makes the behaviour more realistic
        bulletBody.velocity = shooterBody.velocity;
      }
      // Apply speed to the bullet's body, relative to its current position and rotation
      bulletBody.AddRelativeForce(0f, speed, 0f, ForceMode.VelocityChange);
    }
  }
}
