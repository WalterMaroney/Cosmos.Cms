$(document).ready(function() {
    $("#timeZone").html(getLocalTimeZone());
});

function getLocalTimeZone() {
    var datetime = new Date();
    var dateTimeString = datetime.toString();
    var timezone = dateTimeString.substring(dateTimeString.indexOf("(") - 1);
    return timezone;
}

// Takes an input UTC date/time and makes a local datetime.
function convertUtcToLocalDateTime(utcDateTimeString) {
    // This will turn UTC to local time
    var datetime = new Date(utcDateTimeString);
    var utc = new Date(Date.UTC(datetime.getFullYear(), datetime.getMonth(), datetime.getDay(), datetime.getHours(), datetime.getMinutes(), datetime.getSeconds()));
    return utc;
}