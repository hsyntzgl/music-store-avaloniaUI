using System.Collections.ObjectModel;
using System.Reactive.Linq;
using ReactiveUI;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading;
using Avalonia.MusicStore.Models;

namespace Avalonia.MusicStore.ViewModels;

public class MusicStoreViewModel : ViewModelBase
{
    public MusicStoreViewModel()
    {
        BuyMusicCommand = ReactiveCommand.Create(() => { return SelectedAlbum; }
        );

        this.WhenAnyValue(x => x.SearchText).Throttle(TimeSpan.FromMilliseconds(400))
            .ObserveOn(RxApp.MainThreadScheduler).Subscribe(DoSearch!);
    }

    private async void DoSearch(string? s)
    {
        IsBusy = true;
        SearchResults.Clear();

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = _cancellationTokenSource.Token;

        if (!string.IsNullOrWhiteSpace(s))
        {
            var albums = await Album.SearchAsync(s);

            foreach (var album in albums)
            {
                var vm = new AlbumViewModel(album);
                SearchResults.Add(vm);
            }
        }

        if (!cancellationToken.IsCancellationRequested)
        {
            LoadCovers(cancellationToken);
        }

        IsBusy = false;
    }

    private async void LoadCovers(CancellationToken cancellationToken)
    {
        foreach (var album in SearchResults.ToList())
        {
            await album.LoadCover();

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }
        }
    }

    public ReactiveCommand<Unit, AlbumViewModel?> BuyMusicCommand { get; }

    private CancellationTokenSource _cancellationTokenSource;

    private AlbumViewModel? _selectedAlbum;

    public ObservableCollection<AlbumViewModel> SearchResults { get; } = new();

    public AlbumViewModel? SelectedAlbum
    {
        get => _selectedAlbum;
        set => this.RaiseAndSetIfChanged(ref _selectedAlbum, value);
    }

    private string? _searchText;
    private bool _isBusy;

    public string? SearchText
    {
        get => _searchText;
        set => this.RaiseAndSetIfChanged(ref _searchText, value);
    }

    public bool IsBusy
    {
        get => _isBusy;
        set => this.RaiseAndSetIfChanged(ref _isBusy, value);
    }
}