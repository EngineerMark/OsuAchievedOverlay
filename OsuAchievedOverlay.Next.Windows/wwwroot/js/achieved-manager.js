const Gamemode = {
    Standard: 0,
    Taiko: 1,
    CatchTheBeat: 2,
    Mania: 3
};

var ctxP = null;
var mapChart = null;

//Instead of document ready, we use this due to the tab load system
function appReady() {
    $('#settingsSectionThemes').hide();
    $('.mdb-select').materialSelect();
    inspectorOpenMain();

    var observer = new MutationObserver(function (m, me) {
        var elem = document.getElementById("inspectorMapRankChart");
        if (elem) {
            ctxP = elem.getContext('2d');
            mapChart = new Chart(ctxP, {
                type: 'pie',
                data: {
                    labels: ["Ranked", "Loved", "Unranked"],
                    datasets: [{
                        data: [0, 0, 0],
                        backgroundColor: ["#b3ff66", "#ff66ab", "#969696"],
                        hoverBackgroundColor: ["#caff94", "#ff8abf", "#bfbfbf"]
                    }]
                },
                options: {
                    responsive: true,
                    elements: {
                        arc: {
                            borderWidth: 0
                        }
                    }
                }
            });
            me.disconnect();
            return;
        }
    });
    observer.observe(document, {
        childList: true,
        subtree: true
    });

    populateToolUserGraphs();

    $("[data-addon]").hide();
    $("[data-type=\"addonLink\"]").click(function (e) {
        $("[data-addon]").hide();
        var name = $(this).attr("data-toggle");
        $("[data-addon=\"" + name + "\"]").show();
        $("#tools-modal").modal("toggle");
    });

    $('#playerViewer').hide();
}

function addApiField(data, hash) {
    var deserializedData = JSON.parse(data);
    var htmlData = "<tr>";
    htmlData += "<th class=\"align-middle\" scope=\"row\"><div class=\"form-check\"><input type=\"checkbox\" class=\"form-check-input\" id=\"api_check" + hash + "\" " + (deserializedData["Active"] == true ? "checked" : "") + "><label class=\"form-check-label\" for=\"api_check" + hash +"\"></label></div></th>";
    htmlData += "<td class=\"align-middle\">" + deserializedData["DisplayName"] + "</td>";
    htmlData += "<td class=\"float-right\">";
    htmlData += "<a data-toggle=\"modal\" data-target=\"#api-editor-modal\" class=\"btn btn-sm btn-light\" id=\"api-editor-button-"+hash+"\" data-api-id=\"" + hash + "\"><i class=\"fas fa-edit\"></i></a>";
    htmlData += "<a class=\"btn btn-sm btn-light\" data-api-id=\"" + hash +"\"><i class=\"fas fa-trash-alt\"></i></a>";
    htmlData += "</td>";
    $("#localApiList").append(htmlData);
    $("#api-editor-update-button").attr("data-api-updateID", hash);

    $("#api-editor-button-" + hash).click(function () {
        $("#api-editor-title").html(deserializedData["DisplayName"]);
    });
}

function updateMapChart(ranked, loved, unranked) {
    mapChart.data.datasets[0].data = [ranked, loved, unranked];
    mapChart.update();
}

function inspectorOpenMain() {
    $('#inspectorviewMain').show(500);
    $('#inspectorviewBeatmapListing').hide(500);
    $('#inspectorviewScoreListing').hide(500);
    $('#inspectorviewCollectionListing').hide(500);
}

function inspectorOpenBeatmapListing() {
    $('#inspectorviewMain').hide(500);
    $('#inspectorviewBeatmapListing').show(500);
    $('#inspectorviewScoreListing').hide(500);
}

function inspectorOpenScoreListing() {
    $('#inspectorviewMain').hide(500);
    $('#inspectorviewBeatmapListing').hide(500);
    $('#inspectorviewScoreListing').show(500);
}

function inspectorOpenCollectionListing() {
    $('#inspectorviewMain').hide(500);
    $('#inspectorviewBeatmapListing').hide(500);
    $('#inspectorviewScoreListing').hide(500);
    $('#inspectorviewCollectionListing').show(500);
}

function getTabFields(){
    var data = document.querySelectorAll('[data-tab]');
    var namesOnly = [];
    data.forEach(element => namesOnly.push(element.getAttribute("data-tab")));
    return namesOnly;
}

function populateTab(tabname, tabdata) {
    var data = tabdata;
    var element = document.querySelector('[data-tab="' + tabname + '"]');
    element.innerHTML = data;
}

function getAddonFields() {
    var data = document.querySelectorAll('[data-addon]');
    var namesOnly = [];
    data.forEach(element => namesOnly.push(element.getAttribute("data-addon")));
    return namesOnly;
}

function populateAddon(addonname, addondata) {
    var data = addondata;
    var element = document.querySelector('[data-addon="' + addonname + '"]');
    element.innerHTML = data;
}

// $('#settingsNsfwMode input:checkbox').change(function(){
//     console.log("wow");
//     if ($(this).is(':checked')) {
//         $('#settingsSectionThemes').show();
//     }else{
//         $('#settingsSectionThemes').hide();
//     }
// });

function applyTheme(themeStyle){
    $('#appCustomTheme').empty();
    $('#appCustomTheme').text(themeStyle);
}

$(document).on("change", "input[id='settingsNsfwMode']", function () {
    if (this.checked) {
        $('#settingsSectionThemes').show(500);
        toastr.warning('You have enabled NSFW mode for theme usage. If you wish to see no anime material, turn it off!');
    }else{
        $('#settingsSectionThemes').hide(500);
    }
});

const contextMenu = document.getElementById("contextMenuParent");
const scope = document.querySelector("body");

