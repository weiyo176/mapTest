using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Maps;
using M2Mqtt;
using M2Mqtt.Messages;
using static Android.Icu.Text.Transliterator;
using Android.Util;

namespace mapTest
{
    public partial class MainPage : ContentPage
    {
        private Polyline polyline;
        private Polyline user1;
        private List<Xamarin.Forms.Maps.Position> positions;
        private Xamarin.Forms.Maps.Position lastPosition;
        int i = 0;
        MqttClient MClient;
        Label m;
        public MainPage()
        {
            InitializeComponent();
            //MClient = new MqttClient("broker.MQTTGO.io");
            //MClient.Connect("MQTTGO-3113472595");
            //if (MClient.IsConnected)
            //{
            //    string[] topics = new string[2];
            //    topics[0] = "123";
            //    topics[1] = "456";
            //    byte[] msgs = new byte[2];
            //    msgs[0] = MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE;
            //    msgs[1] = MqttMsgBase.QOS_LEVEL_AT_LEAST_ONCE;
            //    _ = MClient.Subscribe(topics, msgs);
            //    MClient.MqttMsgPublishReceived += MClient_MqttMsgPublishReceived;
            //}
            user1 = new Polyline
            {
                StrokeColor = Color.Blue,
                StrokeWidth = 12
            };
            map.MapElements.Add(user1);

            lastPosition = new Xamarin.Forms.Maps.Position(24.13333, 120.68333); // 初始位置
            AddPin();
            StartLocationUpdate();
            m = mqtt1;
            // create client instance --------------------------------------------
            MqttClient client = new MqttClient("broker.MQTTGO.io");
            //
            // register to message received 
            client.MqttMsgPublishReceived += client_MqttMsgPublishReceived;

            string clientId = Guid.NewGuid().ToString();
            client.Connect(clientId);

            // subscribe to the topic "/home/temperature" with QoS 2 
            client.Subscribe(new string[] { "123" }, new byte[] { MqttMsgBase.QOS_LEVEL_EXACTLY_ONCE });

            //get random number
            int n = Get_Random(1,500); 
        }
        public static int Get_Random(int min, int max)
        {
            //Random rnd = new Random(Guid.NewGuid().GetHashCode());
            Random rnd = new Random();
            int result = rnd.Next(min, max);
            return result;
        }


         void client_MqttMsgPublishReceived(object sender, MqttMsgPublishEventArgs e)
        {
            string str = System.Text.Encoding.Default.GetString(e.Message);
            //string location = "24.13333 120.68333";
            //change location into Location
            //try cover string to double
            try
            {
                string[] loc = str.Split(' ');
                double lat = Convert.ToDouble(loc[0]);
                double lon = Convert.ToDouble(loc[1]);
                Xamarin.Forms.Maps.Position position = new Xamarin.Forms.Maps.Position(lat, lon);
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    mqtt1.Text = str;
                    AddPin(position);
                    AddLine(position);
                });
            }
            catch (Exception ex)
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    mqtt1.Text = str;
                });
            }
            //double lat = Convert.ToDouble(loc[0]);
            //double lon = Convert.ToDouble(loc[1]);

            // handle message received 
        }

        private void AddLine(Xamarin.Forms.Maps.Position position)
        {
            if (positions.Count == 0)
            {
                positions.Add(position);
                user1.Geopath.Add(position);
            }
            else
            {
                if (positions[positions.Count-1] !=position)
                {
                positions.Add(position);
                user1.Geopath.Add(position);
                }
            }
            
        }

        //static void MClient_MqttMsgPublishReceived(object sender,MqttMsgConnectEventArgs e)
        //{
        //    string topic = e.Topic;
        //    string value = Encoding.UTF8.GetString(e.Message);
        //    MainThread.BeginInvokeOnMainThread(() =>
        //    {
        //        if (topic == "123") mqtt.Text = value;
        //        if (topic == "456") mqtt.Text = value;
        //    });
        //}
        public void AddPin(Xamarin.Forms.Maps.Position position)
        {
            //clear map
            map.Pins.Clear();
            //camera focus on user
            //map.MoveToRegion(MapSpan.FromCenterAndRadius(position, Distance.FromKilometers(0.5)));
            //Add pin in map and pin ID
            Pin pin = new Pin
            {
                Label = "user",
                Address = position.ToString(),
                Type = PinType.Place,
                Position = position
            };
            map.Pins.Add(pin);
        }
        private bool StartLocationUpdate()
        {
            // 設定每5秒更新一次位置
            Device.StartTimer(TimeSpan.FromSeconds(3), () =>
            {
                Device.BeginInvokeOnMainThread(async () =>
                {
                    await GetAndDisplayLocation();
                });

                // 返回 true，以繼續定時器的執行；返回 false，以停止定時器。
                return true;
            });

            return true;
        }
        public async Task GetAndDisplayLocation()
        {
            try
            {
                var location = await Geolocation.GetLocationAsync(new GeolocationRequest
                {
                    DesiredAccuracy = GeolocationAccuracy.Best,
                    Timeout = TimeSpan.FromSeconds(30)
                });
                Xamarin.Forms.Maps.Position loc = new Xamarin.Forms.Maps.Position(location.Latitude, location.Longitude);
                if (lastPosition != loc)
                {
                    // 添加新的位置到 Polyline
                    polyline.Geopath.Add(loc);
                    lastPosition = loc;
                }

            }
            catch (Exception ex)
            {
                return;
                // 处理异常
            }
        }
        public void AddPin()
        {
            Xamarin.Forms.Maps.Position position = new Xamarin.Forms.Maps.Position(24.13333, 120.68333);
            // Instantiate a Circle
            Circle circle = new Circle
            {
                Center = position,
                Radius = new Distance(250),
                StrokeColor = Color.Red,
                StrokeWidth = 8,
                FillColor = new Color(1, 0, 0, 0.2)
            };
            Pin pin = new Pin
            {
                Label = "Santa Cruz",
                Address = "The city with a boardwalk",
                Type = PinType.Place,
                Position = position
            };
            map.Pins.Add(pin);
            // Add the Circle to the map's MapElements collection
            map.MapElements.Add(circle);

            // Initialize the Polyline
            polyline = new Polyline
            {
                StrokeColor = Color.Blue,
                StrokeWidth = 12
            };
            map.MapElements.Add(polyline);
        }


        private void reset(object sender, EventArgs e)
        {
            polyline.Geopath.Clear();
        }
        

    }
}
