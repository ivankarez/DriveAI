public class Signal<T>
{
    public delegate void OnChangedHandler(T oldValue, T newValue);
    public event OnChangedHandler OnChanged;

    private T value;
    public T Value
    {
        get => value;
        set
        {
            var oldValue = this.value;
            this.value = value;
            OnChanged?.Invoke(oldValue, value);
        }
    }

    public Signal(T value)
    {
        this.value = value;
    }
}
