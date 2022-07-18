using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Buk.PhysicsLogic.Implementation.Multiplayer
{
  public class MultiplayerChargeablePhysicsGun : MultiplayerPhysicsGun
  {
    // This is the minimum power of the gun, when you don't hold down the trigger any time at all.
    public float minMuzzleSpeed = 1.0f;
    // This is how many seconds it takes to charge the gun to maximum power.
    public float maxChargeTime = 1.0f;
    // Used to save the moment when the player started holding down the trigger.
    private float triggerTime = float.PositiveInfinity;

    protected override void TriggerPressed(InputAction.CallbackContext _)
    {
      // Save the time the trigger pressed. However, if you've not waited for the cooldown,
      // we instead save the moment the cool-down ends (in the future).
      triggerTime = Math.Max(lastShotTime + coolDown, Time.fixedTime);
    }

    protected override void TriggerReleased(InputAction.CallbackContext _)
    {
      // How long was the trigger held down.
      var timeSinceTriggerHeld = Time.fixedTime - triggerTime;
      // We limit this to be not above the maxChargeTime
      var chargeTime = Math.Min(timeSinceTriggerHeld, maxChargeTime);
      if (chargeTime < 0)
      {
        // You pressed and released the trigger before the cooldown was done.
        // Nothing happens.
        return;
      }
      // This gives us a number from zero to one how much of the maximum speed should be used.
      var chargeMultiplier = chargeTime / maxChargeTime;
      // Now map this linearly the number from zero to one becomes a number from minMuzzleSpeed to maxMuzzleSpeed.
      var speed = chargeMultiplier * (maxMuzzleSpeed - minMuzzleSpeed) + minMuzzleSpeed;
      // Shoot using the calculated speed.
      Shoot(speed);
      // Set triggerTime to the end of eternity. It will be set correctly next time the trigger is pressed.
      triggerTime = float.PositiveInfinity;
    }
  }
}
