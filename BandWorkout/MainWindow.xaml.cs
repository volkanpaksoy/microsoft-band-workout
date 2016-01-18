using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using RestSharp;
using Syncfusion.UI.Xaml.Charts;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BandWorkout
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string _summariesUrl = "https://api.microsofthealth.net/v1/me/Summaries/daily";

        public ObservableCollection<BandSummary> SummaryList { get; set; }
       
        public MainWindow()
        {
            InitializeComponent();

            SummaryList = new ObservableCollection<BandSummary>();

            var columnSeries1 = new ColumnSeries()
            {
                ItemsSource = SummaryList,
                XBindingPath = "Date",
                YBindingPath = "StepCount",
                EnableAnimation = true,
                ShowTooltip = true
            };

            var columnSeries2 = new ColumnSeries()
            {
                ItemsSource = SummaryList,
                XBindingPath = "Date",
                YBindingPath = "Distance",
                EnableAnimation = true,
                ShowTooltip = true
            };

            stepCountChart.Series.Add(columnSeries1);
            stepCountChart.Series.Add(columnSeries2);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Settings.Default.AccessToken))
            {
                GetAuthorization();
                return;
            }

            try
            {
                GetSummaries();
            }
            catch (HttpException httpEx) when (httpEx.GetHttpCode() == 401)
            {
                GetAuthorization();
                GetSummaries();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Dammit!", MessageBoxButton.OK);
            }
        }

        public void GetSummaries()
        {
            var client = new RestClient(_summariesUrl);
            var request = new RestRequest("", Method.GET);
            request.AddHeader("Authorization", $"bearer {Settings.Default.AccessToken}");
            var response = (RestResponse)client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                throw new HttpException(401, "Unauthorized access");
            }

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var content = response.Content;

                ParseSummaries(content);
            }
        }

        public void ParseSummaries(string rawContent)
        {
            var summaryJsonArray = JObject.Parse(rawContent)["summaries"].ToArray();
            foreach (var summaryJson in summaryJsonArray)
            {
                var summary = new BandSummary();
                summary.StepCount = summaryJson["stepsTaken"]?.Value<int>() ?? 0;
                summary.TotalCalories = summaryJson["caloriesBurnedSummary"]["totalCalories"]?.Value<int>() ?? 0;
                summary.Distance = summaryJson["distanceSummary"]["totalDistanceOnFoot"]?.Value<double>() / 100 ?? 0.0; // Distance is in cm, convert to m
                summary.Date = summaryJson["parentDay"].Value<DateTime>();
                SummaryList.Insert(0, summary);
            }
        }
        
        public void GetAuthorization()
        {
            new OAuthWindow().ShowDialog();
        }
    }
}

