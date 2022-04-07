using Dapper;
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
    public class vmAgentAndOperator : ViewModelBase
    {
        static readonly string Path = @"C:\Users\malcolm.wardlaw\Desktop\Marion Download\MARION CAD FINAL MINERAL DATA\MA215500.TXT";
        //private const string AgentErrorPath = @"c:\temp\marion_agents_not_found.txt";

        public bool AgentImportEnabled { get => agentImportEnabled; set { agentImportEnabled = value; RaisePropertyChanged(nameof(AgentImportEnabled)); } }
        public bool AgentUploadEnabled { get => agentUploadEnabled; set { agentUploadEnabled = value; RaisePropertyChanged(nameof(AgentUploadEnabled)); } }
        private bool agentImportEnabled = true;
        private bool agentUploadEnabled = false;

        private mAgent Agent { get; set; }
        public ObservableCollection<mMarionAgent> MarionAgents { get; set; }        
        public static IDictionary<int, long> MarionAgentNumberToNameIdMap { get; private set; } = new Dictionary<int, long>();

        public static ObservableCollection<mCrwOperator> CrwOperators { get; set; }
        public static IDictionary<string, long> CrwOperRrcIDToNameIdMap { get; private set; } = new Dictionary<string, long>();
        public static IDictionary<string, string> CrwRrcToOperIdMap { get; private set; } = new Dictionary<string, string>();              

        public ICommand CommandImportAgents => new RelayCommand(OnImportAgentsAndOperators);
        public ICommand CommandUploadAgentIDs => new RelayCommand(OnUploadAgentsAndOperators);

        private DateTime _updateDate;
        private string _updateBy;

        public vmAgentAndOperator()
        {
            MarionAgents = new ObservableCollection<mMarionAgent>();
            agentImportEnabled = true;
            agentUploadEnabled = false;
        }

        public void OnImportAgentsAndOperators()       // note: following WagApp1, a segment is the same as an OwnerPersonalPropertySegment
        {
            ReadMarionAgentsFlatFileIntoMarionAgents();
           
            GetOperatorsFromAbCrwOperators();

            agentImportEnabled = false;
            agentUploadEnabled = true;
        }

        private void OnUploadAgentsAndOperators()
        {
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
            UploadAgentsToOwners();
            UploadOperatorsToOwners();

            AgentUploadEnabled = false;
            MessageBox.Show($"Finished uploading {MarionAgents.Count()} owners(agents)");
            Messenger.Default.Send<AgentFinishedMessage>(new AgentFinishedMessage());

            agentImportEnabled = false;
            agentUploadEnabled = false;

            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
            Messenger.Default.Send<AgentFinishedMessage>(new AgentFinishedMessage());
        }

        private void UploadOperatorsToOwners()
        {
            try
            {
                using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
                {
                    foreach (mCrwOperator crwOperator in CrwOperators)
                    {
                        var populatedOwner = TranslateFrom_mCrwOperatorTo_mOwner(crwOperator);
                        var primaryOwnerKey = db.Insert<mOwner>(populatedOwner);

                        if (!CrwOperRrcIDToNameIdMap.ContainsKey(populatedOwner.OperRrcID))
                            CrwOperRrcIDToNameIdMap.Add(populatedOwner.OperRrcID, (int)primaryOwnerKey);

                        //if (!CrwRrcToOperIdMap.ContainsKey(populatedOwner.OperRrcID))
                        //    CrwRrcToOperIdMap.Add(crwOperator.OperRrcID, populatedOwner.OperRrcID);

                        var populatedCadOwner = TranslateFrom_mCrwOperatorTo_mCadOwner(crwOperator, primaryOwnerKey);
                        var primaryCadOwnerKey = db.Insert<mCadOwner>(populatedCadOwner);

                    }
                
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error Uploading Owner Data -> {ex}");
                Messenger.Default.Send<AgentFinishedMessage>(new AgentFinishedMessage());
            }
        }

        private void UploadAgentsToOwners()
        {
            try
            {
                using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
                {
                    foreach (mMarionAgent marionAgent in MarionAgents)
                    {
                        var populatedOwner = TranslateFrom_mMarionAgentTo_mOwner(marionAgent);
                        var primaryOwnerKey = db.Insert<mOwner>(populatedOwner);
                        if (!MarionAgentNumberToNameIdMap.ContainsKey(marionAgent.AgentNumber))
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
        }

        private mOwner TranslateFrom_mCrwOperatorTo_mOwner(mCrwOperator crwOperator)
        {
            _updateDate = DateTime.Now;
            _updateBy = "MPW";

            var owner = new mOwner();
            owner.Agnt_YN = true;
            owner.CadID = "MAR";
            owner.NameSortCad = crwOperator.NameSort.Trim();
            
            owner.Stat_YN = true;
            owner.Oper_YN = true;
            owner.OperRrcID = crwOperator.OperRrcID.Trim();
            owner.NameSortFirst = crwOperator.NameSort.Trim();
            owner.NameC = crwOperator.NameSort.Trim();            
            owner.NameSel_YN = true;

            owner.UpdateDate = _updateDate;
            owner.UpdateBy = _updateBy;

            return owner;
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

        private mCadOwner TranslateFrom_mCrwOperatorTo_mCadOwner(mCrwOperator crwOperator, long primaryOwnerKey)
        {
            var cadOwner = new mCadOwner();
            cadOwner.CadID = "MAR";
            var nsLength = crwOperator.NameSort.Length;
            var nsUseLength = nsLength <= 15 ? nsLength : 15;
            cadOwner.CadOwnerID = crwOperator.NameSort.Substring(0, nsUseLength).ToString();
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

        private void GetOperatorsFromAbCrwOperators()
        {
            using (IDbConnection db = new SqlConnection(ConnectionStringHelper.ConnectionString))
            {
                CrwOperators = new ObservableCollection<mCrwOperator>();
                //string sqlString = "select CadID,NameID,OprRrcID,NameSort,CountOfLpdID,Oper_YN from wagapp2_2021_Marion.dbo.AbMarionOperatorsFromCRW ";
                string sqlString = "select * from wagapp2_2021_Marion.dbo.AbMarionOperatorsFromCRW ";
                var crwOperators = db.Query<mCrwOperator>(sqlString).ToList();
                CrwOperators = new ObservableCollection<mCrwOperator>(crwOperators);
            }
        }

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
