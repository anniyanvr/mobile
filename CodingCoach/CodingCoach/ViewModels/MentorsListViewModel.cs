﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using CodingCoach.DotNetClient.Services;
using CodingCoach.Models;
using Xamarin.Forms;

namespace CodingCoach.ViewModels
{
    public class MentorsListViewModel : BaseViewModel
    {
        private string _techFilter;

        private readonly MentorsService _mentorsService;

        public ObservableCollection<MentorDto> Mentors { get; set; }
        public Command LoadItemsCommand { get; set; }

        public ICommand TechListSelectedCommand =>
            new Command<string>(OnTechListSelected);

        private async void OnTechListSelected(string key)
        {
            _techFilter = key;
            await LoadMentors(true);
        }


        public MentorsListViewModel()
        {
            lastPage = 1;
            _mentorsService = new MentorsService();
            Title = "Mentors";
            Mentors = new ObservableCollection<MentorDto>();
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());
        }

        private IList<KeyValuePair<string, string>> _techList;

        public IList<KeyValuePair<string, string>> TechList
        {
            get => _techList;
            private set => SetProperty(ref _techList, value);
        }

        private async Task LoadMentors(bool clearList = false)
        {
            if (clearList)
            {
                Mentors.Clear();
                lastPage = 1;
            }

            var request = new MentorsRequest
            {
                Limit = 10,
                Page = lastPage++
            };
            var mentorsResponse = await _mentorsService.Get(request);
            //TechList = _apiAccessService.GetTechList().Select(t => new KeyValuePair<string, string>(t, t)).ToList();

            if (mentorsResponse != null)
            {
                if (!mentorsResponse.Pagination.HasMore)
                    ItemsThreshold = -1;

                if (mentorsResponse.Data?.Any() ?? false)
                {
                    foreach (var mentor in mentorsResponse.Data)
                    {
                        Mentors.Add(mentor.ToMentorDto());
                    }
                }
            }


        }

        private static int lastPage;

        private async Task ExecuteLoadItemsCommand()
        {
            if (IsBusy)
                return;
            IsBusy = true;
            try
            {
                await LoadMentors();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private int _itemsThreshold;

        public int ItemsThreshold
        {
            get => _itemsThreshold;
            set
            {
                _itemsThreshold = value;
                OnPropertyChanged();
            }
        }
    }
}