scope.addEventListener("contextmenu", (event) => {
    event.preventDefault();
  
    const { clientX: mouseX, clientY: mouseY } = event;
  
    contextMenu.style.top = `${mouseY}px`;
    contextMenu.style.left = `${mouseX}px`;
  
    contextMenu.classList.remove("visible");
  
    setTimeout(() => {
      contextMenu.classList.add("visible");
    });
  });

scope.addEventListener("click", (e) => {
    if (e.target.offsetParent != contextMenu) {
      contextMenu.classList.remove("visible");
    }
});

function hexToRgb(hex) {
    var result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
    return result ? {
        r: parseInt(result[1], 16),
        g: parseInt(result[2], 16),
        b: parseInt(result[3], 16)
    } : null;
}

function osuGetDifficultyColor(diff){
    if (diff < 1.5) {
        return "#4fc0ff";
    }
    if (diff < 2.0) {
        return "#4fffd5";
    }
    if (diff < 2.5) {
        return "#7cff4f";
    }
    if (diff < 3.25) {
        return "#f6f05c";
    }
    if (diff < 4.5) {
        return "#ff8068";
    }
    if (diff < 6.0) {
        return "#ff3c71";
    }
    if (diff < 7.0) {
        return "#6563de";
    }
    if (diff < 8.0) {
        return "#18158e";
    }
    return "#000000";
}

function osuGetDifficultyClass(diff) {
    return "background-color: " + osuGetDifficultyColor(diff);
    //if(diff<2){
    //    return "beatmap-difficulty-easy";
    //}
    //if(diff<2.7){
    //    return "beatmap-difficulty-normal";
    //}
    //if(diff<4){
    //    return "beatmap-difficulty-hard";
    //}
    //if(diff<5.3){
    //    return "beatmap-difficulty-insane";
    //}
    //if(diff<6.5){
    //    return "beatmap-difficulty-expert";
    //}
    //return "beatmap-difficulty-expertplus";
}

class OsuBeatmap {
    constructor(jsonData){
        this.Artist = jsonData.Artist;
        this.DiffStarRatingStandard = jsonData.DiffStarRatingStandard;
        this.DiffStarRatingMania = jsonData.DiffStarRatingMania;
        this.DiffStarRatingCtB = jsonData.DiffStarRatingCtB;
        this.DiffStarRatingTaiko = jsonData.DiffStarRatingTaiko;
        this.GameMode = jsonData.GameMode;
        this.Version = jsonData.Version;
        this.CircleSize = jsonData.CircleSize;
        this.ApproachRate = jsonData.ApproachRate;
        this.HPDrainRate = jsonData.HPDrainRate;
        this.OveralDifficulty = jsonData.OveralDifficulty;
        this.TotalTime = jsonData.TotalTime;
        this.DrainTimeSeconds = jsonData.DrainTimeSeconds;
        this.TimingPoints = jsonData.TimingPoints;
        this.BeatmapChecksum = jsonData.BeatmapChecksum;
    }

    GetDefaultStarrating(){
        if(this.GameMode==Gamemode.Standard){
            return this.DiffStarRatingStandard.None;
        }else if(this.GameMode==Gamemode.Mania){
            return this.DiffStarRatingMania.None;
        }else if(this.GameMode==Gamemode.CatchTheBeat){
            return this.DiffStarRatingCtB.None;
        }else if(this.GameMode==Gamemode.Taiko){
            return this.DiffStarRatingTaiko.None;
        }
    }

    get Html(){
        return "";
    }
}

class OsuBeatmapSet{
    Beatmaps;

    constructor(beatmapSetData, id){
        this.ID = id;
        this.BeatmapSetID = beatmapSetData.BeatmapSetID;
        this.Title = beatmapSetData.Title;
        this.Artist = beatmapSetData.Artist;
        this.RankStatus = beatmapSetData.RankStatus;
        this.SongTags = beatmapSetData.SongTags;
        this.Creator = beatmapSetData.Creator;
        const bgpath = beatmapSetData.BackgroundPath;
        const bgu = new URL(`file:///${bgpath}`).href;
        this.BackgroundPath = bgu;
        this.Beatmaps = [];
        this.Tags = beatmapSetData.SongTags;

        this.maps_std = [];
        this.maps_frt = [];
        this.maps_man = [];
        this.maps_tai = [];

        this.StandardIndex = -1;
        this.FruitsIndex = -1;
        this.ManiaIndex = -1;
        this.TaikoIndex = -1;

        for(let i=0;i<beatmapSetData.Beatmaps.length;i++){
            var beatmap = new OsuBeatmap(beatmapSetData.Beatmaps[i]);

            if(beatmap.GameMode==Gamemode.Standard){
                this.maps_std.push(beatmap);
            }else if(beatmap.GameMode==Gamemode.Mania){
                this.maps_man.push(beatmap);
            }else if(beatmap.GameMode==Gamemode.CatchTheBeat){
                this.maps_frt.push(beatmap);
            }else if(beatmap.GameMode==Gamemode.Taiko){
                this.maps_tai.push(beatmap);
            }
            // this.Beatmaps.push(beatmap);
        }

        if(this.maps_std.length>=2){
            this.maps_std.sort((a,b)=>(a.DiffStarRatingStandard.None>b.DiffStarRatingStandard.None)?1:((b.DiffStarRatingStandard.None>a.DiffStarRatingStandard.None)?-1:0));
        }
        if(this.maps_man.length>=2){
            this.maps_man.sort((a,b)=>(a.DiffStarRatingMania.None>b.DiffStarRatingMania.None)?1:((b.DiffStarRatingMania.None>a.DiffStarRatingMania.None)?-1:0));
        }
        if(this.maps_frt.length>=2){
            this.maps_frt.sort((a,b)=>(a.DiffStarRatingCtB.None>b.DiffStarRatingCtB.None)?1:((b.DiffStarRatingCtB.None>a.DiffStarRatingCtB.None)?-1:0));
        }
        if(this.maps_tai.length>=2){
            this.maps_tai.sort((a,b)=>(a.DiffStarRatingTaiko.None>b.DiffStarRatingTaiko.None)?1:((b.DiffStarRatingTaiko.None>a.DiffStarRatingTaiko.None)?-1:0));
        }

        if(this.maps_std.length>=1){
            this.StandardIndex = 0;
        }
        if(this.maps_man.length>=1){
            this.ManiaIndex = this.maps_std.length;
        }
        if(this.maps_frt.length>=1){
            this.FruitsIndex = this.maps_std.length+this.maps_man.length;
        }
        if(this.maps_tai.length>=1){
            this.TaikoIndex = this.maps_std.length+this.maps_man.length+this.maps_frt.length;
        }

        this.Beatmaps = this.maps_std.concat(this.maps_man, this.maps_frt, this.maps_tai);
    }

