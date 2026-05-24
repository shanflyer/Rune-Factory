using UnityEngine;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TestImage : MonoBehaviour
{
    public Vector2 size;
    public Image image;
    public SpriteResourceRenference spriteResourceRenference;
    public void TestAction()
    {
        spriteResourceRenference.SetImageSprite(image, size);
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TestImage))]
public class TestImageEditor :Editor
{
   public TestImage testImage
    {
        get => target as TestImage;
    }
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Test"))
        {
            testImage.TestAction();
        }
    }
}
#endif
