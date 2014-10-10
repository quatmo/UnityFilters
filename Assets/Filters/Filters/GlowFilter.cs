using UnityEngine;
using System.Collections;

public class GlowFilter : BaseFilter {

	public Color color = Color.white;
	public float strength = MIN_STRENGTH;

	// We use 4 bits for strength information
	public const float MIN_STRENGTH = 1<<0;
	public const float MAX_STRENGTH = 1<<4;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