    Html(){
        var difficultyData = "";

        var truncatedVersion = (this.maps_std.length>9 || this.maps_man.length>9 || this.maps_frt.length>9 || this.maps_tai.length>9);
        for(let i=0;i<this.Beatmaps.length;i++){
            if(this.StandardIndex!=-1&&this.StandardIndex==i){
                difficultyData+="<span data-toggle='tooltip' title='osu!' class='align-middle'><img style='height: 1.6em;' src='./img/gamemodes/osu_32.png' /></span> ";
                if(truncatedVersion){
                    difficultyData+="<span class='align-middle'>"+this.maps_std.length+"</span> ";
                }
            }

            if(this.ManiaIndex!=-1&&this.ManiaIndex==i){
                difficultyData+="<span data-toggle='tooltip' title='Mania' class='align-middle'><img style='height: 1.6em;' src='./img/gamemodes/mania_32.png' /></span> ";
                if(truncatedVersion){
                    difficultyData+=this.maps_man.length+" ";
                }
            }

            if(this.FruitsIndex!=-1&&this.FruitsIndex==i){
                difficultyData+="<span data-toggle='tooltip' title='Catch the Beat' class='align-middle'><img style='height: 1.6em;' src='./img/gamemodes/ctb_32.png' /></span> ";
                if(truncatedVersion){
                    difficultyData+=this.maps_frt.length+" ";
                }
            }

            if(this.TaikoIndex!=-1&&this.TaikoIndex==i){
                difficultyData+="<span data-toggle='tooltip' title='Taiko' class='align-middle'><img style='height: 1.6em;' src='./img/gamemodes/taiko_32.png' /></span> ";
                if(truncatedVersion){
                    difficultyData+=this.maps_tai.length+" ";
                }
            }
            if(!truncatedVersion){
                var sr = this.Beatmaps[i].GetDefaultStarrating();
                //switch(this.Beatmaps.)
                var color = osuGetDifficultyClass(sr);
                var tooltip = "<span style=\"" + color +" !important;\" class=\"badge badge-pill\">"+(Math.round(sr*100)/100)+"*</span> "+(this.Beatmaps[i].Version);
                var badge = "<span data-toggle='tooltip' title='"+tooltip+"' data-html='true' class='badge badge-pill' style='width:4px;height:16px;"+color+"'>&nbsp;</span>";
                difficultyData+=badge+" ";
                // $("#beatmapsetTooltip_id"+tooltipID).tooltip();
            }
        }

        return "<div style='max-height:200px;' class='mt-1 rounded' id='beatmapCard'>"+
                "<a class='card card-image text-white rounded' data-toggle='modal' data-target='#beatmapListingViewer' style='background-image: url(\""+this.BackgroundPath+"\");'>"+
                    "<div class='card-body rounded beatmapCardContent'>"+
                        "<h5 class='card-title'>"+this.Artist+" - "+this.Title+"</h5>"+
                        "<p class='card-text'>Mapped by "+this.Creator+"</p>"+
                        "<p class='card-text text-white' style='text-truncate'>"+
                            "<small><strong>"+difficultyData+"</strong></small>"+
                        "</p>"+
                    "</div>"+
                "</a>"+
            "</div>";
    }
}

var beatmapsets = [];
var beatmapCardPrefab = "";
var setsPerPage = 20;
var pages = -1;
var currentPageIndex = 0;
var generatedPages = [];

function clearBeatmaps(){
    beatmapsets = [];
}

function addBeatmapset(encodedSet, id){
    var data = JSON.parse(encodedSet);
    var set = new OsuBeatmapSet(data, id);
    beatmapsets.push(set);
}

function generateBeatmapsetList(){
    console.log(beatmapsets);
    for(let i=0;i<beatmapsets.length;i++){
        if(typeof beatmapsets[i] !== 'undefined') {
            var item = $(beatmapsets[i].Html()).hide();
            $('#beatmapListGroup').append(item);
            item.show(500);

            item.click(function () {
                beatmapViewerApply(beatmapsets[i]);
            });
        }
    }
    $('[data-toggle="tooltip"]').tooltip();
}

