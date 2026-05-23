using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
[ExecuteAlways]
public class MyLightBase : MonoBehaviour
{
    [HideInInspector]
    [SerializeField]
    Color color;
    [HideInInspector]
    [SerializeField]
    float intensity = 1;
    [HideInInspector]
    [SerializeField]
    float value = 1; 
    [SerializeField]
    protected new Renderer renderer;
    [SerializeField]
    bool fixedColor = false;
    public Color Color
    {
        get => color;
        set
        { 
            if (!fixedColor&&color != value)
            {
                color = value;
              
            }
            RefreshColor();
        }
    }
    public float Intensity
    {
        get => intensity;
        set
        {
            if (intensity != value)
            {
                intensity = value;
              
            }
            RefreshColor();
        }
    }
    public float Value
    {
        get => value;
        set
        {
            if (this.value != value)
            {
                this.value = value;
                RefreshColor();
            }
        }
    }
    protected Color lightColor
    {
        get => Color * Intensity*Value;
    }
    public virtual void OnEnable()
    {
        if (renderer == null)
        {
            renderer = GetComponent<Renderer>();
        }
    }
    public virtual void RefreshColor()
    {

    }
    public virtual void Test()
    {

    }
}
#if UNITY_EDITOR
[CustomEditor(typeof(MyLightBase),true)]
public class MyLightBaseEditor : Editor
{
    MyLightBase MyLightBase => target as MyLightBase;
    SerializedProperty m_Color;
    SerializedProperty m_Intensity;
    SerializedProperty m_Value;

   static GUIContent ColorContent = EditorGUIUtility.TrTextContent("LightColor");
   static GUIContent IntensityContent = EditorGUIUtility.TrTextContent("LightIntensity");
    static GUIContent ValueContent = EditorGUIUtility.TrTextContent("MulValue");
    void OnEnable()
    {
        m_Color = serializedObject.FindProperty("color");
        m_Intensity = serializedObject.FindProperty("intensity");
        m_Value = serializedObject.FindProperty("value");
    }
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        base.OnInspectorGUI();
        EditorGUILayout.Space();
        EditorGUILayout.PropertyField(m_Color, ColorContent);
        EditorGUILayout.PropertyField(m_Intensity,IntensityContent);
        EditorGUILayout.PropertyField(m_Value, ValueContent);
        if (serializedObject.ApplyModifiedProperties())
        {
            MyLightBase.RefreshColor();
        }
        if (GUILayout.Button("≤‚ ‘"))
        {
            MyLightBase.Test();
        }
    }
}
#endif
