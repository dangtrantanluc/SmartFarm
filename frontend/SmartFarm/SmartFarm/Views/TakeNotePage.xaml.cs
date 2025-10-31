namespace SmartFarm.Views;

public partial class TakeNotePage : ContentPage
{
	public TakeNotePage()
	{
		InitializeComponent();
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
        Console.WriteLine("Da nhan nut them ghi chu");
    }
}