var currentBeatmapSet = null;
function beatmapViewerApply(set){
    $('#beatmapListingViewerHeaderImage').css('background-image', 'url("'+set.BackgroundPath+'")');
    $('#beatmapListingViewerHeaderImage').css('background-repeat', 'norepeat');
    $('#beatmapListingViewerHeaderImage').css('background-attachment', 'fixed');
    $('#beatmapListingViewerHeaderImage').css('background-position', 'center');
    $('#beatmapListingViewerHeaderImage').css('background-size', 'cover');

    currentBeatmapSet = set;
    beatmapViewerApplyDiff(0);

    $('#beatmapListingViewerDifficulties').empty();
    for(let i=0;i<set.Beatmaps.length;i++){
        var sr = set.Beatmaps[i].GetDefaultStarrating();
        var color = osuGetDifficultyColor(sr);
        var colorClass = osuGetDifficultyClass(sr);
        var icon = "";
        if(set.Beatmaps[i].GameMode==Gamemode.Standard){
            icon = "icon-mode-osu";
        }else if(set.Beatmaps[i].GameMode==Gamemode.Mania){
            icon = "icon-mode-mania";
        }else if(set.Beatmaps[i].GameMode==Gamemode.CatchTheBeat){
            icon = "icon-mode-ctb";
        }else if(set.Beatmaps[i].GameMode==Gamemode.Taiko){
            icon = "icon-mode-taiko";
        }

        var tooltip = "<span class=\"badge badge-pill\" style=\"" + colorClass+"\">"+(Math.round(sr*100)/100)+"*</span> "+(set.Beatmaps[i].Version);
        var item = "<a onclick='beatmapViewerApplyDiff("+i+");' data-toggle='tooltip' data-html='true' title='"+tooltip+"' class='"+icon+" beatmapViewerDifficultyLink' style='font-size:2rem;color: "+color+";'></a> ";
        $('#beatmapListingViewerDifficulties').append(item);
    }
    $('[data-toggle="tooltip"]').tooltip({ boundary: 'window' });
}

function beatmapViewerApplyDiff(id){
    var set = currentBeatmapSet;
    var map = set.Beatmaps[id];
    cefOsuApp.requestBeatmapScores(map.BeatmapChecksum);
    var apidata = cefOsuApp.requestBeatmapApiData(map.BeatmapChecksum, 0, map.GameMode);
    if (apidata != null) {
        var cvApiData = JSON.parse(apidata);
        var apiMap = cvApiData["Item1"];
        var pp95 = cvApiData["Item2"];
        var pp98 = cvApiData["Item3"];
        var pp99 = cvApiData["Item4"];
        var pp100 = cvApiData["Item5"];
        $("#beatmapViewerAPIError").hide();
        $("#beatmapViewerPP95").html(Math.round(pp95*10)/10+"pp");
        $("#beatmapViewerPP98").html(Math.round(pp98 * 10) / 10+"pp");
        $("#beatmapViewerPP99").html(Math.round(pp99 * 10) / 10+"pp");
        $("#beatmapViewerPP100").html(Math.round(pp100 * 10) / 10 + "pp");

        $("#beatmapViewerPP").show();

    } else {
        $("#beatmapViewerAPIError").show();
        $("#beatmapViewerPP").hide();
    }
    $('#beatmapListingViewerTitle').html(set.Artist+" - "+set.Title+" ["+set.Beatmaps[id].Version+"]");

    $('#beatmapListingViewerCreator').html(set.Creator);

    $('#beatmapListingViewerStatCSLabel').html(set.Beatmaps[id].CircleSize);
    $('#beatmapListingViewerStatCS').css('width', (set.Beatmaps[id].CircleSize*10)+'%');

    $('#beatmapListingViewerStatHPLabel').html(set.Beatmaps[id].HPDrainRate);
    $('#beatmapListingViewerStatHP').css('width', (set.Beatmaps[id].HPDrainRate*10)+'%');
    
    $('#beatmapListingViewerStatODLabel').html(set.Beatmaps[id].OveralDifficulty);
    $('#beatmapListingViewerStatOD').css('width', (set.Beatmaps[id].OveralDifficulty*10)+'%');

    $('#beatmapListingViewerStatARLabel').html(set.Beatmaps[id].ApproachRate);
    $('#beatmapListingViewerStatAR').css('width', (set.Beatmaps[id].ApproachRate*10)+'%');

    $('#beatmapListingViewerStatSRLabel').html(Math.round(set.Beatmaps[id].GetDefaultStarrating()*10)/10);
    $('#beatmapListingViewerStatSR').css('width', (set.Beatmaps[id].GetDefaultStarrating()*10)+'%');

    var totalTime = new Date(set.Beatmaps[id].TotalTime);
    var drainTime = new Date(set.Beatmaps[id].DrainTimeSeconds*1000);

    //$('#beatmapListingViewerTotalTime').html(totalTime.getMinutes+":"+totalTime.getSeconds);
    //$('#beatmapListingViewerDrainTime').html(drainTime.getMinutes+":"+drainTime.getSeconds);

    //$('#beatmapListingViewerBPM').html(0);
    //if(set.Beatmaps[id].TimingPoints!=null && set.Beatmaps[id].TimingPoints.length>0){
    //    $('#beatmapListingViewerBPM').html(Math.round(60000/set.Beatmaps[id].TimingPoints[0].MsPerQuarter));
    //}
    //$('#beatmapListingViewerMaxCombo').html("0x");
}

function beatmapViewerPopulateScores(encodedScores){
    $('#beatmapListingViewerScoreList').empty();
    //if(encodedScores.length>1){
    //    var scores = JSON.parse(encodedScores);

    //    console.log(scores);

    //    if(scores.length>0){
    //        for(let i=0;i<scores.length;i++){
    //            var rank = "<td>#"+(i+1)+" </td>";
    //            var score = "<td>"+$.number(scores[i].Score)+"</td>";
    //            var user = "<td>"+scores[i].PlayerName+"</td>";
    //            var combo = "<td>"+scores[i].Combo+"x</td>";
    //            var field = "<tr>"+rank+score+user+combo+"</tr>";
    //            $('#beatmapListingViewerScoreList').append(field);
    //        }
    //    }
    //}
}

function beatmapsGeneratePagination(length) {
    for (let i = 0; i < length; i++) {
        $('#beatmapPaginationGroup').append('<li onclick="cefOsuApp.beatmapBrowserSetPage(' + i + ')\" class=\"page-item\"><a class=\"page-link text-white\">' + (i + 1) + '</a></li>');
    }
    var t;
}

function beatmapPaginationSet(index){
    // currentPageIndex = index;
    // generateBeatmapList(currentPageIndex);
}

var settings = null;

