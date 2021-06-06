function ApplySession(session, rounding){
    session = JSON.parse(session);
    rounding = parseInt(rounding);

    $('#sessionTotalSSHCount').html(session["SessionDataCurrent"]["DataRankSSH"]);
    $('#sessionTotalSSCount').html(session["SessionDataCurrent"]["DataRankSS"]);
    $('#sessionTotalSHCount').html(session["SessionDataCurrent"]["DataRankSH"]);
    $('#sessionTotalSCount').html(session["SessionDataCurrent"]["DataRankS"]);
    $('#sessionTotalACount').html(session["SessionDataCurrent"]["DataRankA"]);

    $('#sessionDifferenceSSHCount').html((session["SessionDataDifference"]["DataRankSSH"]>=0?"+":"-")+""+session["SessionDataDifference"]["DataRankSSH"]);
    $('#sessionDifferenceSSCount').html((session["SessionDataDifference"]["DataRankSS"]>=0?"+":"-")+""+session["SessionDataDifference"]["DataRankSS"]);
    $('#sessionDifferenceSHCount').html((session["SessionDataDifference"]["DataRankSH"]>=0?"+":"-")+""+session["SessionDataDifference"]["DataRankSH"]);
    $('#sessionDifferenceSCount').html((session["SessionDataDifference"]["DataRankS"]>=0?"+":"-")+""+session["SessionDataDifference"]["DataRankS"]);
    $('#sessionDifferenceACount').html((session["SessionDataDifference"]["DataRankA"]>=0?"+":"-")+""+session["SessionDataDifference"]["DataRankA"]);

    $('#sessionDifferenceSSHCount').removeClass('green').removeClass('red').removeClass('grey').addClass((session["SessionDataDifference"]["DataRankSSH"]>=0?(session["SessionDataDifference"]["DataRankSSH"]==0?"grey":"green"):"red"));
    $('#sessionDifferenceSSHCount').removeClass('green').removeClass('red').removeClass('grey').addClass((session["SessionDataDifference"]["DataRankSS"]>=0?(session["SessionDataDifference"]["DataRankSS"]==0?"grey":"green"):"red"));
    $('#sessionDifferenceSSHCount').removeClass('green').removeClass('red').removeClass('grey').addClass((session["SessionDataDifference"]["DataRankSH"]>=0?(session["SessionDataDifference"]["DataRankSH"]==0?"grey":"green"):"red"));
    $('#sessionDifferenceSSHCount').removeClass('green').removeClass('red').removeClass('grey').addClass((session["SessionDataDifference"]["DataRankS"]>=0?(session["SessionDataDifference"]["DataRankS"]==0?"grey":"green"):"red"));
    $('#sessionDifferenceSSHCount').removeClass('green').removeClass('red').removeClass('grey').addClass((session["SessionDataDifference"]["DataRankA"]>=0?(session["SessionDataDifference"]["DataRankA"]==0?"grey":"green"):"red"));

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

    $('#sessionDifferenceLevel').html((session["SessionDataDifference"]["DataLevel"]>=0?"+":"-")+""+numberWithCommas(session["SessionDataDifference"]["DataLevel"].toFixed(rounding)));
    $('#sessionDifferenceTotalScore').html((session["SessionDataDifference"]["DataTotalScore"]>=0?"+":"-")+""+numberWithCommas(session["SessionDataDifference"]["DataTotalScore"]));
    $('#sessionDifferenceRankedScore').html((session["SessionDataDifference"]["DataRankedScore"]>=0?"+":"-")+""+numberWithCommas(session["SessionDataDifference"]["DataRankedScore"]));
    $('#sessionDifferenceWorldRank').html((session["SessionDataDifference"]["DataPPRank"]>=0?"+":"-")+""+numberWithCommas(session["SessionDataDifference"]["DataPPRank"]));
    $('#sessionDifferenceCountryRank').html((session["SessionDataDifference"]["DataCountryRank"]>=0?"+":"-")+""+numberWithCommas(session["SessionDataDifference"]["DataCountryRank"]));
    $('#sessionDifferencePlaycount').html((session["SessionDataDifference"]["DataPlaycount"]>=0?"+":"-")+""+numberWithCommas(session["SessionDataDifference"]["DataPlaycount"]));
    $('#sessionDifferencePlaytime').html((differencePlayTime>=0?"+":"-")+""+differencePlayTime+" "+diffType);
    $('#sessionDifferenceAccuracy').html((session["SessionDataDifference"]["DataAccuracy"]>=0?"+":"-")+""+session["SessionDataDifference"]["DataAccuracy"].toFixed(rounding));
    $('#sessionDifferencePerformance').html((session["SessionDataDifference"]["DataPerformance"]>=0?"+":"-")+""+numberWithCommas(session["SessionDataDifference"]["DataPerformance"].toFixed(rounding)));
}

function numberWithCommas(x) {
    return x.toString().replace(/\B(?=(\d{3})+(?!\d))/g, ",");
}