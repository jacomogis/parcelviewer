/*------------------------------------------------------------
 * THIS FILE CONTAINS THE AJAX FUNCTIONS CALLED FROM Script.js
 -------------------------------------------------------------*/

function getCoords()
{
	$.ajax({
		type: "POST",
		url: "WebMap1.aspx/GetCoords",
		data: "{ 'parcelnum': '" + parcelnum + "' }",
		contentType: "application/json; charset=utf-8",
		dataType: "json",
		async: false,
		success:
			function (jsonobject)
			{
				xcoord = jsonobject.d[0];
				ycoord = jsonobject.d[1];
			}
	});
}

function getBasicInfo()
{
	/*
	items[0] - lblParcelNum -- also used in BOE form
	items[1] - lblSitusAddr -- also used in BOE form
	items[2] - lblSitusCityStateZip
	items[3] - lblTCA -- also used in BOE form
	items[4] - lblusecode -- also used in BOE form
	items[5] - lblLotSize -- also used in BOE form
	items[6] - lblBldgSqFt -- also used in BOE form
	items[7] - lblNumBR -- N/A for non-residential -- also used in BOE form
	items[8] - lblNumBaths -- N/A for non-residential -- also used in BOE form
	items[9] - lblYearBuilt -- also used in BOE form
	items[10] - nhood -- used only in BOE form
	*/
	$.ajax({
		type: "POST",
		url: "WebMap1.aspx/GetBasicInfo",
		data: "{ 'rawparcelnum': '" + parcelnum + "' }",
		contentType: "application/json; charset=utf-8",
		dataType: "json",
		async: false,
		success:
			function (jsonobject)
			{
				$('#lblParcelNum').text(jsonobject.d[0]);
				$('#lblSitusAddr').text(jsonobject.d[1]);
				situs_address = jsonobject.d[1] + ", " + jsonobject.d[2];  // street addr + city-state-zip for BOE appeal form
				$('#lblSitusCityStateZip').text(jsonobject.d[2]);
				$('#lblTCA').text(jsonobject.d[3]);
				TCA = jsonobject.d[3]; // for BOE appeal form
				$('#lblusecode').text(jsonobject.d[4]);
				landusecode = jsonobject.d[4].replace("%", " percent "); // for BOE appeal form
				if(jsonobject.d[5] == null)
				{
					$('#lblLotSize').text("N/A");
					lot_sqft = "N/A"; // for BOE appeal form ???????
				}
				else
				{
					$('#lblLotSize').text(jsonobject.d[5] + " sq. ft.");
					lot_sqft = jsonobject.d[5]+ " sq. ft."; // for BOE appeal form ???????
				}
				if(jsonobject.d[6] == null)
				{
					$('#lblBldgSqFt').text("N/A");
					bldg_sqft = 0.0; // for BOE appeal form
				}
				else
				{
					$('#lblBldgSqFt').text(jsonobject.d[6] + " sq. ft.");
					var str = jsonobject.d[6];
					var rep = str.replace(",", "");	
					bldg_sqft = parseFloat(rep); // for BOE appeal form
				}
				if(jsonobject.d[7] == null)
				{
					$('#lblNumBR').text("N/A");
					num_beds = 0.0; // for BOE appeal form
				}
				else
				{
					$('#lblNumBR').text(jsonobject.d[7]);
					num_beds = parseFloat(jsonobject.d[7]); // for BOE appeal form
				}
				if(jsonobject.d[8] == null)
				{
					$('#lblNumBaths').text("N/A");
					num_baths = 0.0; // for BOE appeal form
				}
				else
				{
					$('#lblNumBaths').text(jsonobject.d[8]);
					num_baths = parseFloat(jsonobject.d[8]); // for BOE appeal form
				}
				if(jsonobject.d[9] == null)
				{
					$('#lblYearBuilt').text("N/A");
					//year_built = "0"; // for BOE appeal form
					year_built = new Date("2000-01-01"); // for BOE appeal form
				}
				else
				{
					$('#lblYearBuilt').text(jsonobject.d[9]);
					year_built = new Date(jsonobject.d[9] + "-01-01"); // for BOE appeal form
				}
				nhood = jsonobject.d[10]; // for BOE appeal form
			},
		failure: function(response) { alert("Basic info failure: " + response.d); }
	});
}

