<%@ Page Language="C#" AutoEventWireup="true" CodeFile="WebMap1.aspx.cs" Inherits="WebMap1" %>
<html>
<head>
	<title>Jackson County Missouri Parcel Viewer</title>
	<meta charset="utf-8">
	<meta name="viewport" content="width=device-width, initial-scale=1">
	<link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/4.4.1/css/bootstrap.min.css">
	<script src="https://ajax.googleapis.com/ajax/libs/jquery/3.4.1/jquery.min.js"></script>
	<script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.16.0/umd/popper.min.js"></script>
	<script src="https://maxcdn.bootstrapcdn.com/bootstrap/4.4.1/js/bootstrap.min.js"></script>

	<link rel="stylesheet" href="//code.jquery.com/ui/1.12.1/themes/smoothness/jquery-ui.css">
	<script src="//code.jquery.com/jquery-1.12.4.js"></script>
	<script src="//code.jquery.com/ui/1.12.1/jquery-ui.js"></script>

	<link rel="stylesheet" href="https://js.arcgis.com/4.13/esri/css/main.css"/>
	<script src="https://js.arcgis.com/4.13/"></script>

	<script type="text/javascript">
		// global variables - need these in main web page
		var parcelnum, theMap, theView, situs_address, legaldescription, owner, landusecode, xcoord, ycoord, photoCount, searchWidget, parcelsLayerquery, lot_sqft;
		var photoURLs = [];
		// for appeals forms ... misc basic info variables not already in list above ...
		var bldg_sqft, num_beds, num_baths, year_built, TCA, exemption, nhood;
		// ... and property values variables (for Year0)
		var land_ag_val, land_com_value, land_res_value, imp_ag_val, imp_com_value, imp_res_value, newcon_ag_val, newcon_com_value, newcon_res_value, TMV, TAV, TTV;
	</script>

	<!-- style sheet and javascript for app -->
	<link rel="stylesheet" href="Style.css?v=2"/>
	<script src="Ajax.js?v=8"></script>
	<script src="Script.js?v=20"></script>

</head>