function FillSessionDataList(data){
    $('#sessionListTable tbody tr').remove();
    data = JSON.parse(data);

    var html = '';
    for(var i=0;i<data.length;i++){
        html+='<tr sessionid="'+btoa(data[i]["FileDate"])+'">'+
            '<th>'+data[i]["FileName"]+'</th>'+
            '<th>'+time2TimeAgo(new Date(data[i]["FileDate"]))+'</th>'+
            '</tr>';
    }
    $('#sessionListTable tbody').html(html);

    $('#sessionListTable tbody tr').click(function(){
        var selected = $(this).hasClass('highlight');
        $("#sessionListTable tr").removeClass('highlight');
        if(!selected)
            $(this).addClass('highlight');
    });
}

function loadSessionById(){
    var selected = $('#sessionListTable tbody').find('.highlight');
    var selectedAttr = selected.attr('sessionid');

    if(typeof selectedAttr === 'undefined')
        toastr.error('You didn\'t select any to load');
    else
        cefOsuApp.sessionHandlerLoad(selectedAttr);
        //toastr.error('Test');
}

function ApplySession(session, rounding){
    const positive = "<i class=\"fas fa-caret-up\"></i> ";
    const nochange = "";
    const negative = "<i class=\"fas fa-caret-down\"></i> ";

    session = JSON.parse(session);
    rounding = parseInt(rounding);

    $('#sessionTotalSSHCount').html(numberWithCommas(session["SessionDataCurrent"]["DataRankSSH"]));
    $('#sessionTotalSSCount').html(numberWithCommas(session["SessionDataCurrent"]["DataRankSS"]));
    $('#sessionTotalSHCount').html(numberWithCommas(session["SessionDataCurrent"]["DataRankSH"]));
    $('#sessionTotalSCount').html(numberWithCommas(session["SessionDataCurrent"]["DataRankS"]));
    $('#sessionTotalACount').html(numberWithCommas(session["SessionDataCurrent"]["DataRankA"]));

    $('#sessionDifferenceSSHCount').html((session["SessionDataDifference"]["DataRankSSH"]>=0?(session["SessionDataDifference"]["DataRankSSH"]==0?nochange:positive):negative)+""+$.number(Math.abs(session["SessionDataDifference"]["DataRankSSH"])));
    $('#sessionDifferenceSSCount').html((session["SessionDataDifference"]["DataRankSS"]>=0?(session["SessionDataDifference"]["DataRankSS"]==0?nochange:positive):negative)+""+$.number(Math.abs(session["SessionDataDifference"]["DataRankSS"])));
    $('#sessionDifferenceSHCount').html((session["SessionDataDifference"]["DataRankSH"]>=0?(session["SessionDataDifference"]["DataRankSH"]==0?nochange:positive):negative)+""+$.number(Math.abs(session["SessionDataDifference"]["DataRankSH"])));
    $('#sessionDifferenceSCount').html((session["SessionDataDifference"]["DataRankS"]>=0?(session["SessionDataDifference"]["DataRankS"]==0?nochange:positive):negative)+""+$.number(Math.abs(session["SessionDataDifference"]["DataRankS"])));
    $('#sessionDifferenceACount').html((session["SessionDataDifference"]["DataRankA"]>=0?(session["SessionDataDifference"]["DataRankA"]==0?nochange:positive):negative)+""+$.number(Math.abs(session["SessionDataDifference"]["DataRankA"])));

    $('#sessionDifferenceSSHCount').removeClass('green').removeClass('red').removeClass('grey').addClass((session["SessionDataDifference"]["DataRankSSH"]>=0?(session["SessionDataDifference"]["DataRankSSH"]==0?"grey":"green"):"red"));
    $('#sessionDifferenceSSCount').removeClass('green').removeClass('red').removeClass('grey').addClass((session["SessionDataDifference"]["DataRankSS"]>=0?(session["SessionDataDifference"]["DataRankSS"]==0?"grey":"green"):"red"));
    $('#sessionDifferenceSHCount').removeClass('green').removeClass('red').removeClass('grey').addClass((session["SessionDataDifference"]["DataRankSH"]>=0?(session["SessionDataDifference"]["DataRankSH"]==0?"grey":"green"):"red"));
    $('#sessionDifferenceSCount').removeClass('green').removeClass('red').removeClass('grey').addClass((session["SessionDataDifference"]["DataRankS"]>=0?(session["SessionDataDifference"]["DataRankS"]==0?"grey":"green"):"red"));
    $('#sessionDifferenceACount').removeClass('green').removeClass('red').removeClass('grey').addClass((session["SessionDataDifference"]["DataRankA"]>=0?(session["SessionDataDifference"]["DataRankA"]==0?"grey":"green"):"red"));

    var totalPlayTime = session["SessionDataCurrent"]["DataPlaytime"]; // In seconds
    var initialPlayTime = session["SessionDataInitial"]["DataPlaytime"]; // In seconds
    var differencePlayTime = totalPlayTime-initialPlayTime; // In seconds
    var diffType = "";
    if(differencePlayTime/60/60<1){
        differencePlayTime = differencePlayTime/60;
        diffType = "minutes";
    }else{
        differencePlayTime = differencePlayTime/60/60;
        diffType = "hours";
    }
    differencePlayTime = Math.round(differencePlayTime);

    var currentClears = session["SessionDataCurrent"]["DataRankSSH"] +
        session["SessionDataCurrent"]["DataRankSH"] +
        session["SessionDataCurrent"]["DataRankSS"] +
        session["SessionDataCurrent"]["DataRankS"] +
        session["SessionDataCurrent"]["DataRankA"];
    var differenceClears = session["SessionDataDifference"]["DataRankSSH"] +
        session["SessionDataDifference"]["DataRankSH"] +
        session["SessionDataDifference"]["DataRankSS"] +
        session["SessionDataDifference"]["DataRankS"] +
        session["SessionDataDifference"]["DataRankA"];

    var currentTotalhits = session["SessionDataCurrent"]["Data300x"] +
        session["SessionDataCurrent"]["Data100x"] +
        session["SessionDataCurrent"]["Data50x"];

    var differenceTotalhits = session["SessionDataDifference"]["Data300x"] +
        session["SessionDataDifference"]["Data100x"] +
        session["SessionDataDifference"]["Data50x"];

    $('#sessionCurrentLevel').html(numberWithCommas(session["SessionDataCurrent"]["DataLevel"].toFixed(rounding)));
    $('#sessionCurrentTotalScore').html(numberWithCommas(session["SessionDataCurrent"]["DataTotalScore"]));
    $('#sessionCurrentRankedScore').html(numberWithCommas(session["SessionDataCurrent"]["DataRankedScore"]));
    $('#sessionCurrentWorldRank').html("#"+numberWithCommas(session["SessionDataCurrent"]["DataPPRank"]));
    $('#sessionCurrentCountryRank').html("#"+numberWithCommas(session["SessionDataCurrent"]["DataCountryRank"]));
    $('#sessionCurrentPlaycount').html(numberWithCommas(session["SessionDataCurrent"]["DataPlaycount"]));
    $('#sessionCurrentPlaytime').html(Math.round(totalPlayTime/60/60)+" hours");
    $('#sessionCurrentClears').html(numberWithCommas(currentClears));
    $('#sessionCurrentTotalhits').html(numberWithCommas(currentTotalhits));
    $('#sessionCurrentAccuracy').html(session["SessionDataCurrent"]["DataAccuracy"].toFixed(rounding)+"%");
    $('#sessionCurrentPerformance').html(numberWithCommas(session["SessionDataCurrent"]["DataPerformance"].toFixed(rounding))+"pp");
    $('#sessionCurrent300x').html(numberWithCommas(session["SessionDataCurrent"]["Data300x"]));
    $('#sessionCurrent100x').html(numberWithCommas(session["SessionDataCurrent"]["Data100x"]));
    $('#sessionCurrent50x').html(numberWithCommas(session["SessionDataCurrent"]["Data50x"]));
    $('#sessionCurrentHitsPerPlay').html(numberWithCommas(Math.round(session["SessionDataCurrent"]["DataHitsPerPlay"]*100)/100));

    $('#sessionDifferenceLevel').html((session["SessionDataDifference"]["DataLevel"]>=0?(session["SessionDataDifference"]["DataLevel"]==0?nochange:positive):negative)+""+numberWithCommas(session["SessionDataDifference"]["DataLevel"].toFixed(rounding)));
    $('#sessionDifferenceTotalScore').html((session["SessionDataDifference"]["DataTotalScore"]>=0?(session["SessionDataDifference"]["DataTotalScore"]==0?nochange:positive):negative)+""+numberWithCommas(session["SessionDataDifference"]["DataTotalScore"]));
    $('#sessionDifferenceRankedScore').html((session["SessionDataDifference"]["DataRankedScore"]>=0?(session["SessionDataDifference"]["DataRankedScore"]==0?nochange:positive):negative)+""+numberWithCommas(session["SessionDataDifference"]["DataRankedScore"]));
    $('#sessionDifferenceWorldRank').html((session["SessionDataDifference"]["DataPPRank"]>=0?(session["SessionDataDifference"]["DataPPRank"]==0?nochange:negative):positive)+""+numberWithCommas(Math.abs(session["SessionDataDifference"]["DataPPRank"])));
    $('#sessionDifferenceCountryRank').html((session["SessionDataDifference"]["DataCountryRank"]>=0?(session["SessionDataDifference"]["DataCountryRank"]==0?nochange:negative):positive)+""+numberWithCommas(Math.abs(session["SessionDataDifference"]["DataCountryRank"])));
    $('#sessionDifferencePlaycount').html((session["SessionDataDifference"]["DataPlaycount"]>=0?(session["SessionDataDifference"]["DataPlaycount"]==0?nochange:positive):negative)+""+numberWithCommas(session["SessionDataDifference"]["DataPlaycount"]));
    $('#sessionDifferencePlaytime').html((differencePlayTime>=0?(differencePlayTime==0?nochange:positive):negative)+""+differencePlayTime+" "+diffType);
    $('#sessionDifferenceClears').html((differenceClears >= 0 ? (differenceClears == 0 ? nochange : positive) : negative) + "" + numberWithCommas(differenceClears));
    $('#sessionDifferenceAccuracy').html((session["SessionDataDifference"]["DataAccuracy"] >= 0 ? (session["SessionDataDifference"]["DataAccuracy"] == 0 ? nochange : positive) : negative) + "" + Math.abs(session["SessionDataDifference"]["DataAccuracy"]).toFixed(rounding));
    $('#sessionDifferencePerformance').html((session["SessionDataDifference"]["DataPerformance"] >= 0 ? (session["SessionDataDifference"]["DataPerformance"] == 0 ? nochange : positive) : negative) + "" + numberWithCommas(session["SessionDataDifference"]["DataPerformance"].toFixed(rounding)));
    $('#sessionDifferenceTotalhits').html((differenceTotalhits >= 0 ? (differenceTotalhits == 0 ? nochange : positive) : negative) + "" + numberWithCommas(differenceTotalhits));
    $('#sessionDifference300x').html((session["SessionDataDifference"]["Data300x"] >= 0 ? (session["SessionDataDifference"]["Data300x"] == 0 ? nochange : positive) : negative) + "" + numberWithCommas(session["SessionDataDifference"]["Data300x"]));
    $('#sessionDifference100x').html((session["SessionDataDifference"]["Data100x"] >= 0 ? (session["SessionDataDifference"]["Data100x"] == 0 ? nochange : positive) : negative) + "" + numberWithCommas(session["SessionDataDifference"]["Data100x"]));
    $('#sessionDifference50x').html((session["SessionDataDifference"]["Data50x"] >= 0 ? (session["SessionDataDifference"]["Data50x"] == 0 ? nochange : positive) : negative) + "" + numberWithCommas(session["SessionDataDifference"]["Data50x"]));
    $('#sessionDifferenceHitsPerPlay').html((session["SessionDataDifference"]["DataHitsPerPlay"] >= 0 ? (session["SessionDataDifference"]["DataHitsPerPlay"] == 0 ? nochange : positive) : negative) + "" + numberWithCommas(Math.round(session["SessionDataDifference"]["DataHitsPerPlay"]*100)/100));

    setTextColorToSign("#sessionDifferenceLevel", session["SessionDataDifference"]["DataLevel"]);
    setTextColorToSign("#sessionDifferenceTotalScore", session["SessionDataDifference"]["DataTotalScore"]);
    setTextColorToSign("#sessionDifferenceRankedScore", session["SessionDataDifference"]["DataRankedScore"]);
    setTextColorToSign("#sessionDifferenceWorldRank", session["SessionDataDifference"]["DataPPRank"], true);
    setTextColorToSign("#sessionDifferenceCountryRank", session["SessionDataDifference"]["DataCountryRank"], true);
    setTextColorToSign("#sessionDifferencePlaycount", session["SessionDataDifference"]["DataPlaycount"]);
    setTextColorToSign("#sessionDifferencePlaytime", differencePlayTime);
    setTextColorToSign("#sessionDifferenceClears", differenceClears);
    setTextColorToSign("#sessionDifferenceAccuracy", session["SessionDataDifference"]["DataAccuracy"]);
    setTextColorToSign("#sessionDifferencePerformance", session["SessionDataDifference"]["DataPerformance"]);

    setTextColorToSign("#sessionDifferenceTotalhits", differenceTotalhits);
    setTextColorToSign("#sessionDifference300x", session["SessionDataDifference"]["Data300x"]);
    setTextColorToSign("#sessionDifference100x", session["SessionDataDifference"]["Data100x"]);
    setTextColorToSign("#sessionDifference50x", session["SessionDataDifference"]["Data50x"]);
    setTextColorToSign("#sessionDifferenceHitsPerPlay", Math.round(session["SessionDataDifference"]["DataHitsPerPlay"]*100)/100);
}

