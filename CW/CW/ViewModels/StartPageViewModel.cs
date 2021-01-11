using System;
using System.Collections.Generic;
using System.Text;
using CW.Views;
using System.Windows.Input;
using Xamarin.Forms;
using CW.Validations;
using CW.Views.InsideViews;

using CW.Services;
using System.Net.Http;
using Newtonsoft.Json;
using CW.Models;
using System.IO;

namespace CW.ViewModels
{
    public class StartPageViewModel : BaseViewModel
    {
        public ICommand AuthorizationCommand { get; protected set; }
        public ICommand ShowLoginFormCommand { get; protected set; }
        public ICommand HideLoginFormCommand { get; protected set; }
        public ICommand OpenMapPageCommand { get; protected set; }
        public ICommand ClosePageCommand { get; protected set; }
        public ICommand OpenExchangesRatesPageCommand { get; protected set; }
        public INavigation Navigation { get; set; }

        private bool _isButtonEnabled;

        public StartPageViewModel()
        {
            _isButtonEnabled = true;
            AuthorizationCommand = new Command(Authorize, (_) => IsButtonEnabled);

            OpenMapPageCommand = new Command(OpenMapPage, () => IsButtonEnabled);
            OpenExchangesRatesPageCommand = new Command(OpenExchangesRatesPage, () => IsButtonEnabled);
            ClosePageCommand = new Command(Back);

            ShowLoginFormCommand = new Command(() => IsLoginFormVisible = true);
            HideLoginFormCommand = new Command(() => {
                IsLoginFormVisible = false;
                AutorizationInfo = "Введите Ваш логин и пароль";
            });

            UserLogin = new ValidatableObject<string>();
            UserPassword = new ValidatableObject<string>();
            AddValidations();

        }

        private async void Authorize(object obj)
        {
            IsButtonEnabled = false;

            if (Validate())
            {
                Tuple<bool, string> response = await UserService.Instance.Login(UserLogin.Value, UserPassword.Value);

                if (response.Item1 == true)
                {
                    IsLoginFormVisible = false;
                    MessagingCenter.Send(this, "authorized");
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Message", response.Item2, "OK");
                    IsButtonEnabled = true;
                }
            }
            else
            {
                IsButtonEnabled = true;
            }
        }

        // Enable or disable all buttons on the current page
        private bool IsButtonEnabled
        {
            get => _isButtonEnabled;

            set
            {
                if (value != _isButtonEnabled)
                {
                    _isButtonEnabled = value;

                    (AuthorizationCommand as Command)?.ChangeCanExecute();
                    (OpenMapPageCommand as Command)?.ChangeCanExecute();
                    (OpenExchangesRatesPageCommand as Command)?.ChangeCanExecute();
                }
            }
        }

        private bool _isLoginFormVisible;
        public bool IsLoginFormVisible
        {
            get => _isLoginFormVisible;
            set
            {
                if (value != _isLoginFormVisible)
                {
                    _isLoginFormVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        private string _autorizationIngo = "Введите Ваш логин и пароль";
        public string AutorizationInfo
        {
            get => _autorizationIngo;
            set
            {
                if (_autorizationIngo != value)
                {
                    _autorizationIngo = value;
                    OnPropertyChanged();
                }
            }
        }

        //private ValidatableObject<string> _userLogin;
        public ValidatableObject<string> UserLogin { get; set; }

        public ValidatableObject<string> UserPassword { get; set; }


        private void OpenMapPage()
        {
            IsButtonEnabled = false;
            Navigation.PushAsync(new NearbyBanksView(this));
        }

        private void OpenExchangesRatesPage()
        {
            IsButtonEnabled = false;
            Navigation.PushAsync(new ExchangeRates(this));
        }

        private void Back()
        {
            IsButtonEnabled = true;
            Navigation.PopAsync();
        }

        private void AddValidations()
        {
            UserLogin.Validations.Add(new IsNotNullOrEmptyRule<string>
            {
                ValidationMessage = "Username is required."
            });
            UserPassword.Validations.Add(new IsNotNullOrEmptyRule<string>
            {
                ValidationMessage = "Password is required."
            });
        }

        private bool Validate()
        {
            bool isValidUser = ValidateUserName();
            bool isValidPassword = ValidatePassword();
            return isValidUser && isValidPassword;
        }

        private bool ValidateUserName()
        {
            return UserLogin.Validate();
        }

        private bool ValidatePassword()
        {
            return UserPassword.Validate();
        }
    }
}
