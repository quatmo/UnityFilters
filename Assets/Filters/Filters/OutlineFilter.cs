using UnityEngine;
using System;


public class OutlineFilter : BaseFilter
{
	private const int MAX_DEPTH = 256;
	private const int MIN_DEPTH = 1;

	public int depth = 1;
	public Color color;		

	private float _width;
	public float width {
		set {
			if(value.IsWithinRange(MIN_DEPTH, MAX_DEPTH)) {
				_width = value;
			} else {
				_width = Mathf.Clamp(value, MIN_DEPTH, MAX_DEPTH);
				Debug.Log(String.Format("Argument was {} but should be in range [{1}{2}]. Value clamped to {4}", value, MIN_DEPTH, MAX_DEPTH, _width));
			}
		} get {
			return _width;
		}
	}

	private System.Func<Color> _updateColor;
	public System.Func<Color> updateColor {
		set {
			_updateColor = value;
		}
	}


	void Update() {
		if(_updateColor != null) {
			color = _updateColor();
		}
	}
}