function getExemptionAndLegal()
{
	$.ajax({
		type: "POST",
		url: "WebMap1.aspx/GetExemptionAndLegal",
		data: "{ 'parcelnum': '" + parcelnum + "' }",
		contentType: "application/json; charset=utf-8",
		dataType: "json",
		async: false,
		success:
			function (jsonobject)
			{
				$('#lblExemption').text(jsonobject.d[0]);
				exemption = jsonobject.d[0].replace("%", " percent "); // for BOE appeal form
				$('#lblLegalDescr').text(jsonobject.d[1]);
				legaldescription = jsonobject.d[1].replace("%", " percent "); // for BOE appeal form
			},
		failure: function(response) { alert("Ascend legal failure: " + response.d); }
	});
}
	
function getValues()
{
	$.ajax({
		type: "POST",
		url: "WebMap1.aspx/GetValues",
		data: "{ 'parcelnum': '" + parcelnum + "' }",
		contentType: "application/json; charset=utf-8",
		dataType: "json",
		async: false,
		success:
			function (jsonobject)
			{
		  		$('#lblYear0').text(jsonobject.d[0]);
		  		$('#lblYear1').text(jsonobject.d[1]);
		  		$('#lblYear2').text(jsonobject.d[2]);
		  		$('#lblYear3').text(jsonobject.d[3]);

				/////////////// YEAR0 ITEMS ///////////////
				$('#lblYear0AgLand').text(jsonobject.d[4]);
				$('#lblYear0CommLand').text(jsonobject.d[5]);
				$('#lblYear0ResLand').text(jsonobject.d[6]);
				$('#lblYear0AgImp').text(jsonobject.d[7]);
				$('#lblYear0CommImp').text(jsonobject.d[8]);
				$('#lblYear0ResImp').text(jsonobject.d[9]);
				$('#lblYear0AgNC').text(jsonobject.d[10]);
				$('#lblYear0CommNC').text(jsonobject.d[11]);
				$('#lblYear0ResNC').text(jsonobject.d[12]);
				$('#lblYear0TMV').text(jsonobject.d[13]);
				$('#lblYear0TAV').text(jsonobject.d[14]);
				$('#lblYear0TTV').text(jsonobject.d[15]);			
				
				////////// THIS NEXT SECTION IS FOR BOE APPEAL VALUES /////////////
		  		if(jsonobject.d[4] != "") // Year0 agricultural
				{
					var str4 = jsonobject.d[4];
					var rep4 = str4.replace(",", "");					
					land_ag_val = parseFloat(rep4); // for BOE appeal form
					var str7 = jsonobject.d[7];
					var rep7 = str7.replace(",", "");					
					imp_ag_val = parseFloat(rep7); // for BOE appeal form
					if(jsonobject.d[10] == "")
					{
						newcon_ag_val = 0.0; // for BOE appeal form
					}
					else
					{
						var str10 = jsonobject.d[10];
						var rep10 = str10.replace(",", "");
						newcon_ag_val = parseFloat(rep10); // for BOE appeal form
					}
				}
				else
				{
					land_ag_val = 0.0; // for BOE appeal form
					imp_ag_val = 0.0; // for BOE appeal form
					newcon_ag_val = 0.0; // for BOE appeal form
				}
		  		if(jsonobject.d[5] != "") // Year0 commercial
				{
					var str5 = jsonobject.d[5];
					var rep5 = str5.replace(",", "");
					land_com_val = parseFloat(rep5); // for BOE appeal form
					var str8 = jsonobject.d[8];
					var rep8 = str8.replace(",", "");
					imp_com_val = parseFloat(rep8); // for BOE appeal form
					if(jsonobject.d[11] == "")
					{
						newcon_com_val = 0.0; // for BOE appeal form
					}
					else
					{
						var str11 = jsonobject.d[11];
						var rep11 = str11.replace(",", "");
						newcon_com_val = parseFloat(rep11); // for BOE appeal form
					}
				}
				else
				{
					land_com_val = 0.0; // for BOE appeal form
					imp_com_val = 0.0; // for BOE appeal form
					newcon_com_val = 0.0; // for BOE appeal form
				}
		  		if(jsonobject.d[6] != "") // Year0 residential
				{
					var str6 = jsonobject.d[6];
					var rep6 = str6.replace(",", "");				
					land_res_val = parseFloat(rep6); // for BOE appeal form
					var str9 = jsonobject.d[9];
					var rep9 = str9.replace(",", "");				
					imp_res_val = parseFloat(rep9); // for BOE appeal form
					if(jsonobject.d[12] == "")
					{
						newcon_res_val = 0.0; // for BOE appeal form
					}
					else
					{
						var str12 = jsonobject.d[12];
						var rep12 = str12.replace(",", "");
						newcon_res_val = parseFloat(rep12); // for BOE appeal form
					}
				}
				else
				{
					land_res_val = 0.0; // for BOE appeal form
					imp_res_val = 0.0; // for BOE appeal form
					newcon_res_val = 0.0; // for BOE appeal form
				}
				var str13 = jsonobject.d[13];
				var rep13 = str13.replace(",", "");
				TMV = parseFloat(rep13); // for BOE appeal form
				var str14 = jsonobject.d[14];
				var rep14 = str14.replace(",", "");
				TAV = parseFloat(rep14); // for BOE appeal form
				var str15 = jsonobject.d[15];
				var rep15 = str15.replace(",", "");
				TTV = parseFloat(rep15); // for BOE appeal form
				/////////// END OF BOE SECTION //////////////////////

				/////////////// YEAR1 ITEMS ///////////////
				$('#lblYear1AgLand').text(jsonobject.d[16]);
				$('#lblYear1CommLand').text(jsonobject.d[17]);
				$('#lblYear1ResLand').text(jsonobject.d[18]);
				$('#lblYear1AgImp').text(jsonobject.d[19]);
				$('#lblYear1CommImp').text(jsonobject.d[20]);
				$('#lblYear1ResImp').text(jsonobject.d[21]);
				$('#lblYear1AgNC').text(jsonobject.d[22]);
				$('#lblYear1CommNC').text(jsonobject.d[23]);
				$('#lblYear1ResNC').text(jsonobject.d[24]);
				$('#lblYear1TMV').text(jsonobject.d[25]);
				$('#lblYear1TAV').text(jsonobject.d[26]);
				$('#lblYear1TTV').text(jsonobject.d[27]);

				/////////////// YEAR2 ITEMS ///////////////
				$('#lblYear2AgLand').text(jsonobject.d[28]);
				$('#lblYear2CommLand').text(jsonobject.d[29]);
				$('#lblYear2ResLand').text(jsonobject.d[30]);
				$('#lblYear2AgImp').text(jsonobject.d[31]);
				$('#lblYear2CommImp').text(jsonobject.d[32]);
				$('#lblYear2ResImp').text(jsonobject.d[33]);
				$('#lblYear2AgNC').text(jsonobject.d[34]);
				$('#lblYear2CommNC').text(jsonobject.d[35]);
				$('#lblYear2ResNC').text(jsonobject.d[36]);
				$('#lblYear2TMV').text(jsonobject.d[37]);
				$('#lblYear2TAV').text(jsonobject.d[38]);
				$('#lblYear2TTV').text(jsonobject.d[39]);
				
				/////////////// YEAR3 ITEMS ///////////////
				$('#lblYear3AgLand').text(jsonobject.d[40]);
				$('#lblYear3CommLand').text(jsonobject.d[41]);
				$('#lblYear3ResLand').text(jsonobject.d[42]);
				$('#lblYear3AgImp').text(jsonobject.d[43]);
				$('#lblYear3CommImp').text(jsonobject.d[44]);
				$('#lblYear3ResImp').text(jsonobject.d[45]);
				$('#lblYear3AgNC').text(jsonobject.d[46]);
				$('#lblYear3CommNC').text(jsonobject.d[47]);
				$('#lblYear3ResNC').text(jsonobject.d[48]);
				$('#lblYear3TMV').text(jsonobject.d[49]);
				$('#lblYear3TAV').text(jsonobject.d[50]);
				$('#lblYear3TTV').text(jsonobject.d[51]);
			}
	});
}
	
