namespace MarionUpload.Models
{
    public class mMarionExport
    {
        //[Dapper.Key]

        // miscellaneous
        public int? ImportID { get; set; }                                                       //ignore
        public int? Job { get; set; }                        //2,8    N,7,0   JOB NUMBER        //ignore        
        public char? RenderedCode { get; set; }              //14,14  A 1     RENDERED CODE     //ignore
        public string SortCode { get; set; }                 //29,29  A 1     SORT CODE         //ignore

        // tlkpCurrentTaxYear                                                                   
        public int? CurrentTaxYear { get; set; }             //9,12   S,4,0   YEAR              //tlkpCurrentTaxYear.Tax Year

        // tblAccount
        public decimal? DecimalInterest { get; set; }        //34,40  N,76    DECIMAL INTEREST  //tblAccount.PctProp
        public int? InterestType { get; set; }               //17,17  N,10    INTEREST TYPE     //tblAccount.PctType
        public string PropertyType { get; set; }             //16,16   A,1    TYPE PROPERTY     //tblAccount.PropType
        public string Protest { get; set; }                  //22,22  A,1     PROTEST=P         //tblAccount.Protest_YN
        public int? AccountNumber { get; set; }              //668,674 S 7    MINERAL ACCOUNT NUMBER      //tblAccount.SeqNmbr
        public int? AccountSequence { get; set; }            //675,681 S 7    MINERAL ACCOUNT SEQUENCE #  //tblAccount.SeqNmbr
        public string GeoRef { get; set; }                   //622,646 A,25   CUSTOMER GEO#     //tblCadOwners.CadAcctID        

        // tblProperty
        public string SPTBCode { get; set; }                 //23,25   A,3    S.P.T.B. CODE     //tblProperty.PtdClassSub       
        public string Description1 { get; set; }             //91,120  A,30   DESCRIPTION 1     //tblProperty.Legal
        public string Description2 { get; set; }             //121,150 A,30   DESCRIPTION 2     //tblProperty.Legal

        // tblName
        public string OwnerName { get; set; }                //151,180 A,30  OWNER NAME         //tblName.NameSortCad
        public string InCareOf { get; set; }                 //181,210 A,30  IN CARE OF         //ignore
        public string StreetAddress { get; set; }            //211,240 A,30  STREET ADDRESS     //tblName.Mail1
        public string CityStateZip { get; set; }             //241,270 A,30  CITY,ST,ZIP CDE OR OUT COUNTRY
                                                                                                //tblName.MailCi,MailSt,MailZip
        public int? OwnerNumber { get; set; }                //594,600 N,7,0 OWNER NUMBER       //tblCadOwners.CadOwnerID
        public int? AgentNumber { get; set; }                //26,28   N,3,0 AGENT NUMBER       //tblCadOwners.CadOwnerID           

        // tblLease
        public string LeaseName { get; set; }                //41,70   A,30  LEASE NAME(DESC 3) IF TYPE=2,3
                                                                                                //tblLease.LeaseNameWag                                                                                                
        public int? LeaseNumber { get; set; }                //601,607 N,7,0 LEASE NUMBER       //tblCadLease.CadLeaseID
                                                                                                //tblCadProperty.CadPropID
        public string OperatorName { get; set; }             //71,90   A,20  OPER NAME(DESC 4)  //tblName.NameSortCad 

        // tblTract
        public float? acres { get; set; }                    //585,593 N,9,3 ACRES              //tblTract.LeasePct or Memo

        // tblWell
        public string RRC { get; set; }                      //271,290 A,20  RRC#               //tblWell.RrcLease        
        public int? YearLeaseStarted { get; set; }           //18,21 N,4,0   YEAR LEASE STARTED //tblWell.ProdDateFirst


        public string AbsoluteExemptionCode { get; set; }        
        public int? PollutionControlValue { get; set; }
        public int? PreviousAccountNumber { get; set; }
        public int? PreviousAccountSequence { get; set; }
        public string PrivacyCode { get; set; }
        public string ComplianceCode { get; set; }
        public string TCEQFlag { get; set; }                
        public decimal? NewImprovementPercent {get;set;}

        //public string NewTCEQflag { get; set; }
        //public int MineralAccountNumber { get; set; }
        //public int MineralAccountSequence { get; set; }
        //public int PreviousMineralAccountSequence { get; set; }
        //public int PreviousMineralAccount { get; set; }
        //public decimal NewImprovementNumber { get; set; }

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

        public string Juris1TaxableValue { get; set; }
        public string Juris2TaxableValue { get; set; }
        public string Juris3TaxableValue { get; set; }
        public string Juris4TaxableValue { get; set; }
        public string Juris5TaxableValue { get; set; }
        public string Juris6TaxableValue { get; set; }
        public string Juris7TaxableValue { get; set; }
        public string Juris8TaxableValue { get; set; }
        public string Juris9TaxableValue { get; set; }
        public string Juris10TaxableValue { get; set; }
        public string Juris11TaxableValue { get; set; }
        public string Juris12TaxableValue { get; set; }

