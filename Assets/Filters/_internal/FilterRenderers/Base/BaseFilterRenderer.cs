using UnityEngine;
using System;

namespace Filters
{
	public class BaseFilterRenderer
	{
		public bool enabled {
			get {
				return baseSettings.enabled && isSupported;
			}
		}
		private BaseSettings baseSettings;
		protected bool isSupported = true;

		public BaseFilterRenderer (BaseSettings settings)
		{
			this.baseSettings = settings;
			CheckResources();
		}

		protected virtual void CheckResources() {
		}

		protected Material CheckShaderAndCreateMaterial(Shader s, Material m2Create)  {

			if (!s) { 
				Debug.Log("Missing shader in " + this.ToString ());
				baseSettings.enabled = false;	
				return null;
			}

			if (s.isSupported && m2Create && m2Create.shader == s) 
				return m2Create;
			
			if (!s.isSupported) {
				NotSupported ();
				Debug.Log("The shader " + s.ToString() + " on effect "+this.ToString()+" is not supported on this platform!");
				return null;
			}
			else {
				m2Create = new Material (s);	
				m2Create.hideFlags = HideFlags.DontSave;		
				if (m2Create) 
					return m2Create;
				else return null;
			}
		}

		private void NotSupported () {
			isSupported = false;
		}
	}
}

