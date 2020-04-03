using UnityEngine;
using UnityEngine.Rendering;

public class SpriteMaskingComponent : MonoBehaviour
{
//	[HideInInspector]
	public SpriteMaskTrueSoft owner;

	public virtual bool isEnabled {
		get {
			return owner == null ? false : owner.enabled;
		}
	}

	protected virtual void doUpdateSprite (Transform t, CompareFunction comp, int stencil)
	{
		if (owner != null) {
			owner.doUpdateSprite (t, comp, stencil);
		}
	}
}
