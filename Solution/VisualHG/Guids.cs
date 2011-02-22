using System;

namespace VisualHG
{
    // How to: Obtain a PLK for a VSPackage
    // http://msdn.microsoft.com/en-us/vstudio/cc655795.aspx

    /*
    VisualHG
    Package Version 	1.0.0.0
    Package GUID 	F3DB741F-B40B-40be-A606-571A4BC653AB
    Product Name 	<none>
    Company Name 	VisualHG@live.de
    Visual Studio Version 	Visual Studio 2008
    Minimum Edition 	Professional
    Package Load Key (PLK) 	HPCED8Z3IRZQIRZ1D9IQRHJ9H0KCKMDIPTK0Z3QQJJDDIAKPAPM1PDMQMKC2JTQIICM3KECKQJPPK9HKHQQRD8DQP8JZAJIZJRM8ZMPCM8M0H1PJE2R9JQK0MERMJ3K2
    Description 	Mercurial source control integration
    */
    /*
    VisualHG
    Package Version 	1.0.0.1
    Package GUID 	F3DB741F-B40B-40be-A606-571A4BC653AB
    Product Name 	<none>
    Company Name 	VisualHG@live.de
    Visual Studio Version 	Visual Studio 2008
    Minimum Edition 	Professional
    Package Load Key (PLK) 	Z9IERHMDAERIDIP3CAHTJ2ZQQEZJEMAJD8R3IRA1DTD1M9DKPKEMQ9KTJQRIAPAPMZAMQJIAIZMPP9JIDRDMD2IPPMMTJ2RJAHJIJ9MJRMAPIEA2MAJ1PKZ1C2K8E3JD
    Description 	Mercurial source control integration
    */
    /*
    VisualHG
    Package Version 	1.0.0.3
    Package GUID 	DADADA00-348D-4EB9-95F2-DE3C44642044
    Product Name 	<none>
    Company Name 	VisualHG@live.de
    Visual Studio Version 	Visual Studio 2005
    Minimum Edition 	Standard
    Package Load Key (PLK) 		Q3KCEJI0DJC8A2ARAZQREIC2RDQPRPEIIZD2M1I8AMZQQ9H3IQPHKTAMIDP9H1MTC1J1RCJIAKZZHEZERMADPTJCRAD1A2KQECM3CHPAH8ETR2QPDEHQH8C8K8C2CRIK
    Description 	Mercurial source control integration

    public const string PackageGuid     = "DADADA00-348D-4EB9-95F2-DE3C44642044";
    public const string PackageVersion  = "1.0.0.3";
    public const string PakageName      = "VisualHG";
    public const string CompanyName     = "VisualHG@live.de";
    public const string MinEdition      = "Standard";
    Q3KCEJI0DJC8A2ARAZQREIC2RDQPRPEIIZD2M1I8AMZQQ9H3IQPHKTAMIDP9H1MTC1J1RCJIAKZZHEZERMADPTJCRAD1A2KQECM3CHPAH8ETR2QPDEHQH8C8K8C2CRIK

     * 
    Ver 1.1.0
    public const string PackageGuid     = "DADADA00-19aa-4a19-a6c4-25f8d4019b4d";
    public const string PackageVersion  = "1.0.0.4";
    public const string PakageName      = "VisualHG";
    public const string CompanyName     = "VisualHG@live.de";
    public const string MinEdition      = "Standard";
	AMRKJQE1ECKIDED1APDZMIE3ZPDACCPCAQA3M3KRC3KKI0M8PHQKQTD0JZJ2A2MDJ2C2H0AZA9DPC0R2PECECIJ9P1IRJPZ9I0EKJDR8QAJ1QAEHMAD2EAMMR2KEIIHD

    * 
    Ver 1.1.1
    public const string PackageGuid     = "DADADA00-caaf-423f-b21d-2df00fa25ca3";
    public const string PackageVersion  = "1.0.0.5";
    public const string PakageName      = "VisualHG";
    public const string CompanyName     = "VisualHG@live.de";
    public const string MinEdition      = "Standard";
    EHDKIZK1PCQCH8R2IMDHAKQQATQ3RZMEC0D1IZJDEJI2IMQCQIDRC1Z3E8CRIJPPKJIDHPMCCJQ9CQE2ADCAJQDPEQJ1D9KHZZCKHZRMPRE9JJJZP2Z1QAETHZI3K2KE
	
*/