function getOwnerInfo()
{
	var ownercount;
	var ownernames = [];
	var owneraddresses = [];
	var ownercitiesstateszipscountries = [];

	// get the # of owners
	$.ajax({
		type: "POST",
		url: "WebMap1.aspx/ReturnOwnerCount",
		data: "{'parcelnum':'" + parcelnum + "'}",
		contentType: "application/json; charset=utf-8",
		dataType: "json",
		async: false,
		success:
			function (jsonobject)
			{
				ownercount = jsonobject.d;
			},
		failure: function(response) { alert("Owner names failure: " + response.d); }
	});

	// get the names
	$.ajax({
		type: "POST",
		url: "WebMap1.aspx/GetOwnerNames",
		data: "{'parcelnum':'" + parcelnum + "'}",
		contentType: "application/json; charset=utf-8",
		dataType: "json",
		async: false,
		success:
			function (jsonobject)
			{
				for(var i = 0; i < ownercount; i++)
				{
					ownernames[i] = jsonobject.d[i];
				}
				owner = ownernames[0]; // for BOE appeal forms
				$('#lblowner1name').text(ownernames[0]);
				if(ownercount > 1) $('#lblowner2name').text(ownernames[1]);
				if(ownercount > 2)
				{
					for(j=2; j<ownercount; j++)
					{
						if(j==2) document.getElementById("otherownerslist").innerHTML = ownernames[j] + "<br/>";
						else document.getElementById("otherownerslist").innerHTML += ownernames[j] + "<br/>";
					}
				}
			},
		failure: function(response) { alert("Owner names failure: " + response.d); }
	});

	// get the addresses
	$.ajax({
		type: "POST",
		url: "WebMap1.aspx/GetOwnerAddresses",
		data: "{'parcelnum':'" + parcelnum + "'}",
		contentType: "application/json; charset=utf-8",
		dataType: "json",
		async: false,
		success:
			function (jsonobject)
			{
				for(var i = 0; i < ownercount; i++)
				{
					owneraddresses[i] = jsonobject.d[i];
				}
				$('#lblowner1address').text(owneraddresses[0]);
				if(ownercount > 1) $('#lblowner2address').text(owneraddresses[1]);
			},
		failure: function(response) { alert("Owner addresses failure: " + response.d); }
	});

	// get the city, state, zip
	$.ajax({
		type: "POST",
		url: "WebMap1.aspx/GetOwnerCitiesStatesZipsCountries",
		data: "{'parcelnum':'" + parcelnum + "'}",
		contentType: "application/json; charset=utf-8",
		dataType: "json",
		async: false,
		success:
			function (jsonobject)
			{
				for(var i = 0; i < ownercount; i++)
				{
					ownercitiesstateszipscountries[i] = jsonobject.d[i];
				}
				$('#lblowner1citystatezipcountry').text(ownercitiesstateszipscountries[0]);
				if(ownercount > 1) $('#lblowner2citystatezipcountry').text(ownercitiesstateszipscountries[1]);
			},
		failure: function(response) { alert("Owner cities/states/cities/zips/countries failure: " + response.d); }
	});

	if(ownercount == 1)
	{
		document.getElementById("firstowner").style.display = "block";
		document.getElementById("secondowner").style.display = "none";
		document.getElementById("otherowners").style.display = "none";
	}
	else if(ownercount == 2)
	{
		document.getElementById("firstowner").style.display = "block";
		document.getElementById("secondowner").style.display = "block";
		document.getElementById("otherowners").style.display = "none";
	}
	else // ownercount > 2
	{
		document.getElementById("firstowner").style.display = "block";
		document.getElementById("secondowner").style.display = "block";
		document.getElementById("otherowners").style.display = "block";
	}
} // end of getOwnerInfo function