function setTextColorToSign(element, valueToTest, invert = false){
    var col = invert?
        (valueToTest>=0?(valueToTest==0?"text-muted":"text-danger"):"text-success"):
        (valueToTest>=0?(valueToTest==0?"text-muted":"text-success"):"text-danger");
    $(element).removeClass('text-success').removeClass('text-danger').removeClass('text-muted').addClass(col);
}

function numberWithCommas(x) {
    return x.toString().replace(/\B(?<!\.\d*)(?=(\d{3})+(?!\d))/g, ",");
}

function time2TimeAgo(ts) {
    // This function computes the delta between the
    // provided timestamp and the current time, then test
    // the delta for predefined ranges.

    var d=new Date();  // Gets the current time
    var nowTs = Math.floor(d.getTime()/1000); // getTime() returns milliseconds, and we need seconds, hence the Math.floor and division by 1000
    var seconds = nowTs-ts;

    if(seconds<0)
        seconds = 0;

    // more that two days
    if (seconds > 2*24*3600) {
       return "a few days ago";
    }
    // a day
    if (seconds > 24*3600) {
       return "yesterday";
    }

    if (seconds > 3600) {
       return "a few hours ago";
    }
    if (seconds > 1800) {
       return "Half an hour ago";
    }
    if (seconds > 60) {
       return Math.floor(seconds/60) + " minutes ago";
    }
    return seconds + " seconds ago";
}