        public string Juris1MarketValue { get; set; }
        public string Juris2MarketValue { get; set; }
        public string Juris3MarketValue { get; set; }
        public string Juris4MarketValue { get; set; }
        public string Juris5MarketValue { get; set; }
        public string Juris6MarketValue { get; set; }
        public string Juris7MarketValue { get; set; }
        public string Juris8MarketValue { get; set; }
        public string Juris9MarketValue { get; set; }
        public string Juris10MarketValue { get; set; }
        public string Juris11MarketValue { get; set; }
        public string Juris12MarketValue { get; set; }

        public string ExemptionMinOwnerFlagJuris1 { get; set; }
        public string ExemptionMinOwnerFlagJuris2 { get; set; }
        public string ExemptionMinOwnerFlagJuris3 { get; set; }
        public string ExemptionMinOwnerFlagJuris4 { get; set; }
        public string ExemptionMinOwnerFlagJuris5 { get; set; }
        public string ExemptionMinOwnerFlagJuris6 { get; set; }
        public string ExemptionMinOwnerFlagJuris7 { get; set; }
        public string ExemptionMinOwnerFlagJuris8 { get; set; }
        public string ExemptionMinOwnerFlagJuris9 { get; set; }
        public string ExemptionMinOwnerFlagJuris10 { get; set; }
        public string ExemptionMinOwnerFlagJuris11 { get; set; }
        public string ExemptionMinOwnerFlagJuris12 { get; set; }        

        public string PnAMinOwnerFlagJuris1 { get; set; }
        public string PnAMinOwnerFlagJuris2 { get; set; }
        public string PnAMinOwnerFlagJuris3 { get; set; }
        public string PnAMinOwnerFlagJuris4 { get; set; }
        public string PnAMinOwnerFlagJuris5 { get; set; }
        public string PnAMinOwnerFlagJuris6 { get; set; }
        public string PnAMinOwnerFlagJuris7 { get; set; }
        public string PnAMinOwnerFlagJuris8 { get; set; }
        public string PnAMinOwnerFlagJuris9 { get; set; }
        public string PnAMinOwnerFlagJuris10 { get; set; }
        public string PnAMinOwnerFlagJuris11 { get; set; }
        public string PnAMinOwnerFlagJuris12 { get; set; }

        public string TaxableValueNewJuris1 { get; set; }
        public string TaxableValueNewJuris2 { get; set; }
        public string TaxableValueNewJuris3 { get; set; }
        public string TaxableValueNewJuris4 { get; set; }
        public string TaxableValueNewJuris5 { get; set; }
        public string TaxableValueNewJuris6 { get; set; }
        public string TaxableValueNewJuris7 { get; set; }
        public string TaxableValueNewJuris8 { get; set; }
        public string TaxableValueNewJuris9 { get; set; }
        public string TaxableValueNewJuris10 { get; set; }
        public string TaxableValueNewJuris11 { get; set; }
        public string TaxableValueNewJuris12 { get; set; }

        public decimal? Abt1NewAbtValue { get; set; }
        public decimal? Abt2NewAbtValue { get; set; }
        public decimal? Abt3NewAbtValue { get; set; }
        public decimal? Abt4NewAbtValue { get; set; }
        public decimal? Abt5NewAbtValue { get; set; }
        public decimal? Abt6NewAbtValue { get; set; }
        public decimal? Abt7NewAbtValue { get; set; }
        public decimal? Abt8NewAbtValue { get; set; }
        public decimal? Abt9NewAbtValue { get; set; }
        public decimal? Abt10NewAbtValue { get; set; }
        public decimal? Abt11NewAbtValue { get; set; }
        public decimal? Abt12NewAbtValue { get; set; }     

        public decimal? NewExemptValueJuris1 { get; set; }
        public decimal? NewExemptValueJuris2 { get; set; }
        public decimal? NewExemptValueJuris3 { get; set; }
        public decimal? NewExemptValueJuris4 { get; set; }
        public decimal? NewExemptValueJuris5 { get; set; }
        public decimal? NewExemptValueJuris6 { get; set; }
        public decimal? NewExemptValueJuris7 { get; set; }
        public decimal? NewExemptValueJuris8 { get; set; }
        public decimal? NewExemptValueJuris9 { get; set; }
        public decimal? NewExemptValueJuris10 { get; set; }
        public decimal? NewExemptValueJuris11 { get; set; }
        public decimal? NewExemptValueJuris12 { get; set; }

       

    }
}
