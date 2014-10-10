using UnityEngine;
using System.Collections;

public static class MathUtils  {

	/// <summary>
	/// Min inclusive, max exclusive. 
	/// </summary>
	/// <returns><c>true</c> if is within range the specified entry x y; otherwise, <c>false</c>.</returns>
	/// <param name="entry">Entry.</param>
	/// <param name="x">Range delimiter, can be either min or max.</param>
	/// <param name="y">Range delimiter, can be either min or max.</param>
	public static bool IsWithinRange(this int entry, float x, float y){ 
		return (entry < x && entry >= y) || (entry >= x && entry < y);  
	}

	/// <summary>
	/// Min inclusive, max inclusive. 
	/// </summary>
	/// <returns><c>true</c> if is within range the specified entry x y; otherwise, <c>false</c>.</returns>
	/// <param name="entry">Entry.</param>
	/// <param name="x">Range delimiter, can be either min or max.</param>
	/// <param name="y">Range delimiter, can be either min or max.</param>
	public static bool IsWithinRange(this float entry, float x, float y){ 
		return (entry <= x && entry >= y) || (entry >= x && entry <= y);  
	}

}