function getEconDistricts()
{
	$.ajax({
		type: "POST",
		url: "WebMap1.aspx/GetEconDistricts",
		data: "{'parcelnum': '" + parcelnum + "','X':'" + xcoord + "','Y':'" + ycoord + "'}",
		contentType: "application/json; charset=utf-8",
		dataType: "json",
		async: false,
		success:
			function (jsonobject)
			{
				$('#lblTIFdist').text(jsonobject.d[0]); // TIF District name
				$('#lblTIFdistStartDate').text(jsonobject.d[1]); // TIF District start date
				$('#lblTIFdistEndDate').text(jsonobject.d[2]); // TIF District end date
				$('#lblTIFdistDuration').text(jsonobject.d[3]); // TIF District duration in years
				$('#lblTIFprojOrd').text(jsonobject.d[4]); // TIF Ordinance #
				$('#lblTIFprojStartDate').text(jsonobject.d[5]); // TIF Project start date
				$('#lblTIFprojEndDate').text(jsonobject.d[6]); // TIF Project end date
				$('#lblTIFprojDuration').text(jsonobject.d[7]); // TIF Project duration in years
				$('#lbl353descr').text(jsonobject.d[8]); // 353 description, if any
				$('#lbl353fromYr').text(jsonobject.d[9]); // 353 abatement from year
				$('#lbl353toYr').text(jsonobject.d[10]); // 353 abatement to year
				$('#lbl99descr').text(jsonobject.d[11]); // 99 description, if any
				$('#lbl99fromYr').text(jsonobject.d[12]); // 99 abatement from year
				$('#lbl99toYr').text(jsonobject.d[13]); // 99 abatement to year
				$('#lblCID').text(jsonobject.d[14]); // Community Improvement District name
			},
		failure: function(response) { alert("Econ Districts failure: " + response.d); }
	});
}

