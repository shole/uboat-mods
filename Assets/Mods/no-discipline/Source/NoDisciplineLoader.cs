using UnityEngine;

public class NoDisciplineLoader : UBOAT.Game.IUserMod {
	public void OnLoaded() {
		GameObject go = new GameObject("NoDiscipline");
		go.AddComponent<NoDiscipline>();
		GameObject.DontDestroyOnLoad(go);
	}
}
