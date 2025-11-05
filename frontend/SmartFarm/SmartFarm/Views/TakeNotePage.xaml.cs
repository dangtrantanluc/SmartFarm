using System.Threading.Tasks;
using SmartFarm.Models;
using SmartFarm.Services;
namespace SmartFarm.Views;

public partial class TakeNotePage : ContentPage
{
    private readonly NotesViewModel _viewModel;
    NotificationService notificationService;
    string timeOfNote;
    DateTime pressStart;
    string tempKeyOfNote;
    public TakeNotePage()
	{
		InitializeComponent();
        _viewModel = new NotesViewModel();
        BindingContext = _viewModel;
        notificationService = new NotificationService();
	}
    private void OnTextChanged(object sender, TextChangedEventArgs e)
    {
        var newText = e.NewTextValue;
        Console.WriteLine($"Text changed: {newText}");
    }

    private void OnEditorUnfocused(object sender, FocusEventArgs e)
    {
        Console.WriteLine("Editor focus");
    }

    private void OnAddNote(object sender, EventArgs e)
    {
        Overlay.IsVisible = true;
        InfoFrame.IsVisible = true;
        AddBtn.IsVisible = true;
        SaveBtn.IsVisible = false;
        CancelBtn.IsVisible = false;
        timePicker.Time = DateTime.Now.TimeOfDay;
    }
    private async void SaveSChedule(object sender, EventArgs e)
    {
        string userId = Preferences.Get("user_uid", string.Empty);
        string message = noteEntry.Text;
        DateTime timestamp = DateTime.Today.Add(timePicker.Time);
        await notificationService.SaveNotification(userId, message, timestamp);
        _ = _viewModel.LoadNotesAsync();
        noteEntry.Text = "";
        Overlay.IsVisible = false;
        InfoFrame.IsVisible = false;
    }
    private async void UpdateSChedule(object sender, EventArgs e)
    {
        string userId = Preferences.Get("user_uid", string.Empty);
        string newMessage = noteEntry.Text;
        DateTime newTime = DateTime.Today.Add(timePicker.Time);

        await notificationService.updateNotification(userId, tempKeyOfNote, newMessage, newTime);
        _ = _viewModel.LoadNotesAsync();
        noteEntry.Text = "";
        Overlay.IsVisible = false;
        InfoFrame.IsVisible = false;
    }
    private void DisablePopUp(object sender, TappedEventArgs e)
    {
        noteEntry.Text = "";
        Overlay.IsVisible = false;
        InfoFrame.IsVisible = false;
    }

    private void OnTimeChanged(object sender, TimeChangedEventArgs e)
    {
        var selectedTime = e.NewTime;
        var formattedTime = selectedTime.ToString(@"hh\:mm"); // định dạng HH:mm
        timeOfNote = formattedTime;
    }

    private async void OnFrameTapped(object sender, TappedEventArgs e)
    {
        InfoFrame.IsVisible = true;
        Overlay.IsVisible = true;
        AddBtn.IsVisible = false;
        SaveBtn.IsVisible = true;
        CancelBtn.IsVisible = true;
        // Lấy item tương ứng của Frame
        var frame = sender as View;
        if (frame?.BindingContext is Note note)
        {
            noteEntry.Text = note.Message;
            timePicker.Time = note.TimeStamp.TimeOfDay;
            tempKeyOfNote = note.Key;
        }
    } 
    private async void CancelUpdate(object sender, EventArgs e)
    {
        string userId = Preferences.Get("user_uid", string.Empty);

        await notificationService.RemoveNotification(userId, tempKeyOfNote);

        _ = _viewModel.LoadNotesAsync();
        noteEntry.Text = "";
        Overlay.IsVisible = false;
        InfoFrame.IsVisible = false;
    }
}
