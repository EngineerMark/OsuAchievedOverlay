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
    console.log(html);
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