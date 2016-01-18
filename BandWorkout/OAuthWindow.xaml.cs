using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BandWorkout
{
    /// <summary>
    /// Interaction logic for OAuthWindow.xaml
    /// </summary>
    public partial class OAuthWindow : Window
    {
        private readonly string baseUrl = "https://login.live.com";
        private readonly string _scope = "mshealth.ReadActivityHistory";
        private readonly string _redirectUri = "https://login.live.com/oauth20_desktop.srf";

        public OAuthWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            string authUri = $"{baseUrl}/oauth20_authorize.srf?client_id={Settings.Default.ClientID}&scope={_scope}&response_type=code&redirect_uri={_redirectUri}";
            webBrowser.Navigate(authUri);
        }

        private void webBrowser_Navigated(object sender, System.Windows.Navigation.NavigationEventArgs e)
        {
            if (e.Uri.Query.Contains("code=") && e.Uri.Query.Contains("lc="))
            {
                string code = e.Uri.Query.Substring(1).Split('&')[0].Split('=')[1];

                string authUriRedeem = $"/oauth20_token.srf?client_id={Settings.Default.ClientID}&redirect_uri={_redirectUri}&client_secret={Settings.Default.ClientSecret}&code={code}&grant_type=authorization_code";

                var client = new RestClient(baseUrl);
                var request = new RestRequest(authUriRedeem, Method.GET);
                var response = (RestResponse)client.Execute(request);
                var content = response.Content;

                // Parse content and get auth code
                Settings.Default.AccessToken = JObject.Parse(content)["access_token"].Value<string>();
                Settings.Default.Save();

                Close();
            }
        }
    }
}
