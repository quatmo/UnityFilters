using UnityEngine;
using System.Collections;
using System;

namespace Filters {

	[Serializable]
	public class GlowSettings : BaseSettings  {

		[SerializeField]
		public GlowFilterOutput output;
	}
	
}