<body>
	<!-- Help Container -->
	<!-- LEAVE THIS ALONE IT'S NOT GOING TO GET BETTER -->
	<div id="helpContainer" style="position:absolute; width:100%; height:100%; display:none">
		<div class="splashscreenoverlay d-flex align-items-center min-vh-100 splashbackground"></div>
		<div class="container" id="helpscreen">
			<div class="row" style="margin-top:3%; margin-left:3%">
				<div class="col-xl-3"><button class="btn btn-primary" onclick="openVideo()">Quick Start Video</button></div><div class="col-xl-9"></div>
			</div>
			<div class="row" id="helpimage">
				 <img style="max-height:100%; max-width:100%" src="images/helpdoc.png"/>
			</div>
			<div class="row">
				<div class="col-xl-10"></div><div class="col-xl-2"><button type="button" class="btn btn-primary" onclick="closeMe('helpContainer');">Close</button></div>
			</div>
		</div>
	</div>

	<!-- Video Container -->
	<div id="videoContainer" style="position:absolute; width:100%; height:100%; display:none;">
		<div id="videoDiv">
			<video id="helpVideo" controls><source src="video/quickstartguidepv.mp4" type="video/mp4"></video>
		</div>
		<div id="videobackground" style="position:absolute; width:100%; height:100%;background-color:black;z-index:3;">
			<button onclick="closeMe('videoContainer')" id="videoClose" class="btn btn-primary">CLOSE</button>
		</div>
	</div>

	<!-- Splash Container -->
	<div id="splashContainer" style="position:absolute; width:100%; height:100%;">
		<div class="splashscreenoverlay d-flex align-items-center min-vh-100 splashbackground"></div>
		<div class="jumbotron container" id="splashscreen">
			<div class="row">
				<span style="width:100%;text-align:center;font-size:3vh"><b>Jackson County Parcel Viewer</b></span><p/>
				<div class="col-xl-3"><button class="btn btn-primary" id="quickstartbutton" onclick="openVideo()">Quick Start Video</button></div><div class="col-xl-9"></div>
			</div>
			<div class="row" id="splash-content">
				<div class="col-xl-12">
					<div class="row">
					<!--<span style="width:100%;text-align:center;color:red;font-size:large"><b>NOTICE:</b><br>
						Parcel Viewer is currently experiencing slow performance communicating with our Assessment database. Please be patient and wait for the information pane at the bottm to respond.<br>
						You may need to keep pressing the wait button on your browser.<br><br>
					</span>-->
					</div>
					<div class="row">
						<span style="width:100%;text-align:center;color:red;"><b>INTERNET EXPLORER WILL NOT BE SUPPORTED</b> - Please Use Edge, Chrome, Firefox</span>
					</div>
					<div class="row">
						<span style="width:100%;text-align:center;color:blue;"><b>Parcel Viewer is now mobile-friendly!</b></span>
					</div>
					<div class="row">
						<span style="width:100%;text-align:center;color:red;"><b>DISCLAIMER:</b></span>
					</div>
					<div class="row" id="disclaimer" style="font-size:small;text-align:left;">
						The information contained in this website is provided to the recipient pursuant to Section 610.011, RSMo.  Jackson County makes no warranties or representations of any kind, express or implied, regarding the information, data, or images provided herein.  All information, data, and service are provided "as is," and "with all faults" and by accessing this website, the recipient accepts the risk of any errors or omissions.  Jackson County is not liable in any way to the users of information, data, or service provided herein.  By accessing and using this information, data, or service, you agree to hold the County harmless in all matters and accounts arising from the use or reliance on the completeness or accuracy of this information, data, or service.
					<p>
					</div>
					<div class="row" style="font-size:small;text-align:left;">
						Please respond to our questionnaire about your experience with Parcel Viewer - click on the <img style="width: 3vh"src="images/questions.png">&nbsp icon to fill out the form.
					</div>
				</div>
			</div>
			<div class="row">
				<div class="col-xl-3"></div>
				<div class="col-xl-6" style="text-align:center"><button type="button" class="btn btn-primary btn-lg" onclick="closeMe('splashContainer');">I Agree</button></div>
				<div class="col-xl-3"></div>
			</div>
		</div> <!-- end of splashscreen jumbotron div -->
	</div> <!-- end of splash div -->

	<!-- div for modal photos popup -->
	<div id="theModal">
		<span id="modalClose" onclick="closeMe('theModal')";>&times;</span>
		<img id="modalPhotos"> <!-- this is *supposed* to be missing the 'src' tag - source is determined in the code -->
	</div>

	<!-- Measure Widget -->
	<div id="topbar">
		<button
			class="action-button esri-icon-measure-line"
			id="distanceButton"
			type="button"
			title="Measure distance between two or more points"
		></button>
		<button
			class="action-button esri-icon-measure-area"
			id="areaButton"
			type="button"
			title="Measure area"
		></button>
		<!-- Help and Questions buttons -->
		<div class="esri-widget--button" id="helpbutton" onclick="openHelp()"><span class="esri-icon-question"></span></div>
		<div class="esri-widget--button" id="questionsbutton" onclick="question()"><span class="esri-icon-comment"></span></div>
	</div>

	<!-- The map div -->
	<div class="container-fluid" id="mapDiv"></div>

	<!-- container for info panel at bottom -->
	<div id="container-all" class="resizable">
		<!-- this is the bar at the top of the info table used to re-size it -->
		<div id="container-topbar">
			<div id="container-handle" class="ui-resizable-handle ui-resizable-n"></div><div id="container-closebutton" onclick="closeMe('container-all')"><img src="images/close.png"/></div>
		</div>

		<!-- this is the row of tab buttons at the top of the table -->
		<div id="buttonscontainer" style="width:99%;">
			<ul class="nav nav-pills nav-justified">
				<li class="nav-item">
					<a class="nav-link active" id="basicvaluetab" data-toggle="pill" href="#basicvalueinfo">BASIC & VALUE INFORMATION</a>
				</li>
				<li class="nav-item">
					<a class="nav-link" id="ownertab" data-toggle="pill" href="#ownership">OWNERSHIP</a>
				</li>
				<li class="nav-item">
					<a class="nav-link" id="econtab" data-toggle="pill" href="#econdevel">ECON DEVELOPMENT</a>
				</li>
				<li class="nav-item">
					<a class="nav-link" id="photostab" data-toggle="pill" href="#photos">PHOTOS</a>
				</li>
				<li class="nav-item">
					<a class="nav-link" id="requeststab" data-toggle="pill" href="#requests">PROPERTY REQUESTS</a>
				</li>
				<!--<li class="nav-item">
					<a class="nav-link" id="electedtab" data-toggle="pill" href="#electedofficials">ELECTED OFFICIALS</a>
				</li>-->
				<li class="nav-item">
					<a class="nav-link" id="printsavetab" data-toggle="pill" href="#printsave">PRINT/SAVE</a>
				</li>
			</ul>
		</div>

		<div class="tab-content">
			<!-- tab for the basic and value information sections -->
			<div id="basicvalueinfo" class="container-fluid tab-pane active">
				<div class="row" style="height:94%;">
					<div id="basic" class="col-xl-3" style="background-color:white;height:94%;margin:1%;box-shadow: black 8px 6px 13px -6px;left:2%;overflow:auto;font-size:1em;">
						<div class="row"><div class="col-xl-12"><h1><b>Basic Information</b></h1></div></div>
						<div class="row"><div class="col-xl-12"><b>Parcel #</b></div></div>
						<div class="row"><div class="col-xl-1"></div><div class="col-xl-11" ID="lblParcelNum"></div></div>
						<div class="row"><div class="col-xl-12"><b>Address:</b></div></div>
						<div class="row"><div class="col-xl-1"></div><div class="col-xl-11" ID="lblSitusAddr"></div></div>
						<div class="row"><div class="col-xl-1"></div><div class="col-xl-11" ID="lblSitusCityStateZip"></div></div>
						<div class="row"><div class="col-xl-6"><b>Lot Size:</b></div><div class="col-xl-6" ID="lblLotSize"></div></div>
						<div class="row"><div class="col-xl-6"><b>Bldg Area:</b></div><div class="col-xl-6" ID="lblBldgSqFt"></div></div>
						<div class="row"><div class="col-xl-3"><b>#Beds:</b></div><div class="col-xl-3" ID="lblNumBR"></div><div class="col-xl-3"><b>#Baths:</b></div><div class="col-xl-3" ID="lblNumBaths"></div></div>
						<div class="row"><div class="col-xl-6"><b>Year Built:</b></div><div class="col-xl-6" ID="lblYearBuilt"></div></div>
						<div class="row"><div class="col-xl-6"><b>Tax Code Area:</b></div><div class="col-xl-6" ID="lblTCA"></div></div>
						<div class="row"><div class="col-xl-12"><b>Land Use Code:</b></div></div>
						<div class="row"><div class="col-xl-1"></div><div class="col-xl-11" ID="lblusecode"></div></div>
						<div class="row"><div class="col-xl-6"><b>Exemption:</b></div><div class="col-xl-6" ID="lblExemption"></div></div>
						<div class="row"><div class="col-xl-12"><b>Legal Description:</b></div></div>
						<div class="row"><div class="col-xl-1"></div><div class="col-xl-11" ID="lblLegalDescr"></div></div>
					</div> <!-- end of basic -->
					<div id="value" class="col-xl-8" style="background-color:white;height:94%;margin:1%;box-shadow:black 8px 6px 13px -6px;left:1%;overflow:auto;">
						<div class="row"><div class="col-xl-12"><h1><b>Property Values</b></h1></div></div>
						<div class="container">
							<div class="row">
								<div class="col-xl-3 border">
									<div class="row"><div class="col" id="lblYear0" style="font-weight:bold;font-size:medium;margin:5px 0px 4px 0px;"></div></div>
									<div class="row"><div class="col"><u>Land</u></div></div>
									<div class="row"><div class="col">Agricultural:</div><div class="col" ID="lblYear0AgLand"></div></div>
									<div class="row"><div class="col">Commercial:</div><div class="col" ID="lblYear0CommLand"></div></div>
									<div class="row"><div class="col">Residential:</div><div class="col" ID="lblYear0ResLand"></div></div>
									<div class="row"><div class="col"><u>Improvements</u></div></div>
									<div class="row"><div class="col">Agricultural:</div><div class="col" ID="lblYear0AgImp"></div></div>
									<div class="row"><div class="col">Commercial:</div><div class="col" ID="lblYear0CommImp"></div></div>
									<div class="row"><div class="col">Residential:</div><div class="col" ID="lblYear0ResImp"></div></div>
									<div class="row"><div class="col"><u>New Construction</u></div></div>
									<div class="row"><div class="col">Agricultural:</div><div class="col" ID="lblYear0AgNC"></div></div>
									<div class="row"><div class="col">Commercial:</div><div class="col" ID="lblYear0CommNC"></div></div>
									<div class="row"><div class="col">Residential:</div><div class="col" ID="lblYear0ResNC"></div></div>
									<hr style="height:2px;border-width:0;color:gray;background-color:gray">
									<div class="row"><div class="col">Total Market Value:</div><div class="col" ID="lblYear0TMV"></div></div>
									<div class="row"><div class="col">Total Assessed Value:</div><div class="col" ID="lblYear0TAV"></div></div>
									<div class="row"><div class="col">Total Taxable Value:</div><div class="col" ID="lblYear0TTV"></div></div>
								</div>
								<div class="col-xl-3 border">
									<div class="row"><div class="col" id="lblYear1" style="font-weight:bold;font-size:medium;margin:5px 0px 4px 0px;"></div></div>
									<div class="row"><div class="col"><u>Land</u></div></div>
									<div class="row"><div class="col">Agricultural:</div><div class="col" ID="lblYear1AgLand"></div></div>
									<div class="row"><div class="col">Commercial:</div><div class="col" ID="lblYear1CommLand"></div></div>
									<div class="row"><div class="col">Residential:</div><div class="col" ID="lblYear1ResLand"></div></div>
									<div class="row"><div class="col"><u>Improvements</u></div></div>
									<div class="row"><div class="col">Agricultural:</div><div class="col" ID="lblYear1AgImp"></div></div>
									<div class="row"><div class="col">Commercial:</div><div class="col" ID="lblYear1CommImp"></div></div>
									<div class="row"><div class="col">Residential:</div><div class="col" ID="lblYear1ResImp"></div></div>
									<div class="row"><div class="col"><u>New Construction</u></div></div>
									<div class="row"><div class="col">Agricultural:</div><div class="col" ID="lblYear1AgNC"></div></div>
									<div class="row"><div class="col">Commercial:</div><div class="col" ID="lblYear1CommNC"></div></div>
									<div class="row"><div class="col">Residential:</div><div class="col" ID="lblYear1ResNC"></div></div>
									<hr style="height:2px;border-width:0;color:gray;background-color:gray">
									<div class="row"><div class="col">Total Market Value:</div><div class="col" ID="lblYear1TMV"></div></div>
									<div class="row"><div class="col">Total Assessed Value:</div><div class="col" ID="lblYear1TAV"></div></div>
									<div class="row"><div class="col">Total Taxable Value:</div><div class="col" ID="lblYear1TTV"></div></div>
								</div>
								<div class="col-xl-3 border">
									<div class="row"><div class="col" id="lblYear2" style="font-weight:bold;font-size:medium;margin:5px 0px 4px 0px;"></div></div>
									<div class="row"><div class="col"><u>Land</u></div></div>
									<div class="row"><div class="col">Agricultural:</div><div class="col" ID="lblYear2AgLand"></div></div>
									<div class="row"><div class="col">Commercial:</div><div class="col" ID="lblYear2CommLand"></div></div>
									<div class="row"><div class="col">Residential:</div><div class="col" ID="lblYear2ResLand"></div></div>
									<div class="row"><div class="col"><u>Improvements</u></div></div>
									<div class="row"><div class="col">Agricultural:</div><div class="col" ID="lblYear2AgImp"></div></div>
									<div class="row"><div class="col">Commercial:</div><div class="col" ID="lblYear2CommImp"></div></div>
									<div class="row"><div class="col">Residential:</div><div class="col" ID="lblYear2ResImp"></div></div>
									<div class="row"><div class="col"><u>New Construction</u></div></div>
									<div class="row"><div class="col">Agricultural:</div><div class="col" ID="lblYear2AgNC"></div></div>
									<div class="row"><div class="col">Commercial:</div><div class="col" ID="lblYear2CommNC"></div></div>
									<div class="row"><div class="col">Residential:</div><div class="col" ID="lblYear2ResNC"></div></div>
									<hr style="height:2px;border-width:0;color:gray;background-color:gray">
									<div class="row"><div class="col">Total Market Value:</div><div class="col" ID="lblYear2TMV"></div></div>
									<div class="row"><div class="col">Total Assessed Value:</div><div class="col" ID="lblYear2TAV"></div></div>
									<div class="row"><div class="col">Total Taxable Value:</div><div class="col" ID="lblYear2TTV"></div></div>
								</div>
								<div class="col-xl-3 border">
									<div class="row"><div class="col" id="lblYear3" style="font-weight:bold;font-size:medium;margin:5px 0px 4px 0px;"></div></div>
									<div class="row"><div class="col"><u>Land</u></div></div>
									<div class="row"><div class="col">Agricultural:</div><div class="col" ID="lblYear3AgLand"></div></div>
									<div class="row"><div class="col">Commercial:</div><div class="col" ID="lblYear3CommLand"></div></div>
									<div class="row"><div class="col">Residential:</div><div class="col" ID="lblYear3ResLand"></div></div>
									<div class="row"><div class="col"><u>Improvements</u></div></div>
									<div class="row"><div class="col">Agricultural:</div><div class="col" ID="lblYear3AgImp"></div></div>
									<div class="row"><div class="col">Commercial:</div><div class="col" ID="lblYear3CommImp"></div></div>
									<div class="row"><div class="col">Residential:</div><div class="col" ID="lblYear3ResImp"></div></div>
									<div class="row"><div class="col"><u>New Construction</u></div></div>
									<div class="row"><div class="col">Agricultural:</div><div class="col" ID="lblYear3AgNC"></div></div>
									<div class="row"><div class="col">Commercial:</div><div class="col" ID="lblYear3CommNC"></div></div>
									<div class="row"><div class="col">Residential:</div><div class="col" ID="lblYear3ResNC"></div></div>
									<hr style="height:2px;border-width:0;color:gray;background-color:gray">
									<div class="row"><div class="col">Total Market Value:</div><div class="col" ID="lblYear3TMV"></div></div>
									<div class="row"><div class="col">Total Assessed Value:</div><div class="col" ID="lblYear3TAV"></div></div>
									<div class="row"><div class="col">Total Taxable Value:</div><div class="col" ID="lblYear3TTV"></div></div>
								</div>
							</div>
						</div> <!-- end of container for values -->
					</div> <!-- end of value -->
				</div> <!-- end of row -->
			</div>  <!-- end of basicvalueinfo div -->

			<!-- tab for property owners -->
			<div id="ownership" class="container-fluid tab-pane fade">
				<div class="row" style="height:94%;font-size:1.2em;">
					<div id="firstowner" class="col-xl-4" style="background-color:white;height:94%;margin:1%;box-shadow: black 8px 6px 13px -6px;left:1%;">
						<table style="margin:2%">
							<tr><td><h1><b>Primary Owner:</b></h1></td></tr>
							<tr><td><b>Name:</b></td><td ID="lblowner1name"></td></tr>
							<tr><td><b>Address:</b></td><td ID="lblowner1address"></td></tr>
							<tr><td><b>City, State, Zip:</b></td><td ID="lblowner1citystatezipcountry"></td></tr>
						</table>
					</div>
					<div id="secondowner" class="col-xl-4" style="background-color:white;height:94%;margin:1%;box-shadow: black 8px 6px 13px -6px;left:1%;">
						<table style="margin:2%">
							<tr><td><h1><b>Second Owner:</b></h1></td></tr>
							<tr><td><b>Name:</b></td><td ID="lblowner2name"></td></tr>
							<tr><td><b>Address:</b></td><td ID="lblowner2address"></td></tr>
							<tr><td><b>City, State, Zip:</b></td><td ID="lblowner2citystatezipcountry"></td></tr>
						</table>
					</div>
					<div id="otherowners" class="col-xl-3" style="background-color:white;height:94%;margin:1%;box-shadow: black 8px 6px 13px -6px;left:1%;">
						<div class="row"><div class="col-xl-12"><h1><b>Other Owners:</b></h1></div></div>
						<div id="otherownerslist" style="margin:2%"></div>
					</div>
				</div>
			</div> <!-- end of ownership div -->

			<!-- tab for TIF, abatement and CID info -->
			<div id="econdevel" class="container-fluid tab-pane fade">
				<div class="row" style="height:94%;font-size:1em;">
					<div id="TIFs" class="col-xl-5" style="background-color:white;height:94%;margin:1%;box-shadow: black 8px 6px 13px -6px;left:1%;">
						<div class="row"><div class="col-xl-12"><h1><b>Tax Increment Financing:</b></h1></div></div>
						<div class="row">
							<div class="col-xl-6">
								<table id="TIFdistrict" style="width:100%;margin:1%;">
									<tr><td style="width:50%"><b>TIF District Name:</b></td><td style="width:50%;" ID="lblTIFdist"></td></tr>
									<tr><td style="width:50%"><b>Start Date:</b></td><td style="width:50%;" ID="lblTIFdistStartDate"></td></tr>
									<tr><td style="width:50%"><b>End Date:</b></td><td style="width:50%;" ID="lblTIFdistEndDate"></td></tr>
									<tr><td style="width:50%"><b>Duration in Years:</b></td><td style="width:50%;" ID="lblTIFdistDuration"></td></tr>
								</table>
							</div>
							<div class="col-xl-6">
								<table id="TIFproject" style="width:100%;margin:1%;">
									<tr><td style="width:50%"><b>TIF Ordinance #:</b></td><td style="width:50%;" ID="lblTIFprojOrd"></td></tr>
									<tr><td style="width:50%"><b>Start Date:</b></td><td style="width:5%0;" ID="lblTIFprojStartDate"></td></tr>
									<tr><td style="width:50%"><b>End Date:</b></td><td style="width:50%;" ID="lblTIFprojEndDate"></td></tr>
									<tr><td style="width:50%"><b>Duration in Years:</b></td><td style="width:50%;" ID="lblTIFprojDuration"></td></tr>
								</table>
							</div>
						</div>
					</div>
					<div id="abatements" class="col-xl-3" style="background-color:white;height:94%;margin:1%;box-shadow: black 8px 6px 13px -6px;left:1%;">
						<div class="row"><div class="col-xl-12"><h1><b>Abatements:</b></h1></div></div>
						<table id="Abatements" style="width:100%;margin:1%;">
							<tr><td style="width:20%"><b>353:</b></td><td colspan="2" id="lbl353descr"></td></tr>
							<tr><td style="width:20%"></td><td style="width:60%">353 From Year:</td><td ID="lbl353fromYr"></td></tr>
							<tr><td style="width:20%"></td><td style="width:60%">353 To Year:</td><td ID="lbl353toYr"></td></tr>
							<tr><td style="width:20%"><b>99:</b></td><td colspan="2" id="lbl99descr"></td></tr>
							<tr><td style="width:20%"></td><td style="width:60%">99 From Year:</td><td ID="lbl99fromYr"></td></tr>
							<tr><td style="width:20%"></td><td style="width:60%">99 To Year:</td><td ID="lbl99toYr"></td></tr>
						</table>
					</div>
					<div id="CommImpDist" class="col-xl-3" style="background-color:white;height:94%;margin:1%;box-shadow: black 8px 6px 13px -6px;left:1%;">
						<div class="row"><div class="col"><h1><b>Community Improvement District:</b></h1></div></div>
						<table id="CID" style="width:100%;margin:1%;">
							<tr><td ID="lblCID"></td></tr>
						</table>
					</div>
				</div>
			</div> <!-- end of econdevel div -->

			<div id="photos" class="container-fluid tab-pane fade">
			</div> <!-- end of photos div -->

			<!-- tab for various requests about a property -->
			<div id="requests" class="container-fluid tab-pane fade">
				<div class="row" style="height:94%;font-size:1.1em;padding-left:2%;">
					<div class="col-md-2" style="background-color:white;height:94%;margin:0.5%;overflow:auto;box-shadow: black 8px 6px 13px -6px;">
						<div class="row"><div class="col" style="text-align:center;"><h1><b>Pay Your Property Taxes</b></h1></div></div>
						<div class="row"><div class="col">Why stand in line at the Collection office or post office, when you can complete your payment online at anytime? Get started and go directly to this page or follow these step-by-step instructions.</div></div>
						<div class="row"><div class="col"><br/></div></div>
						<div class="row"><div class="col"><input class="btn btn-primary" style="width:60%;margin-left:20%;margin-right:20%;background-color:navy;" value="Click Here" onclick="window.open('https://www.jacksongov.org/176/Paying-Your-Taxes-Online')"/></div></div>
					</div>
					<div class="col-md-2" style="background-color:white;height:94%;margin:0.5%;box-shadow: black 8px 6px 13px -6px;">
						<div class="row"><div class="col" style="text-align:center;"><h1><b>Correct Your Property Information</b></h1></div></div>
						<div class="row"><div class="col">See some incorrect information about your parcel? Go here to get it corrected.</div></div>
						<div class="row"><div class="col"><br/></div></div>
						<div class="row"><div class="col" style="text-align:center;"><h1><b>COMING SOON</b></h1></div></div>					
					</div>
					<div class="col-md-3" style="background-color:white;height:94%;margin:0.5%;overflow:auto;box-shadow: black 8px 6px 13px -6px;">
						<div class="row"><div class="col" style="text-align:center;"><h1><b>If You Disagree With Your Property Assessment</b></h1></div></div>
						<div class="row" style="margin:1%;padding:1%">
							<!--<div class="col-md-5" style="border:1px solid #DCE1F0;margin:2%;padding:2%">
								<div class="col">
									<div id="lblInformal" class="row">Request an&nbsp<b>INFORMAL REVIEW</b> to your property value.</div><br><br>
									<div class="row"><input id="btnInformalAppeal" class="btn btn-primary" style="width:80%;margin-left:10%;margin-right:10%;background-color:#09AD19;" value="Click" onclick="doAppeal('informal')"/></div>
								</div>
							</div>-->
							<div class="col-md-11" style="border:1px solid #DCE1F0;margin:2%;padding:2%">
								<div class="col">
									<div id="lblBOE" class="row">Request a&nbsp<b>FORMAL APPEAL</b> to your property value.</div><br><br><br>
									<div class="row"><input id="btnBOEAppeal" class="btn btn-primary" style="width:60%;margin-left:20%;margin-right:20%;background-color:#30A7B0;" value="Click" onclick="doAppeal('formal')"/></div>
								</div>
							</div>
						</div>
						<br/>
						<div class="row"><div class="col-md-10" id="lblAppealsMsg" style="margin-left:4%;margin-right:4%;"></div></div>
					</div>
					<div class="col-md-2" style="background-color:white;height:94%;margin:0.5%;box-shadow: black 8px 6px 13px -6px;">
						<div class="row"><div class="col" style="text-align:center;"><h1><b>Change Your Address</b></h1></div></div>
						<div class="row"><div class="col">Fill out this form to request to change your property address.</div></div>
						<div class="row"><div class="col" style="text-align:center;"><h1><b>COMING SOON</b></h1></div></div>
						<div class="row"><div class="col"><br/></div></div>
						<!--<div class="row"><div class="col"><input class="btn btn-primary" type="button" style="width:60%;margin-left:20%;margin-right:20%;background-color:#12731C;" value="Click Here" onclick="changeAddress()"/></div></div>-->
					</div>
					<div class="col-md-2" style="background-color:white;height:94%;margin:0.5%;box-shadow: black 8px 6px 13px -6px;">
						<div class="row"><div class="col" style="text-align:center;"><h1><b>Merge Your Property</b></h1></div></div>
						<div class="row"><div class="col">Combine your parcel with an adjacent one.</div></div>
						<div class="row"><div class="col"><br/></div></div>
						<div class="row"><div class="col" style="text-align:center;"><h1><b>COMING SOON</b></h1></div></div>
						<!--<div class="row"><div class="col"><input class="btn btn-primary" type="button" style="width:60%;margin-left:20%;margin-right:20%;background-color:#9690F5;" value="Click Here" onclick="mergeProperty()"/></div></div>-->
					</div>
				</div> <!-- end of row of requests -->
			</div> <!-- end of requests div -->

			<!-- tab to list elected officials for the property 
			<div id="electedofficials" class="container-fluid tab-pane fade">
				<div class="row" style="height:94%;font-size: 1.1em;">
					<div id="local" class="col-xl-4" style="background-color:white;height:94%;margin:1%;box-shadow: black 8px 6px 13px -6px;">
						<div class="row"><div class="col"><h1><b>Jackson County</b></h1></div></div>
						<div class="row"><div class="col"><b>County Executive:</b></div></div>
						<div class="row"><div class="col">&nbsp&nbsp&nbsp <label ID="lblJaCoExecName"></label> (<label ID="lblJaCoExecParty"></label>) &nbsp&nbsp&nbsp <label ID="hlJaCoExecWebsite"></label></div></div>
						<div class="row"><div class="col"><b>Individual Legislator:</b></div></div>
						<div class="row"><div class="col">&nbsp&nbsp&nbsp <label ID="lblJaCoIndivName"></label> (<label ID="lblJaCoIndivParty"></label>) - District #<label ID="lblJaCoIndivDist"></label> &nbsp&nbsp&nbsp <label ID="hlJaCoIndivWebsite"></label></div></div>
						<div class="row"><div class="col"><b>At-Large Legislator:</b></div></div>
						<div class="row"><div class="col">&nbsp&nbsp&nbsp <label ID="lblJaCoAtLargeName"></label> (<label ID="lblJaCoAtLargeParty"></label>) - District #<label ID="lblJaCoAtLargeDist"></label> &nbsp&nbsp&nbsp <label ID="hlJaCoAtLargeWebsite"></label></div></div>				
					</div>
					<div id="state" class="col-xl-4" style="background-color:white;height:94%;margin:1%;box-shadow: black 8px 6px 13px -6px;">
						<div class="row"><div class="col"><h1><b>State of Missouri</b></h1></div></div>
						<div class="row"><div class="col"><b>Governor:</b></div></div>
						<div class="row"><div class="col">&nbsp&nbsp&nbsp <label ID="lblMOgovName"></label> (<label ID="lblMOgovParty"></label>) &nbsp&nbsp&nbsp <label ID="hlMOgovWebsite"><a href="https://governor.mo.gov/">Website</a></label></div></div>
						<div class="row"><div class="col"><b>Senator:</b></div></div>
						<div class="row"><div class="col">&nbsp&nbsp&nbsp <label ID="lblMOsenName"></label> (<label ID="lblMOsenParty"></label>) - District #<label ID="lblMOsenDist"></label> &nbsp&nbsp&nbsp <label ID="hlMOsenWebsite"></label></div></div>
						<div class="row"><div class="col"><b>Representative:</b></div></div>
						<div class="row"><div class="col">&nbsp&nbsp&nbsp <label ID="lblMOrepName"></label> (<label ID="lblMOrepParty"></label>) - District #<label ID="lblMOrepDist"></label> &nbsp&nbsp&nbsp <label ID="hlMOrepWebsite"></label></div></div>
					</div>
					<div id="federal" class="col-xl-3" style="background-color:white;height:94%;margin:1%;box-shadow: black 8px 6px 13px -6px;">
						<div class="row"><div class="col"><h1><b>United States</b></h1></div></div>
						<div class="row"><div class="col"><b>Senator:</b></div></div>
						<div class="row"><div class="col">&nbsp&nbsp&nbsp <label ID="lblUSsen1Name"></label> (<label ID="lblUSsen1Party"></label>) &nbsp&nbsp&nbsp <label ID="hlUSsen1Website"></label></div></div>
						<div class="row"><div class="col"><b>Senator:</b></div></div>
						<div class="row"><div class="col">&nbsp&nbsp&nbsp <label ID="lblUSsen2Name"></label> (<label ID="lblUSsen2Party"></label>) &nbsp&nbsp&nbsp <label ID="hlUSsen2Website"></label></div></div>
						<div class="row"><div class="col"><b>Representative:</b></div></div>
						<div class="row"><div class="col">&nbsp&nbsp&nbsp <label ID="lblUSHouseName"></label> (<label ID="lblUSHouseParty"></label>) - District #<label ID="lblUSHouseDist"></label> &nbsp&nbsp&nbsp <label ID="hlUSHouseWebsite"></label></div></div>
					</div>
				</div>
			</div>--> <!-- end of elected officials div -->

			<!-- Print/save tab -->
			<div id="printsave" class="container-fluid tab-pane fade">
			<div class="row" style="height:94%;font-size: 1.1em;">
				<div class="col-xl-6" style="background-color:white;height:94%;margin:1%;box-shadow: black 8px 6px 13px -6px;">
					<div class="col-xl-12">
						<div class="row" style="text-align:center;"><div class="col-xl-12"><h1>Print Property Info, Map or Photos</h1></div></div>
						<div class="row" style="text-align:center;"><div class="col-xl-12"><b>Include the following:</b></div></div>
						<br/>
						<div class="row"><div class="col-xl-1"><input type="checkbox" id="chkInfo" checked=checked></div><div class="col-xl-11"><label> Property Info</label></div></div>
						<div class="row"><div class="col-xl-1"><input type="checkbox" id="chkMap"></div><div class="col-xl-11"><label> Map</label></div></div>
						<div class="row"><div class="col-xl-1"><input type="checkbox" id="chkPhotos"></div><div class="col-xl-11"><label> Photos</label></div></div>
						<div class="row" style="text-align:center;"><div class="col-xl-4"></div><div class="col-xl-4"><button type="button" class="btn btn-info btn-lg" style="background-color:#007bff;" onclick="PrintStuff()">Print</button></div><div class="col-xl-4"></div></div>
					</div>
				</div>
			</div>
			</div> <!-- end of PrintSave div -->

		</div> <!-- end of tab-content -->
		
	</div>  <!-- end of container-all -->
</body>
</html>