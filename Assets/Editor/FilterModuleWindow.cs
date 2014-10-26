using UnityEngine;
using System.Collections;
using UnityEditor;

public class FilterModuleWindow : EditorWindow {

	private enum State : int {
		Ready,
		NoSelection,
		SelectionHasNoMeshRenderer
	}

	private enum Panel : int {
		Global, Local
	}


	private const string FEEDBACK_SELECTION_NO_MESHRENDERER = "Filters need gameobjects with a mesh renderer attached.";
	private const string FEEDBACK_SELECTION_MISSING = "Select a game object with an attached mesh renderer to apply filters.";

	private State state;
	private Panel activePanel;

	[MenuItem ("Window/Filters")]
	public static void  ShowWindow () {
		FilterModuleWindow window = (FilterModuleWindow) EditorWindow.GetWindow(typeof(FilterModuleWindow));
		window.title = "Filters";
		window.autoRepaintOnSceneChange = true;
		window.activePanel = Panel.Global;
	}

	void OnGUI () {

		DrawToolBar();

		switch(activePanel) {
			case Panel.Local : 
				RenderGuiForLocal();
				break;
			case Panel.Global : 
				RenderGuiForGlobal();
				break;
		}


	}

	private void RenderGuiForLocal() {
		if(state == State.Ready) ExposeUI();
		else ReportErrors();
	}

	private void RenderGuiForGlobal() {
	}

	private void ExposeUI() {

		EditorGUILayout.BeginHorizontal();
		
		EditorGUILayout.LabelField("Fine");
		
		EditorGUILayout.EndHorizontal();
	}

	void DrawToolBar() {
		EditorGUILayout.BeginHorizontal();
			// Logic: If a given toggle button has been pressed we update the activePanel enum. If not
			// we leave it alone. 
			activePanel = GUILayout.Toggle(activePanel == Panel.Local, "Instance", EditorStyles.toolbarButton) ? 
				Panel.Local : activePanel;
			activePanel = GUILayout.Toggle(activePanel == Panel.Global, "Global settings", EditorStyles.toolbarButton) ? 
				Panel.Global : activePanel;
			
		EditorGUILayout.EndHorizontal();
	}


	private void ReportErrors() {
		
		EditorGUILayout.BeginHorizontal();

		string feedback = GetFeedback();

		EditorGUILayout.LabelField(feedback);
		
		EditorGUILayout.EndHorizontal();
	}

	private string GetFeedback() {
		switch(state) {
			case State.NoSelection : 
				return FEEDBACK_SELECTION_MISSING;
			case State.SelectionHasNoMeshRenderer : 
				return FEEDBACK_SELECTION_NO_MESHRENDERER;
			default : 
				return "";
		}
	}

	void OnSelectionChange() {
		GameObject[] selection = Selection.gameObjects;	

		state = State.Ready;

		System.Array.ForEach( selection, element => {
			if(element.GetComponent<MeshRenderer>() == null) state = State.SelectionHasNoMeshRenderer; 
		});

		if(selection.Length == 0) state = State.NoSelection;

		Repaint();
	}
}
