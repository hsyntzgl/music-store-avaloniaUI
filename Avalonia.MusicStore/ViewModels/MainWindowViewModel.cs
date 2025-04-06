using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Windows.Input;
using Avalonia.MusicStore.Models;
using DynamicData;
using ReactiveUI;
using System.Reactive.Concurrency;

namespace Avalonia.MusicStore.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel()
    {
        RxApp.MainThreadScheduler.Schedule(LoadAlbums);
        ShowDialog = new Interaction<MusicStoreViewModel, AlbumViewModel?>();
        
        BuyMusicCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            var store = new MusicStoreViewModel();
            var result = await ShowDialog.Handle(store);
            if (result != null)
            {
                Albums.Add(result);
                await result.SaveToDiskAsync();
            }
        });
    }

    private async void LoadAlbums()
    {
        var albums = (await Album.LoadCachedAsync()).Select(x => new AlbumViewModel(x));

        foreach (var album in albums)
        {
            Albums.Add(album);
        }
        
        foreach (var album in Albums.ToList())
        {
            await album.LoadCover();
        }
    }

    public ObservableCollection<AlbumViewModel> Albums { get; } = new();
    
    public Interaction<MusicStoreViewModel, AlbumViewModel?> ShowDialog { get; }
    public string Greeting { get; } = "Welcome to My Mac!";
    
    public ICommand BuyMusicCommand { get; }
}