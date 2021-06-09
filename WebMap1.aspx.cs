using System;
using System.Net;
using System.Collections.Generic;
using System.Web.Services;
using System.Data.Odbc;
using System.Web.Script.Serialization;

public partial class WebMap1 : System.Web.UI.Page
{
	/* ----------------------------------------------------------------------------------------------
	  THIS SECTION IS FOR METHODS THAT ARE USED ONLY BY OTHER METHODS IN THIS FILE, NOT WEBMETHODS
	-------------------------------------------------------------------------------------------------*/
	
	// Global connection string to Ascend database. IMPORTANT: This is specific to the machine this app is running on.
	//	If Ascend connection name is different than the one below, it needs to be changed here:
	private string connstr = "Dsn=ascend30;";
	
	// This is the method all other methods call to get which years to process. This is the only place you need to set the years every year
	private string[] GetYears()
	{
		string[] Years = { "2021", "2020", "2019", "2018" }; // change these 4 every year

		return Years;
	}

	// This is used frequently in several methods
	private string GetPID(string parcelnum)
	{
		string PropID = "0";
		string parcelnum_trim = parcelnum.Trim();

		// Create the connection to Ascend, can be used for both queries/readers below
		OdbcConnection conn = new OdbcConnection(connstr);
		conn.Open();

		// Get the property ID number, which will be used to get and count the owners
		string strPropIDquery = "SELECT id FROM property WHERE parcel_number = '" + parcelnum_trim + "'";
		OdbcCommand cmdPropID = new OdbcCommand(strPropIDquery, conn);
		OdbcDataReader rdrPropID = cmdPropID.ExecuteReader();
		while (rdrPropID.Read())
			PropID = rdrPropID["id"].ToString().Trim();

		conn.Close();

		return PropID;
	} // end of GetPID method

	// Called from GetBasicInfo
	private string GetLandUseCodeDescription(string code)
	{
		string descr = "";
		OdbcConnection conn = new OdbcConnection(connstr);
		conn.Open();
		try
		{
			string qs = "SELECT description FROM value_list WHERE code = '" + code + "'";
			OdbcCommand cmd = new OdbcCommand(qs, conn);
			OdbcDataReader rdr = cmd.ExecuteReader();
			while (rdr.Read())
				descr = rdr["description"].ToString();
		}
		catch
		{
			descr = "";
		}
		conn.Close();
		return descr;
	} // end of GetLandUseCodeDescription method

	// Called from GetOwnerCitiesStatesZipsCountries
	private string GetState(string STcode)
	{
		// Do a quick return of some of the more common states ...
		if (STcode == "188") return "MO";
		else if (STcode == "590") return "KS";
		else if (STcode == "186") return "CA";
		else if (STcode == "585") return "CO";
		// ... then do a regular query for the rest
		else
		{
			string state = "";
			try
			{
				OdbcConnection conn = new OdbcConnection(connstr);
				conn.Open();

				string qstr = "SELECT code_table_cd FROM code_table WHERE id =" + STcode;
				OdbcCommand command = new OdbcCommand(qstr, conn);
				OdbcDataReader rdr = command.ExecuteReader();

				while (rdr.Read())
				{
					state = rdr["code_table_cd"].ToString();
				}
				conn.Close();
				return state;
			}
			catch
			{
				state = "N/A";
				return state;
			}
		}
	} // end of GetState method

	// Called from GetOwnerCitiesStatesZipsCountries
	private string GetCountry(string COcode)
	{
		if (COcode == "967" || COcode == "" || COcode == " ") return "USA";
		else
		{
			OdbcConnection conn = new OdbcConnection(connstr);
			conn.Open();

			string qstr = "SELECT code_table_cd FROM code_table WHERE id ='" + COcode + "'";
			OdbcCommand command = new OdbcCommand(qstr, conn);
			OdbcDataReader rdr = command.ExecuteReader();
			string country = "";
			while (rdr.Read())
			{
				country = rdr["code_table_cd"].ToString();
			}
			conn.Close();
			return country;
		}
	} // end of GetCountry method

	// Called from several methods
	private int GetOwnerCount(string parcelnum)
	{
		// Initialize basic variables and trim parcel #, just in case
		int Count = 0;
		string PropID = "0";
		string parcelnum_trim = parcelnum.Trim();

		// Create the connection to Ascend, can be used for both queries/readers below
		OdbcConnection conn = new OdbcConnection(connstr);
		conn.Open();

		// Get the property ID number, which will be used to get and count the owners
		string strPropIDquery = "SELECT id FROM property WHERE parcel_number = '" + parcelnum_trim + "'";
		OdbcCommand cmdPropID = new OdbcCommand(strPropIDquery, conn);
		OdbcDataReader rdrPropID = cmdPropID.ExecuteReader();
		while (rdrPropID.Read())
			PropID = rdrPropID["id"].ToString().Trim();

		// Now get the count of owners for that property ID
		string strCountQuery = "SELECT * FROM party_prop_invlmnt WHERE property_id = '" + PropID + "' AND prop_role_cd = '524' AND eff_to_date IS NULL";
		OdbcCommand cmd_count = new OdbcCommand(strCountQuery, conn);
		OdbcDataReader rdr_count = cmd_count.ExecuteReader();
		while (rdr_count.Read())
			Count++;

		conn.Close();
		return Count;
	} // end of GetOwnerCount method

	// Called from GetEconDistricts method
	private static string[] FindTIFdistrict(string x, string y)
	{
		/*
		data[0] = TIF District name
		data[1] = TIF District start date
		data[2] = TIF District end date
		data[3] = TIF District duration in years
		*/

		string[] data = new string[4];
		string name = "";
		string startdate;
		string enddate;
		int duration = 0;
		string dts1 = "";
		string dts2 = "";
		string distURL = "https://jcgis.jacksongov.org/arcgis/rest/services/Cadastral/Parcel_Viewer_Layers/MapServer/33/query?where=EFFECTIVEYEARFROM+<%3D%27" + DateTime.Today + "%27+AND+EFFECTIVEYEARTO+>%3D%27" + DateTime.Today + "%27&geometry=x%3D" + x + "%2C+y%3D" + y + "&geometryType=esriGeometryPoint&spatialRel=esriSpatialRelIntersects&units=esriSRUnit_Foot&outFields=Name%2C+EFFECTIVEYEARFROM%2C+EFFECTIVEYEARTO%2C+DURATION&returnGeometry=false&returnTrueCurves=false&returnIdsOnly=false&returnCountOnly=false&returnZ=false&returnM=false&returnDistinctValues=false&returnExtentOnly=false&featureEncoding=esriDefault&f=json";
		var distjson = new WebClient().DownloadString(distURL);

		try
		{
			JavaScriptSerializer jss = new JavaScriptSerializer();
			EconDevRootObject distOBJ = jss.Deserialize<EconDevRootObject>(distjson);
			foreach (EconDevFeature distFeature in distOBJ.features)
			{
				name = distFeature.attributes.Name;

				if (distFeature.attributes.EFFECTIVEYEARFROM != null) startdate = distFeature.attributes.EFFECTIVEYEARFROM;
				else startdate = "N/A";
				long temp1 = Convert.ToInt64(startdate);
				long beginTicks1 = new DateTime(1970, 1, 1).Ticks;
				DateTime dt1 = new DateTime(beginTicks1 + temp1 * 10000, DateTimeKind.Utc);
				dts1 = dt1.ToShortDateString();

				if (distFeature.attributes.EFFECTIVEYEARTO != null) enddate = distFeature.attributes.EFFECTIVEYEARTO;
				else enddate = "N/A";
				long temp2 = Convert.ToInt64(enddate);
				long beginTicks2 = new DateTime(1970, 1, 1).Ticks;
				DateTime dt2 = new DateTime(beginTicks1 + temp2 * 10000, DateTimeKind.Utc);
				dts2 = dt2.ToShortDateString();

				duration = distFeature.attributes.DURATION;
			}
			data[0] = name;
			data[1] = dts1;
			data[2] = dts2;
			data[3] = duration.ToString();
			if (data[3] == "0") data[3] = "N/A";
		}
		catch
		{
			data[0] = "No known TIF district for this parcel";
			data[1] = "N/A";
			data[2] = "N/A";
			data[3] = "N/A";
		}
		return data;
	} // end of FindTIFdistrict method

