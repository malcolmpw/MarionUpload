namespace MarionUpload.Models
{
    public class mMarionImport
    {
        //[Dapper.Key]

        public int ImportID { get; set; }
        public string Job { get; set; }                      //2 
        public int CurrentTaxYear { get; set; }              //9 
        public string RenderedCode { get; set; }               //14
        public string PropertyType { get; set; }               //16
        public int InterestType { get; set; }               //17
        public int YearLeaseStarted { get; set; }            //18
        public string Protest { get; set; }                    //22
        public string SPTBCode { get; set; }                 //23 3char        
        public string AgentNumber { get; set; }              //26           
        public string SortCode { get; set; }                   //29
        public string DecimalInterest { get; set; }          //34 7
        public string LeaseName { get; set; }                //41 (DESC 3) IF TYPE = 2,3 30 A
        public string OperatorName { get; set; }             //71 90 OPER NAME(DESC 4) 20 A
        public string Description1 { get; set; }             //91 120 DESCRIPTION 1 30 A
        public string Description2 { get; set; }             //121 150 DESCRIPTION 2 30 A
        public string OwnerName { get; set; }                //151 180 OWNER NAME 30 A
        public string InCareOf { get; set; }                 //181 210 IN CARE OF 30 A
        public string StreetAddress { get; set; }            //211 240 STREET ADDRESS 30 A

        public string CityStateZip { get; set; }             //241 270 CITY,ST,ZIP CDE OR OUT COUNTRY 30 A
        public string RRC { get; set; }                      //271 290 RRC# 20 A

        //public List<string> Jurisdictions { get; set; }

        public string Jurisdiction1 { get; set; }            //297 298 JURISDICTION 1 2 0 N
        public string Jurisdiction2 { get; set; }            //299 298 JURISDICTION 2 2 0 N
        public string Jurisdiction3 { get; set; }            //301 298 JURISDICTION 3 2 0 N
        public string Jurisdiction4 { get; set; }            //303 298 JURISDICTION 4 2 0 N
        public string Jurisdiction5 { get; set; }            //305 298 JURISDICTION 5 2 0 N
        public string Jurisdiction6 { get; set; }            //307 298 JURISDICTION 6 2 0 N
        public string Jurisdiction7 { get; set; }            //309 298 JURISDICTION 7 2 0 N
        public string Jurisdiction8 { get; set; }            //311 298 JURISDICTION 8 2 0 N
        public string Jurisdiction9 { get; set; }            //313 298 JURISDICTION 9 2 0 N
        public string Jurisdiction10 { get; set; }           //315 298 JURISDICTION 10 2 0 N
        public string Jurisdiction11 { get; set; }           //317 318 JURISDICTION 11 2 0 N
        public string Jurisdiction12 { get; set; }

        public string Juris1TaxableValue{ get; set; }
        public string Juris2TaxableValue{ get; set; }
        public string Juris3TaxableValue{ get; set; }
        public string Juris4TaxableValue{ get; set; }
        public string Juris5TaxableValue{ get; set; }
        public string Juris6TaxableValue{ get; set; }
        public string Juris7TaxableValue{ get; set; }
        public string Juris8TaxableValue{ get; set; }
        public string Juris9TaxableValue{ get; set; }
        public string Juris10TaxableValue { get; set; }
        public string Juris11TaxableValue { get; set; }
        public string Juris12TaxableValue { get; set; }

        public string Juris1MarketValue{ get; set; }
        public string Juris2MarketValue{ get; set; }
        public string Juris3MarketValue{ get; set; }
        public string Juris4MarketValue{ get; set; }
        public string Juris5MarketValue{ get; set; }
        public string Juris6MarketValue{ get; set; }
        public string Juris7MarketValue{ get; set; }
        public string Juris8MarketValue{ get; set; }
        public string Juris9MarketValue{ get; set; }
        public string Juris10MarketValue { get; set; }
        public string Juris11MarketValue { get; set; }
        public string Juris12MarketValue { get; set; }

        public float acres { get; set; }
        public int OwnerNumber { get; set; }
        public int LeaseNumber { get; set; }
        public string AbsoluteExemptionCode { get; set; }

        public string Juris1ExemptionMinOwnerFlag{ get; set; }
        public string Juris2ExemptionMinOwnerFlag{ get; set; }
        public string Juris3ExemptionMinOwnerFlag{ get; set; }
        public string Juris4ExemptionMinOwnerFlag{ get; set; }
        public string Juris5ExemptionMinOwnerFlag{ get; set; }
        public string Juris6ExemptionMinOwnerFlag{ get; set; }
        public string Juris7ExemptionMinOwnerFlag{ get; set; }
        public string Juris8ExemptionMinOwnerFlag{ get; set; }
        public string Juris9ExemptionMinOwnerFlag{ get; set; }
        public string Juris10ExemptionMinOwnerFlag { get; set; }
        public string Juris11ExemptionMinOwnerFlag { get; set; }
        public string Juris12ExemptionMinOwnerFlag { get; set; }

        public string CustomerGeo { get; set; }
        public string TnrccValuePollutionControl { get; set; }

        public string Juris1PnAMinimumOwnerFlag { get; set; }
        public string Juris2PnAMinimumOwnerFlag { get; set; }
        public string Juris3PnAMinimumOwnerFlag { get; set; }
        public string Juris4PnAMinimumOwnerFlag { get; set; }
        public string Juris5PnAMinimumOwnerFlag { get; set; }
        public string Juris6PnAMinimumOwnerFlag { get; set; }
        public string Juris7PnAMinimumOwnerFlag { get; set; }
        public string Juris8PnAMinimumOwnerFlag { get; set; }
        public string Juris9PnAMinimumOwnerFlag { get; set; }
        public string Juris10PnAMinimumOwnerFlag { get; set; }
        public string Juris11PnAMinimumOwnerFlag { get; set; }
        public string Juris12PnAMinimumOwnerFlag { get; set; }

        public int MineralAccountNumber { get; set; }
        public int MineralAccountSequence { get; set; }
        public int PreviousMineralAccountSequence { get; set; }
        public int PreviousMineralAccount { get; set; }
        public string PrivacyCode { get; set; }
        public string ComplianceCode { get; set; }

        public string Juris1NewTaxableValue { get; set; }
        public string Juris2NewTaxableValue { get; set; }
        public string Juris3NewTaxableValue { get; set; }
        public string Juris4NewTaxableValue { get; set; }
        public string Juris5NewTaxableValue { get; set; }
        public string Juris6NewTaxableValue { get; set; }
        public string Juris7NewTaxableValue { get; set; }
        public string Juris8NewTaxableValue { get; set; }
        public string Juris9NewTaxableValue { get; set; }
        public string Juris10NewTaxableValue { get; set; }
        public string Juris11NewTaxableValue { get; set; }
        public string Juris12NewTaxableValue { get; set; }       

        public decimal Juris1NewAbtAbtValue { get; set; }
        public decimal Juris2NewAbtAbtValue { get; set; }
        public decimal Juris3NewAbtAbtValue { get; set; }
        public decimal Juris4NewAbtAbtValue { get; set; }
        public decimal Juris5NewAbtAbtValue { get; set; }
        public decimal Juris6NewAbtAbtValue { get; set; }
        public decimal Juris7NewAbtAbtValue { get; set; }
        public decimal Juris8NewAbtAbtValue { get; set; }
        public decimal Juris9NewAbtAbtValue { get; set; }
        public decimal Juris10NewAbtAbtValue { get; set; }
        public decimal Juris11NewAbtAbtValue { get; set; }
        public decimal Juris12NewAbtAbtValue { get; set; }

        public string NewTCEQflag { get; set; }

        public decimal Juris1NewExemptValue { get; set; }
        public decimal Juris2NewExemptValue { get; set; }
        public decimal Juris3NewExemptValue { get; set; }
        public decimal Juris4NewExemptValue { get; set; }
        public decimal Juris5NewExemptValue { get; set; }
        public decimal Juris6NewExemptValue { get; set; }
        public decimal Juris7NewExemptValue { get; set; }
        public decimal Juris8NewExemptValue { get; set; }
        public decimal Juris9NewExemptValue { get; set; }
        public decimal Juris10NewExemptValue { get; set; }
        public decimal Juris11NewExemptValue { get; set; }
        public decimal Juris12NewExemptValue { get; set; }

        public decimal NewImprovementPercent { get; set; }

    }
}