$(function () {
    $('[data-toggle="tooltip"]').tooltip()
});

$('img').on('dragstart', function (event) { event.preventDefault(); });

(function (factory) {
    if (typeof define === 'function' && define.amd) {
        // AMD
        define(['jquery'], factory);
    } else if (typeof exports === 'object') {
        // CommonJS
        factory(require('jquery'));
    } else {
        // Browser globals
        factory(jQuery);
    }
}(function ($) {
    var CountTo = function (element, options) {
        this.$element = $(element);
        this.options = $.extend({}, CountTo.DEFAULTS, this.dataOptions(), options);
        this.init();
    };

    CountTo.DEFAULTS = {
        from: 0,               // the number the element should start at
        to: 0,                 // the number the element should end at
        speed: 1000,           // how long it should take to count between the target numbers
        refreshInterval: 100,  // how often the element should be updated
        decimals: 0,           // the number of decimal places to show
        formatter: formatter,  // handler for formatting the value before rendering
        onUpdate: null,        // callback method for every time the element is updated
        onComplete: null       // callback method for when the element finishes updating
    };

    CountTo.prototype.init = function () {
        this.value = this.options.from;
        this.loops = Math.ceil(this.options.speed / this.options.refreshInterval);
        this.loopCount = 0;
        this.increment = (this.options.to - this.options.from) / this.loops;
    };

    CountTo.prototype.dataOptions = function () {
        var options = {
            from: this.$element.data('from'),
            to: this.$element.data('to'),
            speed: this.$element.data('speed'),
            refreshInterval: this.$element.data('refresh-interval'),
            decimals: this.$element.data('decimals')
        };

        var keys = Object.keys(options);

        for (var i in keys) {
            var key = keys[i];

            if (typeof (options[key]) === 'undefined') {
                delete options[key];
            }
        }

        return options;
    };

    CountTo.prototype.update = function () {
        this.value += this.increment;
        this.loopCount++;

        this.render();

        if (typeof (this.options.onUpdate) == 'function') {
            this.options.onUpdate.call(this.$element, this.value);
        }

        if (this.loopCount >= this.loops) {
            clearInterval(this.interval);
            this.value = this.options.to;

            if (typeof (this.options.onComplete) == 'function') {
                this.options.onComplete.call(this.$element, this.value);
            }
        }
    };

    CountTo.prototype.render = function () {
        var formattedValue = this.options.formatter.call(this.$element, this.value, this.options);
        this.$element.text(formattedValue);
    };

    CountTo.prototype.restart = function () {
        this.stop();
        this.init();
        this.start();
    };

    CountTo.prototype.start = function () {
        this.stop();
        this.render();
        this.interval = setInterval(this.update.bind(this), this.options.refreshInterval);
    };

    CountTo.prototype.stop = function () {
        if (this.interval) {
            clearInterval(this.interval);
        }
    };

    CountTo.prototype.toggle = function () {
        if (this.interval) {
            this.stop();
        } else {
            this.start();
        }
    };

    function formatter(value, options) {
        return value.toFixed(options.decimals);
    }

    $.fn.countTo = function (option) {
        return this.each(function () {
            var $this = $(this);
            var data = $this.data('countTo');
            var init = !data || typeof (option) === 'object';
            var options = typeof (option) === 'object' ? option : {};
            var method = typeof (option) === 'string' ? option : 'start';

            if (init) {
                if (data) data.stop();
                $this.data('countTo', data = new CountTo(this, options));
            }

            data[method].call(data);
        });
    };
}));