	// Called from GetEconDistricts method
	private static string[] FindTIFproject(string x, string y)
	{
		string[] data = new string[4];
		string name = "";
		string startdate;
		string enddate;
		int duration = 0;
		string dts1 = "";
		string dts2 = "";
		string projURL = "https://jcgis.jacksongov.org/arcgis/rest/services/Cadastral/Parcel_Viewer_Layers/MapServer/36/query?where=EFFECTIVEYEARFROM+<%3D%27" + DateTime.Today + "%27+AND+EFFECTIVEYEARTO+>%3D%27" + DateTime.Today + "%27&geometry=x%3D" + x + "%2C+y%3D" + y + "&geometryType=esriGeometryPoint&spatialRel=esriSpatialRelIntersects&units=esriSRUnit_Foot&outFields=Name%2C+EFFECTIVEYEARFROM%2C+EFFECTIVEYEARTO%2C+DURATION&returnGeometry=false&returnTrueCurves=false&returnIdsOnly=false&returnCountOnly=false&returnZ=false&returnM=false&returnDistinctValues=false&returnExtentOnly=false&featureEncoding=esriDefault&f=json";
		var projjson = new WebClient().DownloadString(projURL);

		try
		{
			JavaScriptSerializer jss = new JavaScriptSerializer();
			EconDevRootObject projOBJ = jss.Deserialize<EconDevRootObject>(projjson);
			foreach (EconDevFeature projFeature in projOBJ.features)
			{
				name = projFeature.attributes.Name;

				if (projFeature.attributes.EFFECTIVEYEARFROM != null) startdate = projFeature.attributes.EFFECTIVEYEARFROM;
				else startdate = "N/A";
				long temp1 = Convert.ToInt64(startdate);
				long beginTicks1 = new DateTime(1970, 1, 1).Ticks;
				DateTime dt1 = new DateTime(beginTicks1 + temp1 * 10000, DateTimeKind.Utc);
				dts1 = dt1.ToShortDateString();

				if (projFeature.attributes.EFFECTIVEYEARTO != null) enddate = projFeature.attributes.EFFECTIVEYEARTO;
				else enddate = "N/A";
				long temp2 = Convert.ToInt64(enddate);
				long beginTicks2 = new DateTime(1970, 1, 1).Ticks;
				DateTime dt2 = new DateTime(beginTicks2 + temp2 * 10000, DateTimeKind.Utc);
				dts2 = dt2.ToShortDateString();

				duration = projFeature.attributes.DURATION;
			}
			data[0] = name;
			data[1] = dts1;
			data[2] = dts2;
			data[3] = duration.ToString();
			if (data[3] == "0") data[3] = "N/A";
		}
		catch
		{
			data[0] = "No known TIF project for this parcel";
			data[1] = "N/A";
			data[2] = "N/A";
			data[3] = "N/A";
		}

		return data;
	} // end of FindTIFproject method

	// Called from GetEconDistricts method
	private static string FindCID(string x, string y)
	{
		string CID = "";
		string CIDurl = "https://jcgis.jacksongov.org/arcgis/rest/services/Cadastral/Parcel_Viewer_Layers/MapServer/35/query?where=&geometry=x%3D" + x + "%2Cy%3D" + y + "&geometryType=esriGeometryPoint&spatialRel=esriSpatialRelIntersects&units=esriSRUnit_Foot&outFields=&returnGeometry=false&returnTrueCurves=false&returnIdsOnly=false&returnCountOnly=false&returnZ=false&returnM=false&returnDistinctValues=false&returnExtentOnly=false&featureEncoding=esriDefault&f=pjson";
		var CIDjson = new WebClient().DownloadString(CIDurl);

		try
		{
			JavaScriptSerializer jss = new JavaScriptSerializer();
			EconDevRootObject CIDobj = jss.Deserialize<EconDevRootObject>(CIDjson);
			foreach (EconDevFeature CIDfeature in CIDobj.features)
			{
				CID = CIDfeature.attributes.Name.ToString();
			}
		}
		catch
		{
			CID = "No known CID for this parcel";
		}
		return CID;
	} // end of FindCID function
	/* ----------------------------------------------------------------------------------------------
	 * END OF INTERNAL ONLY METHODS
	-------------------------------------------------------------------------------------------------*/

	[WebMethod]
	public static string[] GetCoords(string parcelnum)
	{
		string[] xy = new string[2];

		try{
			string XYurl = "https://jcgis.jacksongov.org/portal/rest/services/Hosted/SitusAddresses/FeatureServer/0/query?where=ADDPTKEY%3D%27" + parcelnum + "%27&objectIds=&time=&geometry=&geometryType=esriGeometryPoint&inSR=&spatialRel=esriSpatialRelIntersects&distance=&units=esriSRUnit_Foot&relationParam=&outFields=XCOORD%2C+YCOORD&returnGeometry=false&maxAllowableOffset=&geometryPrecision=&outSR=&havingClause=&gdbVersion=&historicMoment=&returnDistinctValues=false&returnIdsOnly=false&returnCountOnly=false&returnExtentOnly=false&orderByFields=&groupByFieldsForStatistics=&outStatistics=&returnZ=false&returnM=false&multipatchOption=xyFootprint&resultOffset=&resultRecordCount=&returnTrueCurves=false&returnCentroid=false&sqlFormat=none&resultType=&datumTransformation=&f=pjson";
			var XYjson = new WebClient().DownloadString(XYurl);
			JavaScriptSerializer jss = new JavaScriptSerializer();
			XYRootObject xyOBJ = jss.Deserialize<XYRootObject>(XYjson);
			foreach (XYFeature xyFeature in xyOBJ.features)
			{
				xy[0] = xyFeature.attributes.XCOORD.ToString();
				xy[1] = xyFeature.attributes.YCOORD.ToString();
			}
		} catch {
			xy[0] = "0";
			xy[1] = "0";
		}
	return xy;
	} // end of GetCoords method

	[WebMethod]
	public static string[] GetBasicInfo(string rawparcelnum)
	{
		/*
		items[0] - lblParcelNum
		items[1] - lblSitusAddr -- also used in BOE form
		items[2] - lblSitusCityStateZip -- also used in BOE form
		items[3] - lblTCA
		items[4] - lblusecode -- also used in BOE form
		items[5] - lblLotSize
		items[6] - lblBldgSqFt
		items[7] - lblNumBR -- N/A for non-residential
		items[8] - lblNumBaths -- N/A for non-residential
		items[9] - lblYearBuilt
		items[10] - nbhd -- used only in BOE form
		*/

		string[] items = new string[11];

		string parcelnum_trim = rawparcelnum.Trim();
		string parcelnum_nodashes = parcelnum_trim.Replace("-", "");
		items[0] = parcelnum_trim;

		WebMap1 cs = new WebMap1();
		string querystr = "SELECT situsaddress, situscity, situszipcode, tcacode FROM ascend_rec_view WHERE parcel_number = '" + parcelnum_trim + "'";
		OdbcConnection conn = new OdbcConnection(cs.connstr);
		OdbcCommand command = new OdbcCommand(querystr, conn);
		conn.Open();
		OdbcDataReader rdr = command.ExecuteReader();
		while (rdr.Read())
		{
			items[1] = rdr["situsaddress"].ToString().Trim(); // also used for BOE appeals
			items[2] = rdr["situscity"].ToString().Trim() + ", MO " + rdr["situszipcode"].ToString().Trim(); // also used for BOE appeals
			items[3] = rdr["tcacode"].ToString();
		}

		int link_id = 0;
		string querystr1 = "SELECT total_sqft, link_id, use_code, nbhd FROM parcel WHERE parcel_id = '" + parcelnum_nodashes + "'";
		OdbcCommand command1 = new OdbcCommand(querystr1, conn);
		OdbcDataReader rdr1 = command1.ExecuteReader();
		WebMap1 description = new WebMap1();
		while (rdr1.Read())
		{
			link_id = Convert.ToInt32(rdr1["link_id"].ToString());
			string descr = description.GetLandUseCodeDescription(rdr1["use_code"].ToString());
			items[4] = rdr1["use_code"].ToString() + " - " + descr;  // also used for BOE appeals
			// for some bizarre reason this only works if you put it in a try-catch even though the ones in GetValues work just fine without try-catch
			try { items[5] = string.Format("{0:n0}", Convert.ToInt32(rdr1["total_sqft"].ToString().Trim())); }
			catch { items[5] = "N/A"; }
			items[10] = rdr1["nbhd"].ToString();
		}

		string querystr2, querystr3;
		char first = items[4][0]; // first digit of land use code
		if (first == '1') // residential
		{
			int fullbaths, halfbaths; double totbaths;
			querystr2 = "SELECT year_built, tot_sqf_l_area, num_bedrooms, full_baths, half_baths FROM residence WHERE link_id = '" + link_id.ToString() + "'";
			OdbcCommand command2 = new OdbcCommand(querystr2, conn);
			OdbcDataReader rdr2 = command2.ExecuteReader();
			while (rdr2.Read())
			{
				items[6] = string.Format("{0:n0}", Convert.ToInt32(rdr2["tot_sqf_l_area"].ToString()));
				items[7] = rdr2["num_bedrooms"].ToString();
				if (rdr2["full_baths"].ToString() != "") fullbaths = Convert.ToInt32(rdr2["full_baths"].ToString());
				else fullbaths = 0;
				if (rdr2["half_baths"].ToString() != "") halfbaths = Convert.ToInt32(rdr2["half_baths"].ToString());
				else halfbaths = 0;
				totbaths = Convert.ToDouble(fullbaths) + (Convert.ToDouble(halfbaths) * 0.5);
				items[8] = totbaths.ToString();
				items[9] = rdr2["year_built"].ToString();
			}
		}
		else if (first == '2' || first == '3') // commercial & industrial
		{
			querystr2 = "SELECT base_fl_area FROM comm_group WHERE link_id = '" + link_id.ToString() + "'";
			OdbcCommand command2 = new OdbcCommand(querystr2, conn);
			OdbcDataReader rdr2 = command2.ExecuteReader();
			while (rdr2.Read())
			{
				items[6] = string.Format("{0:n0}", Convert.ToInt32(rdr2["base_fl_area"].ToString().Trim())) + " ground floor";
				items[7] = "N/A"; // no BR's
				items[8] = "N/A"; // no baths
			}

			querystr3 = "SELECT year_built, eff_yr_built FROM comm_section WHERE link_id = '" + link_id.ToString() + "'";
			OdbcCommand command3 = new OdbcCommand(querystr3, conn);
			OdbcDataReader rdr3 = command3.ExecuteReader();
			while (rdr3.Read())
			{
				if (rdr3["eff_yr_built"].ToString() != "") items[9] = rdr3["year_built"].ToString() + " Effective: " + rdr3["eff_yr_built"].ToString();
				else items[9] = rdr3["year_built"].ToString();
			}
		}
		else { items[6] = "N/A"; items[7] = "N/A"; items[8] = "N/A"; items[9] = "N/A"; } // agricultural & forest & utility/railroad

		conn.Close();
		return items;

	} // end of GetBasicInfo method

