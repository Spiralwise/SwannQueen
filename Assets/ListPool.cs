using System;
using System.Collections.Generic;

public static class ListPool<T> {

	static Stack<List<T>> Stack = new Stack<List<T>>();

	public static List<T> Get () {
		if (Stack.Count > 0)
			return Stack.Pop ();
		return new List<T> ();
	}

	public static void Add (List<T> list) {
		list.Clear ();
		Stack.Push (list);
	}
}