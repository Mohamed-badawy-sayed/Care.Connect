using System.IO;
using Firebase;
using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json.Linq;
using Firebase.Database.Streaming;
using Microsoft.VisualBasic;
using Microsoft.VisualBasic.Logging;
using System.Collections;
using Microsoft.VisualBasic.ApplicationServices;
using System.Linq.Expressions;
using System.Globalization;
using System.Net.Http;
using System.Net;
using System.Web;
using System.IO;
using static System.Windows.Forms.AxHost;
using FireSharp.Config;
using FireSharp.Interfaces;
using FireSharp.Response;
using MySqlX.XDevAPI;
using Newtonsoft.Json;
using MySqlX.XDevAPI.Common;
using System.Configuration;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MainServer
{

    public partial class Form1 : Form
    {
        Firebase.Database.FirebaseClient FireClient;
        Dictionary<string, double> DistanceValuesAPI = new Dictionary<string, double>();
        string smsMessage = "";



        static IFirebaseConfig config = new FirebaseConfig
        {
            AuthSecret = "Ssanw9rmCXkVYABLZ9pjCX0CECOgIM3bPBCs6zv6",
            BasePath = "https://careconnect-1c393-default-rtdb.firebaseio.com/"
        };
        IFirebaseClient client = new FireSharp.FirebaseClient(config);

        private DistanceService distanceService;
        private PatientInfo patientInfo;
        Emergency emergency;



        public Form1()
        {
            InitializeComponent();
            FireClient = new Firebase.Database.FirebaseClient("https://careconnect-1c393-default-rtdb.firebaseio.com/");
            distanceService = new DistanceService();
            patientInfo = new PatientInfo();
            patientInfo.LoadData();

            //   MessageBox.Show("ds");

            //        MessageBox.Show(GoogleTranslate("Moahmed Badawy Sayed"));

        }



        private void Form1_Load(object sender, EventArgs e)
        {
            /*
            var obserable = FireClient.Child("CareConnect/Emergency").AsObservable<object>();
            var Subscription = obserable.Subscribe(async snapshot =>
            {
                if (snapshot.EventType == FirebaseEventType.InsertOrUpdate)
                {
                    string CollectionName = snapshot.Key;  // a random id for each record in emergency collection will be generated by flutter app
                    emergency = new Emergency();
                    emergency.Ambulance = await FireClient.Child($"CareConnect/Emergency/{CollectionName}/AmbulaceId").OnceSingleAsync<string>();
                    emergency.FingerPrint = await FireClient.Child($"CareConnect/Emergency/{CollectionName}/FingerPrint").OnceSingleAsync<string>();
                    emergency.Location = await FireClient.Child($"CareConnect/Emergency/{CollectionName}/location").OnceSingleAsync<string>();

                    if (emergency.Ambulance != null)
                    {
                        MessageBox.Show(emergency.ToString());
                        EmergencyFunctions(emergency);
                        //  DeleteRecord(CollectionName);
                    }

                }
            });*/
        }

        private void EmergencyFunctions(Emergency emergency)
        {/*
            GetingStart();
            distanceService.LoadDataFromDatabase();
            UpdateStatusTextBox();
            distanceService.CalculateDistance(this.emergency.Location);
            UpdateStatusTextBox();
            DistanceValuesAPI = distanceService.CalculateDistanceAPI(this.emergency.Location);
            UpdateStatusTextBox();
            CheckFreeBed();
*/





        }

        private void CheckFreeBed()
        {

            foreach (var hospital in DistanceValuesAPI)
            {
                MessageBox.Show(hospital.Key + "  " + hospital.Value);
                if (Check_Rooms_availability(hospital.Key))
                {

                    ConfigurationManager.AppSettings["StatusText"] += "A free bed was found in : " + GetHospitalName(hospital.Key) + "\n";
                    smsMessage += "the patient has been transferred to : " + GetHospitalName(hospital.Key) + "\n";
                    smsMessage += "Hospital location : " + GetGoogleMapsUrl(GetHospitalAddress(hospital.Key)) + "\n";

                    MessageBox.Show("Message : \n" + smsMessage);

                    UpdateStatusTextBox();
                    break;
                }
                else
                {
                    ConfigurationManager.AppSettings["StatusText"] += "No free bed was found in : " + GetHospitalName(hospital.Key) + "\n";
                }
            }

        }

        public string GoogleTranslate(string text)
        {
            string from = "en";
            string to = "ar";
            string url = $"https://translate.googleapis.com/translate_a/single?client=gtx&sl={from}&tl={to}&dt=t&q={text}";
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string result = reader.ReadToEnd();
            reader.Close();
            response.Close();
            return result;
        }

        public static string GetGoogleMapsUrl(string location)
        {
            string latitude = location.Split(',')[0];
            string longitude = location.Split(',')[1];
            return $"https://www.google.com/maps?q={latitude},{longitude}";
        }

        private string GetHospitalName(string hospitalKey)
        {
            FirebaseResponse DataResponse = client.Get("CareConnect/HospitalData/" + Convert.ToString(hospitalKey));
            JObject HospitalData = JObject.Parse(DataResponse.Body);
            return HospitalData["Name"].ToString();
        }

        private string GetHospitalAddress(string hospitalKey)
        {
            FirebaseResponse DataResponse = client.Get("CareConnect/HospitalData/" + Convert.ToString(hospitalKey));
            JObject HospitalData = JObject.Parse(DataResponse.Body);
            return HospitalData["Address"].ToString();
        }


        private void GetingStart()
        {
            ConfigurationManager.AppSettings["StatusText"] += "New Request for Ambulance : " + this.emergency.FingerPrint + "\n";
            UpdateStatusTextBox();
            ConfigurationManager.AppSettings["StatusText"] += "Patient ID : " + this.emergency.Ambulance + "\n";
            ConfigurationManager.AppSettings["StatusText"] += "Patient Name : " + patientInfo.UserNameInfo(this.emergency.Ambulance) + "\n";
            ConfigurationManager.AppSettings["StatusText"] += "Blood Type : " + patientInfo.BloodInfo(this.emergency.Ambulance) + "\n";
            ConfigurationManager.AppSettings["StatusText"] += "***************************************\n\n";
            UpdateStatusTextBox();
        }

        private async Task DeleteRecord(string collectionName)
        {
            try
            {
                await FireClient.Child($"CareConnect/Emergency/{collectionName}").DeleteAsync();
                MessageBox.Show($"Record {collectionName} deleted successfully.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting record {collectionName}: {ex.Message}");
            }
        }

        private void UpdateStatusTextBox()
        {
            string text = ConfigurationManager.AppSettings["StatusText"];
            Thread.Sleep(1000);
            if (StatusTextBox.InvokeRequired)
            {
                StatusTextBox.Invoke(new Action(() => StatusTextBox.Text = text));
            }
            else
            {
                StatusTextBox.Text = text;

            }

        }
        private void ClearBtn_Click(object sender, EventArgs e)
        {
            ConfigurationManager.AppSettings["StatusText"] = "";
            UpdateStatusTextBox();
        }
        private void AddNewHospital_Click(object sender, EventArgs e)
        {
            AddHospital addHospitalForm = new AddHospital();
            addHospitalForm.Show();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            UpdateStatusTextBox();
        }



        private bool Check_Rooms_availability(string Hospital_Key)
        {
            // ------------- connect to firebase and get the data ----------------
            FirebaseResponse DataResponse = client.Get("CareConnect/HospitalData/" + Convert.ToString(Hospital_Key));
            JObject HospitalData = JObject.Parse(DataResponse.Body);

            // ----------- get info about number of max and current available rooms in this hospital -----------
            int Room_MaxSize_MED = Convert.ToInt32(HospitalData["MaxSize_MED"]);
            int Room_MaxSize_IR = Convert.ToInt32(HospitalData["MaxSize_IR"]);
            int Room_MaxSize_ICU = Convert.ToInt32(HospitalData["MaxSize_ICU"]);
            int Room_MaxSize_EOR = Convert.ToInt32(HospitalData["MaxSize_EOR"]);
            int Room_CurSize_MED = Convert.ToInt32(HospitalData["CurSize_MED"]);
            int Room_CurSize_IR = Convert.ToInt32(HospitalData["CurSize_IR"]);
            int Room_CurSize_ICU = Convert.ToInt32(HospitalData["CurSize_ICU"]);
            int Room_CurSize_EOR = Convert.ToInt32(HospitalData["CurSize_EOR"]);

            if (Room_MaxSize_MED - Room_CurSize_MED > 0)
            {
                Update_Rooms_Values(Hospital_Key, "CurSize_MED", "MED", Room_CurSize_MED - 1);
                return true;
            }
            else if (Room_MaxSize_ICU - Room_CurSize_ICU > 0)
            {
                Update_Rooms_Values(Hospital_Key, "CurSize_ICU", "ICU", Room_CurSize_ICU - 1);
                return true;
            }
            else if (Room_MaxSize_EOR - Room_CurSize_EOR > 0)
            {
                Update_Rooms_Values(Hospital_Key, "CurSize_EOR", "EOR", Room_CurSize_EOR - 1);
                return true;
            }
            else if (Room_MaxSize_IR - Room_CurSize_IR > 0)
            {
                Update_Rooms_Values(Hospital_Key, "CurSize_IR", "IR", Room_CurSize_IR - 1);
                return true;
            }
            else
                return false;
        }
        private async void Update_Rooms_Values(string Hospital_Key, string Room_Name, string Room_Name_In_Date_Category, int New_Value)
        {
            try
            {
                var UpdateData = new Dictionary<string, object>
                {
                    { Room_Name , New_Value }
                };
                await client.UpdateAsync($"CareConnect/HospitalData/{Hospital_Key}/", UpdateData);


                var UpdateDate = new Dictionary<string, object>
                {
                    { Room_Name_In_Date_Category+"_LastEdit" ,  DateTime.Now}
                };
                await client.UpdateAsync($"CareConnect/HospitalData/{Hospital_Key}/", UpdateDate);
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to Update Data, Please check your internet connection", "Connection Failure", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ----------- The following 10 functions are resposible for checking blood availablility and decreasing its amount if found ----------
        private bool Check_blood_availability(string Hospital_Key, ref Dictionary<string, int> dic, string Blood_Type)
        {
            bool Isfound = false;
            if (Blood_Type == "ABPlus")
                Check_ABPlus(Hospital_Key, ref dic, ref Isfound);
            else if (Blood_Type == "ABMinus")
                Check_ABMinus(Hospital_Key, ref dic, ref Isfound);
            else if (Blood_Type == "APlus")
                Check_APlus(Hospital_Key, ref dic, ref Isfound);
            else if (Blood_Type == "AMinus")
                Check_AMinus(Hospital_Key, ref dic, ref Isfound);
            else if (Blood_Type == "BPlus")
                Check_BPlus(Hospital_Key, ref dic, ref Isfound);
            else if (Blood_Type == "BMinus")
                Check_BMinus(Hospital_Key, ref dic, ref Isfound);
            else if (Blood_Type == "OPlus")
                Check_OPlus(Hospital_Key, ref dic, ref Isfound);
            else
                Check_OMinus(Hospital_Key, ref dic, ref Isfound);
            return Isfound;
        }
        private void Check_ABPlus(string Hospital_Key, ref Dictionary<string, int> dic, ref bool Isfound)
        {
            if (!Isfound && dic["ABPlus"] >= 2)
            {
                Update_Blood_Values(Hospital_Key, "ABPlus", dic["ABPlus"] - 2);
                Isfound = true;
            }
            if (!Isfound && dic["OPlus"] >= 2)
            {
                Update_Blood_Values(Hospital_Key, "OPlus", dic["OPlus"] - 2);
                Isfound = true;
            }
            if (!Isfound && dic["APlus"] >= 2)
            {
                Update_Blood_Values(Hospital_Key, "APlus", dic["APlus"] - 2);
                Isfound = true;
            }
            if (!Isfound && dic["BPlus"] >= 2)
            {
                Update_Blood_Values(Hospital_Key, "BPlus", dic["BPlus"] - 2);
                Isfound = true;
            }
            if (!Isfound && dic["OMinus"] >= 2)
            {
                Update_Blood_Values(Hospital_Key, "OMinus", dic["OMinus"] - 2);
                Isfound = true;
            }
            if (!Isfound && dic["AMinus"] >= 2)
            {
                Update_Blood_Values(Hospital_Key, "AMinus", dic["AMinus"] - 2);
                Isfound = true;
            }
            if (!Isfound && dic["BMinus"] >= 2)
            {
                Update_Blood_Values(Hospital_Key, "BMinus", dic["BMinus"] - 2);
                Isfound = true;
            }
            if (!Isfound && dic["ABMinus"] >= 2)
            {
                Update_Blood_Values(Hospital_Key, "ABMinus", dic["ABMinus"] - 2);
                Isfound = true;
            }

        }
        private void Check_ABMinus(string Hospital_Key, ref Dictionary<string, int> dic, ref bool Isfound)
        {
            if (!Isfound && dic["ABMinus"] >= 2)
            {
                Update_Blood_Values(Hospital_Key, "ABMinus", dic["ABMinus"] - 2);
                Isfound = true;
            }
            if (!Isfound && dic["OMinus"] >= 2)
            {
                Update_Blood_Values(Hospital_Key, "OMinus", dic["OMinus"] - 2);
                Isfound = true;
            }
            if (!Isfound && dic["AMinus"] >= 2)
            {
                Update_Blood_Values(Hospital_Key, "AMinus", dic["AMinus"] - 2);
                Isfound = true;
            }
            if (!Isfound && dic["BMinus"] >= 2)
            {
                Update_Blood_Values(Hospital_Key, "BMinus", dic["BMinus"] - 2);
                Isfound = true;
            }
        }
        private void Check_APlus(string Hospital_Key, ref Dictionary<string, int> dic, ref bool Isfound)
        {
            if (!Isfound && dic["APlus"] >= 2)
            {
                Update_Blood_Values(Hospital_Key, "APlus", dic["APlus"] - 2);
                Isfound = true;
            }
            if (!Isfound && dic["OPlus"] >= 2)
            {
                Update_Blood_Values(Hospital_Key, "OPlus", dic["OPlus"] - 2);
                Isfound = true;
            }
            if (!Isfound && dic["OMinus"] >= 2)
            {
                Update_Blood_Values(Hospital_Key, "OMinus", dic["OMinus"] - 2);
                Isfound = true;
            }
            if (!Isfound && dic["AMinus"] >= 2)
            {
                Update_Blood_Values(Hospital_Key, "AMinus", dic["AMinus"] - 2);
                Isfound = true;
            }
        }
        private void Check_AMinus(string Hospital_Key, ref Dictionary<string, int> dic, ref bool Isfound)
        {
            if (!Isfound && dic["AMinus"] >= 2)
            {
                Update_Blood_Values(Hospital_Key, "AMinus", dic["AMinus"] - 2);
                Isfound = true;
            }
            if (!Isfound && dic["OMinus"] >= 2)
            {
                Update_Blood_Values(Hospital_Key, "OMinus", dic["OMinus"] - 2);
                Isfound = true;
            }
        }
        private void Check_BPlus(string Hospital_Key, ref Dictionary<string, int> dic, ref bool Isfound)
        {
            if (!Isfound && dic["BPlus"] >= 2)
            {
                Update_Blood_Values(Hospital_Key, "BPlus", dic["BPlus"] - 2);
                Isfound = true;
            }
            if (!Isfound && dic["OPlus"] >= 2)
            {
                Update_Blood_Values(Hospital_Key, "OPlus", dic["OPlus"] - 2);
                Isfound = true;
            }
            if (!Isfound && dic["OMinus"] >= 2)
            {
                Update_Blood_Values(Hospital_Key, "OMinus", dic["OMinus"] - 2);
                Isfound = true;
            }
            if (!Isfound && dic["BMinus"] >= 2)
            {
                Update_Blood_Values(Hospital_Key, "BMinus", dic["BMinus"] - 2);
                Isfound = true;
            }
        }
        private void Check_BMinus(string Hospital_Key, ref Dictionary<string, int> dic, ref bool Isfound)
        {
            if (!Isfound && dic["BMinus"] >= 2)
            {
                Update_Blood_Values(Hospital_Key, "BMinus", dic["BMinus"] - 2);
                Isfound = true;
            }
            if (!Isfound && dic["OMinus"] >= 2)
            {
                Update_Blood_Values(Hospital_Key, "OMinus", dic["OMinus"] - 2);
                Isfound = true;
            }
        }
        private void Check_OPlus(string Hospital_Key, ref Dictionary<string, int> dic, ref bool Isfound)
        {
            if (!Isfound && dic["OPlus"] >= 2)
            {
                Update_Blood_Values(Hospital_Key, "OPlus", dic["OPlus"] - 2);
                Isfound = true;
            }
            if (!Isfound && dic["OMinus"] >= 2)
            {
                Update_Blood_Values(Hospital_Key, "OMinus", dic["OMinus"] - 2);
                Isfound = true;
            }
        }
        private void Check_OMinus(string Hospital_Key, ref Dictionary<string, int> dic, ref bool Isfound)
        {
            if (!Isfound && dic["OMinus"] >= 2)
            {
                Update_Blood_Values(Hospital_Key, "OMinus", dic["OMinus"] - 2);
                Isfound = true;
            }
        }
        private async void Update_Blood_Values(string Hospital_Key, string Blood_type, int New_Value)
        {
            try
            {
                var UpdateData = new Dictionary<string, object>
                {
                    { Blood_type , New_Value }
                };
                await client.UpdateAsync($"CareConnect/HospitalData/{Hospital_Key}/", UpdateData);


                var UpdateDate = new Dictionary<string, object>
                {
                    { Blood_type+"_LastEdit" ,  DateTime.Now}
                };
                await client.UpdateAsync($"CareConnect/HospitalData/{Hospital_Key}/", UpdateDate);
            }
            catch (Exception)
            {
                MessageBox.Show("Failed to Update Data, Please check your internet connection", "Connection Failure", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void label3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
