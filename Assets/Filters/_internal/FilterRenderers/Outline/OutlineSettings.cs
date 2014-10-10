using UnityEngine;
using System.Collections;
using System;

namespace Filters {

	[Serializable]
	public class OutlineSettings : BaseSettings {

		[SerializeField]
		public OutlineMethod method;
		[SerializeField]
		public OutlineFilterOutput output;
	}
	
}
