using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Maps.MapControl.WPF;
using Petfinder_API;

namespace Final_Project
{
    
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        List<PetInfo> petInfo = new List<PetInfo>();

        List<ShelterInfo> shelterInfo = new List<ShelterInfo>();


        private const string apikey = "df084e4b84f0efbb831a4b24250b1b2a";

        public MainWindow()
        {
            InitializeComponent();
            animalType.ItemsSource = Enum.GetValues(typeof(animalType)).Cast<animalType>();
        }

        private string FindPets(string loc, string type)
        {
            string peturl = $@"http://api.petfinder.com/pet.find?key={apikey}&animal={type}&location={loc}&count=50";
            string petData;

            WebRequest request = HttpWebRequest.Create(peturl);
            request.Method = "GET";
            WebResponse rstream = (HttpWebResponse)request.GetResponse();

            using (Stream stream = rstream.GetResponseStream())
            using (StreamReader sr = new StreamReader(stream))
            {
                petData = sr.ReadToEnd();
            }

            return petData;
        }

        private string GetShelter(string id)
        {
            string peturl = $@"http://api.petfinder.com/shelter.get?key={apikey}&id={id}";
            string petData;

            WebRequest request = HttpWebRequest.Create(peturl);
            request.Method = "GET";
            WebResponse rstream = (HttpWebResponse)request.GetResponse();

            using (Stream stream = rstream.GetResponseStream())
            using (StreamReader sr = new StreamReader(stream))
            {
                petData = sr.ReadToEnd();
            }

            return petData;
        }

        private petfinder GetData(string data)
        {
            XmlSerializer xs = new XmlSerializer(typeof(petfinder));
            StringReader sr = new StringReader(data);
            XmlTextReader xts = new XmlTextReader(sr);
            return ((petfinder)xs.Deserialize(xts));
        }

        private void Pets(petfinderPetRecordList petList)
        {
            List<String> shelterIds = new List<string>();
            List<PetInfo> pets = new List<PetInfo>();
            List<ShelterInfo> shelters = new List<ShelterInfo>();

            foreach (var pet in petList.pet)
            {
                Dictionary<string, string> data = new Dictionary<string, string>();
                if(!shelterIds.Contains(pet.shelterId))
                {
                    string shelterData = GetShelter(pet.shelterId);
                    petfinder shelter = GetData(shelterData);

                    petfinderShelterRecord shelterRecord = (petfinderShelterRecord)shelter.Item;

                    if (shelterRecord != null)
                    {
                        ShelterInfo record = new ShelterInfo
                        {
                            shelterName = shelterRecord.name,
                            shelterIdField = shelterRecord.id,
                            address1Field = shelterRecord.address1,
                            address2Field = shelterRecord.address2,
                            cityField = shelterRecord.city,
                            stateField = shelterRecord.state,
                            latitudeField = (double)shelterRecord.latitude,
                            longitudeField = (double)shelterRecord.longitude,
                            phoneField = shelterRecord.phone
                        };

                        shelters.Add(record);
                    }

                    shelterIds.Add(pet.shelterId);
                }

                var item = shelters.FirstOrDefault(d => d.shelterIdField == pet.shelterId);

                data.Add("Breed", string.Join(", ", pet.breeds.breed.ToList()));
                data.Add("Age", pet.age.ToString());
                data.Add("Sex", pet.sex.ToString());
                data.Add("Shelter", pet.contact.name);
                if (item != null)
                    data.Add("Address", item.address1Field + "\n" + item.address2Field + "\n" + item.cityField + ", " + item.stateField);
                data.Add("Phone", pet.contact.phone);
                data.Add("Fax", pet.contact.fax);
                data.Add("Email", pet.contact.email);

                PetInfo info = new PetInfo
                {
                    nameField = pet.name,
                    mediaField = pet.media,
                    shelterIdField = pet.shelterId,
                    petData = data
                };
                pets.Add(info);
            }
            petInfo = pets;
            shelterInfo = shelters;
        }

        private void Search_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string petData = FindPets(zipCode.Text, animalType.SelectedItem.ToString().ToLower());

                petfinder petList = GetData(petData);

                if (petList.Item == null)
                    throw new Exception(petList.header.status.message);

                Pets((petfinderPetRecordList)petList.Item);

                listBox.ItemsSource = petInfo;

                Map.Children.Clear();

                List<Location> locs = new List<Location>();
                foreach (var shelter in shelterInfo)
                {
                    Location location = new Location(shelter.latitudeField, shelter.longitudeField);
                    Pushpin pin = new Pushpin
                    {
                        Location = location,
                        ToolTip = shelter.shelterName + "\n" + shelter.address1Field + "\n" + shelter.address2Field + "\n" + shelter.cityField + ", " + shelter.stateField + "\n" + shelter.phoneField
                    };

                    // Adds the pushpin to the map.
                    Map.Children.Add(pin);
                    locs.Add(location);
                }
                Map.SetView(locs, new Thickness(100), 0);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
