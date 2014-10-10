using UnityEngine;
using System.Collections;

public class BaseFilter : MonoBehaviour {

	void Awake () {
		FilterModule.Touch();
		FilterModule.Subscribe(this);
	}

	void OnDestroy() {
		FilterModule.Unsubscribe(this);
	}

	protected virtual void CleanUp() {
	}

	// Update is called once per frame
	void Update () {
	
	}
}