	[WebMethod]
	public static string[] GetExemptionAndLegal(string parcelnum)
	{
		string[] items = new string[2];
		string PropID = "0";

		WebMap1 cs = new WebMap1();
		OdbcConnection conn_all = new OdbcConnection(cs.connstr);
		conn_all.Open();

		// This gets the most recent year for the query string below
		WebMap1 Years = new WebMap1();
		string Year0 = Years.GetYears()[0];

		string parcelnum_trim = parcelnum.Trim();
		string qstr1 = "SELECT id FROM property WHERE parcel_number = '" + parcelnum_trim + "'";
		OdbcCommand command1 = new OdbcCommand(qstr1, conn_all);
		OdbcDataReader rdr1 = command1.ExecuteReader();
		while (rdr1.Read())
			PropID = rdr1["id"].ToString().Trim();

		string tempcode = "";
		// status_cd='2001' means it's an active exemption, then also query for propertyID #
		string exemptcode = "SELECT exmpt_type_code FROM exempt_applicatn WHERE property_id = '" + PropID + "' AND app_status_cd = '2001'";
		OdbcCommand commandExmpt = new OdbcCommand(exemptcode, conn_all);
		OdbcDataReader rdrExmpt = commandExmpt.ExecuteReader();
		while (rdrExmpt.Read())
			tempcode = rdrExmpt["exmpt_type_code"].ToString().Trim();
		
		if (tempcode == "") items[0] = "None";
		else
		{
			string qryDescr = "SELECT description FROM exemption_type WHERE code = '" + tempcode + "'";
			OdbcCommand commandDescr = new OdbcCommand(qryDescr, conn_all);
			OdbcDataReader rdrDescr = commandDescr.ExecuteReader();
			while (rdrDescr.Read())
			{
				items[0] = rdrDescr["description"].ToString().Trim();
			}
		}

		string qstr2 = "SELECT ll.legal_desc_line FROM legal_lines ll, legal_description ld WHERE ld.id = ll.legal_id AND ld.eff_from_date <= TODAY AND (ld.eff_to_date IS NULL OR ld.eff_to_date >= TODAY)AND ld.property_id = '" + PropID + "'";
		OdbcCommand command2 = new OdbcCommand(qstr2, conn_all);
		OdbcDataReader rdr2 = command2.ExecuteReader();
		while (rdr2.Read())
			items[1] += rdr2["legal_desc_line"].ToString().Trim() + " ";

		conn_all.Close();
		return items;
	}  // end of GetExemptionAndLegal method

