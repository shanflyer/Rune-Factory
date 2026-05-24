
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public delegate void ColorEvent(Color c);
public class ColorPickerPanel : GamePanel<MyColor>
{
    private  ColorEvent onCC=>data.colorEvent;
    [SerializeField]
    Image Bright;
    [SerializeField]
    RectRangeView rectRangeView;
    [SerializeField]
    Slider colorH;
    [SerializeField]
    Button closeButton;
    [SerializeField]
    TMP_InputField RInput, GInput, BInput;
    [SerializeField]
    Image colorPreview;

    protected override void Awake()
    {
        colorH.onValueChanged.AddListener((float value) =>
        {
            Color color = Color.HSVToRGB(value, rectRangeView.m_Value.x, rectRangeView.m_Value.y);
            RefreshColor(color);

            Color rectImageColor = Color.HSVToRGB(value, 1, 1);
            Bright.SetGradientColorTR(rectImageColor);
        });
        rectRangeView.vector2Delegate = (Vector2 value) =>
        {
            Color color = Color.HSVToRGB(colorH.value, value.x, value.y);

            RefreshColor(color);
        };
        RInput.onValueChanged.AddListener((string value) =>
        {
            RefreshInputColor();
        });
        GInput.onValueChanged.AddListener((string value) =>
        {
            RefreshInputColor();
        });
        BInput.onValueChanged.AddListener((string value) =>
        {
            RefreshInputColor();
        });

        closeButton.onClick.AddListener(Close);
        base.Awake();
    }
    void RefreshColor(Color color)
    {
        colorPreview.color = color;
        RInput.SetTextWithoutNotify(((int)(color.r * 255)).ToString());
        GInput.SetTextWithoutNotify(((int)(color.g * 255)).ToString());
        BInput.SetTextWithoutNotify(((int)(color.b * 255)).ToString());
        if (onCC != null)
        {
            onCC.Invoke(color);
        }
    }
    void RefreshInputColor()
    {
        int rValue = int.Parse(RInput.text);
        int gValue = int.Parse(GInput.text);
        int bValue = int.Parse(BInput.text);
        Color color=new Color(rValue/225.0f, gValue / 225.0f, bValue / 225.0f);
        RefreshColor(color);
    }
    public override void InitReferenceData(MyColor myColor)
    {
        base.InitReferenceData(myColor);
        var color = myColor.color;
        Color.RGBToHSV(color, out float h, out float s, out float v);
        colorH.value = h;
        rectRangeView.m_Value = new Vector2(s, v);

        Color rectImageColor = Color.HSVToRGB(h, 1, 1);
        Bright.SetGradientColorTR(rectImageColor);
    }
    public override void SetPanelUISerializeObj()
    {
        base.SetPanelUISerializeObj();
        colorH = FindChildGameObject<Slider>("SliderMain");
        rectRangeView = FindChildGameObject<RectRangeView>("Bright");
        closeButton = FindChildGameObject<Button>("Close");
        RInput = FindChildGameObject<TMP_InputField>("InputFieldRed");
        GInput = FindChildGameObject<TMP_InputField>("InputFieldGreen");
        BInput = FindChildGameObject<TMP_InputField>("InputFieldBlue");
        colorPreview = FindChildGameObject<Image>("ColorPreview");
        Bright = FindChildGameObject<Image>("Bright");
    }



}