function checkAppeals()
{
	$.ajax({
		type: "POST",
		url: "WebMap1.aspx/CheckAppeals",
		data: "{ 'parcelnum': '" + parcelnum + "' }",
		contentType: "application/json; charset=utf-8",
		dataType: "json",
		async: false,
		success:
			function (jsonobject)
			{
				
				// if there is an informal they can still have a formal but not another informal, so make formal button active but not informal
				// if there is a formal they cannot have an informal nor another formal, so make both buttons inactive
				// if there is neither make both buttons active
				if(jsonobject.d == "formal")
				{
					//lblInformal.style.display = "none";
					//btnInformalAppeal.style.display = "none";
					lblBOE.style.display = "none";
					btnBOEAppeal.style.display = "none";
					$('#lblAppealsMsg').text("There is already an active FORMAL B.O.E. appeal on this property; if this is an error please contact the office of the Board of Equalization at 816-881-3309 or email to: boardofequalization@jacksongov.org immediately");
				}
				else if(jsonobject.d == "informal")
				{
					//btnInformalAppeal.style.display = "none";
					//lblInformal.style.display = "none";
					lblBOE.style.display = "block";
					btnBOEAppeal.style.display = "block";
					$('#lblAppealsMsg').text("There is already an active INFORMAL REVIEW on this property");
				}
				else
				{
					//lblInformal.style.display = "block";
					//btnInformalAppeal.style.display = "block";
					lblBOE.style.display = "block";
					btnBOEAppeal.style.display = "block";
					$('#lblAppealsMsg').text(jsonobject.d);
				}
			},
		failure: function(response) { alert("Appeals failure: " + response.d); }
	});
}

function getPhotos()
{
	$.ajax({
		type: "POST",
		url: "WebMap1.aspx/GetPhotos",
		data: "{ 'parcelnum': '" + parcelnum + "' }",
		contentType: "application/json; charset=utf-8",
		dataType: "json",
		async: false,
		success:
			function (jsonobject)
			{
				photoCount = jsonobject.d.length;
				if (photoCount == 0) document.getElementById("photos").innerHTML = "No photos available for this parcel";
				else
				{
					for(var i = 0; i < photoCount; i++)
					{
						photoURLs[i] = jsonobject.d[i];
						if (i == 0) document.getElementById("photos").innerHTML = "<img src='" + photoURLs[i] + "' onclick='doModal(this.src);'/>";
						else document.getElementById("photos").innerHTML += "&nbsp&nbsp<img src='" + photoURLs[i] + "' onclick='doModal(this.src);'/>";
					}
				}
			},
		failure: function(response) { alert("Photos failure: " + response.d); }
	});
}
// This is called from getPhotos above so put it here even though it's not an Ajax function
function doModal(imgsrc)
{
	theModal.style.display = "block";
	modalPhotos.src = imgsrc;
}
		