	[WebMethod]
	public static string[] GetValues(string parcelnum)
	{
		/*-------------------------------------------------------------------
					GUIDE TO ARRAY CREATED IN THIS METHOD
		array index = label name on aspx page = jsonobject index in Ajax.js
		---------------------------------------------------------------------
		info[0] = lblYear0 = jsonobject.d[0]
		info[1] = lblYear1 = jsonobject.d[1]
		info[2] = lblYear2 = jsonobject.d[2]
		info[3] = lblYear3 = jsonobject.d[3]
		info[4] = lblYear0AgLand = jsonobject.d[4]
		info[5] = lblYear0CommLand = jsonobject.d[5]
		info[6] = lblYear0ResLand = jsonobject.d[6]
		info[7] = lblYear0AgImp = jsonobject.d[7]
		info[8] = lblYear0CommImp = jsonobject.d[8]
		info[9] = lblYear0ResImp = jsonobject.d[9]
		info[10] = lblYear0AgNC = jsonobject.d[10]
		info[11] = lblYear0CommNC = jsonobject.d[11]
		info[12] = lblYear0ResNC = jsonobject.d[12]
		info[13] = lblYear0TMV = jsonobject.d[13]
		info[14] = lblYear0TAV = jsonobject.d[14]
		info[15] = lblYear0TTV = jsonobject.d[15]

		info[16] = lblYear1AgLand = jsonobject.d[16]
		info[17] = lblYear1CommLand = jsonobject.d[17]
		info[18] = lblYear1ResLand = jsonobject.d[18]
		info[19] = lblYear1AgImp = jsonobject.d[19]
		info[20] = lblYear1CommImp = jsonobject.d[20]
		info[21] = lblYear1ResImp = jsonobject.d[21]
		info[22] = lblYear1AgNC = jsonobject.d[22]
		info[23] = lblYear1CommNC = jsonobject.d[23]
		info[24] = lblYear1ResNC = jsonobject.d[24]
		info[25] = lblYear1TMV = jsonobject.d[25]
		info[26] = lblYear1TAV = jsonobject.d[26]
		info[27] = lblYear1TTV = jsonobject.d[27]

		info[28] = lblYear2AgLand = jsonobject.d[28]
		info[29] = lblYear2CommLand = jsonobject.d[29]
		info[30] = lblYear2ResLand = jsonobject.d[30]
		info[31] = lblYear2AgImp = jsonobject.d[31]
		info[32] = lblYear2CommImp = jsonobject.d[32]
		info[33] = lblYear2ResImp = jsonobject.d[33]
		info[34] = lblYear2AgNC = jsonobject.d[34]
		info[35] = lblYear2CommNC = jsonobject.d[35]
		info[36] = lblYear2ResNC = jsonobject.d[36]
		info[37] = lblYear2TMV = jsonobject.d[37]
		info[38] = lblYear2TAV = jsonobject.d[38]
		info[39] = lblYear2TTV = jsonobject.d[39]

		info[40] = lblYear3AgLand = jsonobject.d[40]
		info[41] = lblYear3CommLand = jsonobject.d[41]
		info[42] = lblYear3ResLand = jsonobject.d[42]
		info[43] = lblYear3AgImp = jsonobject.d[43]
		info[44] = lblYear3CommImp = jsonobject.d[44]
		info[45] = lblYear3ResImp = jsonobject.d[45]
		info[46] = lblYear3AgNC = jsonobject.d[46]
		info[47] = lblYear3CommNC = jsonobject.d[47]
		info[48] = lblYear3ResNC = jsonobject.d[48]
		info[49] = lblYear3TMV = jsonobject.d[49]
		info[50] = lblYear3TAV = jsonobject.d[50]
		info[51] = lblYear3TTV = jsonobject.d[51]
		*/

		string[] info = new string[52]; // size of the json array containing the output
		
		// set default values so you don't get "null" readings if nothing's there
		for (int i = 0; i < 52; i++)
			info[i] = "";

		string parcelnum_trim = parcelnum.Trim();
		// this is seperate for new construction
		int[] newconstr = new int[24];

		/// get the property ID #
		WebMap1 PID = new WebMap1();
		string PropID = PID.GetPID(parcelnum_trim);

		WebMap1 Years = new WebMap1();
		info[0] = Years.GetYears()[0];
		info[1] = Years.GetYears()[1];
		info[2] = Years.GetYears()[2];
		info[3] = Years.GetYears()[3];

		// this is a global connection string that can be used for all 5 years
		WebMap1 cs = new WebMap1();
		OdbcConnection conn = new OdbcConnection(cs.connstr);
		conn.Open();

		// query strings for each year
		string qsYear0 = "SELECT value_type, tax_year, modified_value FROM val_component WHERE property_id = '" + PropID + "' AND tax_year = '" + info[0] + "'";
		string qsYear1 = "SELECT value_type, tax_year, modified_value FROM val_component WHERE property_id = '" + PropID + "' AND tax_year = '" + info[1] + "'";
		string qsYear2 = "SELECT value_type, tax_year, modified_value FROM val_component WHERE property_id = '" + PropID + "' AND tax_year = '" + info[2] + "'";
		string qsYear3 = "SELECT value_type, tax_year, modified_value FROM val_component WHERE property_id = '" + PropID + "' AND tax_year = '" + info[3] + "'";

		OdbcCommand commandYear0 = new OdbcCommand(qsYear0, conn);
		OdbcDataReader rdrYear0 = commandYear0.ExecuteReader();
		OdbcCommand commandYear1 = new OdbcCommand(qsYear1, conn);
		OdbcDataReader rdrYear1 = commandYear1.ExecuteReader();
		OdbcCommand commandYear2 = new OdbcCommand(qsYear2, conn);
		OdbcDataReader rdrYear2 = commandYear2.ExecuteReader();
		OdbcCommand commandYear3 = new OdbcCommand(qsYear3, conn);
		OdbcDataReader rdrYear3 = commandYear3.ExecuteReader();

		// ------------------------------------------------------------------------------------------------------------------------------------------//
		// IMPORTANT: If changing the index numbers for any reason, don't forget to also change the 2nd index number in the catch statements below!!!
		// ------------------------------------------------------------------------------------------------------------------------------------------//

		while (rdrYear0.Read())
		{
			switch (rdrYear0["value_type"].ToString())
			{
				// fields in the database are min 5 characters so need spaces after some codes
				case "LANDA": try { info[4] = string.Format("{0:n0}", Convert.ToInt32(rdrYear0["modified_value"].ToString())); } catch { info[4] = "0"; } break;
				case "LANDC": try { info[5] = string.Format("{0:n0}", Convert.ToInt32(rdrYear0["modified_value"].ToString())); } catch { info[5] = "0"; } break;
				case "LANDR": try { info[6] = string.Format("{0:n0}", Convert.ToInt32(rdrYear0["modified_value"].ToString())); } catch { info[6] = "0"; } break;
				case "IMPRA": try { info[7] = string.Format("{0:n0}", Convert.ToInt32(rdrYear0["modified_value"].ToString())); } catch { info[7] = "0"; } break;
				case "IMPRC": try { info[8] = string.Format("{0:n0}", Convert.ToInt32(rdrYear0["modified_value"].ToString())); } catch { info[8] = "0"; } break;
				case "IMPRR": try { info[9] = string.Format("{0:n0}", Convert.ToInt32(rdrYear0["modified_value"].ToString())); } catch { info[9] = "0"; } break;

				// next 2 will be added together to create info[10]
				case "NCAL ": try { newconstr[0] = Convert.ToInt32(rdrYear0["modified_value"].ToString()); } catch { newconstr[0] = 0; } break;
				case "NCA  ": try { newconstr[1] = Convert.ToInt32(rdrYear0["modified_value"].ToString()); } catch { newconstr[1] = 0; } break;
				// next 2 will be added together to create info[11]
				case "NCCL ": try { newconstr[2] = Convert.ToInt32(rdrYear0["modified_value"].ToString()); } catch { newconstr[2] = 0; } break;
				case "NCC  ": try { newconstr[3] = Convert.ToInt32(rdrYear0["modified_value"].ToString()); } catch { newconstr[3] = 0; } break;
				// next 2 will be added together to create info[12]
				case "NCRL ": try { newconstr[4] = Convert.ToInt32(rdrYear0["modified_value"].ToString()); } catch { newconstr[4] = 0; } break;
				case "NCR  ": try { newconstr[5] = Convert.ToInt32(rdrYear0["modified_value"].ToString()); } catch { newconstr[5] = 0; } break;

				case "MKTTL": try { info[13] = string.Format("{0:n0}", Convert.ToInt32(rdrYear0["modified_value"].ToString())); } catch { info[13] = "0"; } break;
				case "AVR  ": try { info[14] = string.Format("{0:n0}", Convert.ToInt32(rdrYear0["modified_value"].ToString())); } catch { info[14] = "0"; } break;
				case "TVR  ": try { info[15] = string.Format("{0:n0}", Convert.ToInt32(rdrYear0["modified_value"].ToString())); } catch { info[15] = "0"; } break;

				default: break;
			}
		}
		int ten = newconstr[0] + newconstr[1];
		int eleven = newconstr[2] + newconstr[3];
		int twelve = newconstr[4] + newconstr[5];
		if(ten== 0) info[10] = "";
		else info[10] = string.Format("{0:n0}", Convert.ToInt32(ten.ToString()));
		if (eleven == 0) info[11] = "";
		else info[11] = string.Format("{0:n0}", Convert.ToInt32(eleven.ToString()));
		if (twelve == 0) info[12] = "";
		else info[12] = string.Format("{0:n0}", Convert.ToInt32(twelve.ToString()));

		while (rdrYear1.Read())
		{
			switch (rdrYear1["value_type"].ToString())
			{
				// fields in the database are min 5 characters so need spaces after some codes
				case "LANDA": try { info[16] = "$" + string.Format("{0:n0}", Convert.ToInt32(rdrYear1["modified_value"].ToString())); } catch { info[16] = "N/A"; } break;
				case "LANDC": try { info[17] = "$" + string.Format("{0:n0}", Convert.ToInt32(rdrYear1["modified_value"].ToString())); } catch { info[17] = "N/A"; } break;
				case "LANDR": try { info[18] = "$" + string.Format("{0:n0}", Convert.ToInt32(rdrYear1["modified_value"].ToString())); } catch { info[18] = "N/A"; } break;
				case "IMPRA": try { info[19] = "$" + string.Format("{0:n0}", Convert.ToInt32(rdrYear1["modified_value"].ToString())); } catch { info[19] = "N/A"; } break;
				case "IMPRC": try { info[20] = "$" + string.Format("{0:n0}", Convert.ToInt32(rdrYear1["modified_value"].ToString())); } catch { info[20] = "N/A"; } break;
				case "IMPRR": try { info[21] = "$" + string.Format("{0:n0}", Convert.ToInt32(rdrYear1["modified_value"].ToString())); } catch { info[21] = "N/A"; } break;

				// next 2 will be added together to create info[22]
				case "NCAL ": try { newconstr[6] = Convert.ToInt32(rdrYear1["modified_value"].ToString()); } catch { newconstr[6] = 0; } break;
				case "NCA  ": try { newconstr[7] = Convert.ToInt32(rdrYear1["modified_value"].ToString()); } catch { newconstr[7] = 0; } break;
				// next 2 will be added together to create info[23]
				case "NCCL ": try { newconstr[8] = Convert.ToInt32(rdrYear1["modified_value"].ToString()); } catch { newconstr[8] = 0; } break;
				case "NCC  ": try { newconstr[9] = Convert.ToInt32(rdrYear1["modified_value"].ToString()); } catch { newconstr[9] = 0; } break;
				// next 2 will be added together to create info[24]
				case "NCRL ": try { newconstr[10] = Convert.ToInt32(rdrYear1["modified_value"].ToString()); } catch { newconstr[10] = 0; } break;
				case "NCR  ": try { newconstr[11] = Convert.ToInt32(rdrYear1["modified_value"].ToString()); } catch { newconstr[11] = 0; } break;

				case "MKTTL": try { info[25] = "$" + string.Format("{0:n0}", Convert.ToInt32(rdrYear1["modified_value"].ToString())); } catch { info[15] = "N/A"; } break;
				case "AVR  ": try { info[26] = "$" + string.Format("{0:n0}", Convert.ToInt32(rdrYear1["modified_value"].ToString())); } catch { info[16] = "N/A"; } break;
				case "TVR  ": try { info[27] = "$" + string.Format("{0:n0}", Convert.ToInt32(rdrYear1["modified_value"].ToString())); } catch { info[17] = "N/A"; } break;

				default: break;
			}
		}
		int twentytwo = newconstr[6] + newconstr[7];
		int twentythree = newconstr[8] + newconstr[9];
		int twentyfour = newconstr[10] + newconstr[11];
		if (twentytwo == 0) info[22] = "";
		else info[22] = "$" + string.Format("{0:n0}", Convert.ToInt32(twentytwo.ToString()));
		if (twentythree == 0) info[23] = "";
		else info[23] = "$" + string.Format("{0:n0}", Convert.ToInt32(twentythree.ToString()));
		if (twentyfour == 0) info[24] = "";
		else info[24] = "$" + string.Format("{0:n0}", Convert.ToInt32(twentyfour.ToString()));

		while (rdrYear2.Read())
		{
			switch (rdrYear2["value_type"].ToString())
			{
				// fields in the database are min 5 characters so need spaces after some codes
				case "LANDA": try { info[28] = "$" + string.Format("{0:n0}", Convert.ToInt32(rdrYear2["modified_value"].ToString())); } catch { info[28] = "N/A"; } break;
				case "LANDC": try { info[29] = "$" + string.Format("{0:n0}", Convert.ToInt32(rdrYear2["modified_value"].ToString())); } catch { info[29] = "N/A"; } break;
				case "LANDR": try { info[30] = "$" + string.Format("{0:n0}", Convert.ToInt32(rdrYear2["modified_value"].ToString())); } catch { info[30] = "N/A"; } break;
				case "IMPRA": try { info[31] = "$" + string.Format("{0:n0}", Convert.ToInt32(rdrYear2["modified_value"].ToString())); } catch { info[31] = "N/A"; } break;
				case "IMPRC": try { info[32] = "$" + string.Format("{0:n0}", Convert.ToInt32(rdrYear2["modified_value"].ToString())); } catch { info[32] = "N/A"; } break;
				case "IMPRR": try { info[33] = "$" + string.Format("{0:n0}", Convert.ToInt32(rdrYear2["modified_value"].ToString())); } catch { info[33] = "N/A"; } break;

				// next 2 will be added together to create info[34]
				case "NCAL ": try { newconstr[12] = Convert.ToInt32(rdrYear2["modified_value"].ToString()); } catch { newconstr[12] = 0; } break;
				case "NCA  ": try { newconstr[13] = Convert.ToInt32(rdrYear2["modified_value"].ToString()); } catch { newconstr[13] = 0; } break;
				// next 2 will be added together to create info[35]
				case "NCCL ": try { newconstr[14] = Convert.ToInt32(rdrYear2["modified_value"].ToString()); } catch { newconstr[14] = 0; } break;
				case "NCC  ": try { newconstr[15] = Convert.ToInt32(rdrYear2["modified_value"].ToString()); } catch { newconstr[15] = 0; } break;
				// next 2 will be added together to create info[36]
				case "NCRL ": try { newconstr[16] = Convert.ToInt32(rdrYear2["modified_value"].ToString()); } catch { newconstr[16] = 0; } break;
				case "NCR  ": try { newconstr[17] = Convert.ToInt32(rdrYear2["modified_value"].ToString()); } catch { newconstr[17] = 0; } break;

				case "MKTTL": try { info[37] = "$" + string.Format("{0:n0}", Convert.ToInt32(rdrYear2["modified_value"].ToString())); } catch { info[37] = "N/A"; } break;
				case "AVR  ": try { info[38] = "$" + string.Format("{0:n0}", Convert.ToInt32(rdrYear2["modified_value"].ToString())); } catch { info[38] = "N/A"; } break;
				case "TVR  ": try { info[39] = "$" + string.Format("{0:n0}", Convert.ToInt32(rdrYear2["modified_value"].ToString())); } catch { info[39] = "N/A"; } break;

				default: break;
			}
		}
		int thirtyfour = newconstr[12] + newconstr[13];
		int thirtyfive = newconstr[14] + newconstr[15];
		int thirtysix = newconstr[16] + newconstr[17];
		if (thirtyfour == 0) info[34] = "";
		else info[34] = "$" + string.Format("{0:n0}", Convert.ToInt32(thirtyfour.ToString()));
		if (thirtyfive == 0) info[35] = "";
		else info[35] = "$" + string.Format("{0:n0}", Convert.ToInt32(thirtyfive.ToString()));
		if (thirtysix == 0) info[36] = "";
		else info[36] = "$" + string.Format("{0:n0}", Convert.ToInt32(thirtysix.ToString()));

		while (rdrYear3.Read())
		{
			switch (rdrYear3["value_type"].ToString())
			{
				// fields in the database are min 5 characters so need spaces after some codes
				case "LANDA": try { info[40] = "$" + string.Format("{0:n0}", Convert.ToInt32(rdrYear3["modified_value"].ToString())); } catch { info[40] = "N/A"; } break;
				case "LANDC": try { info[41] = "$" + string.Format("{0:n0}", Convert.ToInt32(rdrYear3["modified_value"].ToString())); } catch { info[41] = "N/A"; } break;
				case "LANDR": try { info[42] = "$" + string.Format("{0:n0}", Convert.ToInt32(rdrYear3["modified_value"].ToString())); } catch { info[42] = "N/A"; } break;
				case "IMPRA": try { info[43] = "$" + string.Format("{0:n0}", Convert.ToInt32(rdrYear3["modified_value"].ToString())); } catch { info[43] = "N/A"; } break;
				case "IMPRC": try { info[44] = "$" + string.Format("{0:n0}", Convert.ToInt32(rdrYear3["modified_value"].ToString())); } catch { info[44] = "N/A"; } break;
				case "IMPRR": try { info[45] = "$" + string.Format("{0:n0}", Convert.ToInt32(rdrYear3["modified_value"].ToString())); } catch { info[45] = "N/A"; } break;

				// next 2 will be added together to create info[46]
				case "NCAL ": try { newconstr[18] = Convert.ToInt32(rdrYear3["modified_value"].ToString()); } catch { newconstr[18] = 0; } break;
				case "NCA  ": try { newconstr[19] = Convert.ToInt32(rdrYear3["modified_value"].ToString()); } catch { newconstr[19] = 0; } break;
				// next 2 will be added together to create info[47]
				case "NCCL ": try { newconstr[20] = Convert.ToInt32(rdrYear3["modified_value"].ToString()); } catch { newconstr[20] = 0; } break;
				case "NCC  ": try { newconstr[21] = Convert.ToInt32(rdrYear3["modified_value"].ToString()); } catch { newconstr[21] = 0; } break;
				// next 2 will be added together to create info[48]
				case "NCRL ": try { newconstr[22] = Convert.ToInt32(rdrYear3["modified_value"].ToString()); } catch { newconstr[22] = 0; } break;
				case "NCR  ": try { newconstr[23] = Convert.ToInt32(rdrYear3["modified_value"].ToString()); } catch { newconstr[23] = 0; } break;

				case "MKTTL": try { info[49] = "$" + string.Format("{0:n0}", Convert.ToInt32(rdrYear3["modified_value"].ToString())); } catch { info[49] = "N/A"; } break;
				case "AVR  ": try { info[50] = "$" + string.Format("{0:n0}", Convert.ToInt32(rdrYear3["modified_value"].ToString())); } catch { info[50] = "N/A"; } break;
				case "TVR  ": try { info[51] = "$" + string.Format("{0:n0}", Convert.ToInt32(rdrYear3["modified_value"].ToString())); } catch { info[51] = "N/A"; } break;

				default: break;
			}
		}
		int fortysix = newconstr[18] + newconstr[19];
		int fortyseven = newconstr[20] + newconstr[21];
		int fortyeight = newconstr[22] + newconstr[23];
		if (fortysix == 0) info[46] = "";
		else info[46] = "$" + string.Format("{0:n0}", Convert.ToInt32(fortysix.ToString()));
		if (fortyseven == 0) info[47] = "";
		else info[47] = "$" + string.Format("{0:n0}", Convert.ToInt32(fortyseven.ToString()));
		if (fortyeight == 0) info[48] = "";
		else info[48] = "$" + string.Format("{0:n0}", Convert.ToInt32(fortyeight.ToString()));

		conn.Close();
		return info;
	} // end of GetValues method

