using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using SmartFarm.Models;
using SmartFarm.Services;

namespace SmartFarm.Models
{
    public class NotesViewModel : INotifyPropertyChanged
    {

        public ObservableCollection<Note> FilteredNotes { get; } = new();

        public NotesViewModel()
        {
            _ = LoadNotesAsync();
        }

        public async Task LoadNotesAsync()
        {
            string userId = Preferences.Get("user_uid", string.Empty);
            var notifications = new NotificationService();
            await notifications.getNotifications(userId, FilteredNotes);
        }

        public ICommand TapCommand => new Command<Notes>(async item =>
        {
            var notifications = new NotificationService();

        });

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}