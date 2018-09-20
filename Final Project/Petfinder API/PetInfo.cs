using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Petfinder_API
{
    public class ShelterInfo
    {
        public string shelterName { get; set; }

        public string shelterIdField { get; set; }

        public string address1Field { get; set; }

        public string address2Field { get; set; }

        public string cityField { get; set; }

        public string stateField { get; set; }

        public double latitudeField;

        public double longitudeField;

        public string phoneField { get; set; }
    }

    public class PetInfo
    {
        public string nameField { get; set; }

        public string shelterIdField { get; set; }

        public petfinderPetRecordMedia mediaField { get; set; }

        public Dictionary<string, string> petData { get; set; }
    }
}