function getElectedOfficials()
{
	$.ajax({
		type: "POST",
		url: "WebMap1.aspx/GetElectedOfficials",
		data: "{'X':'" + xcoord + "','Y':'" + ycoord + "'}",
		contentType: "application/json; charset=utf-8",
		dataType: "json",
		async: false,
		success:
			function (jsonobject)
			{
				// These are elected officials common to the entire county and can be hard-coded here
				// ----------------------------------------------------------------------------------
				$('#lblJaCoExecName').text("Frank White");
				$('#lblJaCoExecParty').text("D");
				document.getElementById("hlJaCoExecWebsite").innerHTML = "<a href='http://www.jacksongov.org/395/County-Executive' target='_blank' style='color:blue;margin:0;padding:0'>View Website</a>";
				$('#lblMOgovName').text("Michael L Parson");
				$('#lblMOgovParty').text("R");
				document.getElementById("hlMOgovWebsite").innerHTML = "<a href='https://governor.mo.gov/' target='_blank' style='color:blue;margin:0;padding:0'>View Website</a>";
				$('#lblUSsen1Name').text("Roy Blunt");
				$('#lblUSsen1Party').text("R");
				document.getElementById("hlUSsen1Website").innerHTML = "<a href='http://blunt.senate.gov/public/' target='_blank' style='color:blue;margin:0;padding:0'>View Website</a>";
				$('#lblUSsen2Name').text("Josh Hawley");
				$('#lblUSsen2Party').text("R");
				document.getElementById("hlUSsen2Website").innerHTML = "<a href='https://www.hawley.senate.gov/' target='_blank' style='color:blue;margin:0;padding:0'>View Website</a>";
				// ----------------------------------------------------------------------------------

				// Now the variable ones ...
				// County individual districts
				$('#lblJaCoIndivName').text(jsonobject.d[0]);
				$('#lblJaCoIndivParty').text(jsonobject.d[1]);
				$('#lblJaCoIndivDist').text(jsonobject.d[2]);
				if(jsonobject.d[3] == "" || jsonobject.d[3] == null || jsonobject.d[3] == " "|| jsonobject.d[3] == "N/A") document.getElementById("hlJaCoIndivWebsite").innerHTML = "Website N/A";
				else document.getElementById("hlJaCoIndivWebsite").innerHTML = "<a href='" + jsonobject.d[3] + "' target='_blank' style='color:blue;margin:0;padding:0'>View Website</a>";
				// County at-large districts
				$('#lblJaCoAtLargeName').text(jsonobject.d[4]);
				$('#lblJaCoAtLargeParty').text(jsonobject.d[5]);
				$('#lblJaCoAtLargeDist').text(jsonobject.d[6]);
				if(jsonobject.d[7] == "" || jsonobject.d[7] == null || jsonobject.d[7] == " "|| jsonobject.d[7] == "N/A") document.getElementById("hlJaCoAtLargeWebsite").innerHTML = "Website N/A";
				else document.getElementById("hlJaCoAtLargeWebsite").innerHTML = "<a href='" + jsonobject.d[7] + "' target='_blank' style='color:blue'>View Website</a>";
				// State senators
				$('#lblMOsenName').text(jsonobject.d[8]);
				$('#lblMOsenParty').text(jsonobject.d[9]);
				$('#lblMOsenDist').text(jsonobject.d[10]);
				if(jsonobject.d[11] == "" || jsonobject.d[11] == null || jsonobject.d[11] == " "|| jsonobject.d[11] == "N/A") document.getElementById("hlMOsenWebsite").innerHTML = "Website N/A";
				else document.getElementById("hlMOsenWebsite").innerHTML = "<a href='" + jsonobject.d[11] + "' target='_blank' style='color:blue'>View Website</a>";
				// State representatives
				$('#lblMOrepName').text(jsonobject.d[12]);
				$('#lblMOrepParty').text(jsonobject.d[13]);
				$('#lblMOrepDist').text(jsonobject.d[14]);
				if(jsonobject.d[15] == "" || jsonobject.d[15] == null || jsonobject.d[15] == " "|| jsonobject.d[15] == "N/A") document.getElementById("hlMOrepWebsite").innerHTML = "Website N/A";
				else document.getElementById("hlMOrepWebsite").innerHTML = "<a href='" + jsonobject.d[15] + "' target='_blank' style='color:blue'>View Website</a>";
				// Federal (US House)
				$('#lblUSHouseName').text(jsonobject.d[16]);
				$('#lblUSHouseParty').text(jsonobject.d[17]);
				$('#lblUSHouseDist').text(jsonobject.d[18]);
				if(jsonobject.d[19] == "" || jsonobject.d[19] == null || jsonobject.d[19] == " "|| jsonobject.d[19] == "N/A") document.getElementById("hlUSHouseWebsite").innerHTML = "Website N/A";
				else document.getElementById("hlUSHouseWebsite").innerHTML = "<a href='" + jsonobject.d[19] + "' target='_blank' style='color:blue'>View Website</a>";
			},
		failure: function(response) { alert("Elected officials failure: " + response.d); }
	});
}

