using System.ComponentModel;
using System.Text.Json.Serialization;

public class Note : INotifyPropertyChanged
{
    [JsonPropertyName("userId")]
    public string UserId { get; set; }

    public string Key { get; set; }

    private string _message;
    [JsonPropertyName("message")]
    public string Message
    {
        get => _message;
        set
        {
            if (_message != value)
            {
                _message = value;
                OnPropertyChanged(nameof(Message));
            }
        }
    }

    private DateTime _timestamp;
    [JsonPropertyName("timestamp")]
    public DateTime TimeStamp
    {
        get => _timestamp;
        set
        {
            if (_timestamp != value)
            {
                _timestamp = value;
                OnPropertyChanged(nameof(TimeStamp));
                OnPropertyChanged(nameof(TimeDisplay));
            }
        }
    }

    public string TimeDisplay => TimeStamp.ToString("HH:mm");

    public event PropertyChangedEventHandler PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
