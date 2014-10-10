using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Filters;
using System.Linq;

public class FilterModule : MonoBehaviour {

	private static FilterModule _singleton;

	[SerializeField]
	public GlowSettings glowSettings;
	[SerializeField]
	public OutlineSettings outlineSettings;
	[SerializeField]
	public GhostSettings ghostSettings;

	private GlowRenderer glowRenderer;
	private OutlineRenderer outlineRenderer;
	private GhostRenderer ghostRenderer;

	private List<BaseFilter> _subscribers;
	public  List<BaseFilter> subscribers {
		get {
			_subscribers = _subscribers ?? new List<BaseFilter>();
			return _subscribers;
		}
	}

	public static void Touch() {
		Init();
	}

	public static void Subscribe(BaseFilter filter) {
		_singleton._Subscribe(filter);
	}

	public static void Unsubscribe(BaseFilter filter) {
		_singleton._Unsubscribe(filter);
	}


	private static void Init() {
		if(_singleton != null) return;
		if(Camera.main == null) {
			Debug.LogError("No main camera in scene");
			return;
		}

		GameObject filterRendererGameObject = Camera.main.gameObject;
		_singleton = filterRendererGameObject.GetComponent<FilterModule>() ?? filterRendererGameObject.AddComponent<FilterModule>();
	}
		
	private void _Subscribe(BaseFilter filter) {
		subscribers.Add(filter);
	}
	
	private void _Unsubscribe(BaseFilter filter) {
		subscribers.Remove(filter);
	}

	// Use this for initialization
	void Start () {
		if(!SystemInfo.supportsImageEffects) {
			Debug.LogError("The system does not support image effects");
			this.enabled = false;
		}
		
		glowRenderer = new GlowRenderer(glowSettings);
		outlineRenderer = new OutlineRenderer(outlineSettings);
		ghostRenderer = new GhostRenderer(ghostSettings);
	}

	void OnRenderImage(RenderTexture source, RenderTexture destination) {	

		List<BaseFilter> activeSubscribers = subscribers.Where( s => s.enabled ).ToList();
		
		// Blit the screen contents as background for the filters.
		Graphics.Blit(source, destination);
		
		if(glowRenderer.enabled) {
			// Get glow filter subscribers
			List<GlowFilter> glowFilterSubscribers = activeSubscribers.FindAll( s => s is GlowFilter ).ConvertAll( s => s as GlowFilter );
			glowRenderer.RenderGlowFilter(glowFilterSubscribers, this.camera, source, destination);
		}
		
		if(outlineRenderer.enabled) {
			// Get the outline filter subscribers
			List<OutlineFilter> outlineSubscribers = activeSubscribers.FindAll( s => s is OutlineFilter ).ConvertAll( s => s as OutlineFilter );
			outlineRenderer.RenderOutlineFilter(outlineSubscribers, this.camera, source, destination);
		}

	}

	void OnPostRender() {
		// For effects that can be part of unitys standard rendering flow, and needs access to the scenes depth buffer.
		List<BaseFilter> activeSubscribers = subscribers.Where( s => s.enabled ).ToList();

		if(ghostRenderer.enabled) {
			// Get the ghost subscribers
			List<GhostFilter> ghostSubscribers = activeSubscribers.FindAll( s => s is GhostFilter ).ConvertAll( s => s as GhostFilter );
			ghostRenderer.RenderGhosts(ghostSubscribers, this.camera);
		}
	}

	// Update is called once per frame
	void Update () {
	}
}
