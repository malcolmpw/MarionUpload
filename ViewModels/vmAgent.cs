using Dapper.Contrib.Extensions;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Messaging;
using MarionUpload.Helpers;
using MarionUpload.Messages;
using MarionUpload.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;


namespace MarionUpload.ViewModels
{
    public class vmAgent : ViewModelBase
    {
        static readonly string Path = @"C:\Users\malcolm.wardlaw\Desktop\Marion Download\MARION CAD FINAL MINERAL DATA\MA215500.TXT";
        //private const string AgentErrorPath = @"c:\temp\marion_agents_not_found.txt";

        public bool AgentImportEnabled { get => agentImportEnabled; set { agentImportEnabled = value; RaisePropertyChanged(nameof(AgentImportEnabled)); } }
        public bool AgentUploadEnabled { get => agentUploadEnabled; set { agentUploadEnabled = value; RaisePropertyChanged(nameof(AgentUploadEnabled)); } }
        private bool agentImportEnabled = true;
        private bool agentUploadEnabled = false;

        public ObservableCollection<mMarionAgent> MarionAgents { get; set; }
        
        private mAgent Agent { get; set; }
        public static IDictionary<int, long> MarionAgentNumberToNameIdMap { get; private set; } = new Dictionary<int, long>();

        public ICommand CommandImportAgents => new RelayCommand(OnImportAgents);
        public ICommand CommandUploadAgentIDs => new RelayCommand(OnUploadAgents);

        private DateTime _updateDate;
        private string _updateBy;

        public vmAgent()
        {
            MarionAgents = new ObservableCollection<mMarionAgent>();
            agentImportEnabled = true;
            agentUploadEnabled = false;
        }

        public void OnImportAgents()       // note: following WagApp1, a segment is the same as an OwnerPersonalPropertySegment
        {
            ReadMarionAgentsFlatFileIntoMarionAgents();
            //MarionAgents.OrderBy(a => a.AgentNumber);
            //var backupAgents = MarionAgents;
            //MarionAgents = null;
            //MarionAgents = backupAgents;
            agentImportEnabled = false;
            agentUploadEnabled = true;
        }

        private void OnUploadAgents()
        {
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);

            try
            {
                using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
                {
                    foreach (mMarionAgent marionAgent in MarionAgents)
                    {
                        var populatedOwner = TranslateFrom_mMarionAgentTo_mOwner(marionAgent);
                        var primaryOwnerKey = db.Insert<mOwner>(populatedOwner);
                       if(!MarionAgentNumberToNameIdMap.ContainsKey(marionAgent.AgentNumber))
                        MarionAgentNumberToNameIdMap.Add(marionAgent.AgentNumber, (int)primaryOwnerKey);

                        var populatedCadOwner = TranslateFrom_mMarionOwnerTo_mCadOwner(marionAgent, primaryOwnerKey);
                        var primaryCadOwnerKey = db.Insert<mCadOwner>(populatedCadOwner);

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error Uploading Owner Data -> {ex}");
                Messenger.Default.Send<AgentFinishedMessage>(new AgentFinishedMessage());
            }

            AgentUploadEnabled = false;
            MessageBox.Show($"Finished uploading {MarionAgents.Count()} owners(agents)");
            Messenger.Default.Send<AgentFinishedMessage>(new AgentFinishedMessage());

            agentImportEnabled = false;
            agentUploadEnabled = false;

            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
            Messenger.Default.Send<AgentFinishedMessage>(new AgentFinishedMessage());
        }

        private mCadOwner TranslateFrom_mMarionOwnerTo_mCadOwner(mMarionAgent marionAgent, long primaryOwnerKey)
        {
            var cadOwner = new mCadOwner();
            cadOwner.CadID = "MAR";
            cadOwner.CadOwnerID = marionAgent.AgentNumber.ToString();
            cadOwner.delflag = false;
            cadOwner.NameID = (int)primaryOwnerKey;
            return cadOwner;
        }

        private mOwner TranslateFrom_mMarionAgentTo_mOwner(mMarionAgent marionAgent)
        {
            NameSorts nsorts = new NameSorts();
            _updateDate = DateTime.Now;
            _updateBy = "MPW";

            var owner = new mOwner();
            owner.Agnt_YN = true;
            owner.CadID = "MAR";
            owner.NameSortCad = marionAgent.AgentName.Trim();
            owner.Stat_YN = true;
            owner.NameSortFirst = marionAgent.AgentName.Trim();
            owner.NameC = marionAgent.AgentName.Trim();
            owner.Name2 = marionAgent.AgentNumber.ToString();
            owner.NameSel_YN = true;
            owner.Mail1 = marionAgent.AgentStreet.Trim();
            owner.MailCi = marionAgent.AgentCity.Trim();
            owner.MailSt = marionAgent.AgentState.Trim();
            owner.MailZ = marionAgent.AgentZip.ToString();
            owner.MailZ4 = marionAgent.AgentZip4.ToString().PadLeft(4);

            owner.UpdateDate = _updateDate;
            owner.UpdateBy = _updateBy;

            return owner;
        }

        public List<mMarionAgent> marionAgents;

        public void ReadMarionAgentsFlatFileIntoMarionAgents()
        {
            using (StreamReader sr = new StreamReader(Path))
            {
                MarionAgents.Clear();
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    line = line.PadRight(131);
                    mMarionAgent agent = new mMarionAgent();
                    agent = ParseLineFromMarionAgentsFlatFile(line);
                    MarionAgents.Add(agent);
                }
            }           
        }

        static mMarionAgent ParseLineFromMarionAgentsFlatFile(string line)
        {
            var data = new mMarionAgent();
            Int32.TryParse(GetString(line, 1, 3), out int intAgentNumber);
            data.AgentNumber = intAgentNumber;
            data.AgentName = GetString(line, 4, 33);
            data.AgentInCareOf = GetString(line, 34, 63);
            data.AgentStreet = GetString(line, 64, 93);
            data.AgentCity = GetString(line, 94, 109);
            data.AgentState = GetString(line, 110, 111);
            var alpha = GetString(line, 1, 3);
            Int32.TryParse(GetString(line, 112, 116), out int intZip);
            data.AgentZip = intZip;
            Int32.TryParse(GetString(line, 118, 121), out int intZip4);
            data.AgentZip4 = intZip4;
            return data;
        }

        private static string GetString(string line, int pos1, int pos2)
        {
            string val = line.Substring(pos1 - 1, pos2 - pos1 + 1);
            return val;
        }

        private static char GetChar(string line, int pos1)
        {
            return GetString(line, pos1, pos1)[0];
        }

        private static int GetInt(string line, int pos1, int pos2)
        {
            return int.Parse(GetString(line, pos1, pos2));
        }

        private static List<string> GetList(string line, int startPosition, int length, int numberOfValues)
        {
            List<string> results = new List<string>();
            for (int i = 0; i < numberOfValues; i++)
            {
                var nextValue = GetString(line, startPosition + i * length, startPosition + i * length + length - 1);
                results.Add(nextValue);
            }

            return results;
        }

        private static List<char> GetListOfCharacters(string line, int startPosition, int numberOfValues)
        {
            List<char> results = new List<char>();
            for (int i = 0; i < numberOfValues; i++)
            {
                var nextValue = GetChar(line, startPosition + i);
                results.Add(nextValue);
            }
            return results;
        }
    }
}