    public static class PLK
    {
        public const string PackageGuid     = "DADADA00-caaf-423f-b21d-2df00fa25ca3";
        public const string PackageVersion  = "1.0.0.5";
        public const string PakageName      = "VisualHG";
        public const string CompanyName     = "VisualHG@live.de";
        public const string MinEdition      = "Standard";
    };
    
    /// <summary>
	/// This class is used only to expose the list of Guids used by this package.
	/// This list of guids must match the set of Guids used inside the VSCT file.
	/// </summary>
    public static class GuidList
    {
    	// Now define the list of guids as public static members.
        public const string HGPendingChangesToolWindowGuid = "DADADA00-d3b4-4d5c-a138-a87ca494f6c2";
        public const string ProviderOptionsPageGuid = "DADADA00-09a5-4795-a3ca-c3b49448184d";
   
        // Unique ID of the source control provider; this is also used as the command UI context to show/hide the pacakge UI
        public const string ProviderGuid = "DADADA00-63c7-4363-b107-ad5d9d915d45";
        public static readonly Guid guidSccProvider = new Guid(ProviderGuid);
        // The guid of the source control provider service (implementing IVsSccProvider interface)
        public const string ProviderServiceGuid = "DADADA00-d8ac-4ba7-8b05-5166c8f08ef5";
        public static readonly Guid guidSccProviderService = new Guid(ProviderServiceGuid);
        // The guid of the source control provider package (implementing IVsPackage interface)
        public static readonly Guid guidSccProviderPkg = new Guid(PLK.PackageGuid);
        // Other guids for menus and commands
        public static readonly Guid guidSccProviderCmdSet = new Guid("{DADADA00-1fd3-4e26-9c1d-c9cb723cea0e}");
    };

