using UnityEngine;
using UnityEngine.InputSystem;

namespace Buk {
  public class VrHand : MonoBehaviour {
    public InputAction orientation;
    public void Awake() {
      orientation.performed += Orientation;
      orientation.Enable();
    }

    private void Orientation(InputAction.CallbackContext context) {
      transform.localRotation = context.ReadValue<Quaternion>();
    }

    public void OnDestroy() {
      orientation.performed -= Orientation;
    }
  }
}
