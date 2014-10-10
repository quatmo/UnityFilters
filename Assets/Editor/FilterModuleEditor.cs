using UnityEngine;
using System.Collections;
using UnityEditor;
using Filters;


[CustomEditor(typeof(FilterModule))]
public class FilterModuleEditor : Editor {

	private FilterModule filterModule {
		get { 
			_filterModule = _filterModule ?? this.target as FilterModule;
			return _filterModule;
		}
	}
	private FilterModule _filterModule;


	void OnEnable() {
		FilterModule.Touch();
	}

	private void DrawGlowFilterGUI() {
		string prefix = "Glow ";	
		filterModule.glowSettings.enabled = EditorGUILayout.Toggle(prefix + "Enabled", filterModule.glowSettings.enabled);
		filterModule.glowSettings.output = (GlowFilterOutput) EditorGUILayout.EnumPopup(prefix + "Output", (System.Enum) filterModule.glowSettings.output );
	}

	private void DrawOutlineFilterGUI() {
		string prefix = "Outlines ";
		filterModule.outlineSettings.enabled = EditorGUILayout.Toggle(prefix + "Enabled", filterModule.outlineSettings.enabled);
		filterModule.outlineSettings.method = (OutlineMethod) EditorGUILayout.EnumPopup(prefix + "Method", (System.Enum) filterModule.outlineSettings.method );
		filterModule.outlineSettings.output = (OutlineFilterOutput) EditorGUILayout.EnumPopup(prefix + "Output", (System.Enum) filterModule.outlineSettings.output );
	}


	public override void OnInspectorGUI() {
		//EditorGUILayout.InspectorTitlebar("hi title", );
		EditorGUILayout.Space();

		DrawGlowFilterGUI();	
		EditorGUILayout.Space();

		DrawOutlineFilterGUI();	
		EditorGUILayout.Space();
	}

}