    /*
        {DADADA00-dfd3-4e42-a61c-499121e136f3}
        {DADADA00-2892-4e5d-8b7a-05dfff9a1add}
        {DADADA00-4fdb-4909-ab25-aeeacf120525}
        {DADADA00-3c04-4dfb-95f7-5a0f175303eb}
        {DADADA00-251f-4242-a751-93159e04046f}
        {DADADA00-fbb1-4ddf-bce8-2fc1f9600a55}
        {DADADA00-acad-4733-b0a1-c42200d92d79}
        {DADADA00-96d1-4556-b380-de1e339630fa}
        {DADADA00-e3c8-4830-b18a-e3df14df329a}
        {DADADA00-605f-4ac3-80f2-9567c91d5cba}
        {DADADA00-f79b-4f1c-9b28-de83eca39e58}
        {DADADA00-bc1b-4436-a49b-252d63e19a90}
        {DADADA00-6d78-482d-b2f3-9fb364370b14}
        {DADADA00-bb71-4b34-8076-8ee560187a3e}
        {DADADA00-550e-47b2-8af3-3d0af3624706}
        {DADADA00-58f7-4374-81a3-a5518fcd0b64}
        {DADADA00-278c-4fe8-a9b0-5b1c99fa3258}
        {DADADA00-5b02-43dd-bc6f-9126797127bf}
        {DADADA00-c9e6-4b5a-ba14-8c1a97e78e1f}
        {DADADA00-bf73-41ac-8e1e-abb09390ca1c}
        {DADADA00-30fa-4e25-ae49-e0da94ef5663}
        {DADADA00-9fd7-4321-9dcd-b1a404f5e013}
        {DADADA00-bbb7-4b2c-85ca-c31cb96617f4}
        {DADADA00-668a-4c8b-b2ad-bdd72d3db0a8}
        {DADADA00-39d5-438c-8f25-88dbc24ef90d}
        {DADADA00-9af8-4e17-a9f4-7c29d9557fcb}
        {DADADA00-5bc1-4370-92d1-c8b9a963a555}
        {DADADA00-b07f-4a79-a6a0-4c22937dc9e0}
        {DADADA00-3c2e-4c2e-b827-6437dd0026b3}
        {DADADA00-2be2-47c8-a103-59614bb29632}
        {DADADA00-e459-42e9-8026-7d99712af853}
        {DADADA00-5561-4e85-80b7-82ba91cb9f6c}
        {DADADA00-6e3d-4cc0-b98e-cf8cb8657a85}
        {DADADA00-9cd5-4998-9b63-f0a0282b4321}
        {DADADA00-6b6f-4605-b50c-bc2bd53bec5a}
        {DADADA00-3575-4513-91c9-fc9d7349dacd}
        {DADADA00-56b7-4059-9012-2716791812da}
        {DADADA00-142d-4169-b595-85bfa8f7a477}
        {DADADA00-f37e-4075-9873-a0c761bba0c1}
        {DADADA00-cf8e-49f2-a256-10ecc21ebf2f}
        {DADADA00-8282-447d-89e3-d2658d686754}
        {DADADA00-b03a-4b67-bebf-6871c98f65b1}
        {DADADA00-cdeb-4580-a971-8d05e64a3da6}
        {DADADA00-4d87-45cd-8754-28e537c14fbc}
        {DADADA00-662b-42e6-a466-28511398288d}
        {DADADA00-08d1-4d8e-ac87-3370705bf639}
        {DADADA00-0052-436b-aa7c-e3de964ab709}
        {DADADA00-4d63-420c-859d-736247997b17}
        {DADADA00-1f56-4760-bd54-27835925cd63}
        {DADADA00-1564-4269-ae66-4f89e9ae4605}
        {DADADA00-9de8-446e-a7c0-122cdfb38dec}
        {DADADA00-6610-412c-9c42-7d2573d022cb}
        {DADADA00-f783-4a69-a3fb-c69859cb1b13}
        {DADADA00-e2b6-4f86-89d4-b461397dbe17}
        {DADADA00-61fb-4fd6-a04e-304fe092affd}
        {DADADA00-a474-4e63-a38f-5be6ecdfa861}
        {DADADA00-8020-484f-96fc-79c623a125d4}
        {DADADA00-2e11-46e1-b2f8-889be592c90a}
        {DADADA00-e44f-47e0-ac66-428007fa203c}
        {DADADA00-ee45-44af-a335-9ff88c87a4b1}
        {DADADA00-2c95-4d97-9cd7-b5bc807f1a71}
        {DADADA00-cbcf-43ed-8b54-04a0015983ba}
        {DADADA00-d44d-4c53-a0d3-209a99d7735f}
        {DADADA00-c356-4eaa-ad47-032c606c7e3a}
        {DADADA00-7de8-4f30-bd18-ee5a766509da}
        {DADADA00-ee22-42d1-abbd-c4a87cc0496d}
        {DADADA00-9e23-4991-8cb8-fe556628a72e}
        {DADADA00-9632-4af6-9750-41807b606721}
        {DADADA00-8fd5-4946-a3be-097891226104}
        {DADADA00-dde2-4ed8-97b8-3ac58dcfbf2b}
        {DADADA00-b7a1-4763-b388-f335fd05c469}
        {DADADA00-9e3a-4b42-ab98-987230af1e46}
        {DADADA00-20ce-4ce3-9f0c-6118d4eaabd9}
        {DADADA00-42b1-450a-804b-b211dca4c118}
        {DADADA00-9210-468c-9f2d-ade327f20e0f}
        {DADADA00-22fc-4734-89a4-025212670bf2}
        {DADADA00-1537-4e57-889a-44e601fe69b3}
        {DADADA00-9f3e-4071-bcd1-7b3dd93c8d0a}
        {DADADA00-3c7f-429b-a265-c10219476322}
        {DADADA00-8fbb-4af3-838c-cd64666a9ed7}
        {DADADA00-afb7-4488-86ce-da65a1272ff8}
        {DADADA00-827e-46f6-9772-c5ebca661d59}
        {DADADA00-156b-4386-bd1a-68ae6c64c2b1}
        {DADADA00-5818-4774-a4d0-0e841881f48c}
        {DADADA00-c33f-457f-9657-e5ff0429c73e}
        {DADADA00-17fb-44b4-8c5f-04dd45ba42ba}
    */
}