	// for returning to web page
	[WebMethod]
	public static int ReturnOwnerCount(string parcelnum)
	{
		WebMap1 theCount = new WebMap1();
		int owncount = theCount.GetOwnerCount(parcelnum);
		return owncount;
	} // end of ReturnOwnerCount method

	[WebMethod]
	public static string[] GetOwnerNames(string parcelnum)
	{
		string[] PartyID;
		string[] QueryArray;
		string[] NamesArray;
		string parcelnum_trim = parcelnum.Trim();

		WebMap1 theCount = new WebMap1();
		int owncount = theCount.GetOwnerCount(parcelnum_trim);

		WebMap1 cs = new WebMap1();
		OdbcConnection conn_all = new OdbcConnection(cs.connstr);
		conn_all.Open();

		/// get the property ID #
		WebMap1 PID = new WebMap1();
		string PropID = PID.GetPID(parcelnum_trim);

		// This is used for both of the next two table readers and is the same query string used to determine # of owners
		string str_both = "SELECT party_id, address_id, prop_role_cd, role_percentage, eff_to_date FROM party_prop_invlmnt WHERE property_id = '" + PropID + "' AND prop_role_cd = '524' AND eff_to_date IS NULL";

		// So, now size the owner data array, party ID and org name query string arrays based on the count of owners. Owner data array will have 6 columns
		PartyID = new string[owncount];
		QueryArray = new string[owncount];
		NamesArray = new string[owncount];

		// For each owner, get their party ID and create a query string array for each owner. This will get their actual names in the next section
		int k = 0;
		OdbcCommand command_three = new OdbcCommand(str_both, conn_all);
		OdbcDataReader rdr_three = command_three.ExecuteReader();
		while (rdr_three.Read())
		{
			PartyID[k] = rdr_three["party_id"].ToString();
			QueryArray[k] = "SELECT org_name FROM organization WHERE party_id = '" + PartyID[k] + "'";
			k++;
		}

		// Finally, this gets the owner names
		for (int i = 0; i < owncount; i++)
		{
			OdbcCommand command = new OdbcCommand(QueryArray[i], conn_all);
			OdbcDataReader reader = command.ExecuteReader();
			while (reader.Read())
				NamesArray[i] = reader["org_name"].ToString().Trim();
		}

		conn_all.Close();
		return NamesArray;
	} // end of GetOWnerNames method

	[WebMethod]
	public static string[] GetOwnerAddresses(string parcelnum)
	{
		string[] OwnerAddresses;
		string[] AddressIDs;
		string parcelnum_trim = parcelnum.Trim();

		// Get the # of owners for this parcel
		WebMap1 theCount = new WebMap1();
		int owncount = theCount.GetOwnerCount(parcelnum_trim);

		// Then, size the address data array, party ID, address ID and org name query string arrays based on the count of owners
		OwnerAddresses = new string[owncount];
		AddressIDs = new string[owncount];

		WebMap1 cs = new WebMap1();
		OdbcConnection conn_all = new OdbcConnection(cs.connstr);
		conn_all.Open();

		/// get the property ID #
		WebMap1 PID = new WebMap1();
		string PropID = PID.GetPID(parcelnum_trim);

		// This is used for both of the next two table readers and is the same query string used to determine # of owners
		string str_both = "SELECT party_id, address_id, prop_role_cd, role_percentage, eff_to_date FROM party_prop_invlmnt WHERE property_id = '" + PropID + "' AND prop_role_cd = '524' AND eff_to_date IS NULL";

		// This gets the Sddress ID, which is needed below to get the specific address information
		OdbcCommand command_two = new OdbcCommand(str_both, conn_all);
		OdbcDataReader rdr_two = command_two.ExecuteReader();
		int i = 0;
		while (rdr_two.Read())
		{
			AddressIDs[i] = rdr_two["address_id"].ToString().Trim();
			i++;
		}

		// This gets each owner's name using the Party ID retreived above
		for (int k = 0; k < owncount; k++)
		{
			string str_address = "SELECT line_1, city, province_state_cd, zip_postal_code, country_cd FROM address WHERE id = '" + AddressIDs[k] + "'";
			OdbcCommand command_four = new OdbcCommand(str_address, conn_all);
			OdbcDataReader rdr_four = command_four.ExecuteReader();
			while (rdr_four.Read())
				OwnerAddresses[k] = rdr_four["line_1"].ToString().Trim();
		}

		conn_all.Close();
		return OwnerAddresses;
	} // end of GetOWnerAddresses method