// This is called when the user clicks on one of the buttons to do a BOE appeal
function doAppeal(appealtype)
{
	var lat, lon;
			
	$.ajax({
		type: "POST",
		url: "WebMap1.aspx/GetLatLong",
		data: "{'X':'" + xcoord + "','Y':'" + ycoord + "'}",
		contentType: "application/json; charset=utf-8",
		dataType: "json",
		async: false,
		success:
			function (jsonobject)
			{
				lat = jsonobject.d[0];
				lon = jsonobject.d[1];
				if(appealtype == "formal")
					window.open("https://survey123.arcgis.com/share/717d975e6b914f53b2a0fa7c546331a7?" + "field:situs_or_location_address=" + situs_address + 
						"&field:parcel_number=" + parcelnum + "&field:tax2=" + legaldescription + "&field:ownerappellant=" + owner + 
						"&field:prop_use_jan=" + landusecode + "&field:tca=" + TCA + "&field:nbhd=" + nhood + "&field:exemption=" + exemption + "&field:ascend_bldg_sqft=" + bldg_sqft + 
						"&field:ascend_num_beds=" + num_beds + "&field:ascend_num_baths=" + num_baths +  "&field:bv_ag_lnd=" + land_ag_val + 
						"&field:bv_com_lnd=" + land_com_val + "&field:bv_res_lnd=" + land_res_val + "&field:bv_ag_imp=" + imp_ag_val + "&field:bv_com_imp=" + imp_com_val + 
						"&field:bv_res_imp=" + imp_res_val + "&field:bv_ag_nc=" + newcon_ag_val + "&field:bv_com_nc=" + newcon_com_val + "&field:bv_res_nc=" + newcon_res_val + 
						"&field:bv_tm=" + TMV + "&field:bv_ta=" + TAV + "&field:bv_tt=" + TTV + "&field:ascend_year_built=" + year_built + "&center=" + lon + "," + lat, '_self');
				/*else //appealtype == "informal"
					window.open("https://survey123.arcgis.com/share/5d04b51f6a4748568e64cc86c1462183?" + "field:situs_or_location_address=" + situs_address + 
						"&field:parcel_number=" + parcelnum + "&field:tax2=" + legaldescription + "&field:ownerappellant=" + owner + 
						"&field:prop_use_jan=" + landusecode + "&field:tca=" + TCA + "&field:nbhd=" + nhood + "&field:exemption=" + exemption + "&field:ascend_bldg_sqft=" + bldg_sqft + 
						"&field:ascend_num_beds=" + num_beds + "&field:ascend_num_baths=" + num_baths +  "&field:bv_ag_lnd=" + land_ag_val + 
						"&field:bv_com_lnd=" + land_com_val + "&field:bv_res_lnd=" + land_res_val + "&field:bv_ag_imp=" + imp_ag_val + "&field:bv_com_imp=" + imp_com_val + 
						"&field:bv_res_imp=" + imp_res_val + "&field:bv_ag_nc=" + newcon_ag_val + "&field:bv_com_nc=" + newcon_com_val + "&field:bv_res_nc=" + newcon_res_val + 
						"&field:bv_tm=" + TMV + "&field:bv_ta=" + TAV + "&field:bv_tt=" + TTV + "&field:ascend_year_built=" + year_built + "&center=" + lon + "," + lat, '_self');*/
			},
		failure: function(response) { alert("Lat Long failure: " + response.d); }
	});
}