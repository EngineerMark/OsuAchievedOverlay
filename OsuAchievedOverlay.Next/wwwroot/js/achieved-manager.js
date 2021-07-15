const Gamemode = {
    Standard: 0,
    Taiko: 1,
    CatchTheBeat: 2,
    Mania: 3
};

function osuGetDifficultyColor(diff){
    if(diff<2){
        return "#88B300";
    }
    if(diff<2.7){
        return "#66CCFF";
    }
    if(diff<4){
        return "#FFCC22";
    }
    if(diff<5.3){
        return "#FF66AA";
    }
    if(diff<6.5){
        return "#8866EE";
    }
    return "#000000";
}

function osuGetDifficultyClass(diff){
    if(diff<2){
        return "beatmap-difficulty-easy";
    }
    if(diff<2.7){
        return "beatmap-difficulty-normal";
    }
    if(diff<4){
        return "beatmap-difficulty-hard";
    }
    if(diff<5.3){
        return "beatmap-difficulty-insane";
    }
    if(diff<6.5){
        return "beatmap-difficulty-expert";
    }
    return "beatmap-difficulty-expertplus";
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
                var tooltip = "<span class=\"badge badge-pill "+color+"\">"+(Math.round(sr*100)/100)+"*</span> "+(this.Beatmaps[i].Version);
                var badge = "<span data-toggle='tooltip' title='"+tooltip+"' data-html='true' class='badge badge-pill "+color+"' style='width:4px;height:16px;'>&nbsp;</span>";
                difficultyData+=badge+" ";
                // $("#beatmapsetTooltip_id"+tooltipID).tooltip();
            }
        }

        return "<div style='max-height:200px;' class='mt-1 rounded' id='beatmapCard'>"+
                "<div class='card card-image text-white rounded' style='background-image: url(\""+this.BackgroundPath+"\");'>"+
                    "<div class='card-body rounded' style='background-color: rgba(0,0,0,0.7);'>"+
                        "<h5 class='card-title'>"+this.Artist+" - "+this.Title+"</h5>"+
                        "<p class='card-text'>Mapped by "+this.Creator+"</p>"+
                        "<p class='card-text text-white' style='text-truncate'>"+
                            "<small><strong>"+difficultyData+"</strong></small>"+
                        "</p>"+
                    "</div>"+
                "</div>"+
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
            var setCard = beatmapsets[i].Html();
            $('#beatmapListGroup').append(setCard);
        }
    }
    $('[data-toggle="tooltip"]').tooltip();
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

    $('#sessionDifferenceSSHCount').html((session["SessionDataDifference"]["DataRankSSH"]>=0?(session["SessionDataDifference"]["DataRankSSH"]==0?nochange:positive):negative)+""+Math.abs(session["SessionDataDifference"]["DataRankSSH"]));
    $('#sessionDifferenceSSCount').html((session["SessionDataDifference"]["DataRankSS"]>=0?(session["SessionDataDifference"]["DataRankSS"]==0?nochange:positive):negative)+""+Math.abs(session["SessionDataDifference"]["DataRankSS"]));
    $('#sessionDifferenceSHCount').html((session["SessionDataDifference"]["DataRankSH"]>=0?(session["SessionDataDifference"]["DataRankSH"]==0?nochange:positive):negative)+""+Math.abs(session["SessionDataDifference"]["DataRankSH"]));
    $('#sessionDifferenceSCount').html((session["SessionDataDifference"]["DataRankS"]>=0?(session["SessionDataDifference"]["DataRankS"]==0?nochange:positive):negative)+""+Math.abs(session["SessionDataDifference"]["DataRankS"]));
    $('#sessionDifferenceACount').html((session["SessionDataDifference"]["DataRankA"]>=0?(session["SessionDataDifference"]["DataRankA"]==0?nochange:positive):negative)+""+Math.abs(session["SessionDataDifference"]["DataRankA"]));

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

    $('#sessionCurrentLevel').html(numberWithCommas(session["SessionDataCurrent"]["DataLevel"].toFixed(rounding)));
    $('#sessionCurrentTotalScore').html(numberWithCommas(session["SessionDataCurrent"]["DataTotalScore"]));
    $('#sessionCurrentRankedScore').html(numberWithCommas(session["SessionDataCurrent"]["DataRankedScore"]));
    $('#sessionCurrentWorldRank').html("#"+numberWithCommas(session["SessionDataCurrent"]["DataPPRank"]));
    $('#sessionCurrentCountryRank').html("#"+numberWithCommas(session["SessionDataCurrent"]["DataCountryRank"]));
    $('#sessionCurrentPlaycount').html(numberWithCommas(session["SessionDataCurrent"]["DataPlaycount"]));
    $('#sessionCurrentPlaytime').html(Math.round(totalPlayTime/60/60)+" hours");
    $('#sessionCurrentAccuracy').html(session["SessionDataCurrent"]["DataAccuracy"].toFixed(rounding)+"%");
    $('#sessionCurrentPerformance').html(numberWithCommas(session["SessionDataCurrent"]["DataPerformance"].toFixed(rounding))+"pp");

    $('#sessionDifferenceLevel').html((session["SessionDataDifference"]["DataLevel"]>=0?(session["SessionDataDifference"]["DataLevel"]==0?nochange:positive):negative)+""+numberWithCommas(session["SessionDataDifference"]["DataLevel"].toFixed(rounding)));
    $('#sessionDifferenceTotalScore').html((session["SessionDataDifference"]["DataTotalScore"]>=0?(session["SessionDataDifference"]["DataTotalScore"]==0?nochange:positive):negative)+""+numberWithCommas(session["SessionDataDifference"]["DataTotalScore"]));
    $('#sessionDifferenceRankedScore').html((session["SessionDataDifference"]["DataRankedScore"]>=0?(session["SessionDataDifference"]["DataRankedScore"]==0?nochange:positive):negative)+""+numberWithCommas(session["SessionDataDifference"]["DataRankedScore"]));
    $('#sessionDifferenceWorldRank').html((session["SessionDataDifference"]["DataPPRank"]>=0?(session["SessionDataDifference"]["DataPPRank"]==0?nochange:negative):positive)+""+numberWithCommas(session["SessionDataDifference"]["DataPPRank"]));
    $('#sessionDifferenceCountryRank').html((session["SessionDataDifference"]["DataCountryRank"]>=0?(session["SessionDataDifference"]["DataCountryRank"]==0?nochange:negative):positive)+""+numberWithCommas(session["SessionDataDifference"]["DataCountryRank"]));
    $('#sessionDifferencePlaycount').html((session["SessionDataDifference"]["DataPlaycount"]>=0?(session["SessionDataDifference"]["DataPlaycount"]==0?nochange:positive):negative)+""+numberWithCommas(session["SessionDataDifference"]["DataPlaycount"]));
    $('#sessionDifferencePlaytime').html((differencePlayTime>=0?(differencePlayTime==0?nochange:positive):negative)+""+differencePlayTime+" "+diffType);
    $('#sessionDifferenceAccuracy').html((session["SessionDataDifference"]["DataAccuracy"]>=0?(session["SessionDataDifference"]["DataAccuracy"]==0?nochange:positive):negative)+""+session["SessionDataDifference"]["DataAccuracy"].toFixed(rounding));
    $('#sessionDifferencePerformance').html((session["SessionDataDifference"]["DataPerformance"]>=0?(session["SessionDataDifference"]["DataPerformance"]==0?nochange:positive):negative)+""+numberWithCommas(session["SessionDataDifference"]["DataPerformance"].toFixed(rounding)));

    setTextColorToSign("#sessionDifferenceLevel", session["SessionDataDifference"]["DataLevel"]);
    setTextColorToSign("#sessionDifferenceTotalScore", session["SessionDataDifference"]["DataTotalScore"]);
    setTextColorToSign("#sessionDifferenceRankedScore", session["SessionDataDifference"]["DataRankedScore"]);
    setTextColorToSign("#sessionDifferenceWorldRank", session["SessionDataDifference"]["DataPPRank"], true);
    setTextColorToSign("#sessionDifferenceCountryRank", session["SessionDataDifference"]["DataCountryRank"], true);
    setTextColorToSign("#sessionDifferencePlaycount", session["SessionDataDifference"]["DataPlaycount"]);
    setTextColorToSign("#sessionDifferencePlaytime", differencePlayTime);
    setTextColorToSign("#sessionDifferenceAccuracy", session["SessionDataDifference"]["DataAccuracy"]);
    setTextColorToSign("#sessionDifferencePerformance", session["SessionDataDifference"]["DataPerformance"]);
}

function setTextColorToSign(element, valueToTest, invert = false){
    var col = invert?
        (valueToTest>=0?(valueToTest==0?"text-muted":"text-danger"):"text-success"):
        (valueToTest>=0?(valueToTest==0?"text-muted":"text-success"):"text-danger");
    $(element).removeClass('text-success').removeClass('text-danger').removeClass('text-muted').addClass(col);
}

function numberWithCommas(x) {
    return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
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