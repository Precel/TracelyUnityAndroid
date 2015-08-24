using UnityEngine;
using System.Collections;

public class TracelyExample : MonoBehaviour {

	public void UnityStackOverflow() {
		#if !UNITY_EDITOR
			Overflow(Random.Range(0,1000000).ToString());
		#else
			Debug.LogWarning("StackOverflow aborted in editor environment, this will crash Unity Editor.");
		#endif
	}

	public void DivideByZero() {
		//Supress Warnings
		#pragma warning disable 

		int eight = 8;
		int zero = 0;
		int result = eight / zero;
		
		#pragma warning restore
	}

	public void NullPointerException() {
		Stack s = null;
		s.Push(1);
	}

	public void SystemException() {
		throw new System.Exception();
	}

	public void ParseError() {
		int.Parse("test");
	}

	public void ArrayIndexOutOfRange() {
		//Supress Warnings
		#pragma warning disable 

		int[] arr = {1,2,3};
		int result = arr[4];

		#pragma warning restore
	}

	private void Overflow(string s) {
		Overflow(s);
	}
}