	[WebMethod]
	public static string[] GetOwnerCitiesStatesZipsCountries(string parcelnum)
	{
		string[] OwnerCSZC;
		string[] AddressIDs;
		string parcelnum_trim = parcelnum.Trim();

		// Get the # of owners for this parcel
		WebMap1 theCount = new WebMap1();
		int owncount = theCount.GetOwnerCount(parcelnum_trim);

		// Then, size the address data array, party ID, address ID and org name query string arrays based on the count of owners
		OwnerCSZC = new string[owncount];
		AddressIDs = new string[owncount];

		WebMap1 cs = new WebMap1();
		OdbcConnection conn_all = new OdbcConnection(cs.connstr);
		conn_all.Open();

		/// get the property ID #
		WebMap1 PID = new WebMap1();
		string PropID = PID.GetPID(parcelnum_trim);

		// This is used for both of the next two table readers and is the same query string used to determine # of owners
		string str_both = "SELECT party_id, address_id, prop_role_cd, role_percentage, eff_to_date FROM party_prop_invlmnt WHERE property_id = '" + PropID + "' AND prop_role_cd = '524' AND eff_to_date IS NULL";

		// This gets the Address ID, which is needed below to get the specific address information
		OdbcCommand command_two = new OdbcCommand(str_both, conn_all);
		OdbcDataReader rdr_two = command_two.ExecuteReader();
		int i = 0;
		while (rdr_two.Read())
		{
			AddressIDs[i] = rdr_two["address_id"].ToString().Trim();
			i++;
		}

		// This gets each owner's name using the Party ID retreived above
		for (int k = 0; k < owncount; k++)
		{
			string str_cities = "SELECT city, province_state_cd, zip_postal_code, country_cd FROM address WHERE id = '" + AddressIDs[k] + "'";
			OdbcCommand command_four = new OdbcCommand(str_cities, conn_all);
			OdbcDataReader rdr_four = command_four.ExecuteReader();
			string temp, state, country;
			while (rdr_four.Read())
			{
				temp = rdr_four["province_state_cd"].ToString().Trim();
				WebMap1 getState = new WebMap1();
				state = getState.GetState(temp);
				temp = rdr_four["country_cd"].ToString().Trim();
				WebMap1 getCountry = new WebMap1();
				country = getCountry.GetCountry(temp);
				OwnerCSZC[k] = rdr_four["city"].ToString().Trim() + ", " + state + " " + rdr_four["zip_postal_code"].ToString().Trim() + " " + country;
			}
		}

		conn_all.Close();
		return OwnerCSZC;
	} // end of GetOwnerCitiesStatesZipsCountries method

	[WebMethod]
	public static string[] GetEconDistricts(string parcelnum, string X, string Y)
	{
		/*
		GUIDE TO WHAT'S BEING LOADED INTO THE JSON ARRAY
		------------------------------------------------
		data[0] = TIF District name
		data[1] = TIF District start date
		data[2] = TIF District end date
		data[3] = TIF District duration in years

		data[4] = TIF Ordinance #
		data[5] = TIF Project start date
		data[6] = TIF Project end date
		data[7] = TIF Project duration in years

		data[8] = 353 description
		data[9] = 353 abatement from year
		data[10] = 353 abatement to year

		data[11] = 99 description
		data[12] = 99 abatement from year
		data[13] = 99 abatement to year

		data[14] = Community Improvement District name
		--------------------------------------------------
		*/

		// array to be returned as JSON to Ajax call. See guide above for details
		string[] data = new string[15];
		string CID;
		string parcelnum_trim = parcelnum.Trim();

		// Get the TIF district and populate data[0-3]
		int i = 0;
		foreach (string item in FindTIFdistrict(X, Y)) // FindTIFdistrict in method near top
		{
			if (i == 0)
			{
				if (item == "") data[i] = "No known TIF district for this parcel";
				else data[i] = item;
			}
			else
			{
				if (item == "") data[i] = "N/A";
				else data[i] = item;
			}
			i++;
		}
		// Get the TIF project and populate data[4-7]
		int j = 4;
		foreach (string item in FindTIFproject(X, Y)) // FindTIFproject in method near top
		{
			if (j == 4)
			{
				if (item == "") data[j] = "No known TIF project for this parcel";
				else data[j] = item;
			}
			else
			{
				if (item == "") data[j] = "N/A";
				else data[j] = item;
			}
			j++;
		}
		// Get the CID and assign to temp CID variable
		CID = FindCID(X, Y); // FindCID is method near top

		WebMap1 cs = new WebMap1();
		OdbcConnection conn = new OdbcConnection(cs.connstr);
		conn.Open();

		// get the property ID # for the query below
		WebMap1 PID = new WebMap1();
		string PropID = PID.GetPID(parcelnum_trim);

		// This is redundant from GetExemptionsAndLegal but whatever ...
		// -------------------------------------------------------------------
		string exemptcode = "";
		string qsExmpt = "SELECT exmpt_type_code FROM exempt_applicatn WHERE property_id = '" + PropID + "' AND app_status_cd = '2001'";
		OdbcCommand commandExmpt = new OdbcCommand(qsExmpt, conn);
		OdbcDataReader rdrExmpt = commandExmpt.ExecuteReader();
		while (rdrExmpt.Read())
			exemptcode = rdrExmpt["exmpt_type_code"].ToString().Trim();
		// --------------------------------------------------------------------
		// End of redundant part

		string fromyear = "";
		string toyear = "";

		string qs2 = "SELECT eff_from_year, eff_to_year FROM exempt_applicatn WHERE property_id = '" + PropID + "'";
		OdbcCommand commandFromTo = new OdbcCommand(qs2, conn);
		OdbcDataReader rdrFromTo = commandFromTo.ExecuteReader();
		while (rdrFromTo.Read())
		{
			fromyear = rdrFromTo["eff_from_year"].ToString();
			toyear = rdrFromTo["eff_to_year"].ToString();
		}

		if (exemptcode == " " || exemptcode == "")
		{
			data[8] = "No exemptions";
			data[9] = "N/A";
			data[10] = "N/A";
			data[11] = "N/A";
			data[12] = "N/A";
			data[13] = "N/A";
		}
		else if (exemptcode == "D03") // this is if there's a 99 abatement
		{
			data[8] = "No exemptions";
			data[9] = "N/A";
			data[10] = "N/A";
			data[11] = "D03 Abatement 99 (Freezes taxable amount for 10 years)";
			data[12] = fromyear;
			data[13] = toyear;
		}
		else if (exemptcode == "D05") // this is if there's a 353 1st phase abatement
		{
			data[8] = "D05 Abatement 353 1st Phase";
			data[9] = fromyear;
			data[10] = toyear;
			data[11] = "N/A";
			data[12] = "N/A";
			data[13] = "N/A";
		}
		else if (exemptcode == "D06") // this is if there's a 353 2nd phase abatement
		{
			data[8] = "D06 Abatement 353 2nd Phase 50% off Land and Improvement";
			data[9] = fromyear;
			data[10] = toyear;
			data[11] = "N/A";
			data[12] = "N/A";
			data[13] = "N/A";
		}
		else // in case there's some other value ...
		{
			data[8] = "N/A";
			data[9] = "N/A";
			data[10] = "N/A";
			data[11] = "N/A";
			data[12] = "N/A";
			data[13] = "N/A";
		}

		if (CID != "") data[14] = CID;
		else data[14] = "Property is not in a CID for which Jackson County collects a tax or assessment";

		conn.Close();
		return data;
	}  // end of GetEconDistricts method

