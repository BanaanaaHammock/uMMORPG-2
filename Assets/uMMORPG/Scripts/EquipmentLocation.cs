// Used to find out where an equipped item should be shown in the 3D world. This
// component can be attached to the shoes, hands, shoulders or head of the
// player in the Hierarchy. The _acceptedCategory_ defines the item category
// that is accepted in this slot.
//
// _Note: modify the equipment location's transform to mirror it if necessary._
using UnityEngine;

public class EquipmentLocation : MonoBehaviour {
    public string acceptedCategory;
}