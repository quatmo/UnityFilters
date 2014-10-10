using UnityEngine;
using System.Collections;
using System;

public class GhostFilter : BaseFilter {

	public Texture smokeTexture;
	public Color rimColor;

	// Use this for initialization
	void Start () {
		Array.ForEach( GetComponentsInChildren<MeshRenderer>(), mr => mr.enabled = false ); 
	}

	protected override void CleanUp() {
		Array.ForEach( GetComponentsInChildren<MeshRenderer>(), mr => mr.enabled = true ); 
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
