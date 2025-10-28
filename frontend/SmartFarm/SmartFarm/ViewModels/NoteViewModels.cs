using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SmartFarm.Models;

namespace SmartFarm.ViewModels
{
    public class NoteViewModel : INotifyPropertyChanged
    {
        public ObservableCollection<Note> Notes { get; set; } = new();
        public ObservableCollection<Note> FilteredNotes { get; set; } = new();

        private string _searchText = "";
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                FilterNotes();
                OnPropertyChanged(nameof(SearchText));
            }
        }

        public void AddNote(string content)
        {
            if (!string.IsNullOrWhiteSpace(content))
            {
                var note = new Note { Content = content, IsDone = false };
                Notes.Add(note);
                FilterNotes();
            }
        }

        public void FilterNotes()
        {
            FilteredNotes.Clear();
            foreach (var note in Notes.Where(n => n.Content.ToLower().Contains(SearchText.ToLower())))
            {
                FilteredNotes.Add(note);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

