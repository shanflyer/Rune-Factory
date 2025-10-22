#if UNITY_ANDROID 
using UnityEngine;
public static class FridaLoader {
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
  static void LoadGadget() {
    try {
      using (var sys = new AndroidJavaClass("java.lang.System"))
        sys.CallStatic("loadLibrary", "frida-gadget"); // 对应 libfrida-gadget.so
      Debug.Log("[Frida] Gadget loaded");
    } catch (System.Exception e) {
      Debug.LogWarning("[Frida] Failed to load gadget: " + e);
    }
  }
}
#endif