var toolUsersData = [];
toolUsersData["playcountChart"] = null;
toolUsersData["replaycountChart"] = null;

function populateToolUserGraphs() {
    var ctxL = document.getElementById("playerViewerChartPlaycount").getContext('2d');
    toolUsersData["playcountChart"] = new Chart(ctxL, {
        type: 'line',
        data: {
            labels: [],
            datasets: [
                {
                    label: "Playcount",
                    data: [],
                    backgroundColor: [
                        'rgba(255, 255, 255, .2)',
                    ],
                    borderColor: [
                        'rgba(255, 255, 255, .7)',
                    ],
                    borderWidth: 2
                }
            ]
        }
    });

    ctxL = document.getElementById("playerViewerChartReplays").getContext('2d');
    toolUsersData["replaycountChart"] = new Chart(ctxL, {
        type: 'line',
        data: {
            labels: [],
            datasets: [
                {
                    label: "Replaycount",
                    data: [],
                    backgroundColor: [
                        'rgba(255, 255, 255, .2)',
                    ],
                    borderColor: [
                        'rgba(255, 255, 255, .7)',
                    ],
                    borderWidth: 2
                }
            ]
        }
    });
}

function toolUsersRequestSearchResults() {
    $('#playerViewer').hide();
    var query = $('#playerSearchInput').val();

    var user = cefOsuApp.toolUsersSearch(query);
    if (user == "null") {
        toastr.error('This player was not found', '');
    } else {
        var deserializedUser = JSON.parse(user);
        var profile = cefOsuApp.getOsuUserProfile(deserializedUser["user_id"]);
        if (profile == "null") {
            toastr.error('This player was not found', '');
        } else {
            //var header = cefOsuApp.osuUserGetHeader(deserializedUser["user_id"]);
            var deserializedProfile = JSON.parse(profile);

            // header image
            $('#playerViewerHeaderImage').attr('src', deserializedProfile["cover_url"]);
            $('#playerViewerHeaderLink').click(function () {
                cefOsuApp.openUrl('https://osu.ppy.sh/users/' + deserializedUser["user_id"]);
            });

            // username
            $('#playerViewerName').text(deserializedUser["username"]);

            // avatar
            $('#playerViewerAvatar').attr("src", "https://a.ppy.sh/" + deserializedUser["user_id"]);

            // player groups
            $('#playerViewerGroups').html('');
            for (var i = 0; i < deserializedProfile["groups"].length; i++) {
                $('#playerViewerGroups').append('<span data-toggle="tooltip" title="' + deserializedProfile["groups"][i]["name"]+'" class="badge" style="background-color:' + deserializedProfile["groups"][i]["colour"]+';">' + deserializedProfile["groups"][i]["short_name"]+'</span> ');
            }

            // country
            let regionNames = new Intl.DisplayNames(['en'], { type: 'region' });
            var countryName = regionNames.of(deserializedUser["country"]);
            $('#playerViewerCountry').html('<i data-toggle=\"tooltip\" title=\"' + countryName + '\" class=\"material-tooltip-main twf twf-s twf-' + deserializedUser["country"].toLowerCase() + '"></i> ' + countryName);

            // playcount data
            var playcountData = deserializedProfile["monthly_playcounts"];

            toolUsersData["playcountChart"].data.labels = [];
            toolUsersData["playcountChart"].data.datasets[0].data = [];

            // replaycount data
            var replaycountData = deserializedProfile["replays_watched_counts"];

            toolUsersData["replaycountChart"].data.labels = [];
            toolUsersData["replaycountChart"].data.datasets[0].data = [];

            // populate playcount graph
            playcountData = populateEmptyness(playcountData);
            for (var i = 0; i < playcountData.length; i++) {
                var date = playcountData[i]["start_date"];
                var value = playcountData[i]["count"];

                toolUsersData["playcountChart"].data.labels.push(date);
                toolUsersData["playcountChart"].data.datasets[0].data.push(value);
            }

            toolUsersData["playcountChart"].update();

            // populate replaycount graph
            replaycountData = populateEmptyness(replaycountData);
            for (var i = 0; i < playcountData.length; i++) {
                var date = replaycountData[i]["start_date"];
                var value = replaycountData[i]["count"];

                toolUsersData["replaycountChart"].data.labels.push(date);
                toolUsersData["replaycountChart"].data.datasets[0].data.push(value);
            }

            toolUsersData["replaycountChart"].update();


            // rebuild tooltips
            $('[data-toggle="tooltip"]').tooltip();

            // lets show it
            $('#playerViewer').show();
        }
    }
    //console.log(user);
}