	[WebMethod]
	public static string[] GetPhotos(string parcelnum)
	{
		string parcelnum_nodashes = parcelnum.Replace("-", "");
		WebMap1 cs = new WebMap1();
		OdbcConnection conn = new OdbcConnection(cs.connstr);
		conn.Open();
		string qstr = "SELECT dir_name, file_name, picture_type FROM pictures WHERE parcel_id= '" + parcelnum_nodashes + "' AND end_date IS NULL";
		OdbcCommand cmd = new OdbcCommand(qstr, conn);
		OdbcDataReader rdr = cmd.ExecuteReader();

		List<string> PhotoList = new List<string>();

		while (rdr.Read())
		{
			string temp1 = rdr["dir_name"].ToString();
			string temp2 = temp1.Trim();
			string dir_name = temp2.Replace(@"O:\Pictures", "https://jcgis.jacksongov.org/AscendPics/Pictures").Replace(@"O:\MobileAssessorPhotos", "https://jcgis.jacksongov.org/AscendPics/MobileAssessorPhotos").Replace(@"O:\CameraPhotos", "https://jcgis.jacksongov.org/AscendPics/CameraPhotos");
			string temp3 = rdr["file_name"].ToString();
			string temp4 = temp3.Replace(@"\", "/");
			string file_name = temp4.Trim();
			PhotoList.Add(dir_name + "/" + file_name + "." + rdr["picture_type"].ToString());
		}

		string[] returnarray = new string[PhotoList.Count];
		int j = 0;
		foreach (string val in PhotoList)
		{
			returnarray[j] = val;
			j++;
		}
		conn.Close();
		return returnarray;
	} // end of GetPhotos method

	[WebMethod]
	public static string CheckAppeals(string parcelnum)
	{
		/*	If there is an informal appeal they can still have a formal but not another informal
			If there is a formal appeal they cannot have an informal nor another formal
			Therefore, go through all the appeals on each parcel:
				--> as soon as you find a formal appeal, make highest_appeal_type "formal" and break out of the loop, returning "formal"
				--> if you find an informal appeal, make highest_appeal_type "informal", so, if you get to the end of the appeals and there are only informal ones, highest_appeal_type will be "informal"
		*/
		ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12; // this is needed for server communication

		string appeal_type = "";
		string AppealsUrl = "https://services3.arcgis.com/4LOAHoFXfea6Y3Et/ArcGIS/rest/services/PublicParcelNumber/FeatureServer/0/query?where=parcel_number%3D%27" + parcelnum + "%27+AND+email+IS+NOT+NULL&objectIds=&time=&geometry=&geometryType=esriGeometryEnvelope&inSR=&spatialRel=esriSpatialRelIntersects&resultType=none&distance=0.0&units=esriSRUnit_Meter&returnGeodetic=false&outFields=appeal_type%2C+email&returnHiddenFields=false&returnGeometry=false&featureEncoding=esriDefault&multipatchOption=xyFootprint&maxAllowableOffset=&geometryPrecision=&outSR=&datumTransformation=&applyVCSProjection=false&returnIdsOnly=false&returnUniqueIdsOnly=false&returnCountOnly=false&returnExtentOnly=false&returnQueryGeometry=false&returnDistinctValues=false&cacheHint=false&orderByFields=&groupByFieldsForStatistics=&outStatistics=&having=&resultOffset=&resultRecordCount=&returnZ=false&returnM=false&returnExceededLimitFeatures=true&quantizationParameters=&sqlFormat=none&f=pjson";

		JavaScriptSerializer jss = new JavaScriptSerializer();
		try
		{
			var appealsJson = new WebClient().DownloadString(AppealsUrl);
			AppealsRootobject appealsRO = jss.Deserialize<AppealsRootobject>(appealsJson);

			// loop through the appeals (usually just going to be one, but just in case, do a loop)
			foreach (AppealsFeature appealsFeat in appealsRO.features)
			{
				if (appealsFeat.attributes.appeal_type == "formal")
				{
					return "formal";
				}
				if (appealsFeat.attributes.appeal_type == "informal")
				{
					appeal_type = "informal";
				}
			}

		} catch
		{
			return "Cannot determine number of appeals on this parcel.";
		}
		if (appeal_type == "") return "There are no active appeals on this property.";
		else return appeal_type;
	}

	[WebMethod]
	public static string[] GetElectedOfficials(string X, string Y)
	{
		/*
		GUIDE TO WHAT'S BEING LOADED INTO THE JSON ARRAY
		------------------------------------------------
		Politicians[0] = Individual County Council name
		Politicians[1] = Individual County Council party
		Politicians[2] = Individual County Council district
		Politicians[3] = Individual County Council website
		Politicians[4] = At-large County Council name
		Politicians[5] = At-large County Council party
		Politicians[6] = At-large County Council district
		Politicians[7] = At-large County Council website
		Politicians[8] = State representative name
		Politicians[9] = State representative party
		Politicians[10] = State representative district
		Politicians[11] = State representative website
		Politicians[12] = State senator name
		Politicians[13] = State senator party
		Politicians[14] = State senator district
		Politicians[15] = State senator website
		Politicians[16] = US House name
		Politicians[17] = US House party
		Politicians[18] = US House district
		Politicians[19] = US House website
		--------------------------------------------------
		*/
		
		string[] Politicians = new string[20];

		// string URLs and json objects for county council, MO house of reps, MO senate and US rep
		// ------------------------------------------------------------------------------------------
		string JaCoAtLargeUrl = "http://jcgis.jacksongov.org/arcgis/rest/services/ElectionAdministration/LegislativeDistricts/MapServer/0/query?where=&text=&objectIds=&time=&geometry=" + X + "%2C" + Y + "&geometryType=esriGeometryPoint&inSR=102698&spatialRel=esriSpatialRelIntersects&relationParam=&outFields=DISTRICT%2C+Name%2C+Party%2C+WebSite&returnGeometry=false&returnTrueCurves=false&maxAllowableOffset=&geometryPrecision=&outSR=&returnIdsOnly=false&returnCountOnly=false&orderByFields=&groupByFieldsForStatistics=&outStatistics=&returnZ=false&returnM=false&gdbVersion=&returnDistinctValues=false&resultOffset=&resultRecordCount=&f=json";
		string JaCoIndivUrl = "http://jcgis.jacksongov.org/arcgis/rest/services/ElectionAdministration/LegislativeDistricts/MapServer/1/query?where=&text=&objectIds=&time=&geometry=" + X + "%2C" + Y + "&geometryType=esriGeometryPoint&inSR=102698&spatialRel=esriSpatialRelIntersects&relationParam=&outFields=DISTRICT%2C+name%2C+Party%2C+WebSite&returnGeometry=false&returnTrueCurves=false&maxAllowableOffset=&geometryPrecision=&outSR=&returnIdsOnly=false&returnCountOnly=false&orderByFields=&groupByFieldsForStatistics=&outStatistics=&returnZ=false&returnM=false&gdbVersion=&returnDistinctValues=false&resultOffset=&resultRecordCount=&f=json";
		string MoSenUrl = "http://jcgis.jacksongov.org/arcgis/rest/services/ElectionAdministration/ElectedOfficials/MapServer/1/query?where=&objectIds=&time=&geometry=" + X + "%2C" + Y + "&geometryType=esriGeometryPoint&inSR=102698&spatialRel=esriSpatialRelIntersects&distance=&units=esriSRUnit_Foot&relationParam=&outFields=DISTRICT%2C+REPNAME%2C+Party%2C+WebSite&returnGeometry=false&maxAllowableOffset=&geometryPrecision=&outSR=&gdbVersion=&returnDistinctValues=false&returnIdsOnly=false&returnCountOnly=false&returnExtentOnly=false&orderByFields=&groupByFieldsForStatistics=&outStatistics=&returnZ=false&returnM=false&multipatchOption=&f=json";
		string MoRepUrl = "http://jcgis.jacksongov.org/arcgis/rest/services/ElectionAdministration/ElectedOfficials/MapServer/0/query?where=&text=&objectIds=&time=&geometry=" + X + "%2C" + Y + "&geometryType=esriGeometryPoint&inSR=102698&spatialRel=esriSpatialRelWithin&relationParam=&outFields=DISTRICT%2C+REPNAME%2C+Party%2C+WebSite&returnGeometry=false&returnTrueCurves=false&maxAllowableOffset=&geometryPrecision=&outSR=&returnIdsOnly=false&returnCountOnly=false&orderByFields=&groupByFieldsForStatistics=&outStatistics=&returnZ=false&returnM=false&gdbVersion=&returnDistinctValues=false&resultOffset=&resultRecordCount=&f=pjson";
		string UShouseUrl = "http://jcgis.jacksongov.org/arcgis/rest/services/ElectionAdministration/ElectedOfficials/MapServer/2/query?where=&text=&objectIds=&time=&geometry=" + X + "%2C" + Y + "&geometryType=esriGeometryPoint&inSR=102698&spatialRel=esriSpatialRelWithin&distance=&units=esriSRUnit_Meter&relationParam=&outFields=DISTRICT%2C+REPNAME%2C+Party%2C+WebSite&returnGeometry=false&returnTrueCurves=false&maxAllowableOffset=&geometryPrecision=&outSR=&havingClause=&returnIdsOnly=false&returnCountOnly=false&orderByFields=&groupByFieldsForStatistics=&outStatistics=&returnZ=false&returnM=false&gdbVersion=&historicMoment=&returnDistinctValues=false&resultOffset=&resultRecordCount=&returnExtentOnly=false&datumTransformation=&parameterValues=&rangeValues=&quantizationParameters=&featureEncoding=esriDefault&f=pjson";
		var JaCoAtLargejson = new WebClient().DownloadString(JaCoAtLargeUrl);
		var JaCoIndivjson = new WebClient().DownloadString(JaCoIndivUrl);
		var MoRepjson = new WebClient().DownloadString(MoRepUrl);
		var MoSenjson = new WebClient().DownloadString(MoSenUrl);
		var USHousejson = new WebClient().DownloadString(UShouseUrl);

		// Using JavaScriptJsonSerializer to deserialize objects
		// -------------------------------------------------------------------
		JavaScriptSerializer jss = new JavaScriptSerializer();

		// County individual districts
		try
		{
			indivRootObject JaCoIndivOBJ = jss.Deserialize<indivRootObject>(JaCoIndivjson);
			foreach (indivFeature featureJaCoIndiv in JaCoIndivOBJ.features)
			{
				Politicians[0] = featureJaCoIndiv.attributes.name;
				Politicians[1] = featureJaCoIndiv.attributes.Party;
				Politicians[2] = featureJaCoIndiv.attributes.DISTRICT.ToString();
				Politicians[3] = featureJaCoIndiv.attributes.WebSite;
			}
		} catch
		{
			Politicians[0] = "N/A";
			Politicians[1] = "N/A";
			Politicians[2] = "N/A";
			Politicians[3] = "N/A";
		}
		// County at-large districts
		try
		{
			alRootObject JaCoAtLargeOBJ = jss.Deserialize<alRootObject>(JaCoAtLargejson);
			foreach (alFeature featureJaCoAtLarge in JaCoAtLargeOBJ.features)
			{
				Politicians[4] = featureJaCoAtLarge.attributes.Name;
				Politicians[5] = featureJaCoAtLarge.attributes.Party;
				Politicians[6] = featureJaCoAtLarge.attributes.DISTRICT.ToString();
				Politicians[7] = featureJaCoAtLarge.attributes.WebSite;
			}
		} catch
		{
			Politicians[4] = "N/A";
			Politicians[5] = "N/A";
			Politicians[6] = "N/A";
			Politicians[7] = "N/A";
		}
		// State senators
		try
		{
			stateRootObject MOsenOBJ = jss.Deserialize<stateRootObject>(MoSenjson);
			foreach (stateFeature featureMOsen in MOsenOBJ.features)
			{
				Politicians[8] = featureMOsen.attributes.REPNAME;
				Politicians[9] = featureMOsen.attributes.Party;
				Politicians[10] = featureMOsen.attributes.DISTRICT.ToString();
				Politicians[11] = featureMOsen.attributes.WebSite;
			}
		} catch
		{
			Politicians[8] = "N/A";
			Politicians[9] = "N/A";
			Politicians[10] = "N/A";
			Politicians[11] = "N/A";
		}
		// State representatives
		try
		{
			stateRootObject MOrepOBJ = jss.Deserialize<stateRootObject>(MoRepjson);
			foreach (stateFeature featureMOrep in MOrepOBJ.features)
			{
				Politicians[12] = featureMOrep.attributes.REPNAME;
				Politicians[13] = featureMOrep.attributes.Party;
				Politicians[14] = featureMOrep.attributes.DISTRICT.ToString();
				Politicians[15] = featureMOrep.attributes.WebSite;
			}
		} catch
		{
			Politicians[12] = "N/A";
			Politicians[13] = "N/A";
			Politicians[14] = "N/A";
			Politicians[15] = "N/A";
		}
		// Federal (US House)
		try
		{
			federalRootObject USHouseOBJ = jss.Deserialize<federalRootObject>(USHousejson);
			foreach (federalFeature featureUSHouse in USHouseOBJ.features)
			{
				Politicians[16] = featureUSHouse.attributes.REPNAME;
				Politicians[17] = featureUSHouse.attributes.Party;
				Politicians[18] = featureUSHouse.attributes.DISTRICT.ToString();
				Politicians[19] = featureUSHouse.attributes.WebSite;
			}
		} catch
		{
			Politicians[16] = "N/A";
			Politicians[17] = "N/A";
			Politicians[18] = "N/A";
			Politicians[19] = "N/A";
		}

		return Politicians;

	} // end of GetElectedOfficials method

	[WebMethod]
	public static string[] GetLatLong(string X, string Y)
	{
		string[] latlong = new string[2];
		string LatLongServiceURL = "https://jcgis.jacksongov.org/arcgis/rest/services/Utilities/Geometry/GeometryServer/project?inSR=102698&outSR=4326&geometries=%7B%0D%0A++%22geometryType%22+%3A+%22esriGeometryPoint%22%2C%0D%0A++%22geometries%22+%3A+%5B%0D%0A+++++%7B%0D%0A+++++++%22x%22+%3A+" + X + "%2C++++++++%22y%22+%3A+" + Y + "%0D%0A+++++%7D%0D%0A++%5D%0D%0A%7D&transformation=&transformForward=true&vertical=false&f=pjson";
		var LatLongJson = new WebClient().DownloadString(LatLongServiceURL);

		// Using JavaScriptJsonSerializer to deserialize objects
		// -------------------------------------------------------------------
		JavaScriptSerializer jss = new JavaScriptSerializer();
		try
		{
			LatLongRootObject LatLongOBJ = jss.Deserialize<LatLongRootObject>(LatLongJson);
			foreach (Geometry featureLatLong in LatLongOBJ.geometries)
			{
				latlong[0] = featureLatLong.x.ToString();
				latlong[1] = featureLatLong.y.ToString();
			}
		}
		catch
		{
			latlong[0] = "0.0";
			latlong[1] = "0.0";
		}
		return latlong;
	} // end of GetLatLong method
} // end of WebMap1 class

// -----------------------------------------------------
// Classes for xcoord and ycoord
// -----------------------------------------------------
public class XYRootObject
{
	public XYFeature[] features { get; set; }
}
public class XYFeature
{
	public XYAttributes attributes { get; set; }
}
public class XYAttributes
{
	public float XCOORD { get; set; }
	public float YCOORD { get; set; }
}
public class XYField
{
	public string name { get; set; }
	public string alias { get; set; }
	public string type { get; set; }
}

// -------------------------------------------------
// Classes for individual county council districts
// -------------------------------------------------
public class indivField
{
	public string name { get; set; }
	public string type { get; set; }
	public int length { get; set; }
}

public class indivAttributes
{
	public string name { get; set; }
	public int DISTRICT { get; set; }
	public string Party { get; set; }
	public string WebSite { get; set; }
}

public class indivFeature
{
	public indivAttributes attributes { get; set; }
}

public class indivRootObject
{
	public string displayFieldName { get; set; }
	public List<indivField> fields { get; set; }
	public List<indivFeature> features { get; set; }
}

// ------------------------------------------------
// Classes for county council at-large districts
// ------------------------------------------------
public class alField
{
	public string name { get; set; }
	public string type { get; set; }
	public int length { get; set; }
}

public class alAttributes
{
	public string Name { get; set; }
	public int DISTRICT { get; set; }
	public string Party { get; set; }
	public string WebSite { get; set; }
}

public class alFeature
{
	public alAttributes attributes { get; set; }
}

public class alRootObject
{
	public string displayFieldName { get; set; }
	public List<alField> fields { get; set; }
	public List<alFeature> features { get; set; }
}

// ------------------------------------------------
// Classes for state elected officials
// ------------------------------------------------
public class stateField
{
	public string name { get; set; }
	public string type { get; set; }
	public string alias { get; set; }
	public int? length { get; set; }
}

public class stateAttrib
{
	public string REPNAME { get; set; }
	public double DISTRICT { get; set; }
	public string Party { get; set; }
	public string WebSite { get; set; }
}

public class stateFeature
{
	public stateAttrib attributes { get; set; }
}

public class stateRootObject
{
	public string displayFieldName { get; set; }
	public List<stateField> fields { get; set; }
	public List<stateFeature> features { get; set; }
}

// ------------------------------------------------
// Classes for Federal elected officials
// ------------------------------------------------
public class federalField
{
	public string name { get; set; }
	public string type { get; set; }
	public string alias { get; set; }
	public int? length { get; set; }
}

public class federalAttrib
{
	public string REPNAME { get; set; }
	public double DISTRICT { get; set; }
	public string Party { get; set; }
	public string WebSite { get; set; }
}

public class federalFeature
{
	public federalAttrib attributes { get; set; }
}

public class federalRootObject
{
	public string displayFieldName { get; set; }
	public List<federalField> fields { get; set; }
	public List<federalFeature> features { get; set; }
}

//-------------------------------------------------------------
// ----- Classes for CID's, TIF districts and projects --------
// ------------------------------------------------------------
public class EconDevField
{
	public string name { get; set; }
	public string type { get; set; }
	public string alias { get; set; }
	public int length { get; set; }
}
public class EconDevAttributes
{
	public string EFFECTIVEYEARFROM { get; set; }
	public string EFFECTIVEYEARTO { get; set; }
	public int DURATION { get; set; }
	public string Name { get; set; }
	public string TIFDISTRICT { get; set; }
}
public class EconDevFeature
{
	public EconDevAttributes attributes { get; set; }
}
public class EconDevRootObject
{
	public List<EconDevField> fields { get; set; }
	public List<EconDevFeature> features { get; set; }
}

//-------------------------------------------------------------
// Classes for Informal and BOE appeals
// ------------------------------------------------------------
public class AppealsRootobject
{
	public string objectIdFieldName { get; set; }
	public AppealsUniqueidfield uniqueIdField { get; set; }
	public string globalIdFieldName { get; set; }
	public string geometryType { get; set; }
	public AppealsSpatialreference spatialReference { get; set; }
	public AppealsField[] fields { get; set; }
	public AppealsFeature[] features { get; set; }
}
public class AppealsUniqueidfield
{
	public string name { get; set; }
	public bool isSystemMaintained { get; set; }
}
public class AppealsSpatialreference
{
	public int wkid { get; set; }
	public int latestWkid { get; set; }
}
public class AppealsField
{
	public string name { get; set; }
	public string type { get; set; }
	public string alias { get; set; }
	public string sqlType { get; set; }
	public int length { get; set; }
	public AppealsDomain domain { get; set; }
	public object defaultValue { get; set; }
}
public class AppealsDomain
{
	public string type { get; set; }
	public string name { get; set; }
	public AppealsCodedvalue[] codedValues { get; set; }
}
public class AppealsCodedvalue
{
	public string name { get; set; }
	public string code { get; set; }
}
public class AppealsFeature
{
	public AppealsAttributes attributes { get; set; }
}
public class AppealsAttributes
{
	public string appeal_type { get; set; }
	public string email { get; set; }
}

// ------------------------------------------------
// Classes for Lat and Long needed in appeals forms
// -----------------------------------------------
public class Geometry
{
	public double x { get; set; }
	public double y { get; set; }
}
public class LatLongRootObject
{
	public List<Geometry> geometries { get; set; }
}