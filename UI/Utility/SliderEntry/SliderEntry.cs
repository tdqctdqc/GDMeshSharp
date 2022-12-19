using Godot;
using System;

public class SliderEntry : Control
{
    private static PackedScene _scene = (PackedScene) GD.Load("res://UI/Utility/SliderEntry/SliderEntry.tscn");
    private Action<float> _action;
    private Func<float, string> _labelFunc;
    private Label _label;
    private Slider _slider;
    public float Value => (float)_slider.Value;
    public static SliderEntry GetSliderEntry(Func<float, string> labelFunc, 
        Action<float> action, int min, int max)
    {
        var entry = (SliderEntry)_scene.Instance();
        entry.Setup(labelFunc, action, min, max);
        return entry;
    }

    public void Setup(Func<float, string> labelFunc, Action<float> action,
        int min, int max)
    {
        _action = action;
        _labelFunc = labelFunc;
        _label = (Label) FindNode("Label");
        _slider = (Slider) FindNode("Slider");
        _slider.MinValue = min;
        _slider.MaxValue = max;
        _slider.Connect("value_changed", this, nameof(DoValueChange));
        _label.Text = _labelFunc((float)_slider.Value);
    }

    private void DoValueChange(float value)
    {
        _action?.Invoke(value);
        _label.Text = _labelFunc(value);
    }
}
